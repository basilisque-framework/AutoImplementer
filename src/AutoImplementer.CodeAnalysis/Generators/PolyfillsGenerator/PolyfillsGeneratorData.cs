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

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.PolyfillsGenerator;

/// <summary>
/// Provides data necessary for generating polyfills for older .NET versions
/// </summary>
public static class PolyfillsGeneratorData
{
    private const string CompilerServicesRequiredMemberAttributeCompilationName = "Polyfill_System.Runtime.CompilerServices.RequiredMemberAttribute.g.cs";
    private const string CompilerServicesCompilerFeatureRequiredAttributeCompilationName = "Polyfill_System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute.g.cs";
    private const string CodeAnalysisSetsRequiredMembersAttributeCompilationName = "Polyfill_System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute.g.cs";
    private const string CompilerServicesIsExternalInitCompilationName = "Polyfill_System.Runtime.CompilerServices.IsExternalInit.g.cs";

    private static readonly string _compilerServicesRequiredMemberAttributeSource = $@"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
#if !NET7_0_OR_GREATER

namespace System.Runtime.CompilerServices
{{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute {{ }}
}}

#endif";

    private static readonly string _compilerServicesCompilerFeatureRequiredAttributeSource = $@"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
#if !NET7_0_OR_GREATER

namespace System.Runtime.CompilerServices
{{
    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {{
        public CompilerFeatureRequiredAttribute(string featureName)
        {{
            FeatureName = featureName;
        }}

        public string FeatureName {{ get; }}
        public bool IsOptional {{ get; init; }}

#pragma warning disable IDE1006 // Naming Styles
        public const string RefStructs = nameof(RefStructs);
        public const string RequiredMembers = nameof(RequiredMembers);
#pragma warning restore IDE1006 // Naming Styles
    }}
}}

#endif";

    private static readonly string _codeAnalysisSetsRequiredMembersAttributeSource = $@"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
#if !NET7_0_OR_GREATER

namespace System.Diagnostics.CodeAnalysis
{{
    [System.AttributeUsage(System.AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute {{ }}
}}

#endif";

    private static readonly string _compilerServicesIsExternalInitSource = $@"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
#if !NET5_0_OR_GREATER

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{{
    // this class is needed for init properties.
    // It was added to .NET 5.0 but for earlier versions we need to specify it manually
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit {{ }}
}}

#endif";

    /// <summary>
    /// The list of currently supported polyfills
    /// </summary>
    public static readonly IReadOnlyDictionary<string, (string CompilationName, string Source)> SupportedPolyfills = new Dictionary<string, (string CompilationName, string Source)>()
    {
        { "System.Runtime.CompilerServices.RequiredMemberAttribute", (CompilerServicesRequiredMemberAttributeCompilationName, _compilerServicesRequiredMemberAttributeSource) },
        { "System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute", (CompilerServicesCompilerFeatureRequiredAttributeCompilationName, _compilerServicesCompilerFeatureRequiredAttributeSource) },
        { "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute", (CodeAnalysisSetsRequiredMembersAttributeCompilationName, _codeAnalysisSetsRequiredMembersAttributeSource) },
        { "System.Runtime.CompilerServices.IsExternalInit", (CompilerServicesIsExternalInitCompilationName, _compilerServicesIsExternalInitSource) }
    };
}
