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
[TestCategory(GenericAttributesGeneratorCategory)]
public class GenericClassAttributes_Detection_NoSuffix_WithNamespace : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        // interface to auto implement
        sources.Add(@"
#nullable enable

namespace AutoImpl.GAG.TestObjects.GenericClassAttributes_Detection_NoSuffix_WithNamespace;

/// <summary>
/// The interface to be implemented
/// </summary>
public interface IMyInterface
{ }
");

        // classes that implement the interface
        sources.Add(@"
namespace AutoImpl.GAG.TestObjects.GenericClassAttributes_Detection_NoSuffix_WithNamespace;

/// <summary>
/// The class implementing the interface
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces<IMyInterface>]
public partial class MyImplementation_NoSuffix_WithNamespace : IMyInterface
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
    }
}

