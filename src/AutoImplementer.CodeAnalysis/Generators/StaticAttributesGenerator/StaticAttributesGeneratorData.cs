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
using static Basilisque.AutoImplementer.CodeAnalysis.Generators.CommonGeneratorData;

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.StaticAttributesGenerator;

/// <summary>
/// Provides data necessary for generating auto implementations
/// </summary>
public static class StaticAttributesGeneratorData
{

    internal const string AutoImplementableAttributeName = "AutoImplementInterfaceAttribute";

    internal const string AutoImplementsAttributeName        = "AutoImplementInterfacesAttribute";
        internal const string AutoImplementsAttribute_Strict    = "Strict";

    internal const string AutoImplementAttributeName            = "AutoImplementAttribute";
        internal const string AutoImplementAttribute_Implement      = "Implement";
        internal const string AutoImplementAttribute_AsRequired     = "AsRequired";

    internal const string RequiredAttributeName = "RequiredAttribute";


    internal const string AutoImplementInterfaceAttributeFullName       = $"{AutoImplementedAttributesTargetNamespace}.{AutoImplementableAttributeName}";
    internal const string AutoImplementClassInterfacesAttributeFullName = $"{AutoImplementedAttributesTargetNamespace}.{AutoImplementsAttributeName}";
    internal const string AutoImplementOnMembersAttributeFullName       = $"{AutoImplementedAttributesTargetNamespace}.{AutoImplementAttributeName}";
    internal const string ImplementAsRequiredAttributeFullName          = $"{AutoImplementedAttributesTargetNamespace}.{RequiredAttributeName}";

    private const string AutoImplementInterfaceAttributeCompilationName       = $"{AutoImplementInterfaceAttributeFullName}.g";
    private const string AutoImplementClassInterfacesAttributeCompilationName = $"{AutoImplementClassInterfacesAttributeFullName}.g";
    private const string AutoImplementOnMembersAttributeCompilationName       = $"{AutoImplementOnMembersAttributeFullName}.g";
    private const string ImplementAsRequiredAttributeCompilationName          = $"{ImplementAsRequiredAttributeFullName}.g";

    private static readonly string _autoImplementInterfaceAttributeSource =
    @$"{GeneratedFileSharedHeaderWithUsings}
namespace {AutoImplementedAttributesTargetNamespace}
{{
    /// <summary>
    /// Offers settings to influence how the interface is implemented when using Basilisque.AutoImplementer.
    /// </summary>
    {GeneratedClassSharedAttributes}
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class {AutoImplementableAttributeName} : Attribute
    {{
        /// <summary>
        /// Implements non-nullable interface members as 'required'
        /// </summary>
        public bool {AutoImplementsAttribute_Strict} {{ get; set; }} = true;
    }}
}}";

    private static readonly string _autoImplementClassInterfacesAttributeSource =
    @$"{GeneratedFileSharedHeaderWithUsings}
namespace {AutoImplementedAttributesTargetNamespace}
{{
    /// <summary>
    /// Marks a class for automatic implementation of its interfaces.
    /// By default all interfaces marked with <see cref=""{AutoImplementedAttributesTargetNamespace}.{AutoImplementableAttributeName}""/> will be implemented. Alternatively, the interfaces can be specified explicitly.
    /// </summary>
    {GeneratedClassSharedAttributes}
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal class {AutoImplementsAttributeName} : Attribute
    {{
        public {AutoImplementsAttributeName}(params Type[] interfacesToImplement)
        {{
        }}
    }}
}}";

    private static readonly string _autoImplementOnMembersAttributeSource =
    @$"{GeneratedFileSharedHeaderWithUsings}
namespace {AutoImplementedAttributesTargetNamespace}
{{
    /// <summary>
    /// Marks a member of an interface for automatic implementation
    /// </summary>
    {GeneratedClassSharedAttributes}
    [AttributeUsage(AttributeTargets.Property /*| AttributeTargets.Method | AttributeTargets.Event*/, AllowMultiple = false, Inherited = false)]
    internal sealed class {AutoImplementAttributeName} : Attribute
    {{
        /// <summary>
        /// Determines if the member should be automatically implemented or not
        /// </summary>
        public bool Implement {{ get; set; }} = true;

        /// <summary>
        /// Determines if the property should be implemented with the required modifier
        /// </summary>
        public bool AsRequired {{ get; set; }} = false;
    }}
}}";

    private static readonly string _implementAsRequiredAttributeSource =
    @$"{GeneratedFileSharedHeaderWithUsings}
namespace {AutoImplementedAttributesTargetNamespace}
{{
    /// <summary>
    /// Adds the ""required"" keyword to the generated property
    /// </summary>
    {GeneratedClassSharedAttributes}
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class {RequiredAttributeName} : Attribute
    {{
    }}
}}";

    /// <summary>
    /// The list of currently supported attributes
    /// </summary>
    public static readonly IReadOnlyDictionary<string, (string CompilationName, string Source)> SupportedAttributes = new Dictionary<string, (string CompilationName, string Source)>()
    {
        { AutoImplementInterfaceAttributeFullName, (AutoImplementInterfaceAttributeCompilationName, _autoImplementInterfaceAttributeSource) },
        { AutoImplementClassInterfacesAttributeFullName, (AutoImplementClassInterfacesAttributeCompilationName, _autoImplementClassInterfacesAttributeSource) },
        { AutoImplementOnMembersAttributeFullName, (AutoImplementOnMembersAttributeCompilationName, _autoImplementOnMembersAttributeSource) },
        { ImplementAsRequiredAttributeFullName, (ImplementAsRequiredAttributeCompilationName, _implementAsRequiredAttributeSource) }
    };
}