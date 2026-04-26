export enum ProcessingState {
    Unknown = 0,
    Started = 1,
    Completed = 2,
    Failed = 10 + 5
}

export interface TupleDto {
    pair: { Id: number; Name: string; };
    legacyTuple: { Item1: string; Item2: number; Item3: boolean; };
    state: ProcessingState;
}
