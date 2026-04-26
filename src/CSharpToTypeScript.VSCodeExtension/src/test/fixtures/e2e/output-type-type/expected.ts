export type BaseDto = {
    id: string;
};

export type MyTypeDto = BaseDto & {
    name: string;
    age: number;
};
