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

using Basilisque.AutoImplementer.CodeAnalysis.Generators.StaticAttributesGenerator;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Threading;

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.AutoImplementerGenerator;

internal static class AutoImplementerGeneratorSelectors
{
    private static readonly string _attributeNameWithoutSuffix = StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeClassName.Substring(0, StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeClassName.Length - 9);
    private static readonly (ClassDeclarationSyntax Node, ImmutableArray<INamedTypeSymbol> Interfaces) _emptyNodeInfo = (null, ImmutableArray<INamedTypeSymbol>.Empty)!;

    internal static IncrementalValuesProvider<(ClassDeclarationSyntax Node, ImmutableArray<INamedTypeSymbol> Interfaces)> GetClassesToGenerate(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: isClassToAutoImplementCandidate,
            transform: transformClassCandidates
            )
        .Where(static m => m.Node is not null);

        return classDeclarations;
    }

    private static bool isClassToAutoImplementCandidate(SyntaxNode node, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // ensure node is a class declaration
        if (node is not ClassDeclarationSyntax classDeclaration)
            return false;

        // check if the node is annotated
        // all annotated nodes will be used for implementation or diagnostics
        // (use StartsWith because a.Name can look like 'AutoImplementInterfaces', 'AutoImplementInterfacesAttribute', 'AutoImplementInterfaces<...>' or 'AutoImplementInterfacesAttribute<...>')
        if (classDeclaration.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString().StartsWith(_attributeNameWithoutSuffix))))
            return true;

        // check if classDeclaration has a list of base types, those are all candidates
        if (classDeclaration.BaseList?.Types.Any() == true)
            return true;

        return false;
    }

    private static (ClassDeclarationSyntax Node, ImmutableArray<INamedTypeSymbol> Interfaces) transformClassCandidates(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        var interfacesBuilder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();

        //classDeclaration.AttributeLists.Where(al =>
        //{
        //    al.Attributes.Where(a =>
        //    {
        //        return true;
        //    }).ToList();
        //    return true;
        //}).ToList();

        //var etst = context.SemanticModel.Compilation.GetTypeByMetadataName(GenericAttributesGeneratorData.AutoImplementClassInterfacesAttributeGenericFullName);

        addInterfaces(context, classDeclaration, interfacesBuilder, cancellationToken);

        if (interfacesBuilder.Count == 0)
            return _emptyNodeInfo;

        return (classDeclaration, interfacesBuilder.ToImmutable());
    }

    private static void addInterfaces(GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclaration, ImmutableArray<INamedTypeSymbol>.Builder interfacesBuilder, CancellationToken cancellationToken)
    {
        foreach (var baseType in classDeclaration.BaseList!.Types)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var typeSymbol = context.SemanticModel.GetTypeInfo(baseType.Type).Type;
            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
                continue;

            var hasAttribute = namedTypeSymbol.GetAttributes()
                .Any(attribute => attribute.AttributeClass?.Name == StaticAttributesGeneratorData.AutoImplementInterfaceAttributeClassName &&
                                  attribute.AttributeClass.ContainingNamespace.ToDisplayString() == CommonGeneratorData.AutoImplementedAttributesTargetNamespace);

            if (!hasAttribute)
                continue;

            interfacesBuilder.Add(namedTypeSymbol);
        }
    }
}
