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

using Microsoft.CodeAnalysis.Testing;

namespace Basilisque.AutoImplementer.CodeAnalysis.Tests.Generators.AutoImplementerGenerator;

[TestClass]
[TestCategory(AutoImplementerGeneratorCategory)]
public class Diagnostics_BAS_AUI_001_InvalidConstructorArgumentType : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        // interface to auto implement
        sources.Add(@"
#nullable enable

namespace AutoImpl.GAG.TestObjects.Diagnostics_BAS_AUI_001_InvalidConstructorArgumentType;

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

namespace AutoImpl.GAG.TestObjects.Diagnostics_BAS_AUI_001_InvalidConstructorArgumentType;

/// <summary>
/// A class that is used by the other class for trying to auto implement it
/// </summary>
public class MyTestClass
{ }

/// <summary>
/// A class trying to auto implement types that are no interfaces
/// </summary>
[AutoImplementInterfaces(typeof(IMyInterface1), typeof(string), typeof(IMyInterface2), typeof(MyTestClass))]
public partial class MyImplementation_With_2_Interfaces : IMyInterface1, IMyInterface2
{ }
");
    }

    protected override IEnumerable<(string Name, string SourceText)> GetExpectedInterfaceImplementations()
    {
        yield break;
    }

    protected override IEnumerable<DiagnosticResult> GetExpectedDiagnostics()
    {
        // 'string' is not an interface
        yield return DiagnosticResult.CompilerError("BAS_AUI_001")
            .WithSpan(System.IO.Path.Combine("/0/Test1.cs"), 15, 49, 15, 63);

        // 'MyTestClass' is not an interface
        yield return DiagnosticResult.CompilerError("BAS_AUI_001")
            .WithSpan(System.IO.Path.Combine("/0/Test1.cs"), 15, 88, 15, 107);
    }
}
