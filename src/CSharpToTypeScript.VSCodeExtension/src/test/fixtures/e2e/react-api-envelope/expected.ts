export interface ApiErrorDto {
    code: string;
    message: string;
}

export interface ApiMetaDto {
    page: number;
    pageSize: number;
    total: number;
}

export interface ApiResultDto<TData> {
    data: TData;
    errors: ApiErrorDto[];
    meta: ApiMetaDto;
}

export interface UserCardDto {
    id: string;
    fullName: string;
    avatarUrl: string;
    isOnline: boolean;
}

export interface UserListResponseDto extends ApiResultDto<UserCardDto[]> {
    requestId: string;
}
