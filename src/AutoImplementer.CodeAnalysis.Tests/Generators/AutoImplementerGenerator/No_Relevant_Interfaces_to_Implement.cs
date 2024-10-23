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

using Microsoft.CodeAnalysis.Testing;

namespace Basilisque.AutoImplementer.CodeAnalysis.Tests.Generators.AutoImplementerGenerator;

[TestClass]
[TestCategory("AutoImplementerGenerator")]
public class No_Relevant_Interfaces_to_Implement : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        sources.Add(@"
namespace AutoImpl.AIG.TestObjects.No_Relevant_Interfaces_to_Implement;

/// <summary>
/// Represents a movie
/// </summary>
[Basilisque.AutoImplementer.Annotations.AutoImplementInterfaces()]
public partial class Movie
{ }
");
    }

    protected override IEnumerable<(string Name, string SourceText)> GetExpectedInterfaceImplementations()
    {
        yield break;
    }

    //protected override IEnumerable<DiagnosticResult> GetExpectedDiagnostics()
    //{
    //    //ToDo: expect warning/info that the class is marked for auto implementation but there is nothing to implement
    //}
}

