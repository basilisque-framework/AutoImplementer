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
public class Implement_1_Interface_AllPropertiesAsRequired : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        // interface to auto implement
        sources.Add(@"
#nullable enable

using Basilisque.AutoImplementer.Annotations;

namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_AllPropertiesAsRequired;

/// <summary>
/// The interface to be implemented
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterface(ImplementAllPropertiesAsRequired = true)]
public interface IMyInterface
{
    /// <summary>
    /// int not required
    /// </summary>
    int IntNotRequired { get; set; }

    /// <summary>
    /// int required
    /// </summary>
    [Required] int IntRequired { get; set; }

    /// <summary>
    /// string not required
    /// </summary>
    string StringNotRequired { get; set; }

    /// <summary>
    /// string required
    /// </summary>
    [Required] string StringRequired { get; set; }
}
");

        // class that implements the interface
        sources.Add(@"
namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_AllPropertiesAsRequired;

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
            Name: "AutoImpl.AIG.TestObjects.Implement_1_Interface_AllPropertiesAsRequired.MyImplementation.auto_impl.g.cs",
            SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_AllPropertiesAsRequired
{{
    {CommonGeneratorData.GeneratedClassSharedAttributes}
    public partial class MyImplementation
    {{
        /// <inheritdoc />
        public required int IntNotRequired {{ get; set; }}
        
        /// <inheritdoc />
        public required int IntRequired {{ get; set; }}
        
        /// <inheritdoc />
        public required string StringNotRequired {{ get; set; }}
        
        /// <inheritdoc />
        public required string StringRequired {{ get; set; }}
    }}
}}

#nullable restore");
    }
}

