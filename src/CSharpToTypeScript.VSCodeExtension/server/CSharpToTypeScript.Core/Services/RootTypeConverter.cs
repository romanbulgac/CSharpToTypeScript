using System.Collections.Generic;
using System.Linq;
using CSharpToTypeScript.Core.Constants;
using CSharpToTypeScript.Core.Models;
using CSharpToTypeScript.Core.Models.TypeNodes;
using CSharpToTypeScript.Core.Services.TypeConversionHandlers;
using CSharpToTypeScript.Core.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToTypeScript.Core.Services
{
    internal class RootTypeConverter
    {
        private readonly TypeConversionHandler _typeConverter;

        public RootTypeConverter(TypeConversionHandler typeConverter)
        {
            _typeConverter = typeConverter;
        }

        public RootTypeNode Convert(TypeDeclarationSyntax type)
            => new RootTypeNode(
                name: type.Identifier.ValueText,
                fields: GetAllFields(type),
                genericTypeParameters: type.TypeParameterList?.Parameters
                    .Select(p => p.Identifier.ValueText)
                    .Where(p => !string.IsNullOrWhiteSpace(p)) ?? Enumerable.Empty<string>(),
                baseTypes: ConvertBaseTypes(
                    type.BaseList?.Types ?? Enumerable.Empty<BaseTypeSyntax>(),
                    type),
                fromInterface: type is InterfaceDeclarationSyntax,
                documentation: DocumentationHelper.GetSummary(type));

        private IEnumerable<FieldNode> GetAllFields(TypeDeclarationSyntax type)
        {
            var bodyFields = type.ChildNodes()
                .SelectMany(node => node switch
                {
                    PropertyDeclarationSyntax property when IsSerializable(property, type) => new[] { ConvertProperty(property, type) },
                    FieldDeclarationSyntax field when IsSerializable(field) => ConvertField(field),
                    _ => Enumerable.Empty<FieldNode>()
                })
                .ToList();

            var bodyFieldNames = new HashSet<string>(bodyFields.Select(f => f.Name), System.StringComparer.OrdinalIgnoreCase);

            if (type is RecordDeclarationSyntax record && record.ParameterList != null)
            {
                var paramFields = record.ParameterList.Parameters
                    .Where(p => !bodyFieldNames.Contains(p.Identifier.ValueText))
                    .Select(p => ConvertRecordParameter(p))
                    .ToList();

                return paramFields.Concat(bodyFields);
            }

            return bodyFields;
        }

        private FieldNode ConvertRecordParameter(ParameterSyntax parameter)
        {
            var typeNode = _typeConverter.Handle(parameter.Type);
            return new FieldNode(
                name: parameter.Identifier.ValueText,
                type: typeNode,
                documentation: DocumentationHelper.GetSummary(parameter));
        }

        private FieldNode ConvertProperty(PropertyDeclarationSyntax property, TypeDeclarationSyntax containingType)
        {
            var literalValue = GetLiteralValue(property, containingType);
            var typeNode = literalValue != null 
                ? new StringLiteralNode(literalValue) 
                : _typeConverter.Handle(property.Type);

            var summaryLines = DocumentationHelper.GetSummary(property);
            var validationLines = ExtractValidationDocLines(property);
            var allDocLines = summaryLines.Concat(validationLines);

            return new FieldNode(
                name: property.Identifier.ValueText,
                type: typeNode,
                jsonPropertyName: GetJsonPropertyName(property),
                documentation: allDocLines);
        }

        private string GetLiteralValue(PropertyDeclarationSyntax property, TypeDeclarationSyntax containingType)
        {
            var expression = property.Initializer?.Value ?? property.ExpressionBody?.Expression;
            if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return literal.Token.Text;
            }
            if (expression is IdentifierNameSyntax identifier)
            {
                var field = containingType.Members.OfType<FieldDeclarationSyntax>()
                    .SelectMany(f => f.Declaration.Variables)
                    .FirstOrDefault(v => v.Identifier.ValueText == identifier.Identifier.ValueText);
                    
                if (field?.Initializer?.Value is LiteralExpressionSyntax fieldLiteral && fieldLiteral.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    return fieldLiteral.Token.Text;
                }
            }
            return null;
        }

        private IEnumerable<FieldNode> ConvertField(FieldDeclarationSyntax field)
           => field.Declaration.Variables.Select(v => 
           {
               var literalValue = GetLiteralValue(v);
               var typeNode = literalValue != null
                   ? new StringLiteralNode(literalValue)
                   : _typeConverter.Handle(field.Declaration.Type);

               var summaryLines = DocumentationHelper.GetSummary(field);
               var validationLines = ExtractValidationDocLines(field);
               var allDocLines = summaryLines.Concat(validationLines);

               return new FieldNode(
                   name: v.Identifier.ValueText,
                   type: typeNode,
                   jsonPropertyName: GetJsonPropertyName(field),
                   documentation: allDocLines);
           })
           .Where(f => !string.IsNullOrWhiteSpace(f.Name));

        private string GetLiteralValue(VariableDeclaratorSyntax variable)
        {
            if (variable.Initializer?.Value is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return literal.Token.Text;
            }
            return null;
        }

        private string GetJsonPropertyName(MemberDeclarationSyntax member)
        {
            foreach (var attribute in member.AttributeLists.SelectMany(a => a.Attributes))
            {
                static bool IsNotNamed(AttributeArgumentSyntax attribute)
                    => attribute.NameColon is null && attribute.NameEquals is null;

                static bool ArgumentNameEquals(AttributeArgumentSyntax attribute, string name)
                    => attribute.NameColon?.Name.Identifier.ValueText == name;

                static bool PropertyNameEquals(AttributeArgumentSyntax attribute, string name)
                    => attribute.NameEquals?.Name.Identifier.ValueText == name;

                static bool HasStringExpression(AttributeArgumentSyntax attribute)
                    => attribute.Expression.IsKind(SyntaxKind.StringLiteralExpression);

                var argument = GetAttributeName(attribute) switch
                {
                    Attributes.JsonPropertyName => attribute.ArgumentList?.Arguments
                        .FirstOrDefault(a => HasStringExpression(a)
                            && (IsNotNamed(a) || ArgumentNameEquals(a, AttributeArgumentNames.Name))),
                    Attributes.JsonProperty => attribute.ArgumentList?.Arguments
                        .FirstOrDefault(a => HasStringExpression(a)
                            && (IsNotNamed(a) || ArgumentNameEquals(a, AttributeArgumentNames.PropertyName) || PropertyNameEquals(a, AttributePropertyNames.PropertyName))),
                    _ => null
                };

                if (!(argument is null))
                {
                    return ((LiteralExpressionSyntax)argument.Expression).Token.ValueText;
                }
            }

            return null;
        }

        private IEnumerable<TypeNode> ConvertBaseTypes(IEnumerable<BaseTypeSyntax> baseTypes, TypeDeclarationSyntax containingType)
        {
            var types = baseTypes;
            if (!(containingType is InterfaceDeclarationSyntax))
            {
                types = types.Take(1);
            }

            var namedTypes = types
                .Select(t => _typeConverter.Handle(t.Type))
                .OfType<NamedTypeNode>();

            if (!(containingType is InterfaceDeclarationSyntax))
            {
                namedTypes = namedTypes.Where(t => !t.Name.HasInterfacePrefix());
            }

            return namedTypes;
        }

        private bool IsSerializable(PropertyDeclarationSyntax property, TypeDeclarationSyntax containingType)
            => IsPublic(property, containingType) && IsNotStatic(property)
            && IsGettable(property) && !HasJsonIgnoreAttribute(property);

        private bool IsSerializable(FieldDeclarationSyntax field)
            => IsPublic(field) && IsNotStatic(field) && !HasJsonIgnoreAttribute(field);

        private bool IsNotStatic(MemberDeclarationSyntax member)
            => member.Modifiers.All(m => m.Kind() != SyntaxKind.StaticKeyword);

        private bool HasJsonIgnoreAttribute(MemberDeclarationSyntax member)
            => member.AttributeLists.Any(l => l.Attributes.Any(a => GetAttributeName(a) == Attributes.JsonIgnore));

        private bool IsPublic(MemberDeclarationSyntax property, TypeDeclarationSyntax containingType = null)
            => containingType is InterfaceDeclarationSyntax
            || property.Modifiers.Any(m => m.Kind() == SyntaxKind.PublicKeyword);

        private bool IsGettable(PropertyDeclarationSyntax property)
            => property.AccessorList is null
            || (HasGetter(property, out var getter) && !HasRestrictedAccess(getter));

        private bool HasGetter(PropertyDeclarationSyntax property, out AccessorDeclarationSyntax getter)
            => (getter = property.AccessorList?.Accessors.FirstOrDefault(IsGetter)) is AccessorDeclarationSyntax;

        private bool IsGetter(AccessorDeclarationSyntax accessor)
            => accessor.Kind() == SyntaxKind.GetAccessorDeclaration;

        private bool HasRestrictedAccess(AccessorDeclarationSyntax accessor)
            => accessor.Modifiers.Any(m => RestrictiveAccessModifiers.Contains(m.Kind()));

        private IEnumerable<SyntaxKind> RestrictiveAccessModifiers { get; }
            = new[] { SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword };

        private string GetAttributeName(AttributeSyntax attribute)
            => attribute.Name switch
            {
                QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
                _ => attribute.Name.ToString()
            };

        private IEnumerable<string> ExtractValidationDocLines(MemberDeclarationSyntax member)
        {
            var lines = new List<string>();

            foreach (var attribute in member.AttributeLists.SelectMany(a => a.Attributes))
            {
                var name = GetAttributeName(attribute);
                var argList = attribute.ArgumentList?.Arguments;

                switch (name)
                {
                    case Constants.Attributes.Required:
                        lines.Add("@required");
                        break;

                    case Constants.Attributes.MaxLength:
                        if (argList?.Count > 0)
                            lines.Add($"@maxLength {GetArgumentValue(argList.Value[0])}");
                        break;

                    case Constants.Attributes.MinLength:
                        if (argList?.Count > 0)
                            lines.Add($"@minLength {GetArgumentValue(argList.Value[0])}");
                        break;

                    case Constants.Attributes.StringLength:
                        if (argList?.Count > 0)
                        {
                            lines.Add($"@maxLength {GetArgumentValue(argList.Value[0])}");
                            var minArg = argList.Value.FirstOrDefault(a =>
                                a.NameEquals?.Name.Identifier.ValueText == "MinimumLength");
                            if (minArg != null)
                                lines.Add($"@minLength {GetArgumentValue(minArg)}");
                        }
                        break;

                    case Constants.Attributes.Range:
                        if (argList?.Count >= 2)
                        {
                            lines.Add($"@minimum {GetArgumentValue(argList.Value[0])}");
                            lines.Add($"@maximum {GetArgumentValue(argList.Value[1])}");
                        }
                        break;

                    case Constants.Attributes.EmailAddress:
                        lines.Add("@format email");
                        break;

                    case Constants.Attributes.Phone:
                        lines.Add("@format phone");
                        break;

                    case Constants.Attributes.Url:
                        lines.Add("@format uri");
                        break;

                    case Constants.Attributes.RegularExpression:
                        if (argList?.Count > 0)
                        {
                            var pattern = GetArgumentValue(argList.Value[0]);
                            lines.Add($"@pattern {pattern}");
                        }
                        break;
                }
            }

            return lines;
        }

        private string GetArgumentValue(AttributeArgumentSyntax argument)
        {
            if (argument.Expression is LiteralExpressionSyntax literal)
                return literal.Token.ValueText;
            return argument.Expression.ToString();
        }
    }
}