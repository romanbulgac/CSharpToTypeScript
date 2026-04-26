import * as assert from 'assert';
import * as cp from 'child_process';
import * as fs from 'fs';
import * as path from 'path';
import * as readline from 'readline';

type FixtureCommand = 'replace' | 'toFile';

type ConversionSettings = Partial<{
    export: boolean;
    convertDatesTo: 'string' | 'date' | 'union';
    convertNullablesTo: 'null' | 'undefined';
    toCamelCase: boolean;
    removeInterfacePrefix: boolean;
    generateImports: boolean;
    useKebabCase: boolean;
    appendModelSuffix: boolean;
    quotationMark: 'double' | 'single';
    appendNewLine: boolean;
}>;

interface FixtureMetadata {
    command?: FixtureCommand;
    sourceFileName?: string;
    expectedFileName?: string;
}

interface Fixture {
    name: string;
    directory: string;
    command: FixtureCommand;
    sourceFileName: string;
    expectedFileName?: string;
    settings: ConversionSettings;
}

interface ServerInput {
    code: string;
    fileName?: string;
    useTabs: boolean;
    tabSize: number;
    export: boolean;
    convertDatesTo: 'string' | 'date' | 'union';
    convertNullablesTo: 'null' | 'undefined';
    toCamelCase: boolean;
    removeInterfacePrefix: boolean;
    generateImports: boolean;
    useKebabCase: boolean;
    appendModelSuffix: boolean;
    quotationMark: 'double' | 'single';
    appendNewLine: boolean;
}

interface ServerOutput {
    convertedCode?: string;
    convertedFileName?: string;
    succeeded: boolean;
    errorMessage?: string;
}

const extensionRoot = path.resolve(__dirname, '../../..');
const fixturesRoot = path.join(extensionRoot, 'src', 'test', 'fixtures', 'e2e');

suite('Server E2E Fixtures', function () {
    this.timeout(120000);

    const fixtureNames = fs.readdirSync(fixturesRoot, { withFileTypes: true })
        .filter(entry => entry.isDirectory())
        .map(entry => entry.name)
        .sort((a, b) => a.localeCompare(b));

    fixtureNames.forEach(fixtureName => {
        test(`Fixture: ${fixtureName}`, async () => {
            const fixture = loadFixture(fixtureName);
            const inputCode = fs.readFileSync(path.join(fixture.directory, 'input.cs'), { encoding: 'utf8' });
            const expectedCode = fs.readFileSync(path.join(fixture.directory, 'expected.ts'), { encoding: 'utf8' });

            const serverInput = buildServerInput(
                inputCode,
                fixture.command === 'toFile' ? fixture.sourceFileName : undefined,
                fixture.settings);

            const output = await convertWithServer(serverInput);

            assert.strictEqual(output.succeeded, true, output.errorMessage ?? 'Conversion failed.');
            assert.strictEqual(normalizeText(output.convertedCode ?? ''), normalizeText(expectedCode));

            if (fixture.command === 'toFile') {
                assert.ok(output.convertedFileName, 'Expected converted file name in output.');
                if (fixture.expectedFileName) {
                    assert.strictEqual(output.convertedFileName, fixture.expectedFileName);
                }
            }
        });
    });
});

function loadFixture(name: string): Fixture {
    const directory = path.join(fixturesRoot, name);
    const metadataPath = path.join(directory, 'fixture.json');
    const settingsPath = path.join(directory, 'settings.json');

    const metadata = fileExists(metadataPath)
        ? parseJsonFile<FixtureMetadata>(metadataPath)
        : {};

    const settings = fileExists(settingsPath)
        ? parseJsonFile<ConversionSettings>(settingsPath)
        : {};

    return {
        name,
        directory,
        command: metadata.command ?? 'replace',
        sourceFileName: metadata.sourceFileName ?? 'input.cs',
        expectedFileName: metadata.expectedFileName,
        settings
    };
}

function buildServerInput(code: string, fileName: string | undefined, settings: ConversionSettings): ServerInput {
    return {
        code,
        fileName,
        useTabs: false,
        tabSize: 4,
        export: true,
        convertDatesTo: 'string',
        convertNullablesTo: 'null',
        toCamelCase: true,
        removeInterfacePrefix: true,
        generateImports: false,
        useKebabCase: false,
        appendModelSuffix: false,
        quotationMark: 'double',
        appendNewLine: false,
        ...settings
    };
}

function convertWithServer(input: ServerInput): Promise<ServerOutput> {
    return new Promise((resolve, reject) => {
        const serverPath = resolveServerPath();
        const server = cp.spawn('dotnet', [serverPath], { cwd: extensionRoot });
        let stderr = '';
        let settled = false;
        let rl: readline.Interface | undefined;

        const timeout = setTimeout(() => {
            rejectOnce(new Error(`Timed out waiting for conversion output. Server stderr: ${stderr}`));
        }, 30000);

        const cleanup = () => {
            clearTimeout(timeout);
            rl?.close();

            if (server.stdin && !server.stdin.destroyed) {
                server.stdin.write('EXIT\n');
                server.stdin.end();
            }

            if (!server.killed) {
                server.kill();
            }
        };

        const resolveOnce = (output: ServerOutput) => {
            if (settled) {
                return;
            }

            settled = true;
            cleanup();
            resolve(output);
        };

        const rejectOnce = (error: Error) => {
            if (settled) {
                return;
            }

            settled = true;
            cleanup();
            reject(error);
        };

        if (!server.stdout || !server.stdin) {
            rejectOnce(new Error('Failed to start conversion server with stdio streams.'));
            return;
        }

        rl = readline.createInterface({
            input: server.stdout,
            output: server.stdin
        });

        server.stderr?.on('data', data => {
            stderr += data.toString();
        });

        server.on('error', error => {
            rejectOnce(error as Error);
        });

        server.on('exit', code => {
            if (!settled && code !== 0) {
                rejectOnce(new Error(`Server exited with code ${code}. Stderr: ${stderr}`));
            }
        });

        rl.once('line', line => {
            try {
                const output = JSON.parse(line) as ServerOutput;
                resolveOnce(output);
            } catch (error) {
                rejectOnce(error as Error);
            }
        });

        server.stdin.write(`${JSON.stringify(input)}\n`);
    });
}

function resolveServerPath(): string {
    const releaseDirectory = path.join(extensionRoot, 'server', 'CSharpToTypeScript.Server', 'bin', 'Release');
    if (!fileExists(releaseDirectory)) {
        throw new Error('Release directory not found. Run npm run compile-server first.');
    }

    const frameworks = fs.readdirSync(releaseDirectory, { withFileTypes: true })
        .filter(entry => entry.isDirectory())
        .map(entry => entry.name)
        .sort((a, b) => b.localeCompare(a, undefined, { numeric: true, sensitivity: 'base' }));

    for (const framework of frameworks) {
        const candidate = path.join(
            releaseDirectory,
            framework,
            'publish',
            'CSharpToTypeScript.Server.dll');

        if (fileExists(candidate)) {
            return candidate;
        }
    }

    throw new Error('Published server dll was not found. Run npm run compile-server first.');
}

function parseJsonFile<T>(filePath: string): T {
    return JSON.parse(fs.readFileSync(filePath, { encoding: 'utf8' })) as T;
}

function normalizeText(content: string): string {
    return content.replace(/\r\n/g, '\n').trim();
}

function fileExists(filePath: string): boolean {
    return fs.existsSync(filePath);
}
