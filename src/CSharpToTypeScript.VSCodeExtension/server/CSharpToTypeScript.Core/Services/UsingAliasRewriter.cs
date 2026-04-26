using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToTypeScript.Core.Services
{
    internal class UsingAliasRewriter : CSharpSyntaxRewriter
    {
        private readonly Dictionary<string, TypeSyntax> _aliases = new();

        public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Alias != null)
            {
                _aliases[node.Alias.Name.Identifier.ValueText] = node.Name;
            }
            return node;
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
}
