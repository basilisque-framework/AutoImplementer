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
    private static readonly string _attributeNameWithoutSuffix = StaticAttributesGeneratorData.AutoImplementsAttributeName.Substring(0, StaticAttributesGeneratorData.AutoImplementsAttributeName.Length - 9);

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
            if (attribute.AttributeClass?.Name != StaticAttributesGeneratorData.AutoImplementsAttributeName)
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
            else if (classAttribute.ConstructorArguments.Any(ca => ca.Kind == TypedConstantKind.Type || (ca.Kind == TypedConstantKind.Array && ca.Values.Any())))
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
                var isInBaseList = classSymbol.AllInterfaces.Contains(namedTypeSymbolValue);
                var interfaceInfo = createAutoImplementerGeneratorInterfaceInfo(namedTypeSymbolValue, isInBaseList);

                generatorInfo.Interfaces.Add(namedTypeSymbolValue, interfaceInfo);
            }
        }
    }

    private static void addInterfacesForConstructorParameters(INamedTypeSymbol classSymbol, AttributeData classAttribute, AutoImplementerGeneratorInfo generatorInfo)
    {
        var syntaxReference = classAttribute.ApplicationSyntaxReference;
        if (syntaxReference == null)
            return;

        var syntaxNode = syntaxReference.GetSyntax() as AttributeSyntax;
        if (syntaxNode == null)
            return;

        var argumentList = syntaxNode.ArgumentList;
        if (argumentList == null)
            return;

        if (argumentList.Arguments.Count < classAttribute.ConstructorArguments.Length)
            return;

        for (int i = 0; i < classAttribute.ConstructorArguments.Length; ++i)
        {
            var constructorArgument = classAttribute.ConstructorArguments[i];

            if (constructorArgument.IsNull)
                continue;

            if (constructorArgument.Kind == TypedConstantKind.Type)
            {
                var argument = argumentList.Arguments[i];

                if (constructorArgument.Value is INamedTypeSymbol namedTypeSymbolValue)
                    addInterfaceForConstructorParameter(classSymbol, generatorInfo, namedTypeSymbolValue, argument);
            }
            else if (constructorArgument.Kind == TypedConstantKind.Array)
            {
                if (argumentList.Arguments.Count < constructorArgument.Values.Length)
                    return;

                for (var j = 0; j < constructorArgument.Values.Length; j++)
                {
                    var value = constructorArgument.Values[j];
                    var argument = argumentList.Arguments[j];

                    if (value.IsNull)
                        continue;

                    if (value.Value is not INamedTypeSymbol namedTypeSymbolValue)
                        continue;

                    addInterfaceForConstructorParameter(classSymbol, generatorInfo, namedTypeSymbolValue, argument);
                }
            }
        }
    }

    private static void addInterfaceForConstructorParameter(INamedTypeSymbol classSymbol, AutoImplementerGeneratorInfo generatorInfo, INamedTypeSymbol namedTypeSymbolValue, AttributeArgumentSyntax attributeArgument)
    {
        if (namedTypeSymbolValue.TypeKind != TypeKind.Interface)
        {
            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.GenericAttributeInvalidTypeArgumentType, attributeArgument.GetLocation(), namedTypeSymbolValue?.ToDisplayString() ?? "null");
            generatorInfo.Diagnostics.Add(diagnostic);
            return;
        }

        if (!generatorInfo.Interfaces.ContainsKey(namedTypeSymbolValue))
        {
            var isInBaseList = classSymbol.AllInterfaces.Contains(namedTypeSymbolValue);
            var interfaceInfo = createAutoImplementerGeneratorInterfaceInfo(namedTypeSymbolValue, isInBaseList);

            generatorInfo.Interfaces.Add(namedTypeSymbolValue, interfaceInfo);
        }
    }

    private static void addInterfacesFromBaseTypes(INamedTypeSymbol classSymbol, AttributeData classAttribute, AutoImplementerGeneratorInfo generatorInfo)
    {
        foreach (var interfaceSymbol in classSymbol.Interfaces)
        {
            var attributeData = interfaceSymbol.GetAttributes()
                .Where(attribute => attribute.AttributeClass?.Name == StaticAttributesGeneratorData.AutoImplementableAttributeName &&
                                    attribute.AttributeClass.ContainingNamespace.ToDisplayString() == CommonGeneratorData.AutoImplementedAttributesTargetNamespace)
                .SingleOrDefault();

            if (attributeData is null)
                continue;

            if (!generatorInfo.Interfaces.ContainsKey(interfaceSymbol))
            {
                var interfaceInfo = createAutoImplementerGeneratorInterfaceInfo(attributeData, isInBaseList: true);

                generatorInfo.Interfaces.Add(interfaceSymbol, interfaceInfo);
            }
        }
    }

    private static AutoImplementerGeneratorInterfaceInfo createAutoImplementerGeneratorInterfaceInfo(INamedTypeSymbol interfaceNamedTypeSymbol, bool isInBaseList)
    {
        var attributeData = interfaceNamedTypeSymbol.GetAttributes()
            .Where(attribute => attribute.AttributeClass?.Name == StaticAttributesGeneratorData.AutoImplementableAttributeName &&
                              attribute.AttributeClass.ContainingNamespace.ToDisplayString() == CommonGeneratorData.AutoImplementedAttributesTargetNamespace)
            .SingleOrDefault();

        return createAutoImplementerGeneratorInterfaceInfo(attributeData, isInBaseList);
    }

    private static AutoImplementerGeneratorInterfaceInfo createAutoImplementerGeneratorInterfaceInfo(AttributeData attributeData, bool isInBaseList)
    {
        var result = new AutoImplementerGeneratorInterfaceInfo()
        {
            IsInBaseList = isInBaseList,
        };

        if (attributeData is null)
            return result;

        foreach (var namedArgument in attributeData.NamedArguments)
        {
            switch (namedArgument.Key)
            {
                case StaticAttributesGeneratorData.AutoImplementsAttribute_Strict:
                    if (namedArgument.Value.Kind == TypedConstantKind.Primitive && namedArgument.Value.Value is bool isEnabledValue)
                        result.Strict = isEnabledValue;
                    break;
                default:
                    break;
            }
        }

        return result;
    }

    private static AutoImplementerGeneratorInfo createGeneratorInfo(ClassDeclarationSyntax classDeclaration)
    {
        var className = classDeclaration.Identifier.Text;
        var namespaceName = classDeclaration.GetNamespace();
        var accessModifier = Basilisque.CodeAnalysis.Syntax.AccessModifierExtensions.GetAccessModifier(classDeclaration);

        return new AutoImplementerGeneratorInfo(className, namespaceName, accessModifier);
    }
}
