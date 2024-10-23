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

using Basilisque.AutoImplementer.CodeAnalysis.Extensions;
using Basilisque.AutoImplementer.CodeAnalysis.Generators.GenericAttributesGenerator;
using Basilisque.AutoImplementer.CodeAnalysis.Generators.StaticAttributesGenerator;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.AutoImplementerGenerator;

internal static class AutoImplementerGeneratorSelectors
{
    private static readonly string _attributeNameWithoutSuffix = StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeClassName.Substring(0, StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeClassName.Length - 9);

    internal static IncrementalValuesProvider<AutoImplementerGeneratorInfo> GetClassesToGenerate(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: isClassToAutoImplementCandidate,
            transform: transformClassCandidates
            )
        .Where(static gi => gi?.HasData == true);

        return classDeclarations!;
    }

    private static bool isClassToAutoImplementCandidate(SyntaxNode node, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // ensure node is a class declaration
        if (node is not ClassDeclarationSyntax classDeclaration)
            return false;

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

        if (classSymbol is not INamedTypeSymbol classNamedTypeSymbol)
            return null;

        cancellationToken.ThrowIfCancellationRequested();

        //try to find the attribute on the class
        if (!getClassAttributes(classSymbol, out var classAttributes))
            return null;

        cancellationToken.ThrowIfCancellationRequested();

        if (!addInterfaces(classDeclaration, classNamedTypeSymbol, classAttributes, out var autoImplementerGeneratorInfo))
            return null;

        return autoImplementerGeneratorInfo;
    }

    private static bool getClassAttributes(ISymbol classSymbol, [NotNullWhen(true)] out List<AttributeData>? classAttributes)
    {
        classAttributes = null;

        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name != StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeClassName)
                continue;

            if (attribute.AttributeClass.ContainingNamespace.ToString() != CommonGeneratorData.AutoImplementedAttributesTargetNamespace)
                continue;

            classAttributes = classAttributes ?? new List<AttributeData>();

            classAttributes.Add(attribute);
        }

        return classAttributes is not null;
    }

    private static bool addInterfaces(ClassDeclarationSyntax classDeclaration, INamedTypeSymbol classSymbol, List<AttributeData> classAttributes, [NotNullWhen(true)] out AutoImplementerGeneratorInfo? autoImplementerGeneratorInfo)
    {
        autoImplementerGeneratorInfo = null;

        foreach (var classAttribute in classAttributes)
        {
            Action<INamedTypeSymbol, AttributeData, AutoImplementerGeneratorInfo> action;
            if (classAttribute.AttributeClass?.IsGenericType == true)
                action = addInterfacesForGenericAttribute;
            else if (classAttribute.ConstructorArguments.Any(ca => ca.Values.Any()))
                action = addInterfacesForConstructorParameters;
            else if (classSymbol.AllInterfaces.Any())
                action = addInterfacesFromBaseTypes;
            else
                continue;

            autoImplementerGeneratorInfo = autoImplementerGeneratorInfo ?? createGeneratorInfo(classDeclaration);

            action(classSymbol, classAttribute, autoImplementerGeneratorInfo);
        }

        return autoImplementerGeneratorInfo is not null;
    }

    private static void addInterfacesForGenericAttribute(INamedTypeSymbol classSymbol, AttributeData classAttribute, AutoImplementerGeneratorInfo generatorInfo)
    {
        foreach (var typeArgument in classAttribute.AttributeClass!.TypeArguments)
        {
            if (typeArgument.TypeKind != TypeKind.Interface)
            {
                // type is not an interface, so skip it
                // do not report this as diagnostic because it is already reported by the GenericAttributesGenerator

                //var diagnostic = Diagnostic.Create(DiagnosticDescriptors.GenericAttributeInvalidTypeArgumentType, typeArgument.Locations.First(), typeArgument?.ToDisplayString() ?? "null");
                //generatorInfo.Diagnostics.Add(diagnostic);

                continue;
            }

            if (typeArgument is not INamedTypeSymbol namedTypeSymbolValue)
                continue;

            if (!generatorInfo.Interfaces.ContainsKey(namedTypeSymbolValue))
            {
                var interfaceInfo = new AutoImplementerGeneratorInterfaceInfo()
                {
                    IsInBaseList = classSymbol.AllInterfaces.Contains(namedTypeSymbolValue),
                };

                generatorInfo.Interfaces.Add(namedTypeSymbolValue, interfaceInfo);
            }
        }
    }

    private static void addInterfacesForConstructorParameters(INamedTypeSymbol classSymbol, AttributeData classAttribute, AutoImplementerGeneratorInfo generatorInfo)
    {
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

                if (value.Value is not INamedTypeSymbol namedTypeSymbolValue)
                    continue;

                if (namedTypeSymbolValue.TypeKind != TypeKind.Interface)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.GenericAttributeInvalidTypeArgumentType, namedTypeSymbolValue.Locations.First(), namedTypeSymbolValue?.ToDisplayString() ?? "null");
                    generatorInfo.Diagnostics.Add(diagnostic);
                    continue;
                }

                if (!generatorInfo.Interfaces.ContainsKey(namedTypeSymbolValue))
                {
                    var interfaceInfo = new AutoImplementerGeneratorInterfaceInfo()
                    {
                        IsInBaseList = classSymbol.AllInterfaces.Contains(namedTypeSymbolValue),
                    };

                    generatorInfo.Interfaces.Add(namedTypeSymbolValue, interfaceInfo);
                }
            }
        }
    }

    private static void addInterfacesFromBaseTypes(INamedTypeSymbol classSymbol, AttributeData classAttribute, AutoImplementerGeneratorInfo generatorInfo)
    {
        foreach (var interfaceSymbol in classSymbol.AllInterfaces)
        {
            var hasAttribute = interfaceSymbol.GetAttributes()
                .Any(attribute => attribute.AttributeClass?.Name == StaticAttributesGeneratorData.AutoImplementInterfaceAttributeClassName &&
                                  attribute.AttributeClass.ContainingNamespace.ToDisplayString() == CommonGeneratorData.AutoImplementedAttributesTargetNamespace);

            if (!hasAttribute)
                continue;

            if (!generatorInfo.Interfaces.ContainsKey(interfaceSymbol))
            {
                var interfaceInfo = new AutoImplementerGeneratorInterfaceInfo()
                {
                    IsInBaseList = true,
                };

                generatorInfo.Interfaces.Add(interfaceSymbol, interfaceInfo);
            }
        }
    }

    private static AutoImplementerGeneratorInfo createGeneratorInfo(ClassDeclarationSyntax classDeclaration)
    {
        var className = classDeclaration.Identifier.Text;
        var namespaceName = classDeclaration.GetNamespace();
        var accessModifier = Basilisque.CodeAnalysis.Syntax.AccessModifierExtensions.GetAccessModifier(classDeclaration);

        return new AutoImplementerGeneratorInfo(className, namespaceName, accessModifier);
    }
}
