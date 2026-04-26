public interface IDocument
{
    string Type { get; }
}

public class Album : IDocument
{
    public const string Role = "Admin";

    public static string TypeName = "album";

    public int Id { get; set; }

    public string Type => TypeName;
}

public class Track : IDocument
{
    public int Id { get; set; }

    public string Type { get; } = "track";
}
