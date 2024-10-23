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
public class GenericClassAttributes_GenerateMultipleAttributes : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        // interface to auto implement
        sources.Add(@"
#nullable enable

namespace AutoImpl.GAG.TestObjects.GenericClassAttributes_GenerateMultipleAttributes;

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

/// <summary>
/// The third interface to be implemented
/// </summary>
public interface IMyInterface3
{ }

/// <summary>
/// The fourth interface to be implemented
/// </summary>
public interface IMyInterface4
{ }

/// <summary>
/// The fifth interface to be implemented
/// </summary>
public interface IMyInterface5
{ }
");

        // classes that implement the interface
        sources.Add(@"
using Basilisque.AutoImplementer.Annotations;

namespace AutoImpl.GAG.TestObjects.GenericClassAttributes_GenerateMultipleAttributes;

/// <summary>
/// A class implementing two interfaces
/// </summary>
[AutoImplementInterfaces<IMyInterface1, IMyInterface2>]
public partial class MyImplementation_With_2_Interfaces : IMyInterface1, IMyInterface2
{ }

/// <summary>
/// A class implementing fife interfaces
/// </summary>
[AutoImplementInterfaces<IMyInterface1, IMyInterface2, IMyInterface3, IMyInterface4, IMyInterface5>]
public partial class MyImplementation_With_5_Interfaces : IMyInterface1, IMyInterface2, IMyInterface3, IMyInterface4, IMyInterface5
{ }
");
    }

    protected override IEnumerable<(string Name, string SourceText)> GetExpectedInterfaceImplementations()
    {
        yield return (
            Name: "Basilisque.AutoImplementer.Annotations.AutoImplementInterfacesAttribute_generic_2.g",
            SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace Basilisque.AutoImplementer.Annotations
{{
    /// <summary>
    /// Marks a class for automatic implementation of its interfaces.
    /// By default all interfaces marked with <see cref=""Basilisque.AutoImplementer.Annotations.AutoImplementInterfaceAttribute""/> will be implemented. Alternatively, the interfaces can be specified explicitly.
    /// </summary>
    {CommonGeneratorData.GeneratedClassSharedAttributes}
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal sealed class AutoImplementInterfacesAttribute<TInterface1, TInterface2> : Basilisque.AutoImplementer.Annotations.AutoImplementInterfacesAttribute
            where TInterface1 : class
            where TInterface2 : class
    {{
    }}
}}");

        yield return (
            Name: "Basilisque.AutoImplementer.Annotations.AutoImplementInterfacesAttribute_generic_5.g",
            SourceText: @$"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}
namespace Basilisque.AutoImplementer.Annotations
{{
    /// <summary>
    /// Marks a class for automatic implementation of its interfaces.
    /// By default all interfaces marked with <see cref=""Basilisque.AutoImplementer.Annotations.AutoImplementInterfaceAttribute""/> will be implemented. Alternatively, the interfaces can be specified explicitly.
    /// </summary>
    {CommonGeneratorData.GeneratedClassSharedAttributes}
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal sealed class AutoImplementInterfacesAttribute<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5> : Basilisque.AutoImplementer.Annotations.AutoImplementInterfacesAttribute
            where TInterface1 : class
            where TInterface2 : class
            where TInterface3 : class
            where TInterface4 : class
            where TInterface5 : class
    {{
    }}
}}");
    }
}

