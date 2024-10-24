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
using Basilisque.CodeAnalysis.Syntax;
using MemberTypes = System.Reflection.MemberTypes;

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.AutoImplementerGenerator;

internal static class AutoImplementerGeneratorOutput
{
    internal static void OutputImplementations(SourceProductionContext context, AutoImplementerGeneratorInfo generationInfo, RegistrationOptions registrationOptions)
    {
        if (!checkPreconditions(registrationOptions))
            return;

        if (generationInfo.HasDiagnostics)
        {
            foreach (var diagnostic in generationInfo.Diagnostics)
                context.ReportDiagnostic(diagnostic);
        }

        if (!generationInfo.HasInterfaces)
            return;

        var syntaxNodesToImplement = getSyntaxNodesToImplement(context, generationInfo.Interfaces);

        // skip if nothing to implement
        if (!syntaxNodesToImplement.Any() && !generationInfo.Interfaces.Any(inf => !inf.Value.IsInBaseList))
            return;

        var className = generationInfo.ClassName;
        var namespaceName = generationInfo.ContainingNamespace;

        var compilationName = namespaceName is null ? $"{className}.auto_impl" : $"{namespaceName}.{className}.auto_impl";

        var ci = registrationOptions.CreateCompilationInfo(compilationName, namespaceName);
        ci.EnableNullableContext = true;

        ci.AddNewClassInfo(className, generationInfo.AccessModifier, cl =>
        {
            cl.IsPartial = true;

            cl.BaseClass = getBaseInterfaces(generationInfo.Interfaces);

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

    private static IEnumerable<Basilisque.CodeAnalysis.Syntax.SyntaxNode> getSyntaxNodesToImplement(SourceProductionContext context, Dictionary<INamedTypeSymbol, AutoImplementerGeneratorInterfaceInfo> interfaces)
    {
        foreach (var i in interfaces)
        {
            var members = i.Key.GetMembers();

            foreach (var member in members)
            {
                var autoImplementAttribute = getAutoImplementAttribute(member);
                if (shouldIgnoreProperty(autoImplementAttribute))
                    continue;

                Basilisque.CodeAnalysis.Syntax.SyntaxNode? node;

                switch (member)
                {
                    case IPropertySymbol propertySymbol:
                        node = implementProperty(context, propertySymbol, i.Value, autoImplementAttribute);
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

    private static string? getBaseInterfaces(Dictionary<INamedTypeSymbol, AutoImplementerGeneratorInterfaceInfo> interfaces)
    {
        var baseInterfaces = interfaces.Where(kvp => !kvp.Value.IsInBaseList).Select(kvp => kvp.Key.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).ToList();

        if (!baseInterfaces.Any())
            return null;

        return string.Join(", ", baseInterfaces);
    }

    private static Basilisque.CodeAnalysis.Syntax.PropertyInfo? implementProperty(SourceProductionContext context, IPropertySymbol propertySymbol, AutoImplementerGeneratorInterfaceInfo info, AttributeData? autoImplementAttribute)
    {
        // get the full qualified type name of the property
        var fqtn = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        if (string.IsNullOrWhiteSpace(fqtn))
            return null;

        getMemberAttributeInfo(autoImplementAttribute, MemberTypes.Property, context, out var implementPropertyAsRequired);

        // check if the property is nullable
        if (propertySymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            // check if the type is a value type and not already a nullable type
            if (!fqtn.EndsWith("?") && !fqtn.StartsWith("global::System.Nullable<"))
                fqtn += "?";
        }

        var pi = new Basilisque.CodeAnalysis.Syntax.PropertyInfo(fqtn, propertySymbol.Name);

        copyAttributes(propertySymbol, pi, out var propertyHasRequiredAttribute);

        if (implementPropertyAsRequired || propertyHasRequiredAttribute || info.ImplementAllPropertiesAsRequired)
            pi.IsRequired = true;

        pi.InheritXmlDoc = true;
        pi.AccessModifier = propertySymbol.DeclaredAccessibility.ToAccessModifier();

        return pi;
    }

    private static void getMemberAttributeInfo(AttributeData? autoImplementAttribute, MemberTypes memberType, SourceProductionContext context, out bool implementPropertyAsRequired)
    {
        implementPropertyAsRequired = false;

        if (autoImplementAttribute is null)
            return;

        foreach (var namedArgument in autoImplementAttribute.NamedArguments)
        {
            switch (namedArgument.Key)
            {
                case "AsRequired":
                    if (asRequiredAttributeIsValidOnMemberType(memberType, autoImplementAttribute, context)
                        && namedArgument.Value.Kind == TypedConstantKind.Primitive
                        && namedArgument.Value.Value is bool asRequired)
                    {
                        implementPropertyAsRequired = asRequired;
                    }
                    break;

                case "Implement": //this was already handled earlier
                default:
                    continue;
            }
        }
    }

    private static bool asRequiredAttributeIsValidOnMemberType(MemberTypes memberType, AttributeData autoImplementAttribute, SourceProductionContext context)
    {
        if (memberType == MemberTypes.Property /*|| memberType == MemberTypes.Field*/) // Required modifier would be valid for fields, too. But interfaces can't define fields.
            return true;

        var syntaxNode = autoImplementAttribute.ApplicationSyntaxReference?.GetSyntax() as Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax;

        var arg = syntaxNode?.ArgumentList?.Arguments.FirstOrDefault(arg => arg.NameColon?.Name.ToString() == "AsRequired");

        if (arg is not null)
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MemberAttributePropertyAsRequiredOnInvalidMemberType, arg.GetLocation()));

        return false;
    }

    private static AttributeData? getAutoImplementAttribute(ISymbol memberSymbol)
    {
        return memberSymbol.GetAttributes().SingleOrDefault(a =>
                    a.AttributeClass?.Name == StaticAttributesGeneratorData.AutoImplementOnMembersAttributeClassName
                    && a.AttributeClass.ContainingNamespace.ToDisplayString() == CommonGeneratorData.AutoImplementedAttributesTargetNamespace);
    }

    private static bool shouldIgnoreProperty(AttributeData? autoImplementAttribute)
    {
        if (autoImplementAttribute is null)
            return false;

        foreach (var namedArgument in autoImplementAttribute.NamedArguments)
        {
            if (namedArgument.Key == "Implement"
                && namedArgument.Value.Kind == TypedConstantKind.Primitive
                && namedArgument.Value.Value is bool shouldImplement)
            {
                return !shouldImplement;
            }
        }

        return false;
    }

    private static void copyAttributes(IPropertySymbol propertySymbol, PropertyInfo pi, out bool propertyHasRequiredAttribute)
    {
        propertyHasRequiredAttribute = false;

        var attributes = propertySymbol.GetAttributes();

        foreach (var attribute in attributes)
        {
            if (attribute.AttributeClass?.ContainingNamespace.ToDisplayString() == CommonGeneratorData.AutoImplementedAttributesTargetNamespace)
            {
                if (attribute.AttributeClass.Name == StaticAttributesGeneratorData.ImplementAsRequiredAttributeClassName)
                {
                    propertyHasRequiredAttribute = true;

                    // do not copy the basilisque internal attribute
                    continue;
                }
                else if (attribute.AttributeClass.Name == StaticAttributesGeneratorData.AutoImplementOnMembersAttributeClassName)
                    // do not copy the basilisque internal attribute
                    continue;
            }

            pi.Attributes.Add(new AttributeInfo(attribute.ToString()));
        }
    }
}
