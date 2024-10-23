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
public class Implement_2_Interfaces_Not_In_BaseList : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        // first interface to auto implement
        sources.Add(@"
#nullable enable

namespace AutoImpl.AIG.TestObjects.Implement_2_Interfaces_Not_In_BaseList;

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
");

        // second interface to auto implement
        sources.Add(@"
#nullable enable

namespace AutoImpl.AIG.TestObjects.Implement_2_Interfaces_Not_In_BaseList;

/// <summary>
/// Provides some details
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterface()]
public interface IDetails
{
    /// <summary>
    /// This is the image
    /// </summary>
    byte[]? Image { get; set; }

    /// <summary>
    /// This is the summary
    /// </summary>
    string? Summary { get; set; }
}
");

        // class that implements the two interfaces
        sources.Add(@"
namespace AutoImpl.AIG.TestObjects.Implement_2_Interfaces_Not_In_BaseList;

/// <summary>
/// Represents a movie
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces<ITitle>()]
[Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces(typeof(IDetails))]
public partial class Movie
{ }
");
    }

    protected override IEnumerable<(string Name, string SourceText)> GetExpectedInterfaceImplementations()
    {
        yield return (
            Name: "Basilisque.AutoImplementer.Annotations.AutoImplementInterfacesAttribute_generic_1.g",
            SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace Basilisque.AutoImplementer.Annotations
{{
    /// <summary>
    /// Marks a class for automatic implementation of its interfaces.
    /// By default all interfaces marked with <see cref=""Basilisque.AutoImplementer.Annotations.AutoImplementInterfaceAttribute""/> will be implemented. Alternatively, the interfaces can be specified explicitly.
    /// </summary>
    {CommonGeneratorData.GeneratedClassSharedAttributes}
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal sealed class AutoImplementInterfacesAttribute<TInterface1> : Basilisque.AutoImplementer.Annotations.AutoImplementInterfacesAttribute
            where TInterface1 : class
    {{
    }}
}}");

        yield return (
                Name: "AutoImpl.AIG.TestObjects.Implement_2_Interfaces_Not_In_BaseList.Movie.auto_impl.g.cs",
                SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace AutoImpl.AIG.TestObjects.Implement_2_Interfaces_Not_In_BaseList
{{
    {CommonGeneratorData.GeneratedClassSharedAttributes}
    public partial class Movie : global::AutoImpl.AIG.TestObjects.Implement_2_Interfaces_Not_In_BaseList.ITitle, global::AutoImpl.AIG.TestObjects.Implement_2_Interfaces_Not_In_BaseList.IDetails
    {{
        /// <inheritdoc />
        public string? Title {{ get; set; }}
        
        /// <inheritdoc />
        public byte[]? Image {{ get; set; }}
        
        /// <inheritdoc />
        public string? Summary {{ get; set; }}
    }}
}}

#nullable restore");
    }
}

