using CSharpToTypeScript.Core.Utilities;
using CSharpToTypeScript.Core.Models.TypeNodes;
using CSharpToTypeScript.Core.Options;
using System.Collections.Generic;

namespace CSharpToTypeScript.Core.Models
{
    internal class FieldNode : IWritableNode, IDependentNode
    {
        public FieldNode(string name, TypeNode type, string jsonPropertyName = null)
        {
            Name = name;
            Type = type;
            JsonPropertyName = jsonPropertyName;
        }

        public string Name { get; }
        public TypeNode Type { get; }
        public string JsonPropertyName { get; set; }

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

            if (options.UseOriginalNameAsComment)
            {
                return $"/** {Name} */" + StringUtilities.NewLine + StringUtilities.Indentation(options.UseTabs, options.TabSize) + fieldText;
            }

            return fieldText;
        }
    }
}