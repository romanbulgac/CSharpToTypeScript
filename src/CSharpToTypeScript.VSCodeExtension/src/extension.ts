import * as cp from 'child_process';
import * as fs from 'fs';
import * as path from 'path';
import * as readline from 'readline';
import * as vscode from 'vscode';
import { Input, dateOutputTypes, nullableOutputTypes, quotationMarks, Configuration, outputTypes } from './input';
import { Output } from './output';
import { allowedOrDefault, fullRange, textFromActiveDocument } from './utilities';
import { TextEncoder } from 'util';

let server: cp.ChildProcess;
let rl: readline.Interface;
let serverRunning = false;
let executingCommand = false;

export function activate(context: vscode.ExtensionContext) {
    let standardError = '';

    const serverPath = resolveServerPath(context);
    if (!serverPath) {
        vscode.window.showErrorMessage('"C# to TypeScript" server binary was not found. Run "npm run compile-server" and reload window.');
        return;
    }

    serverRunning = true;
    server = cp.spawn('dotnet', [serverPath]);

    server.on('error', err => {
        serverRunning = false;
        vscode.window.showErrorMessage(`"C# to TypeScript" server related error occurred: "${err.message}".`);
    });
    server.stderr?.on('data', data => {
        standardError += data;
    });
    server.on('exit', code => {
        serverRunning = false;
        vscode.window.showWarningMessage(`"C# to TypeScript" server shutdown with code: "${code}". Standard error: "${standardError}".`);
    });

    if (!server.stdout || !server.stdin) {
        serverRunning = false;
        vscode.window.showErrorMessage('"C# to TypeScript" server streams are unavailable.');
        return;
    }

    rl = readline.createInterface({
        input: server.stdout,
        output: server.stdin
    });

    context.subscriptions.push(
        vscode.commands.registerCommand('csharpToTypeScript.csharpToTypeScriptReplace', replaceCommand),
        vscode.commands.registerCommand('csharpToTypeScript.csharpToTypeScriptToClipboard', toClipboardCommand),
        vscode.commands.registerCommand('csharpToTypeScript.csharpToTypeScriptPasteAs', pasteAsCommand),
        vscode.commands.registerCommand('csharpToTypeScript.csharpToTypeScriptToFile', toFileCommand),
        vscode.commands.registerCommand('csharpToTypeScript.exportToTypeScript', exportToTypeScriptCommand));
}

export function deactivate() {
    if (serverRunning && server.stdin && server.stdin.writable) {
        server.stdin.write('EXIT\n');
    }
}

function resolveServerPath(context: vscode.ExtensionContext): string | undefined {
    const releaseDirectory = context.asAbsolutePath(path.join('server', 'CSharpToTypeScript.Server', 'bin', 'Release'));
    if (!fs.existsSync(releaseDirectory)) {
        return undefined;
    }

    const frameworks = fs.readdirSync(releaseDirectory, { withFileTypes: true })
        .filter(entry => entry.isDirectory())
        .map(entry => entry.name)
        .sort((a, b) => b.localeCompare(a, undefined, { numeric: true, sensitivity: 'base' }));

    for (const framework of frameworks) {
        const candidate = context.asAbsolutePath(path.join(
            'server', 'CSharpToTypeScript.Server', 'bin', 'Release', framework, 'publish', 'CSharpToTypeScript.Server.dll'));
        if (fs.existsSync(candidate)) {
            return candidate;
        }
    }

    return undefined;
}

async function replaceCommand() {
    try {
        if (!vscode.window.activeTextEditor) {
            return;
        }

        const result = await convert(textFromActiveDocument());

        const document = vscode.window.activeTextEditor.document;
        const selection = vscode.window.activeTextEditor.selection;

        await vscode.window.activeTextEditor.edit(
            builder => builder.replace(
                !selection.isEmpty ? selection : fullRange(document),
                result.convertedCode));
    }
    catch (error) {
        vscode.window.showWarningMessage((error as Error).message);
    }
}

async function toClipboardCommand() {
    try {
        const result = await convert(textFromActiveDocument());

        await vscode.env.clipboard.writeText(result.convertedCode!);
    }
    catch (error) {
        vscode.window.showWarningMessage((error as Error).message);
    }
}

async function pasteAsCommand() {
    try {
        if (!vscode.window.activeTextEditor) {
            return;
        }

        const result = await convert(await vscode.env.clipboard.readText());

        const selection = vscode.window.activeTextEditor.selection;

        await vscode.window.activeTextEditor.edit(
            builder => builder.replace(
                selection,
                result.convertedCode));
    }
    catch (error) {
        vscode.window.showWarningMessage((error as Error).message);
    }
}

async function toFileCommand(uri?: vscode.Uri) {
    try {
        uri = uri ?? vscode.window.activeTextEditor?.document.uri;
        if (!uri) {
            return;
        }

        const document = await vscode.workspace.openTextDocument(uri);
        const code = document.getText();
        const filePath = uri.fsPath;

        const result = await convert(code, filePath);

        await vscode.workspace.fs.writeFile(
            vscode.Uri.file(path.join(path.dirname(filePath), result.convertedFileName!)),
            new TextEncoder().encode(result.convertedCode));
    }
    catch (error) {
        vscode.window.showWarningMessage((error as Error).message);
    }
}

