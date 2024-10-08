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

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Basilisque.AutoImplementer.CodeAnalysis.Extensions;

internal static class SyntaxNodeExtensions
{
    /// <summary>
    /// Tries to determine the namespace of a <see cref="SyntaxNode"/>
    /// </summary>
    /// <param name="syntaxNode">The <see cref="SyntaxNode"/> to get the namespace from</param>
    /// <returns>The namespace of the <see cref="SyntaxNode"/> or null</returns>
    public static string? GetNamespace(this SyntaxNode? syntaxNode)
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
        return GetNamespace(syntaxNode.Parent);
    }
}
