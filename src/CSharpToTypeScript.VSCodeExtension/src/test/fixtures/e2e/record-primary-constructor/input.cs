namespace Contracts
{
    /// <summary>Represents a person</summary>
    public record Person(string Name, int Age);

    public record Employee(string Name, int Age, string Department)
    {
        public decimal Salary { get; init; }
    }
}
