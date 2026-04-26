export enum ProcessingState {
    Unknown = 0,
    Started = 1,
    Completed = 2,
    Failed = 10 + 5
}

export interface TupleDto {
    pair: { id: number; name: string; };
    legacyTuple: { item1: string; item2: number; item3: boolean; };
    state: ProcessingState;
}
