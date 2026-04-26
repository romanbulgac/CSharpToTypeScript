using CSharpToTypeScript.Core.Utilities;
using CSharpToTypeScript.Core.Models.TypeNodes;
using CSharpToTypeScript.Core.Options;
using System.Collections.Generic;
using System.Linq;

namespace CSharpToTypeScript.Core.Models
{
    internal class FieldNode : IWritableNode, IDependentNode
    {
        public FieldNode(string name, TypeNode type, string jsonPropertyName = null, IEnumerable<string> documentation = null)
        {
            Name = name;
            Type = type;
            JsonPropertyName = jsonPropertyName;
            Documentation = documentation ?? Enumerable.Empty<string>();
        }

        public string Name { get; }
        public TypeNode Type { get; }
        public string JsonPropertyName { get; set; }
        public IEnumerable<string> Documentation { get; }

        public IEnumerable<string> Requires => Type.Requires;

        public string WriteTypeScript(CodeConversionOptions options, Context context)
        {
            var tsName = (JsonPropertyName?
                .EscapeBackslashes()
                .EscapeQuotes(options.QuotationMark)
                .TransformIf(!JsonPropertyName.IsValidIdentifier(), StringUtilities.InQuotes(options.QuotationMark))
            ?? Name.TransformIf(options.ToCamelCase, StringUtilities.ToCamelCase));

            var separator = "?".If(Type.IsOptional(options, out _)) + ": ";
            var tsType = (Type.IsOptional(options, out var of) ? of.WriteTypeScript(options, context) : Type.WriteTypeScript(options, context));

            var fieldText = tsName + separator + tsType + ";";

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
            else if (options.UseOriginalNameAsComment)
            {
                result += $"/** {Name} */" + StringUtilities.NewLine + indent;
            }

            return result + fieldText;
        }
    }
}