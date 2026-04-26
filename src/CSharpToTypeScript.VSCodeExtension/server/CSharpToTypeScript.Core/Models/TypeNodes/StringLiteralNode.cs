using System.Collections.Generic;
using System.Linq;
using CSharpToTypeScript.Core.Options;

namespace CSharpToTypeScript.Core.Models.TypeNodes
{
    internal class StringLiteralNode : TypeNode
    {
        public StringLiteralNode(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override IEnumerable<string> Requires => Enumerable.Empty<string>();

        public override string WriteTypeScript(CodeConversionOptions options, Context context)
            => Value.StartsWith("\"") ? Value : $"\"{Value}\""; // Preserve original quotes if possible
    }
}
