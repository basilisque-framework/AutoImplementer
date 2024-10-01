/*
   Copyright 2024 Alexander Stärk

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Basilisque.AutoImplementer.CodeAnalysis;

internal static class AutoImplementerGeneratorSelectors
{
    private static (ClassDeclarationSyntax? Node, List<INamedTypeSymbol>? Interfaces) C_EMPTY_NODE_INFO = (null, null);

    internal static IncrementalValuesProvider<(ClassDeclarationSyntax Node, SemanticModel SemanticModel, List<INamedTypeSymbol> Interfaces)> GetClassesToGenerate(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (s, _) => isClassWithBaseTypes(s),
            transform: static (ctx, _) =>
            {
                var (node, interfaces) = getSemanticTargetAndInterfaces(ctx);
                return (Node: node, ctx.SemanticModel, Interfaces: interfaces);
            })
        .Where(static m => m.Node is not null);

        return classDeclarations!;
    }

    private static bool isClassWithBaseTypes(Microsoft.CodeAnalysis.SyntaxNode node)
    {
        // check if node is a class declaration with a base list
        return node is ClassDeclarationSyntax classDeclaration && classDeclaration.BaseList?.Types.Any() == true;
    }

    private static (ClassDeclarationSyntax? Node, List<INamedTypeSymbol>? Interfaces) getSemanticTargetAndInterfaces(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (classDeclaration.BaseList is null)
            return C_EMPTY_NODE_INFO;

        var interfaces = new List<INamedTypeSymbol>();

        foreach (var baseType in classDeclaration.BaseList.Types)
        {
            var typeSymbol = context.SemanticModel.GetTypeInfo(baseType.Type).Type;
            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
                continue;

            var hasAttribute = namedTypeSymbol.GetAttributes()
                .Select(attribute => attribute.AttributeClass)
                .Any(attributeClass => attributeClass?.Name == AutoImplementerGeneratorOutput.C_AUTO_IMPLEMENT_INTERFACE_ATTRIBUTE_CLASSNAME &&
                                       attributeClass.ContainingNamespace.ToDisplayString() == AutoImplementerGeneratorOutput.C_AUTOIMPLEMENTATTRIBUTE_TARGET_NAMESPACE);

            if (!hasAttribute)
                continue;

            interfaces.Add(namedTypeSymbol);
        }

        if (!interfaces.Any())
            return C_EMPTY_NODE_INFO;

        return (classDeclaration, interfaces);
    }
}
