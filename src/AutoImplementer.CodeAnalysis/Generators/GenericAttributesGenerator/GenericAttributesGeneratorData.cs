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

using Basilisque.AutoImplementer.CodeAnalysis.Generators.StaticAttributesGenerator;

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.GenericAttributesGenerator;

/// <summary>
/// Provides data necessary for generating the generic marker attributes
/// </summary>
public static class GenericAttributesGeneratorData
{
    internal const string AutoImplementClassInterfacesAttributeGenericClassName = StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeClassName;
    internal const string AutoImplementClassInterfacesAttributeGenericFullName = StaticAttributesGeneratorData.AutoImplementClassInterfacesAttributeFullName;

    //private const string AutoImplementClassInterfacesAttributeGenericCompilationName = $"{AutoImplementClassInterfacesAttributeGenericFullName}_generic.g";

    //    private static readonly string _generatedFileSharedHeader = $@"{CommonGeneratorData.GeneratedFileSharedHeaderWithNullable}

    //using System;";

    //    /// <summary>
    //    /// Provides the source code template for the generic interface to mark classes for auto implementation of their interfaces
    //    /// </summary>
    //    public static readonly string AutoImplementClassInterfacesAttributeGenericSource =
    //    @$"{_generatedFileSharedHeader}
    //namespace {AutoImplementerGeneratorData.AutoImplementedAttributesTargetNamespace}
    //{{
    //    /// <summary>
    //    /// Marks a class for automatic implementation of its interfaces.
    //    /// By default all interfaces marked with <see cref=""{AutoImplementerGeneratorData.AutoImplementedAttributesTargetNamespace}.{AutoImplementerGeneratorData.AutoImplementInterfaceAttributeClassName}""/> will be implemented. The interfaces also can be explicitly stated.
    //    /// </summary>
    //    {AutoImplementerGeneratorData.GeneratedClassSharedAttributes}
    //    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    //    internal sealed class {AutoImplementClassInterfacesAttributeGenericClassName}<#GENERIC_TYPES#> : {AutoImplementerGeneratorData.AutoImplementedAttributesTargetNamespace}.{AutoImplementerGeneratorData.AutoImplementClassInterfacesAttributeClassName}
    //#GENERIC_TYPE_CONSTRAINTS#
    //    {{
    //    }}
    //}}";
}