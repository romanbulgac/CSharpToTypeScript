using System;

namespace Contracts
{
    public class UserDto
    {
        public bool IsActive { get; set; }
        public byte AgeByte { get; set; }
        public sbyte Delta { get; set; }
        public short ScoreShort { get; set; }
        public ushort ScoreUShort { get; set; }
        public int Count { get; set; }
        public uint CountUnsigned { get; set; }
        public long Total { get; set; }
        public ulong TotalUnsigned { get; set; }
        public float Ratio { get; set; }
        public double RatioDouble { get; set; }
        public decimal Amount { get; set; }
        public char Initial { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid CorrelationId { get; set; }
        public TimeSpan Duration { get; set; }
        public Uri Endpoint { get; set; }
        public int? OptionalCount { get; set; }
        public string? OptionalName { get; set; }
    }
}
