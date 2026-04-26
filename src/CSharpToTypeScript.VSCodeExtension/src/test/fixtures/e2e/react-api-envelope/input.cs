namespace Contracts
{
    public class ApiErrorDto
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }

    public class ApiMetaDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }

    public class ApiResultDto<TData>
    {
        public TData Data { get; set; }
        public ApiErrorDto[] Errors { get; set; }
        public ApiMetaDto Meta { get; set; }
    }

    public class UserCardDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsOnline { get; set; }
    }

    public class UserListResponseDto : ApiResultDto<UserCardDto[]>
    {
        public string RequestId { get; set; }
    }
}
