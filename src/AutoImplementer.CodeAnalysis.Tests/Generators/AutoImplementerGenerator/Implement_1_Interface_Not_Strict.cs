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
public class Implement_1_Interface_Not_Strict : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        // interface to auto implement
        sources.Add(@"
#nullable enable

using Basilisque.AutoImplementer.Annotations;

namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_Not_Strict;

/// <summary>
/// The interface to be implemented
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterface(Strict = false)]
public interface IMyInterface
{
    /// <summary>
    /// int not required because of strict disabled
    /// </summary>
    int NonNullableNotRequired { get; set; }

    /// <summary>
    /// int required because of [Required]
    /// </summary>
    [Required] int NonNullableRequiredByRequiredAttribute { get; set; }

    /// <summary>
    /// nullable int not required
    /// </summary>
    int? NullableNotRequired { get; set; }

    /// <summary>
    /// nullable int required because of [Required]
    /// </summary>
    [Required] int? NullableRequiredByRequiredAttribute { get; set; }

    /// <summary>
    /// nullable int required by AutoImplementAttribute
    /// </summary>
    [Basilisque.AutoImplementer.Annotations.AutoImplement(AsRequired = true)]
    int? NullableRequiredByAutoImplementAttribute { get; set; }
}
");

        // class that implements the interface
        sources.Add(@"
namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_Not_Strict;

/// <summary>
/// The class implementing the interface
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces()]
public partial class MyImplementation : IMyInterface
{ }
");
    }

    protected override IEnumerable<(string Name, string SourceText)> GetExpectedInterfaceImplementations()
    {
        yield return (
            Name: "AutoImpl.AIG.TestObjects.Implement_1_Interface_Not_Strict.MyImplementation.auto_impl.g.cs",
            SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_Not_Strict;

{CommonGeneratorData.GeneratedClassSharedAttributesNotIndented}
public partial class MyImplementation
{{
    /// <inheritdoc />
    public int NonNullableNotRequired {{ get; set; }}
    
    /// <inheritdoc />
    public required int NonNullableRequiredByRequiredAttribute {{ get; set; }}
    
    /// <inheritdoc />
    public int? NullableNotRequired {{ get; set; }}
    
    /// <inheritdoc />
    public required int? NullableRequiredByRequiredAttribute {{ get; set; }}
    
    /// <inheritdoc />
    public required int? NullableRequiredByAutoImplementAttribute {{ get; set; }}
}}

#nullable restore");
    }
}

