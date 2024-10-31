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

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.GenericAttributesGenerator;

/// <summary>
/// Provides data necessary for generating the generic marker attributes
/// </summary>
public static class GenericAttributesGeneratorData
{
    internal const string AutoImplementClassInterfacesAttributeGenericClassName = StaticAttributesGeneratorData.AutoImplementsAttributeName;
    internal const string AutoImplementClassInterfacesAttributeGenericFullName = StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeFullName;

    /// <summary>
    /// Provides the source code template for the generic interface to mark classes for auto implementation of their interfaces
    /// </summary>
    internal static readonly string AutoImplementClassInterfacesAttributeGenericSourceTemplate =
@$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace {CommonGeneratorData.AutoImplementedAttributesTargetNamespace}
{{
    /// <summary>
    /// Marks a class for automatic implementation of its interfaces.
    /// By default all interfaces marked with <see cref=""{StaticAttributesGeneratorData.AutoImplementInterfaceAttributeFullName}""/> will be implemented. Alternatively, the interfaces can be specified explicitly.
    /// </summary>
    {CommonGeneratorData.GeneratedClassSharedAttributes}
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal sealed class {AutoImplementClassInterfacesAttributeGenericClassName}<#GENERIC_TYPES#> : {StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeFullName}
#GENERIC_TYPE_CONSTRAINTS#    {{
    }}
}}";
}