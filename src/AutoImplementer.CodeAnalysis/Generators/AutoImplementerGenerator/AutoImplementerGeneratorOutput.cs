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
using Basilisque.AutoImplementer.CodeAnalysis.Generators.StaticAttributesGenerator;
using Basilisque.CodeAnalysis.Syntax;

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.AutoImplementerGenerator;

internal static class AutoImplementerGeneratorOutput
{
    internal static void OutputImplementations(SourceProductionContext context, AutoImplementerGeneratorInfo generationInfo, RegistrationOptions registrationOptions)
    {
        if (!checkPreconditions(registrationOptions))
            return;

        var syntaxNodesToImplement = getSyntaxNodesToImplement(generationInfo.Interfaces);

        // skip if nothing to implement
        if (!syntaxNodesToImplement.Any())
            return;

        var classDeclaration = generationInfo.ClassDeclaration;

        var className = classDeclaration.Identifier.Text;
        var namespaceName = classDeclaration.GetNamespace();

        var compilationName = namespaceName is null ? $"{className}.auto_impl" : $"{namespaceName}.{className}.auto_impl";

        var ci = registrationOptions.CreateCompilationInfo(compilationName, namespaceName);
        ci.EnableNullableContext = true;

        ci.AddNewClassInfo(className, classDeclaration.GetAccessModifier(), cl =>
        {
            cl.IsPartial = true;

            foreach (var node in syntaxNodesToImplement)
            {
                switch (node)
                {
                    case Basilisque.CodeAnalysis.Syntax.PropertyInfo pi:
                        cl.Properties.Add(pi);
                        break;
                    case Basilisque.CodeAnalysis.Syntax.MethodInfo mi:
                        cl.Methods.Add(mi);
                        break;
                }
            }
        }).AddToSourceProductionContext();
    }

    private static bool checkPreconditions(RegistrationOptions registrationOptions)
    {
        if (registrationOptions.Language != Language.CSharp)
            throw new System.NotSupportedException($"The language '{registrationOptions.Language}' is currently not supported by this generator.");

        return true;
    }

    private static IEnumerable<Basilisque.CodeAnalysis.Syntax.SyntaxNode> getSyntaxNodesToImplement(List<INamedTypeSymbol> interfaces)
    {
        foreach (var i in interfaces)
        {
            var members = i.GetMembers();

            foreach (var member in members)
            {
                Basilisque.CodeAnalysis.Syntax.SyntaxNode? node;

                switch (member)
                {
                    case IPropertySymbol propertySymbol:
                        node = implementProperty(propertySymbol);
                        break;
                    //case IMethodSymbol methodSymbol:
                    //    yield return implementMethod(methodSymbol);
                    //    break;
                    default:
                        node = null;
                        break;
                }

                if (node is null)
                    continue;

                yield return node;
            }
        }
    }

    private static Basilisque.CodeAnalysis.Syntax.PropertyInfo? implementProperty(IPropertySymbol propertySymbol)
    {
        // get the full qualified type name of the property
        var fqtn = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        if (string.IsNullOrWhiteSpace(fqtn))
            return null;

        // check if the property is nullable
        if (propertySymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            // check if the type is a value type and not already a nullable type
            if (!fqtn.EndsWith("?") && !fqtn.StartsWith("global::System.Nullable<"))
                fqtn += "?";
        }

        if (propertySymbol.GetAttributes().Any(a => a.AttributeClass?.Name == StaticAttributesGeneratorData.ImplementAsRequiredAttributeClassName))
            fqtn = "required " + fqtn;

        var pi = new Basilisque.CodeAnalysis.Syntax.PropertyInfo(fqtn, propertySymbol.Name);

        pi.InheritXmlDoc = true;
        pi.AccessModifier = mapAccessibility(propertySymbol.DeclaredAccessibility);

        return pi;
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
