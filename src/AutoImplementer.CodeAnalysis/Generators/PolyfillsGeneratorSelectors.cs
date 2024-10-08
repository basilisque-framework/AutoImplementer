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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators;

internal static class PolyfillsGeneratorSelectors
{
    internal static IncrementalValuesProvider<string> GetExistingPolyfills(IncrementalGeneratorInitializationContext context)
    {
        var existingPolyfills = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (s, _) => isSupportedPolyfillClass(s),
            transform: getAlreadyExistingPolyfills
            );

        return existingPolyfills;
    }

    private static bool isSupportedPolyfillClass(SyntaxNode node)
    {
        // check if node is a class declaration
        if (node is not ClassDeclarationSyntax classDeclaration)
            return false;

        // get full name
        var className = classDeclaration.Identifier.Text;
        var ns = classDeclaration.GetNamespace();
        var fullName = ns is null ? className : $"{ns}.{className}";

        // check if class is one of the supported polyfills
        return PolyfillsGeneratorData.SupportedPolyfills.ContainsKey(fullName);
    }

    private static string getAlreadyExistingPolyfills(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // get full name
        var className = classDeclaration.Identifier.Text;
        var ns = classDeclaration.GetNamespace();
        var fullName = ns is null ? className : $"{ns}.{className}";

        return fullName;
    }
}
