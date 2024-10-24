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
public class Implement_1_Interface_With_Property_Attributes : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        // interface to auto implement
        sources.Add(@"
#nullable enable

namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_With_Property_Attributes;

/// <summary>
/// The interface to be implemented
/// </summary>
public interface IMyInterface
{
    /// <summary>
    /// int not required
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    int SingleAttributeWithConstructorParameter { get; set; }

    /// <summary>
    /// Basilisque.AutoImplementer.Annotations.Required will not be copied
    /// </summary>
    [Basilisque.AutoImplementer.Annotations.Required]
    [System.ComponentModel.Browsable(false)]
    int IgnoreBasilisqueAutoImplementerAnnotationsRequired { get; set; }

    /// <summary>
    /// TwoAttributesSeparate
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.AttributeProvider(typeof(string))]
    string? TwoAttributesSeparate { get; set; }

    /// <summary>
    /// TwoAttributesCommaSeparated
    /// </summary>
    [System.ComponentModel.Browsable(false),
    System.ComponentModel.AttributeProvider(""MyTypeName"")]
    string? TwoAttributesCommaSeparated { get; set; }

    /// <summary>
    /// OneAttributeWithMultipleParameters
    /// </summary>
    [System.ComponentModel.AttributeProvider(""MyTypeName"", ""MyPropertyName"")]
    string? OneAttributeWithMultipleParameters { get; set; }
}
");

        // class that implements the interface
        sources.Add(@"
namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_With_Property_Attributes;

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
            Name: "AutoImpl.AIG.TestObjects.Implement_1_Interface_With_Property_Attributes.MyImplementation.auto_impl.g.cs",
            SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace AutoImpl.AIG.TestObjects.Implement_1_Interface_With_Property_Attributes;

{CommonGeneratorData.GeneratedClassSharedAttributesNotIndented}
public partial class MyImplementation
{{
    /// <inheritdoc />
    [System.ComponentModel.BrowsableAttribute(false)]
    public int SingleAttributeWithConstructorParameter {{ get; set; }}
    
    /// <inheritdoc />
    [System.ComponentModel.BrowsableAttribute(false)]
    public required int IgnoreBasilisqueAutoImplementerAnnotationsRequired {{ get; set; }}
    
    /// <inheritdoc />
    [System.ComponentModel.BrowsableAttribute(false)]
    [System.ComponentModel.AttributeProviderAttribute(typeof(string))]
    public string? TwoAttributesSeparate {{ get; set; }}
    
    /// <inheritdoc />
    [System.ComponentModel.BrowsableAttribute(false)]
    [System.ComponentModel.AttributeProviderAttribute(""MyTypeName"")]
    public string? TwoAttributesCommaSeparated {{ get; set; }}
    
    /// <inheritdoc />
    [System.ComponentModel.AttributeProviderAttribute(""MyTypeName"", ""MyPropertyName"")]
    public string? OneAttributeWithMultipleParameters {{ get; set; }}
}}

#nullable restore");
    }
}

