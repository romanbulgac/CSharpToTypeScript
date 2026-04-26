using System.Collections.Generic;
using System.Linq;
using CSharpToTypeScript.Core.Options;
using CSharpToTypeScript.Core.Utilities;

using static CSharpToTypeScript.Core.Utilities.StringUtilities;

namespace CSharpToTypeScript.Core.Models
{
    internal class RootEnumNode : RootNode
    {
        public RootEnumNode(string name, IEnumerable<EnumMemberNode> members, IEnumerable<string> documentation = null)
        {
            Name = name;
            Members = members;
            Documentation = documentation ?? System.Linq.Enumerable.Empty<string>();
        }

        public override string Name { get; }
        public IEnumerable<EnumMemberNode> Members { get; }
        public IEnumerable<string> Documentation { get; }

        public override string WriteTypeScript(CodeConversionOptions options, Context context)
        {
            string docPrefix = "";
            if (Documentation.Any())
            {
                if (Documentation.Count() == 1)
                {
                    docPrefix = $"/** {Documentation.First()} */" + NewLine;
                }
                else
                {
                    docPrefix = "/**" + NewLine;
                    foreach (var line in Documentation)
                    {
                        docPrefix += $" * {line}" + NewLine;
                    }
                    docPrefix += " */" + NewLine;
                }
            }

            return docPrefix + "export ".If(options.Export) + "enum "
             + Name
            + " {" + NewLine
            + Members.WriteTypeScript(options, context).Indent(options.UseTabs, options.TabSize).LineByLine(separator: ",") + NewLine
            + "}";
        }
    }
}