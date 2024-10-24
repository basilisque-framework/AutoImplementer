﻿/*
   Copyright 2024 Alexander Stärk & Fasteroid

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

using Basilisque.AutoImplementer.Annotations;

namespace Basilisque.AutoImplementer.CodeAnalysis.Tests.Examples;

/* Define the interfaces that should be automatically implemented */

public interface IRequiredPropertiesExample
{
    string? NullableString { get; set; }

    [Required] string RequiredString { get; set; }
}

[Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces(typeof(IRequiredPropertiesExample))]
public partial class RequiredPropertiesImpl : IRequiredPropertiesExample
{ }


[TestClass]
[TestCategory("Examples")]
public partial class RequiredPropertyTests
{
    [TestMethod]
    public void TestRequiredProperty()
    {
        var _ = new RequiredPropertiesImpl()
        {
            RequiredString = "Hello"
        };
    }
}
