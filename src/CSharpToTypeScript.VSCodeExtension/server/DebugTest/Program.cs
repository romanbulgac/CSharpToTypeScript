using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

var serverDll = Path.Combine(
    AppContext.BaseDirectory, "..", "..", "..",
    "..", "..", "CSharpToTypeScript.Server", "bin", "Release", "net8.0", "publish",
    "CSharpToTypeScript.Server.dll");

if (!File.Exists(serverDll))
{
    var altPath = args.Length > 0 ? args[0] : null;
    if (altPath != null && File.Exists(altPath))
        serverDll = altPath;
    else
    {
        Console.Error.WriteLine($"Server not found at {serverDll}");
        Environment.Exit(1);
    }
}

var fixtures = new (string name, string code, string expected)[]
{
    ("record-with-body",
     File.ReadAllText(FindFixture("record-with-body", "input.cs")),
     File.ReadAllText(FindFixture("record-with-body", "expected.ts"))),
    ("required-keyword",
     File.ReadAllText(FindFixture("required-keyword", "input.cs")),
     File.ReadAllText(FindFixture("required-keyword", "expected.ts"))),
    ("tuple-and-enum",
     File.ReadAllText(FindFixture("tuple-and-enum", "input.cs")),
     File.ReadAllText(FindFixture("tuple-and-enum", "expected.ts"))),
};

var jsonOpts = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter() }
};

foreach (var (name, code, expected) in fixtures)
{
    Console.WriteLine($"=== {name} ===");

    var psi = new ProcessStartInfo("dotnet", serverDll)
    {
        UseShellExecute = false,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    using var proc = Process.Start(psi)!;

    var input = new
    {
        code,
        useTabs = false,
        tabSize = 4,
        export = true,
        convertDatesTo = "string",
        convertNullablesTo = "null",
        toCamelCase = true,
        removeInterfacePrefix = true,
        generateImports = false,
        useKebabCase = false,
        appendModelSuffix = false,
        quotationMark = "double",
        appendNewLine = false
    };

    var line = JsonSerializer.Serialize(input, jsonOpts);
    proc.StandardInput.WriteLine(line);
    proc.StandardInput.Flush();

    var response = proc.StandardOutput.ReadLine();
    proc.StandardInput.WriteLine("EXIT");
    proc.StandardInput.Close();
    proc.WaitForExit(5000);

    if (response == null)
    {
        Console.WriteLine("  NO RESPONSE FROM SERVER");
        Console.WriteLine($"  STDERR: {proc.StandardError.ReadToEnd()}");
        continue;
    }

    using var doc = JsonDocument.Parse(response);
    var root = doc.RootElement;
    var succeeded = root.GetProperty("succeeded").GetBoolean();
    var convertedCode = root.GetProperty("convertedCode").GetString() ?? "";
    var errorMessage = root.TryGetProperty("errorMessage", out var em) ? em.GetString() : null;

    Console.WriteLine($"  succeeded: {succeeded}");
    if (!succeeded) Console.WriteLine($"  error: {errorMessage}");

    var normalActual = convertedCode.Replace("\r\n", "\n").Trim();
    var normalExpected = expected.Replace("\r\n", "\n").Trim();

    if (normalActual == normalExpected)
        Console.WriteLine("  MATCH ✔");
    else
    {
        Console.WriteLine("  MISMATCH ✘");
        Console.WriteLine($"  ACTUAL  : {JsonSerializer.Serialize(normalActual)}");
        Console.WriteLine($"  EXPECTED: {JsonSerializer.Serialize(normalExpected)}");
    }
}

string FindFixture(string name, string file)
{
    var dir = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "..",
        "src", "test", "fixtures", "e2e", name);
    if (!Directory.Exists(dir))
    {
        dir = Path.GetFullPath(Path.Combine(
            Environment.CurrentDirectory, "src", "test", "fixtures", "e2e", name));
    }
    var path = Path.Combine(dir, file);
    if (!File.Exists(path))
        throw new FileNotFoundException($"Fixture not found: {path}");
    return path;
}
