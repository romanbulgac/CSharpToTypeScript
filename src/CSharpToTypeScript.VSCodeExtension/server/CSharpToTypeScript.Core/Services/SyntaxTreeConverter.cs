using System;
using System.Collections.Generic;
using System.Linq;
using CSharpToTypeScript.Core.Models;
using CSharpToTypeScript.Core.Options;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToTypeScript.Core.Services
{
    internal class SyntaxTreeConverter
    {
        private readonly RootTypeConverter _rootTypeConverter;
        private readonly RootEnumConverter _rootEnumConverter;

        public SyntaxTreeConverter(RootTypeConverter rootTypeConverter, RootEnumConverter rootEnumConverter)
        {
            _rootTypeConverter = rootTypeConverter;
            _rootEnumConverter = rootEnumConverter;
        }

        public IEnumerable<FileNode> Convert(CompilationUnitSyntax root, CodeConversionOptions options)
        {
            var rootNodes = ConvertRootNodes(root);
            
            if (options.ExportOneFilePerType)
            {
                return rootNodes.Select(node => new FileNode(new[] { node }));
            }

            return new[] { new FileNode(rootNodes) };
        }

        private IEnumerable<RootNode> ConvertRootNodes(CompilationUnitSyntax root)
            => root.DescendantNodes()
                .Where(node => (node is TypeDeclarationSyntax type && IsNotStatic(type)) || node is EnumDeclarationSyntax)
                .Select(node => node switch
                {
                    TypeDeclarationSyntax type => (RootNode)_rootTypeConverter.Convert(type),
                    EnumDeclarationSyntax @enum => _rootEnumConverter.Convert(@enum),
                    _ => throw new ArgumentException("Unknown syntax type.")
                })
                .ToList();

        private bool IsNotStatic(TypeDeclarationSyntax type)
            => type.Modifiers.All(m => m.Kind() != SyntaxKind.StaticKeyword);
    }
}