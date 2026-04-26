namespace Contracts
{
    public enum Status
    {
        Active,
        Inactive,
        Pending
    }

    public class User
    {
        public Status CurrentStatus { get; set; }
    }
}
