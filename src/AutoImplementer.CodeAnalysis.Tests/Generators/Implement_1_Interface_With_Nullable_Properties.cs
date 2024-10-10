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

namespace Basilisque.AutoImplementer.CodeAnalysis.Tests.Generators;

[TestClass]
public class Implement_1_Interface_With_Nullable_Properties : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        // interface to auto implement
        sources.Add(@"
#nullable enable

namespace AutoImpl.TestObjects.Implement_1_Interface_With_Nullable_Properties;

/// <summary>
/// A class used as parameter type
/// </summary>
public class MyTestClass
{ }

/// <summary>
/// The interface to be implemented
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterface()]
public interface IMyInterface
{
    /// <summary>
    /// value type int with ?
    /// </summary>
    int? IntWithQuestionMark { get; set; }

    /// <summary>
    /// value type int with Nullable
    /// </summary>
    System.Nullable<int> IntWithNullable { get; set; }

    /// <summary>
    /// value type bool with ?
    /// </summary>
    bool? BoolWithQuestionMark { get; set; }

    /// <summary>
    /// value type bool with Nullable
    /// </summary>
    System.Nullable<bool> BoolWithNullable { get; set; }

    /// <summary>
    /// string with ?
    /// </summary>
    string? StringWithQuestionMark { get; set; }

    /// <summary>
    /// string with ?
    /// </summary>
    MyTestClass? MyTestClassWithQuestionMark { get; set; }
}
");

        // class that implements the interface
        sources.Add(@"
namespace AutoImpl.TestObjects.Implement_1_Interface_With_Nullable_Properties;

/// <summary>
/// The class implementing the interface
/// </summary>
public partial class MyImplementation : IMyInterface
{ }
");
    }

    protected override IEnumerable<(string Name, string SourceText)> GetExpectedInterfaceImplementations()
    {
        yield return (
            Name: "AutoImpl.TestObjects.Implement_1_Interface_With_Nullable_Properties.MyImplementation.auto_impl.g.cs",
            SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace AutoImpl.TestObjects.Implement_1_Interface_With_Nullable_Properties
{{
    {CommonGeneratorData.GeneratedClassSharedAttributes}
    public partial class MyImplementation
    {{
        /// <inheritdoc />
        public int? IntWithQuestionMark {{ get; set; }}
        
        /// <inheritdoc />
        public int? IntWithNullable {{ get; set; }}
        
        /// <inheritdoc />
        public bool? BoolWithQuestionMark {{ get; set; }}
        
        /// <inheritdoc />
        public bool? BoolWithNullable {{ get; set; }}
        
        /// <inheritdoc />
        public string? StringWithQuestionMark {{ get; set; }}
        
        /// <inheritdoc />
        public global::AutoImpl.TestObjects.Implement_1_Interface_With_Nullable_Properties.MyTestClass? MyTestClassWithQuestionMark {{ get; set; }}
    }}
}}

#nullable restore");
    }
}

