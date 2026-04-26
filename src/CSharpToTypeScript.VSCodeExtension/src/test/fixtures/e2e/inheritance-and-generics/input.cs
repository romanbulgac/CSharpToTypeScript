namespace Contracts
{
    public interface IBaseModel
    {
        int Id { get; set; }
    }

    public interface IPaged<TItem> : IBaseModel
    {
        int Page { get; set; }
        TItem[] Items { get; set; }
    }

    public class ResponseBase
    {
        public string TraceId { get; set; }
    }

    public class ApiResponse<TItem> : ResponseBase, IBaseModel
    {
        public int Id { get; set; }
        public TItem Payload { get; set; }
    }
}
