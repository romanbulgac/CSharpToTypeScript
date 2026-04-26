export interface UserDto {
    isActive: boolean;
    ageByte: number;
    delta: number;
    scoreShort: number;
    scoreUShort: number;
    count: number;
    countUnsigned: number;
    total: number;
    totalUnsigned: number;
    ratio: number;
    ratioDouble: number;
    amount: number;
    initial: string;
    name: string;
    createdAt: string;
    updatedAt: string;
    correlationId: string;
    duration: string;
    endpoint: string;
    optionalCount: number | null;
    optionalName: string | null;
}
