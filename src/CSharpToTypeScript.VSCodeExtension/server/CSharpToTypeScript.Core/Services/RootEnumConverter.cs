using System.Collections.Generic;
using System.Linq;
using CSharpToTypeScript.Core.Models;
using CSharpToTypeScript.Core.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToTypeScript.Core.Services
{
    internal class RootEnumConverter
    {
        public RootEnumNode Convert(EnumDeclarationSyntax @enum)
            => new RootEnumNode(
                name: @enum.Identifier.ValueText,
                members: ConvertEnumMembers(@enum.Members),
                documentation: DocumentationHelper.GetSummary(@enum));

        private IEnumerable<EnumMemberNode> ConvertEnumMembers(IEnumerable<EnumMemberDeclarationSyntax> members)
        {
            var result = new List<EnumMemberNode>();
            int? nextValue = 0;
            foreach (var m in members)
            {
                string value;
                if (m.EqualsValue != null)
                {
                    value = m.EqualsValue.Value.ToString();
                    if (int.TryParse(value, out int parsed))
                    {
                        nextValue = parsed + 1;
                    }
                    else
                    {
                        nextValue = null;
                    }
                }
                else
                {
                    value = nextValue?.ToString();
                    if (nextValue.HasValue) nextValue++;
                }
                result.Add(new EnumMemberNode(m.Identifier.ValueText, value, DocumentationHelper.GetSummary(m)));
            }
            return result;
        }
    }
}