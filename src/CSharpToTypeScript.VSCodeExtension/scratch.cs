using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class Program {
    static void Main() {
        var code = @"
            /// <summary>
            /// Gets the name.
            /// </summary>
            public string Name { get; set; }
        ";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetCompilationUnitRoot();
        var prop = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().First();
        var docTrivia = prop.GetLeadingTrivia()
            .Select(i => i.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();
            
        if (docTrivia != null) {
            var summary = docTrivia.ChildNodes().OfType<XmlElementSyntax>().FirstOrDefault(x => x.StartTag.Name.ToString() == "summary");
            if (summary != null) {
                var content = string.Join("", summary.Content.Select(c => c.ToString().Trim())).Trim();
                Console.WriteLine("Summary: " + content);
            }
        }
    }
}
