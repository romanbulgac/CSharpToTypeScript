using CSharpToTypeScript.Core.Options;

namespace CSharpToTypeScript.Core.Services
{
    public interface ICodeConverter
    {
        System.Collections.Generic.IEnumerable<(string Name, string Code)> ConvertToTypeScript(string code, CodeConversionOptions options);
    }
}