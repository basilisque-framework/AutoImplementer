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

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators;

/// <summary>
/// Provides data necessary for generating auto implementations
/// </summary>
public static class AutoImplementerGeneratorData
{
    internal const string AutoImplementedAttributesTargetNamespace = "Basilisque.AutoImplementer.Annotations";

    internal const string AutoImplementInterfaceAttributeClassName = "AutoImplementInterfaceAttribute";
    internal const string AutoImplementOnMembersAttributeClassName = "AutoImplementAttribute";
    internal const string ImplementAsRequiredAttributeClassName    = "RequiredAttribute";

    private const string AutoImplementInterfaceAttributeFullName = $"{AutoImplementedAttributesTargetNamespace}.{AutoImplementInterfaceAttributeClassName}";
    private const string AutoImplementOnMembersAttributeFullName = $"{AutoImplementedAttributesTargetNamespace}.{AutoImplementOnMembersAttributeClassName}";
    private const string ImplementAsRequiredAttributeFullName    = $"{AutoImplementedAttributesTargetNamespace}.{ImplementAsRequiredAttributeClassName}";

    private const string AutoImplementInterfaceAttributeCompilationName = $"{AutoImplementInterfaceAttributeFullName}.g";
    private const string AutoImplementOnMembersAttributeCompilationName = $"{AutoImplementOnMembersAttributeFullName}.g";
    private const string ImplementAsRequiredAttributeCompilationName    = $"{ImplementAsRequiredAttributeFullName}.g";

    private static readonly string _generatedFileSharedHeader = $@"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}

using System;";

    /// <summary>
    /// Provides the attributes that are output for each class as a string
    /// </summary>
    public static readonly string GeneratedClassSharedAttributes = $@"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""{CommonGeneratorData.AssemblyName.Name}"", ""{CommonGeneratorData.AssemblyName.Version}"")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]";

    private static readonly string _autoImplementInterfaceAttributeSource =
    @$"{_generatedFileSharedHeader}
namespace {AutoImplementedAttributesTargetNamespace}
{{
    /// <summary>
    /// Marks an interface with all members for automatic implementation
    /// </summary>
    {GeneratedClassSharedAttributes}
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class {AutoImplementInterfaceAttributeClassName} : Attribute
    {{
    }}
}}";

    private static readonly string _autoImplementOnMembersAttributeSource =
    @$"{_generatedFileSharedHeader}
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
    @$"{_generatedFileSharedHeader}
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
        { AutoImplementOnMembersAttributeFullName, (AutoImplementOnMembersAttributeCompilationName, _autoImplementOnMembersAttributeSource) },
        { ImplementAsRequiredAttributeFullName, (ImplementAsRequiredAttributeCompilationName, _implementAsRequiredAttributeSource) },
    };
}
