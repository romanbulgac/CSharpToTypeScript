using System.Collections.Generic;
using System.Linq;
using CSharpToTypeScript.Core.Options;
using CSharpToTypeScript.Core.Utilities;

namespace CSharpToTypeScript.Core.Models
{
    internal class EnumMemberNode : IWritableNode
    {
        public EnumMemberNode(string name, string value, IEnumerable<string> documentation = null)
        {
            Name = name;
            Value = value;
            Documentation = documentation ?? Enumerable.Empty<string>();
        }

        public string Name { get; }
        public string Value { get; }
        public IEnumerable<string> Documentation { get; }

        public string WriteTypeScript(CodeConversionOptions options, Context context)
        {
            var value = options.StringEnums
                ? Name.TransformIf(options.EnumStringToCamelCase, StringUtilities.ToCamelCase)
                      .InQuotes(options.QuotationMark)
                : Value?.SquashWhistespace();

            var memberText = Name + (" = " + value).If(!string.IsNullOrWhiteSpace(value));

            string result = "";
            var indent = StringUtilities.Indentation(options.UseTabs, options.TabSize);

            if (Documentation.Any())
            {
                if (Documentation.Count() == 1)
                {
                    result += $"/** {Documentation.First()} */" + StringUtilities.NewLine + indent;
                }
                else
                {
                    result += "/**" + StringUtilities.NewLine;
                    foreach (var line in Documentation)
                    {
                        result += indent + $" * {line}" + StringUtilities.NewLine;
                    }
                    result += indent + " */" + StringUtilities.NewLine + indent;
                }
            }

            return result + memberText;
        }
    }
}