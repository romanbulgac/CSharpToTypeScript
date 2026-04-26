import * as assert from 'assert';
import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';
import * as vscode from 'vscode';

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

const extensionRoot = path.resolve(__dirname, '../../..');
const fixturesRoot = path.join(extensionRoot, 'src', 'test', 'fixtures', 'e2e');

suite('E2E Conversion Fixtures', function () {
    this.timeout(120000);

    const fixtureNames = fs.readdirSync(fixturesRoot, { withFileTypes: true })
        .filter(entry => entry.isDirectory())
        .map(entry => entry.name)
        .sort((a, b) => a.localeCompare(b));

    fixtureNames.forEach(fixtureName => {
        test(`Fixture: ${fixtureName}`, async () => {
            const fixture = loadFixture(fixtureName);
            const restoreSettings = await applySettings(fixture.settings);
            const temporaryDirectory = fs.mkdtempSync(path.join(os.tmpdir(), `cs2ts-${fixture.name}-`));

            try {
                const sourcePath = path.join(temporaryDirectory, fixture.sourceFileName);
                const inputPath = path.join(fixture.directory, 'input.cs');
                const expectedPath = path.join(fixture.directory, 'expected.ts');

                fs.copyFileSync(inputPath, sourcePath);

                const expected = normalizeText(fs.readFileSync(expectedPath, { encoding: 'utf8' }));
                const actual = fixture.command === 'toFile'
                    ? await runToFileScenario(sourcePath, fixture.expectedFileName)
                    : await runReplaceScenario(sourcePath);

                assert.strictEqual(normalizeText(actual), expected);
            } finally {
                await restoreSettings();
                await vscode.commands.executeCommand('workbench.action.closeAllEditors');
                fs.rmSync(temporaryDirectory, { recursive: true, force: true });
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

async function runReplaceScenario(sourcePath: string): Promise<string> {
    const document = await vscode.workspace.openTextDocument(sourcePath);
    await vscode.window.showTextDocument(document);
    await vscode.commands.executeCommand('csharpToTypeScript.csharpToTypeScriptReplace');
    return document.getText();
}

async function runToFileScenario(sourcePath: string, expectedFileName?: string): Promise<string> {
    await vscode.commands.executeCommand(
        'csharpToTypeScript.csharpToTypeScriptToFile',
        vscode.Uri.file(sourcePath));

    const outputFileName = expectedFileName ?? `${path.basename(sourcePath, '.cs')}.ts`;
    const outputPath = path.join(path.dirname(sourcePath), outputFileName);

    assert.ok(fs.existsSync(outputPath), `Expected output file was not created: ${outputFileName}`);

    return fs.readFileSync(outputPath, { encoding: 'utf8' });
}

async function applySettings(settings: ConversionSettings): Promise<() => Promise<void>> {
    const configuration = vscode.workspace.getConfiguration('csharpToTypeScript');
    const keys = Object.keys(settings) as Array<keyof ConversionSettings>;
    const previousValues = new Map<keyof ConversionSettings, unknown>();

    for (const key of keys) {
        previousValues.set(key, configuration.get(key));
        await configuration.update(key, settings[key], vscode.ConfigurationTarget.Global);
    }

    return async () => {
        for (const key of keys) {
            await configuration.update(key, previousValues.get(key), vscode.ConfigurationTarget.Global);
        }
    };
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
