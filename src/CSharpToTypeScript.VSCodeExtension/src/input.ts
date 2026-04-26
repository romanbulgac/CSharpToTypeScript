export interface Input extends Configuration {
    code: string;
    fileName?: string;
    useTabs: boolean;
    tabSize?: number;
}

export interface Configuration {
    export: boolean;
    outputType: string;
    convertDatesTo: string;
    convertNullablesTo: string;
    toCamelCase: boolean;
    useOriginalNameAsComment: boolean;
    exportOneFilePerType: boolean;
    removeInterfacePrefix: boolean;
    generateImports: boolean;
    useKebabCase: boolean;
    appendModelSuffix: boolean;
    quotationMark: string;
    appendNewLine: boolean;
}

export const outputTypes = ['interface', 'class', 'type'];
export const dateOutputTypes = ['string', 'date', 'union'];
export const nullableOutputTypes = ['null', 'undefined'];
export const quotationMarks = ['double', 'single'];