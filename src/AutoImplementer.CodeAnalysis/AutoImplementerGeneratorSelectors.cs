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
using System.Collections.Immutable;
using System.Threading;

namespace Basilisque.AutoImplementer.CodeAnalysis;

internal static class AutoImplementerGeneratorSelectors
{
    private static readonly (ClassDeclarationSyntax Node, SemanticModel SemanticModel, ImmutableArray<INamedTypeSymbol> Interfaces) C_EMPTY_NODE_INFO = (null, null, ImmutableArray<INamedTypeSymbol>.Empty)!;

    internal static IncrementalValuesProvider<(ClassDeclarationSyntax Node, SemanticModel SemanticModel, ImmutableArray<INamedTypeSymbol> Interfaces)> GetClassesToGenerate(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (s, _) => isClassWithBaseTypes(s),
            transform: getSemanticTargetAndInterfaces
            )
        .Where(static m => m.Node is not null);

        return classDeclarations;
    }

    private static bool isClassWithBaseTypes(Microsoft.CodeAnalysis.SyntaxNode node)
    {
        // check if node is a class declaration with a base list
        return node is ClassDeclarationSyntax classDeclaration && classDeclaration.BaseList?.Types.Any() == true;
    }

    private static (ClassDeclarationSyntax Node, SemanticModel SemanticModel, ImmutableArray<INamedTypeSymbol> Interfaces) getSemanticTargetAndInterfaces(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        var interfacesBuilder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();

        foreach (var baseType in classDeclaration.BaseList!.Types)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var typeSymbol = context.SemanticModel.GetTypeInfo(baseType.Type).Type;
            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
                continue;

            var hasAttribute = namedTypeSymbol.GetAttributes()
                .Any(attribute => attribute.AttributeClass?.Name == AutoImplementerGeneratorData.C_AUTO_IMPLEMENT_INTERFACE_ATTRIBUTE_CLASSNAME &&
                                  attribute.AttributeClass.ContainingNamespace.ToDisplayString() == AutoImplementerGeneratorData.C_AUTOIMPLEMENTATTRIBUTE_TARGET_NAMESPACE);

            if (!hasAttribute)
                continue;

            interfacesBuilder.Add(namedTypeSymbol);
        }

        if (interfacesBuilder.Count == 0)
            return C_EMPTY_NODE_INFO;

        return (classDeclaration, context.SemanticModel, interfacesBuilder.ToImmutable());
    }
}
