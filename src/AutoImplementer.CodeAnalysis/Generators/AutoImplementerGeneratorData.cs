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

internal static class AutoImplementerGeneratorData
{
    internal const string AutoImplementedAttributesTargetNamespace = "Basilisque.AutoImplementer.Annotations";

    internal const string AutoImplementInterfaceAttributeClassName = "AutoImplementInterfaceAttribute";
    internal const string AutoImplementOnMembersAttributeClassName = "AutoImplementAttribute";
    internal const string ImplementAsRequiredAttributeClassName = "IRequiredAttribute";

    internal const string AutoImplementInterfaceAttributeCompilationName = $"{AutoImplementedAttributesTargetNamespace}.{AutoImplementInterfaceAttributeClassName}.g";
    internal const string AutoImplementOnMembersAttributeCompilationName = $"{AutoImplementedAttributesTargetNamespace}.{AutoImplementOnMembersAttributeClassName}.g";
    internal const string ImplementAsRequiredAttributeCompilationName = $"{AutoImplementedAttributesTargetNamespace}.{ImplementAsRequiredAttributeClassName}.g";

    private static readonly string _generatedFileSharedHeader = $@"{CommonGeneratorData.GeneratedFileSharedHeader}

using System;";

    private static readonly string _generatedClassSharedAttributes = $@"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""{CommonGeneratorData.AssemblyName.Name}"", ""{CommonGeneratorData.AssemblyName.Version}"")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]";

    internal static readonly string AutoImplementInterfaceAttributeSource =
    @$"{_generatedFileSharedHeader}
namespace {AutoImplementedAttributesTargetNamespace}
{{
    /// <summary>
    /// Marks an interface with all members for automatic implementation
    /// </summary>
    {_generatedClassSharedAttributes}
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class {AutoImplementInterfaceAttributeClassName} : Attribute
    {{
    }}
}}";

    internal static readonly string AutoImplementOnMembersAttributeSource =
    @$"{_generatedFileSharedHeader}
namespace {AutoImplementedAttributesTargetNamespace}
{{
    /// <summary>
    /// Marks a member of an interface for automatic implementation
    /// </summary>
    {_generatedClassSharedAttributes}
    [AttributeUsage(AttributeTargets.Property /*| AttributeTargets.Method | AttributeTargets.Event*/, AllowMultiple = false, Inherited = false)]
    internal sealed class {AutoImplementOnMembersAttributeClassName} : Attribute
    {{
        /// <summary>
        /// Determines if the member should be automatically implemented or not
        /// </summary>
        public bool Implement {{ get; set; }} = true;
    }}
}}";

    internal static readonly string ImplementAsRequiredAttributeSource =
    @$"{_generatedFileSharedHeader}
namespace {AutoImplementedAttributesTargetNamespace}
{{
    /// <summary>
    /// Adds the ""required"" keyword to the generated property
    /// </summary>
    {_generatedClassSharedAttributes}
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class {ImplementAsRequiredAttributeClassName} : Attribute
    {{
    }}
}}";

}
