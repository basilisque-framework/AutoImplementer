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

using Basilisque.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Basilisque.AutoImplementer.CodeAnalysis;

internal static class AutoImplementerGeneratorOutput
{

    internal static void OutputAttributes(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource(
            AutoImplementerGeneratorData.C_AUTO_IMPLEMENT_INTERFACE_ATTRIBUTE_COMPILATIONNAME,
            AutoImplementerGeneratorData.AUTO_IMPLEMENT_INTERFACE_ATTRIBUTE_SOURCE
        );

        context.AddSource(
            AutoImplementerGeneratorData.C_AUTOIMPLEMENTATTRIBUTE_ON_MEMBERS_COMPILATIONNAME,
            AutoImplementerGeneratorData.AUTO_IMPLEMENT_ON_MEMBERS_ATTRIBUTE_SOURCE
        );

        context.AddSource(
            AutoImplementerGeneratorData.C_IMPLEMENT_AS_REQUIRED_ATTRIBUTE_COMPILATIONNAME,
            AutoImplementerGeneratorData.IMPLEMENT_AS_REQUIRED_ATTRIBUTE_SOURCE
        );

        context.AddSource(
            AutoImplementerGeneratorData.C_REQUIRED_DOTNET_6_PATCH_COMPILATIONNAME,
            AutoImplementerGeneratorData.REQUIRED_DOTNET_6_PATCH_SOURCE
        );
    }

    internal static void OutputImplementations(SourceProductionContext context, (ClassDeclarationSyntax ClassToGenerate, SemanticModel SemanticModel, ImmutableArray<INamedTypeSymbol> Interfaces) generationInfo, RegistrationOptions registrationOptions)
    {
        if (!checkPreconditions(registrationOptions))
            return;

        var classDeclaration = generationInfo.ClassToGenerate;

        var className = classDeclaration.Identifier.Text;
        var namespaceName = determineNamespace(classDeclaration);

        var compilationName = namespaceName is null ? $"{className}.auto_impl" : $"{namespaceName}.{className}.auto_impl";

        var ci = registrationOptions.CreateCompilationInfo(compilationName, namespaceName);

        ci.AddNewClassInfo(className, classDeclaration.GetAccessModifier(), cl =>
        {
            cl.IsPartial = true;

            foreach (var interfaceInfo in generationInfo.Interfaces)
            {
                foreach (var member in interfaceInfo.GetMembers())
                {
                    switch (member)
                    {
                        case IPropertySymbol propertySymbol:
                            implementProperty(cl, propertySymbol, generationInfo.SemanticModel);
                            break;

                            //case IMethodSymbol methodSymbol:
                            //    implementMethod(cl, methodSymbol, generationInfo.SemanticModel);
                            //    break;
                    }
                }
            }




            //    var missingAssemblyNameDiagnostic = Diagnostic.Create(DiagnosticDescriptors.MissingAssemblyName, Location.None);
            //    context.ReportDiagnostic(missingAssemblyNameDiagnostic);



            //var initializeDependenciesGeneratedMethod = new MethodInfo(true, "initializeDependenciesGenerated")
            //{
            //    Parameters = {
            //            new ParameterInfo(ParameterKind.Ordinary, "DependencyCollection", "collection")
            //        }
            //};
            //initializeDependenciesGeneratedMethod.Body.Add(@"/* initialize dependencies - generated from assembly dependencies */");
            //addDependenciesToBody(cancellationToken, initializeDependenciesGeneratedMethod.Body, namedDependencyRegistratorTypes);
            //cl.Methods.Add(initializeDependenciesGeneratedMethod);
        }).AddToSourceProductionContext();
    }

    private static bool checkPreconditions(RegistrationOptions registrationOptions)
    {
        if (registrationOptions.Language != Language.CSharp)
            throw new System.NotSupportedException($"The language '{registrationOptions.Language}' is currently not supported by this generator.");

        return true;
    }

    private static string? determineNamespace(Microsoft.CodeAnalysis.SyntaxNode? syntaxNode)
    {
        // there is no namespace when the node is null
        if (syntaxNode == null)
            return null;

        // when the node is a namespace declaration, return the name
        if (syntaxNode is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
            return namespaceDeclarationSyntax.Name.ToString();

        // when the node is a file-scoped namespace declaration, return the name
        if (syntaxNode is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
            return fileScopedNamespaceDeclarationSyntax.Name.ToString();

        // there is no namespace when the node is a compilation unit syntax node
        if (syntaxNode is CompilationUnitSyntax)
            return null;

        // recursively check the parent nodes for the namespace
        return determineNamespace(syntaxNode.Parent);
    }

    private static void implementProperty(Basilisque.CodeAnalysis.Syntax.ClassInfo classInfo, IPropertySymbol propertySymbol, SemanticModel semanticModel)
    {
        // get the full qualified type name of the property
        var fqtn = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        if (string.IsNullOrWhiteSpace(fqtn))
            return;

        // check if the property is nullable
        if (propertySymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            // check if the type is a value type and not already a nullable type
            if (!fqtn.EndsWith("?") && !fqtn.StartsWith("global::System.Nullable<"))
                fqtn += "?";
        }

        if (propertySymbol.GetAttributes().Any(a => a.AttributeClass?.Name == AutoImplementerGeneratorData.C_IMPLEMENT_AS_REQUIRED_ATTRIBUTE_CLASSNAME))
            fqtn = "required " + fqtn;

        var pi = new Basilisque.CodeAnalysis.Syntax.PropertyInfo(fqtn, propertySymbol.Name);

        pi.InheritXmlDoc = true;
        pi.AccessModifier = mapAccessibility(propertySymbol.DeclaredAccessibility);

        classInfo.Properties.Add(pi);
    }

    private static AccessModifier mapAccessibility(Accessibility declaredAccessibility)
    {
        switch (declaredAccessibility)
        {
            case Accessibility.NotApplicable:
                return AccessModifier.Public;
            case Accessibility.Private:
                return AccessModifier.Private;
            case Accessibility.ProtectedAndInternal:
                return AccessModifier.ProtectedInternal;
            case Accessibility.Protected:
                return AccessModifier.Protected;
            case Accessibility.Internal:
                return AccessModifier.Internal;
            case Accessibility.ProtectedOrInternal:
                return AccessModifier.ProtectedInternal;
            case Accessibility.Public:
                return AccessModifier.Public;
            default:
                return AccessModifier.Public;
        }
    }
}
