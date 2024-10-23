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

using static Basilisque.AutoImplementer.CodeAnalysis.Generators.CommonGeneratorData;

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.StaticAttributesGenerator;

/// <summary>
/// Provides data necessary for generating auto implementations
/// </summary>
public static class StaticAttributesGeneratorData
{
    internal const string AutoImplementInterfaceAttributeClassName = "AutoImplementInterfaceAttribute";
    internal const string AutoImplementClassInterfacesAttributeClassName = "AutoImplementInterfacesAttribute";
    internal const string AutoImplementOnMembersAttributeClassName = "AutoImplementAttribute";
    internal const string ImplementAsRequiredAttributeClassName = "RequiredAttribute";

    internal const string AutoImplementInterfaceAttributeFullName = $"{AutoImplementedAttributesTargetNamespace}.{AutoImplementInterfaceAttributeClassName}";
    internal const string AutoImplementClassInterfacesAttributeFullName = $"{AutoImplementedAttributesTargetNamespace}.{AutoImplementClassInterfacesAttributeClassName}";
    internal const string AutoImplementOnMembersAttributeFullName = $"{AutoImplementedAttributesTargetNamespace}.{AutoImplementOnMembersAttributeClassName}";
    internal const string ImplementAsRequiredAttributeFullName = $"{AutoImplementedAttributesTargetNamespace}.{ImplementAsRequiredAttributeClassName}";

    private const string AutoImplementInterfaceAttributeCompilationName = $"{AutoImplementInterfaceAttributeFullName}.g";
    private const string AutoImplementClassInterfacesAttributeCompilationName = $"{AutoImplementClassInterfacesAttributeFullName}.g";
    private const string AutoImplementOnMembersAttributeCompilationName = $"{AutoImplementOnMembersAttributeFullName}.g";
    private const string ImplementAsRequiredAttributeCompilationName = $"{ImplementAsRequiredAttributeFullName}.g";

    private static readonly string _autoImplementInterfaceAttributeSource =
    @$"{GeneratedFileSharedHeaderWithUsings}
namespace {AutoImplementedAttributesTargetNamespace}
{{
    /// <summary>
    /// Marks an interface with all members for automatic implementation
    /// </summary>
    {GeneratedClassSharedAttributes}
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class {AutoImplementInterfaceAttributeClassName} : Attribute
    {{
        /// <summary>
        /// Forces the interface to be automatically implemented event if the implementing class isn't marked with the <see cref=""{AutoImplementedAttributesTargetNamespace}.{AutoImplementClassInterfacesAttributeClassName}""/>.
        /// </summary>
        public bool Force {{ get; set; }} = false;

        /// <summary>
        /// Determines if all properties of the interface should be implemented with the 'required' keyword.
        /// </summary>
        public bool ImplementAllPropertiesRequired {{ get; set; }} = false;
    }}
}}";

    private static readonly string _autoImplementClassInterfacesAttributeSource =
    @$"{GeneratedFileSharedHeaderWithUsings}
namespace {AutoImplementedAttributesTargetNamespace}
{{
    /// <summary>
    /// Marks a class for automatic implementation of its interfaces.
    /// By default all interfaces marked with <see cref=""{AutoImplementedAttributesTargetNamespace}.{AutoImplementInterfaceAttributeClassName}""/> will be implemented. The interfaces also can be explicitly stated.
    /// </summary>
    {GeneratedClassSharedAttributes}
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class {AutoImplementClassInterfacesAttributeClassName} : Attribute
    {{
        public {AutoImplementClassInterfacesAttributeClassName}(params Type[] interfacesToImplement)
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
    internal sealed class {AutoImplementOnMembersAttributeClassName} : Attribute
    {{
        /// <summary>
        /// Determines if the member should be automatically implemented or not
        /// </summary>
        public bool Implement {{ get; set; }} = true;
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
    internal sealed class {ImplementAsRequiredAttributeClassName} : Attribute
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
        { ImplementAsRequiredAttributeFullName, (ImplementAsRequiredAttributeCompilationName, _implementAsRequiredAttributeSource) },
    };
}
