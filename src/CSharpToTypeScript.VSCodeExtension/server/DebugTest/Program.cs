using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpToTypeScript.Core.DependencyInjection;
using CSharpToTypeScript.Core.Options;
using CSharpToTypeScript.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

var code = @"
using MyString = System.String;
using MyInt = System.Int32;
namespace Contracts {
    public class UserDto {
        public MyString Name { get; set; }
        public MyInt Age { get; set; }
    }
}";

var root = CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest)).GetCompilationUnitRoot();

var rewriter = new UsingAliasRewriter();
var rewritten = rewriter.Visit(root);

Console.WriteLine(rewritten.ToFullString());
Console.WriteLine("----");

var sp = new ServiceCollection().AddCSharpToTypeScript().BuildServiceProvider();
var converter = sp.GetRequiredService<ICodeConverter>();
Console.WriteLine(converter.ConvertToTypeScript(rewritten.ToFullString(), new CodeConversionOptions(true, false)));


public class UsingAliasRewriter : CSharpSyntaxRewriter
{
    private readonly Dictionary<string, TypeSyntax> _aliases = new();

    public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
    {
        if (node.Alias != null)
        {
            _aliases[node.Alias.Name.Identifier.ValueText] = node.Name;
        }
        return node; // Don't traverse inside using directive!
    }

    public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
    {
        if (_aliases.TryGetValue(node.Identifier.ValueText, out var typeSyntax))
        {
            return typeSyntax.WithTriviaFrom(node);
        }
        return base.VisitIdentifierName(node);
    }
}
