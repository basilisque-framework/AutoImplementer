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

namespace Basilisque.AutoImplementer.CodeAnalysis.Tests.Generators.AutoImplementerGenerator;

[TestClass]
[TestCategory(AutoImplementerGeneratorCategory)]
public class Implement_1_Interface_With_Skipped_Properties : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        // interface to auto implement
        sources.Add(@"
#nullable enable

namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_With_Skipped_Properties;

/// <summary>
/// The interface to be implemented
/// </summary>
public interface IMyInterface
{
    /// <summary>
    /// Property 1
    /// </summary>
    int Property1 { get; set; }

    /// <summary>
    /// Property 2
    /// </summary>
    [Basilisque.AutoImplementer.Annotations.AutoImplement(Implement = false)]
    int Property2 { get; set; }

    /// <summary>
    /// Property 3
    /// </summary>
    string? Property3 { get; set; }

    /// <summary>
    /// Property 4
    /// </summary>
    [Basilisque.AutoImplementer.Annotations.AutoImplement(Implement = true)]
    string? Property4 { get; set; }

    /// <summary>
    /// Property 5
    /// </summary>
    string? Property5 { get; set; }
}
");

        // class that implements the interface
        sources.Add(@"
namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_With_Skipped_Properties;

/// <summary>
/// The class implementing the interface
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces(typeof(IMyInterface))]
public partial class MyImplementation : IMyInterface
{ }
");
    }

    protected override IEnumerable<(string Name, string SourceText)> GetExpectedInterfaceImplementations()
    {
        yield return (
            Name: "AutoImpl.AIG.TestObjects.Implement_1_Interface_With_Skipped_Properties.MyImplementation.auto_impl.g.cs",
            SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_With_Skipped_Properties;

{CommonGeneratorData.GeneratedClassSharedAttributesNotIndented}
public partial class MyImplementation
{{
    /// <inheritdoc />
    public int Property1 {{ get; set; }}
    
    /// <inheritdoc />
    public string? Property3 {{ get; set; }}
    
    /// <inheritdoc />
    public string? Property4 {{ get; set; }}
    
    /// <inheritdoc />
    public string? Property5 {{ get; set; }}
}}

#nullable restore");
    }

    protected override IEnumerable<DiagnosticResult> GetExpectedDiagnostics()
    {
        // CS0535 - 'MyImplementation' does not implement interface member 'IMyInterface.Property2'
        yield return DiagnosticResult.CompilerError("CS0535")
            .WithSpan(System.IO.Path.Combine("/0/Test1.cs"), 8, 41, 8, 53);
    }
}

