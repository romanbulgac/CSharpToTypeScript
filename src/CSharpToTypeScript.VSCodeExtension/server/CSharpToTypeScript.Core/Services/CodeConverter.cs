using CSharpToTypeScript.Core.Options;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToTypeScript.Core.Services
{
    internal class CodeConverter : ICodeConverter
    {
        private readonly SyntaxTreeConverter _syntaxTreeConverter;

        public CodeConverter(SyntaxTreeConverter syntaxTreeConverter)
        {
            _syntaxTreeConverter = syntaxTreeConverter;
        }

        public string ConvertToTypeScript(string code, CodeConversionOptions options)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest));
            var root = syntaxTree.GetCompilationUnitRoot();
            var rewrittenRoot = (CompilationUnitSyntax)new UsingAliasRewriter().Visit(root);
            return _syntaxTreeConverter.Convert(rewrittenRoot).WriteTypeScript(options);
        }
    }
}