async function exportToTypeScriptCommand(uri?: vscode.Uri, selectedUris?: vscode.Uri[]) {
    try {
        const urisToProcess = selectedUris && selectedUris.length > 0 ? selectedUris : (uri ? [uri] : []);
        if (urisToProcess.length === 0) {
            return;
        }

        const filesToProcess: string[] = [];
        for (const selectedUri of urisToProcess) {
            const stat = await vscode.workspace.fs.stat(selectedUri);
            if (stat.type === vscode.FileType.Directory) {
                const filesInDir = await vscode.workspace.findFiles(new vscode.RelativePattern(selectedUri, '**/*.cs'));
                filesToProcess.push(...filesInDir.map(f => f.fsPath));
            } else if (selectedUri.fsPath.endsWith('.cs')) {
                filesToProcess.push(selectedUri.fsPath);
            }
        }

        if (filesToProcess.length === 0) {
            vscode.window.showInformationMessage('No .cs files found to export.');
            return;
        }

        const destinationUri = await vscode.window.showOpenDialog({
            canSelectFiles: false,
            canSelectFolders: true,
            canSelectMany: false,
            openLabel: 'Export Here',
            title: 'Select Destination Folder'
        });

        if (!destinationUri || destinationUri.length === 0) {
            return; // User cancelled
        }

        const destinationFolder = destinationUri[0].fsPath;
        let exportedCount = 0;

        await vscode.window.withProgress({
            location: vscode.ProgressLocation.Notification,
            title: "Exporting C# to TypeScript",
            cancellable: false
        }, async (progress) => {
            const increment = 100 / filesToProcess.length;
            for (const filePath of filesToProcess) {
                try {
                    const document = await vscode.workspace.openTextDocument(filePath);
                    const code = document.getText();
                    
                    const result = await convert(code, filePath);

                    if (result.convertedFiles && result.convertedFiles.length > 0) {
                        for (const file of result.convertedFiles) {
                            await vscode.workspace.fs.writeFile(
                                vscode.Uri.file(path.join(destinationFolder, file.convertedFileName)),
                                new TextEncoder().encode(file.convertedCode)
                            );
                            exportedCount++;
                        }
                    } else if (result.convertedFileName) {
                        await vscode.workspace.fs.writeFile(
                            vscode.Uri.file(path.join(destinationFolder, result.convertedFileName)),
                            new TextEncoder().encode(result.convertedCode)
                        );
                        exportedCount++;
                    }
                } catch (error) {
                    console.error(`Failed to convert ${filePath}:`, error);
                }
                progress.report({ increment });
            }
        });

        vscode.window.showInformationMessage(`Successfully exported ${exportedCount} file(s) to TypeScript.`);
    }
    catch (error) {
        vscode.window.showWarningMessage((error as Error).message);
    }
}

function convert(code: string, fileName?: string) {
    return new Promise<{ convertedCode: string, convertedFileName?: string, convertedFiles?: { convertedCode: string, convertedFileName: string }[] }>((resolve, reject) => {
        if (!serverRunning) {
            reject(new Error(`"C# to TypeScript" server isn't running! Reload Window to restart it.`));
            return;
        }

        if (executingCommand) {
            reject(new Error('Conversion in progress...'));
            return;
        }

        executingCommand = true;

        const configuration = vscode.workspace.getConfiguration()
            .get<Configuration>('csharpToTypeScript')!;

        const input: Input = {
            code: code,
            fileName: fileName,
            useTabs: !(vscode.window.activeTextEditor?.options.insertSpaces ?? true),
            tabSize: vscode.window.activeTextEditor?.options.tabSize as number ?? 4,
            export: !!configuration.export,
            outputType: allowedOrDefault(configuration.outputType, outputTypes),
            convertDatesTo: allowedOrDefault(configuration.convertDatesTo, dateOutputTypes),
            convertNullablesTo: allowedOrDefault(configuration.convertNullablesTo, nullableOutputTypes),
            toCamelCase: !!configuration.toCamelCase,
            useOriginalNameAsComment: !!configuration.useOriginalNameAsComment,
            exportOneFilePerType: !!configuration.exportOneFilePerType,
            removeInterfacePrefix: !!configuration.removeInterfacePrefix,
            generateImports: !!configuration.generateImports,
            useKebabCase: !!configuration.useKebabCase,
            appendModelSuffix: !!configuration.appendModelSuffix,
            quotationMark: allowedOrDefault(configuration.quotationMark, quotationMarks),
            appendNewLine: !!configuration.appendNewLine
        };

        const inputLine = JSON.stringify(input) + '\n';

        rl.question(inputLine, outputLine => {
            try {
                const { convertedCode, convertedFileName, convertedFiles, succeeded, errorMessage } = JSON.parse(outputLine) as Output;

                if (!succeeded) {
                    reject(new Error(`"C# to TypeScript" extension encountered an error while converting your code: "${errorMessage}".`));
                } else {
                    resolve({
                        convertedCode: convertedCode ?? '',
                        convertedFileName,
                        convertedFiles
                    });
                }
            }
            catch (error) {
                reject(error as Error);
            }
            finally {
                executingCommand = false;
            }
        });
    });
}

