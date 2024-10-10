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

namespace Basilisque.AutoImplementer.CodeAnalysis.Tests.Generators.PolyfillsGenerator;

[TestClass]
public class ExistingPolyfill_IsExternalInit_DoesNotOutputPolyfill : BaseAutoImplementerGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        sources.Add(@"
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
");
    }

    protected override IEnumerable<(string Name, string SourceText)> GetExpectedPolyfillSources(IReadOnlyDictionary<string, (string CompilationName, string Source)> supportedPolyfills)
    {
        // return all polyfills as expected except for the existing IsExternalInit
        foreach (var polyfill in supportedPolyfills)
        {
            if (polyfill.Key == "System.Runtime.CompilerServices.IsExternalInit")
                continue;

            yield return polyfill.Value;
        }
    }

    protected override IEnumerable<(string Name, string SourceText)> GetExpectedInterfaceImplementations()
    {
        yield break;
    }
}

