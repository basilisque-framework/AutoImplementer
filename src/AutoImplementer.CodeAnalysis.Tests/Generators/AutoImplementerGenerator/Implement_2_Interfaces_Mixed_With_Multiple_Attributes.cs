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
public class Implement_2_Interfaces_Mixed_With_Multiple_Attributes : BaseAutoImplementerGeneratorTest
{
    public enum TestType
    {
        ExplicitOnly,
        ImplicitOnly,
        Both
    }

    private TestType _testType;

    [DataTestMethod]
    [DataRow(TestType.ExplicitOnly)]
    [DataRow(TestType.ImplicitOnly)]
    [DataRow(TestType.Both)]
    public Task Test(TestType testType)
    {
        _testType = testType;

        return base.Test();
    }

    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        // interfaces to auto implement
        sources.Add(@"
#nullable enable

namespace AutoImpl.AIG.TestObjects.Implement_2_Interfaces_Mixed_With_Multiple_Attributes;

/// <summary>
/// Provides a title
/// </summary>
public interface ITitle
{
    /// <summary>
    /// This is the title
    /// </summary>
    string? Title { get; set; }
}

/// <summary>
/// Provides information about a movie
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterface()]
public interface IMovie : ITitle
{
    /// <summary>
    /// This is the summary
    /// </summary>
    string? Summary { get; set; }
}
");

        var impl = _testType == TestType.ImplicitOnly || _testType == TestType.Both;
        var expl = _testType == TestType.ExplicitOnly || _testType == TestType.Both;

        // class that implements the interfaces
        sources.Add($@"
namespace AutoImpl.AIG.TestObjects.Implement_2_Interfaces_Mixed_With_Multiple_Attributes;

/// <summary>
/// Represents a movie
/// </summary>
{(expl ? "[Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces(typeof(ITitle))]" : "")}
{(impl ? "[Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces()]" : "")}
public partial class Movie : IMovie
{{ }}
");
    }

    protected override IEnumerable<(string Name, string SourceText)> GetExpectedInterfaceImplementations()
    {
        var impl = "";
        var space = "";
        var expl = "";

        if (_testType == TestType.ExplicitOnly || _testType == TestType.Both)
        {
            expl = @"    /// <inheritdoc />
    public string? Title { get; set; }";

            if (_testType == TestType.Both)
                expl = expl + @"
";
        }

        if (_testType == TestType.ImplicitOnly || _testType == TestType.Both)
        {
            impl = @"    /// <inheritdoc />
    public string? Summary { get; set; }";
        }

        if (_testType == TestType.Both)
            space = @"    
";

        yield return (
            Name: "AutoImpl.AIG.TestObjects.Implement_2_Interfaces_Mixed_With_Multiple_Attributes.Movie.auto_impl.g.cs",
            SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace AutoImpl.AIG.TestObjects.Implement_2_Interfaces_Mixed_With_Multiple_Attributes;

{CommonGeneratorData.GeneratedClassSharedAttributesNotIndented}
public partial class Movie
{{
{expl}{space}{impl}
}}

#nullable restore");
    }

    protected override IEnumerable<DiagnosticResult> GetExpectedDiagnostics()
    {
        if (_testType == TestType.Both)
            yield break;

        // 'Movie' does not implement interface member 'ITitle.Title' / 'IMovie.Summary'
        yield return DiagnosticResult.CompilerError("CS0535")
            .WithSpan(System.IO.Path.Combine("/0/Test1.cs"), 9, 30, 9, 36);
    }
}

