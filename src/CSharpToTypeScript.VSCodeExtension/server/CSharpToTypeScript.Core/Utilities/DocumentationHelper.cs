using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToTypeScript.Core.Utilities
{
    internal static class DocumentationHelper
    {
        public static IEnumerable<string> GetSummary(SyntaxNode node)
        {
            var docTrivia = node.GetLeadingTrivia()
                .Select(i => i.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            if (docTrivia != null)
            {
                var summary = docTrivia.ChildNodes()
                    .OfType<XmlElementSyntax>()
                    .FirstOrDefault(x => x.StartTag.Name.ToString() == "summary");

                if (summary != null)
                {
                    var content = string.Join("\n", summary.Content.Select(c => c.ToString()));
                    var cleanedLines = content.Split('\n')
                        .Select(line => line.TrimStart())
                        .Select(line => line.StartsWith("///") ? line.Substring(3).Trim() : line.Trim())
                        .Where(line => !string.IsNullOrEmpty(line));
                    
                    return cleanedLines;
                }
            }

            return Enumerable.Empty<string>();
        }
    }
}
