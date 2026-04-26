using System.Collections.Generic;
using System.Linq;
using CSharpToTypeScript.Core.Models.TypeNodes;
using CSharpToTypeScript.Core.Options;
using CSharpToTypeScript.Core.Utilities;

using static CSharpToTypeScript.Core.Utilities.StringUtilities;

namespace CSharpToTypeScript.Core.Models
{
    internal class RootTypeNode : RootNode
    {
        public RootTypeNode(string name, IEnumerable<FieldNode> fields, IEnumerable<string> genericTypeParameters,
            IEnumerable<TypeNode> baseTypes, bool fromInterface)
        {
            Name = name;
            Fields = fields;
            GenericTypeParameters = genericTypeParameters;
            BaseTypes = baseTypes;
            FromInterface = fromInterface;
        }

        public override string Name { get; }
        public IEnumerable<FieldNode> Fields { get; }
        public IEnumerable<string> GenericTypeParameters { get; }
        public IEnumerable<TypeNode> BaseTypes { get; }
        public bool FromInterface { get; }

        public override IEnumerable<string> Requires
            => Fields.SelectMany(f => f.Requires)
                .Concat(BaseTypes.SelectMany(b => b.Requires))
                .Except(GenericTypeParameters)
                .Distinct();

        public override string WriteTypeScript(CodeConversionOptions options, Context context)
        {
            context = context.Clone();
            context.GenericTypeParameters = GenericTypeParameters;

            string typeWord = options.OutputType == OutputType.Type ? "type" : (!FromInterface && options.OutputType == OutputType.Class ? "class" : "interface");

            string declaration = "export ".If(options.Export)
                + typeWord + " "
                + Name.TransformIf(options.RemoveInterfacePrefix, StringUtilities.RemoveInterfacePrefix)
                + ("<" + GenericTypeParameters.ToCommaSepratedList() + ">").If(GenericTypeParameters.Any());

            if (options.OutputType == OutputType.Type)
            {
                return declaration + " = "
                    + (BaseTypes.Any() ? string.Join(" & ", BaseTypes.WriteTypeScript(options, context)) + " & " : "")
                    + "{" + NewLine
                    + Fields.WriteTypeScript(options, context).Indent(options.UseTabs, options.TabSize).LineByLine() + NewLine
                    + "};";
            }

            return declaration
                + (" extends " + BaseTypes.WriteTypeScript(options, context).ToCommaSepratedList()).If(BaseTypes.Any())
                + " {" + NewLine
                + Fields.WriteTypeScript(options, context).Indent(options.UseTabs, options.TabSize).LineByLine() + NewLine
                + "}";
        }
    }
}