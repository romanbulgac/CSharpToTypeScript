export interface BaseModel {
    id: number;
}

export interface Paged<TItem> extends BaseModel {
    page: number;
    items: TItem[];
}

export interface ResponseBase {
    traceId: string;
}

export interface ApiResponse<TItem> extends ResponseBase {
    id: number;
    payload: TItem;
}
