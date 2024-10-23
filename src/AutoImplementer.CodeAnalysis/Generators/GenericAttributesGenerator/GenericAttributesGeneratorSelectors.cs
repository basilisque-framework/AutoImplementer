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
using System.Threading;

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.GenericAttributesGenerator;

internal static class GenericAttributesGeneratorSelectors
{
    private static readonly string _attributeNameWithoutSuffix = StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeClassName.Substring(0, StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeClassName.Length - 9);

    internal static IncrementalValuesProvider<IReadOnlyList<GenericAttributesGeneratorInfo>> GetGenericAttributesToGenerate(IncrementalGeneratorInitializationContext context)
    {
        var attributeInfo = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: isAttributeToGenerateCandidate,
            transform: transformAttributeCandidates
            )
            .Where(l => l is not null);

        return attributeInfo!;
    }

    private static bool isAttributeToGenerateCandidate(SyntaxNode node, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // ensure node is a class declaration
        if (node is not ClassDeclarationSyntax classDeclaration)
            return false;

        // check if class declaration has a relevant attribute
        return classDeclaration.AttributeLists.Any(isAttributeListRelevant);
    }

    private static bool isAttributeListRelevant(AttributeListSyntax attributeList)
    {
        return attributeList.Attributes.Any(IsAttributeRelevant);
    }

    internal static bool IsAttributeRelevant(AttributeSyntax attribute)
    {
        return isAttributeRelevant(attribute, out var _);
    }

    private static bool isAttributeRelevant(AttributeSyntax attribute, out GenericNameSyntax? genericNameSyntax)
    {
        // ensure attribute is generic

        // without namespace like [AutoImplementInterfaces<IType1>]
        if (attribute.Name is GenericNameSyntax gan)
        {
            genericNameSyntax = gan;
        }
        // with namespace like [Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces<IType1>]
        else if (attribute.Name is QualifiedNameSyntax qns
            && qns.Left is QualifiedNameSyntax qnsl && qnsl.ToString() == CommonGeneratorData.AutoImplementedAttributesTargetNamespace
            && qns.Right is GenericNameSyntax ganr)
        {
            genericNameSyntax = ganr;
        }
        else
        {
            genericNameSyntax = null;
            return false;
        }

        // ensure attribute has at least one generic type parameter
        if (genericNameSyntax.Arity < 1)
            return false;

        // ensure TypeArgumentList is not null
        if (genericNameSyntax.TypeArgumentList is null)
            return false;

        // get the text of the attribute in the source
        // This is exactly what the developer typed.
        // It can be some unfinished stuff like 'AutoImplementInterfaces<IInterface1' or 'AutoImplementInterfacesAttribute<IInterface1, IInte'
        var attributeName = genericNameSyntax.ToString();

        // check if attribute name starts with AutoImplementInterfaces
        // examples
        // [AutoImplementInterfaces<IType1>]
        // [AutoImplementInterfacesAttribute<IType1>]
        // [AutoImplementInterfacesAttribute<IType1, IT
        return attributeName.StartsWith(_attributeNameWithoutSuffix);
    }

    private static IReadOnlyList<GenericAttributesGeneratorInfo>? transformAttributeCandidates(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        List<GenericAttributesGeneratorInfo>? result = null;

        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!isAttributeRelevant(attribute, out var genericName))
                    continue;

                result ??= new List<GenericAttributesGeneratorInfo>();

                result.Add(new GenericAttributesGeneratorInfo()
                {
                    Arity = genericName!.Arity,
                    InvalidTypeArguments = getInvalidTypeArguments(context.SemanticModel, genericName.TypeArgumentList.Arguments)
                });
            }
        }

        return result;
    }

    private static List<GenericAttributesGeneratorInvalidTypeInfo>? getInvalidTypeArguments(SemanticModel semanticModel, SeparatedSyntaxList<TypeSyntax> typeArguments)
    {
        List<GenericAttributesGeneratorInvalidTypeInfo>? result = null;

        foreach (var typeArgument in typeArguments)
        {
            var symbolInfo = semanticModel.GetTypeInfo(typeArgument);
            var typeSymbol = symbolInfo.Type;

            if (typeSymbol?.TypeKind == TypeKind.Interface)
                continue;

            result ??= new List<GenericAttributesGeneratorInvalidTypeInfo>();

            result.Add(new GenericAttributesGeneratorInvalidTypeInfo()
            {
                TypeName = typeSymbol?.ToDisplayString() ?? "null",
                DiagnosticDescriptor = DiagnosticDescriptors.GenericAttributeInvalidTypeArgumentType,
                Location = typeArgument.GetLocation()
            });
        }

        return result;
    }
}
