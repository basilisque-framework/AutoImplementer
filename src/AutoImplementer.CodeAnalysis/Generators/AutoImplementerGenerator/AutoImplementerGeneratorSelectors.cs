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

using Basilisque.AutoImplementer.CodeAnalysis.Generators.GenericAttributesGenerator;
using Basilisque.AutoImplementer.CodeAnalysis.Generators.StaticAttributesGenerator;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.AutoImplementerGenerator;

internal static class AutoImplementerGeneratorSelectors
{
    private static readonly string _attributeNameWithoutSuffix = StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeClassName.Substring(0, StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeClassName.Length - 9);

    internal static IncrementalValuesProvider<AutoImplementerGeneratorInfo?> GetClassesToGenerate(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: isClassToAutoImplementCandidate,
            transform: transformClassCandidates
            )
        .Where(static gi => gi is not null);

        return classDeclarations;
    }

    private static bool isClassToAutoImplementCandidate(SyntaxNode node, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // ensure node is a class declaration
        if (node is not ClassDeclarationSyntax classDeclaration)
            return false;

        // check if classDeclaration has a list of base types, those are all candidates
        if (classDeclaration.BaseList?.Types.Any() == true)
            return true;

        // check if the node is annotated
        // all annotated nodes will be used for implementation or diagnostics
        if (classDeclaration.AttributeLists.Any(isAttributeListRelevant))
            return true;

        return false;
    }

    private static bool isAttributeListRelevant(AttributeListSyntax attributeList)
    {
        return attributeList.Attributes.Any(isAttributeRelevant);
    }

    private static bool isAttributeRelevant(AttributeSyntax attribute)
    {
        if (isNonGenericAttributeRelevant(attribute))
            return true;

        return GenericAttributesGeneratorSelectors.IsAttributeRelevant(attribute);
    }

    private static bool isNonGenericAttributeRelevant(AttributeSyntax attribute)
    {
        IdentifierNameSyntax? identifierNameSyntax;

        // without namespace like [AutoImplementInterfaces]
        if (attribute.Name is IdentifierNameSyntax ins)
            identifierNameSyntax = ins;
        // with namespace like [Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces]
        else if (attribute.Name is QualifiedNameSyntax qns
            && qns.Left is QualifiedNameSyntax qnsl && qnsl.ToString() == CommonGeneratorData.AutoImplementedAttributesTargetNamespace
            && qns.Right is IdentifierNameSyntax insr)
        {
            identifierNameSyntax = insr;
        }
        else
        {
            return false;
        }

        // get the text of the attribute in the source
        // This is exactly what the developer typed.
        // It can be some unfinished stuff like 'AutoImplementIn'
        var attributeName = identifierNameSyntax.ToString();

        // check if attribute name starts with AutoImplementInterfaces
        // examples
        // [AutoImplementInterfaces]
        // [AutoImplementInterfacesAttribute]
        return attributeName.StartsWith(_attributeNameWithoutSuffix);
    }

    private static AutoImplementerGeneratorInfo? transformClassCandidates(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken);

        if (classSymbol is null)
            return null;

        cancellationToken.ThrowIfCancellationRequested();

        var interfacesBuilder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();

        // check if the class is annotated and add the interfaces accordingly
        if (getClassAttribute(classSymbol, out var classAttribute))
            addInterfaces(context, classSymbol, classAttribute, interfacesBuilder, cancellationToken);

        // check for annotations on implemented interfaces and add them accordingly
        addInterfaces(context, classDeclaration, interfacesBuilder, cancellationToken);

        if (interfacesBuilder.Count == 0)
            return null;

        return new AutoImplementerGeneratorInfo()
        {
            Node = classDeclaration,
            Interfaces = interfacesBuilder.ToImmutable(),
            //Diagnostics = 
        };
    }

    private static bool getClassAttribute(ISymbol classSymbol, [NotNullWhen(true)] out AttributeData? classAttribute)
    {
        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name != StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeClassName)
                continue;

            if (attribute.AttributeClass.ContainingNamespace.ToString() != CommonGeneratorData.AutoImplementedAttributesTargetNamespace)
                continue;

            classAttribute = attribute;
            return true;
        }

        classAttribute = null;
        return false;
    }

    private static void addInterfaces(GeneratorSyntaxContext context, ISymbol classSymbol, AttributeData classAttribute, ImmutableArray<INamedTypeSymbol>.Builder interfacesBuilder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var constructorArgument in classAttribute.ConstructorArguments)
        {
            if (constructorArgument.IsNull)
                continue;

            if (constructorArgument.Kind != TypedConstantKind.Array)
                continue;

            foreach (var value in constructorArgument.Values)
            {
                if (value.IsNull)
                    continue;

                if (value.Type is not INamedTypeSymbol namedTypeSymbolValue)
                    continue;

                if (!interfacesBuilder.Contains(namedTypeSymbolValue))
                    interfacesBuilder.Add(namedTypeSymbolValue);
            }
        }

        //if (classAttributeSyntax is GenericNameSyntax genericAttributeName)
        //    addInterfacesFromGenericAttribute(context, genericAttributeName, interfacesBuilder, cancellationToken);
        //else if (classAttributeSyntax is IdentifierNameSyntax identifierAttributeName)
        //{
        //    // Fall für [AutoImplementInterfaces(typeof(IMyInterface1), typeof(IMyInterface2))]
        //    var attributeData = context.SemanticModel.GetSymbolInfo(identifierAttributeName, cancellationToken).Symbol as IMethodSymbol;
        //    if (attributeData != null)
        //    {
        //        // Durchlaufe die Argumente des Attributs
        //        foreach (var argument in attributeData.Parameters)
        //        {
        //            var type = argument.Type as INamedTypeSymbol;
        //            if (type != null)
        //            {
        //                interfacesBuilder.Add(type);
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    // Fall für [AutoImplementInterfaces] - Interfaces von der Klasse selbst hinzufügen
        //    foreach (var interfaceSymbol in classSymbol.AllInterfaces)
        //    {
        //        interfacesBuilder.Add(interfaceSymbol);
        //    }
        //}
    }

    private static void addInterfacesFromGenericAttribute(GeneratorSyntaxContext context, GenericNameSyntax genericName, ImmutableArray<INamedTypeSymbol>.Builder interfacesBuilder, CancellationToken cancellationToken)
    {
        var typeArguments = genericName.TypeArgumentList.Arguments;
        foreach (var typeArgument in typeArguments)
        {
            var type = context.SemanticModel.GetTypeInfo(typeArgument, cancellationToken).Type as INamedTypeSymbol;
            if (type is null)
                continue;

            interfacesBuilder.Add(type);
        }
    }



    //var etst = context.SemanticModel.Compilation.GetTypeByMetadataName(GenericAttributesGeneratorData.AutoImplementClassInterfacesAttributeGenericFullName);

    private static void addInterfaces(GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclaration, ImmutableArray<INamedTypeSymbol>.Builder interfacesBuilder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

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
