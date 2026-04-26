namespace Contracts
{
    public class QualifiedDto
    {
        public System.Int32 Value { get; set; }
        public System.Boolean Enabled { get; set; }
        public System.String Label { get; set; }
        public System.Collections.Generic.List<System.Guid> Ids { get; set; }
        public System.Collections.Generic.Dictionary<string, System.DateTime> Map { get; set; }
    }
}
