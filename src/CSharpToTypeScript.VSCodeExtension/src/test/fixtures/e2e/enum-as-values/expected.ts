export enum Status {
    Active = 0,
    Inactive = 1,
    Pending = 2
}

export interface User {
    currentStatus: Status;
}
