export interface CollectionsDto {
    scores: number[];
    matrix: number[][];
    avatarBytes: string;
    tags: string[];
    ids: string[];
    dates: string[];
    indexes: number[];
    totals: number[];
    counters: { [key: string]: number; };
    reverseLookup: { [key: number]: string; };
    nestedMap: { [key: string]: number[]; };
    untypedArray: any[];
}
