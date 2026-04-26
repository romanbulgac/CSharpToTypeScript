export interface Output {
    convertedCode?: string;
    convertedFileName?: string;
    convertedFiles?: { convertedCode: string; convertedFileName: string; }[];
    succeeded: boolean;
    errorMessage?: string;
}