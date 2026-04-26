namespace Contracts
{
    public record ProductRecordDto
    {
        public string Sku { get; init; }
        public decimal Price { get; init; }
    }
}
