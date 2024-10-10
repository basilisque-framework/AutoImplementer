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

using System.Collections.Immutable;
using System.Text;

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.GenericAttributesGenerator;

internal class GenericAttributesGeneratorOutput
{
    internal static void OutputGenericAttributes(SourceProductionContext context, ImmutableArray<IReadOnlyList<GenericAttributesGeneratorInfo>> attributeInfos)
    {
        List<int> alreadyImplementedAttributes = new();

        foreach (var genericAttributeInfo in attributeInfos.SelectMany(list => list))
        {
            reportDiagnostic(context, genericAttributeInfo.InvalidTypeArguments);

            if (alreadyImplementedAttributes.Contains(genericAttributeInfo.Arity))
                continue;

            implementGenericInterface(context, genericAttributeInfo.Arity);

            alreadyImplementedAttributes.Add(genericAttributeInfo.Arity);
        }
    }

    private static void reportDiagnostic(SourceProductionContext context, IReadOnlyList<GenericAttributesGeneratorInvalidTypeInfo>? invalidTypeArguments)
    {
        if (invalidTypeArguments is null)
            return;

        foreach (var invalidTypeArgument in invalidTypeArguments)
        {
            var diagnostic = Diagnostic.Create(invalidTypeArgument.DiagnosticDescriptor, invalidTypeArgument.Location, invalidTypeArgument.TypeName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void implementGenericInterface(SourceProductionContext context, int arity)
    {
        var compilationName = $"{GenericAttributesGeneratorData.AutoImplementClassInterfacesAttributeGenericFullName}_generic_{arity}.g";

        var genericTypes = new StringBuilder(@"TInterface1");
        var genericTypeConstraints = new StringBuilder(@"            where TInterface1 : class
");

        for (int i = 2; i <= arity; i++)
        {
            genericTypes.Append($", TInterface{i}");
            genericTypeConstraints.AppendLine($"            where TInterface{i} : class");
        }

        var source = GenericAttributesGeneratorData.AutoImplementClassInterfacesAttributeGenericSourceTemplate
            .Replace("#GENERIC_TYPES#", genericTypes.ToString())
            .Replace("#GENERIC_TYPE_CONSTRAINTS#", genericTypeConstraints.ToString());

        context.AddSource(compilationName, source);
    }
}
