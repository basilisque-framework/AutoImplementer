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
public class Implement_1_Interface_In_Base_Class : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        // interfaces to auto implement
        sources.Add(@"
#nullable enable

namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_In_Base_Class;

/// <summary>
/// Provides a title
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterface()]
public interface ITitle
{
    /// <summary>
    /// This is the title
    /// </summary>
    string? Title { get; set; }
}

/// <summary>
/// Provides a summary
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterface()]
public interface ISummary
{
    /// <summary>
    /// This is the summary
    /// </summary>
    string? Summary { get; set; }
}
");

        // base class
        sources.Add(@"
#nullable enable

namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_In_Base_Class;

/// <summary>
/// Base class
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces()]
public abstract partial class MyBaseClass : ITitle
{ }
");

        // class that inherits from the base class
        sources.Add(@"
namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_In_Base_Class;

/// <summary>
/// Represents a movie
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces]
public partial class Movie : MyBaseClass, ISummary
{ }
");
    }

    protected override IEnumerable<(string Name, string SourceText)> GetExpectedInterfaceImplementations()
    {
        yield return (
                Name: "AutoImpl.AIG.TestObjects.Implement_1_Interface_In_Base_Class.MyBaseClass.auto_impl.g.cs",
                SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_In_Base_Class;

{CommonGeneratorData.GeneratedClassSharedAttributesNotIndented}
public partial class MyBaseClass
{{
    /// <inheritdoc />
    public string? Title {{ get; set; }}
}}

#nullable restore");

        yield return (
                Name: "AutoImpl.AIG.TestObjects.Implement_1_Interface_In_Base_Class.Movie.auto_impl.g.cs",
                SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_In_Base_Class;

{CommonGeneratorData.GeneratedClassSharedAttributesNotIndented}
public partial class Movie
{{
    /// <inheritdoc />
    public string? Summary {{ get; set; }}
}}

#nullable restore");
    }
}

