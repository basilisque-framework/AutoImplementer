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

using Basilisque.AutoImplementer.CodeAnalysis.Generators;
using Microsoft.CodeAnalysis.Testing;

namespace Basilisque.AutoImplementer.CodeAnalysis.Tests.Generators.GenericAttributesGenerator;

[TestClass]
[TestCategory("GenericAttributesGenerator")]
public class Diagnostics_BAS_AUI_001_InvalidGenericArgumentType : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        // interface to auto implement
        sources.Add(@"
#nullable enable

namespace AutoImpl.GAG.TestObjects.Diagnostics_BAS_AUI_001_InvalidGenericArgumentType;

/// <summary>
/// The first interface to be implemented
/// </summary>
public interface IMyInterface1
{ }

/// <summary>
/// The second interface to be implemented
/// </summary>
public interface IMyInterface2
{ }
");

        // class that implements the interface
        sources.Add(@"
using Basilisque.AutoImplementer.Annotations;

namespace AutoImpl.GAG.TestObjects.Diagnostics_BAS_AUI_001_InvalidGenericArgumentType;

/// <summary>
/// A class that is used by the other class for trying to auto implement it
/// </summary>
public class MyTestClass
{ }

/// <summary>
/// A class trying to auto implement types that are no interfaces
/// </summary>
[AutoImplementInterfaces<IMyInterface1, string, IMyInterface2, MyTestClass>]
public partial class MyImplementation_With_2_Interfaces : IMyInterface1, IMyInterface2
{ }
");
    }

    protected override IEnumerable<(string Name, string SourceText)> GetExpectedInterfaceImplementations()
    {
        yield return (
            Name: "Basilisque.AutoImplementer.Annotations.AutoImplementInterfacesAttribute_generic_4.g",
            SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace Basilisque.AutoImplementer.Annotations
{{
    /// <summary>
    /// Marks a class for automatic implementation of its interfaces.
    /// By default all interfaces marked with <see cref=""Basilisque.AutoImplementer.Annotations.AutoImplementInterfaceAttribute""/> will be implemented. The interfaces also can be explicitly stated.
    /// </summary>
    {CommonGeneratorData.GeneratedClassSharedAttributes}
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal sealed class AutoImplementInterfacesAttribute<TInterface1, TInterface2, TInterface3, TInterface4> : Basilisque.AutoImplementer.Annotations.AutoImplementInterfacesAttribute
            where TInterface1 : class
            where TInterface2 : class
            where TInterface3 : class
            where TInterface4 : class
    {{
    }}
}}");
    }

    protected override IEnumerable<DiagnosticResult> GetExpectedDiagnostics()
    {
        // There should be exactly one warning CS8618 stating that 'StringNotRequired' must contain a non-null value when exiting constructor.
        // The 'IntNotRequired' property doesn't raise this warning and the other two properties are marked as required.
        yield return DiagnosticResult.CompilerError("BAS_AUI_001")
            .WithSpan(System.IO.Path.Combine("/0/Test1.cs"), 15, 64, 15, 75);

        yield return DiagnosticResult.CompilerError("BAS_AUI_001")
            .WithSpan(System.IO.Path.Combine("/0/Test1.cs"), 15, 41, 15, 47);
    }
}
