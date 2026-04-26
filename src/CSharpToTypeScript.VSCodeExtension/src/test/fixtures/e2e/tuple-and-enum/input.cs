namespace Contracts
{
    public enum ProcessingState
    {
        Unknown = 0,
        Started = 1,
        Completed = 2,
        Failed = 10 + 5
    }

    public class TupleDto
    {
        public (int Id, string Name) Pair { get; set; }
        public System.Tuple<string, int, bool> LegacyTuple { get; set; }
        public ProcessingState State { get; set; }
    }
}
