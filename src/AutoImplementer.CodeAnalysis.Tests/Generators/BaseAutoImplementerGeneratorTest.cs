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

using Basilisque.AutoImplementer.CodeAnalysis.Generators.PolyfillsGenerator;
using Basilisque.AutoImplementer.CodeAnalysis.Generators.StaticAttributesGenerator;
using Basilisque.CodeAnalysis.TestSupport.MSTest.SourceGenerators.UnitTests.Verifiers;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace Basilisque.AutoImplementer.CodeAnalysis.Tests.Generators;

public abstract class BaseAutoImplementerGeneratorTest : BaseAutoImplementerGeneratorTest<IncrementalSourceGeneratorVerifier<AutoImplementerGenerator>>
{ }

public abstract class BaseAutoImplementerGeneratorTest<TVerifier> : BaseAutoImplementerGeneratorTest<AutoImplementerGenerator, TVerifier>
    where TVerifier : IncrementalSourceGeneratorVerifier<AutoImplementerGenerator>, new()
{ }

public abstract class BaseAutoImplementerGeneratorTest<TGenerator, TVerifier>
    where TGenerator : Microsoft.CodeAnalysis.IIncrementalGenerator, new()
    where TVerifier : IncrementalSourceGeneratorVerifier<TGenerator>, new()
{
    public virtual Microsoft.CodeAnalysis.CSharp.LanguageVersion? LanguageVersion { get { return null; } }

    [TestMethod]
    public virtual async Task Test()
    {
        var verifier = GetVerifier();

        var expectedSources = getExpectedSources();
        foreach (var expectedSource in expectedSources)
        {
            var name = expectedSource.Name;

            if (!name.EndsWith(".cs"))
                name = $"{name}.cs";

            verifier.TestState.GeneratedSources.Add((typeof(TGenerator), name, SourceText.From(expectedSource.SourceText, Encoding.UTF8)));
        }

        await verifier.RunAsync();
    }

    protected virtual TVerifier GetVerifier()
    {
        //define reference assemblies
#if NET462 || NET48
        //the tests compiled for NET462 or NET48 actually test the NETSTANDARD2_0 assemblies
        var refAssemblies = new Microsoft.CodeAnalysis.Testing.ReferenceAssemblies(
            "netstandard2.0",
            new Microsoft.CodeAnalysis.Testing.PackageIdentity(
                "NETStandard.Library",
                "2.0.3"),
            @"build\netstandard2.0\ref")
            .AddAssemblies(ImmutableArray.Create("netstandard"))
            .WithPackages(System.Collections.Immutable.ImmutableArray.Create(new Microsoft.CodeAnalysis.Testing.PackageIdentity("Microsoft.Extensions.DependencyInjection", "6.0.0")));
#elif NETSTANDARD2_1
        var refAssemblies = new Microsoft.CodeAnalysis.Testing.ReferenceAssemblies(
            "netstandard2.1",
            new Microsoft.CodeAnalysis.Testing.PackageIdentity(
                "NETStandard.Library",
                "2.0.3"),
            @"build\netstandard2.1\ref")
            .AddAssemblies(ImmutableArray.Create("netstandard"));
#elif NET6_0
        var refAssemblies = Microsoft.CodeAnalysis.Testing.ReferenceAssemblies.Net.Net60
            .WithPackages(System.Collections.Immutable.ImmutableArray.Create(new Microsoft.CodeAnalysis.Testing.PackageIdentity("Microsoft.AspNetCore.App.Ref", "6.0.0")));
#elif NET7_0
        var refAssemblies = new ReferenceAssemblies(
                    "net7.0",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref",
                        "7.0.0"),
                    Path.Combine("ref", "net7.0"))
            .WithPackages(ImmutableArray.Create(new Microsoft.CodeAnalysis.Testing.PackageIdentity("Microsoft.AspNetCore.App.Ref", "7.0.0")));
#elif NET8_0
        var refAssemblies = new ReferenceAssemblies(
                    "net8.0",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref",
                        "8.0.0"),
                    Path.Combine("ref", "net8.0"))
            .WithPackages([new PackageIdentity("Microsoft.AspNetCore.App.Ref", "8.0.0")]);
#else
        throw new PlatformNotSupportedException("Please define reference assemblies for your platform!");
#endif

        //create verifier
        var verifier = new TVerifier()
        {
            ReferenceAssemblies = refAssemblies
            //TestState =
            //{
            //    //AnalyzerConfigFiles = { }
            //}
        };

        if (LanguageVersion.HasValue)
            verifier.LanguageVersion = LanguageVersion.Value;

        //set the diagnostic options
        foreach (var diagOp in GetDiagnosticOptions())
            verifier.DiagnosticOptions.Add(diagOp.key, diagOp.value);

        foreach (var expDiag in GetExpectedDiagnostics())
            verifier.ExpectedDiagnostics.Add(expDiag);

        //add the sources
        AddSourcesUnderTest(verifier.TestState.Sources);

        //add a reference to the auto implementer library
        //verifier.TestState.AdditionalReferences.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(Annotations.AutoImplementAttribute).Assembly.Location));

        return verifier;
    }

    protected virtual IEnumerable<(string key, Microsoft.CodeAnalysis.ReportDiagnostic value)> GetDiagnosticOptions()
    {
        //we can return diagnostic options like this:
        //yield return ("CS1591", Microsoft.CodeAnalysis.ReportDiagnostic.Suppress);
        //yield return ("CS159?", Microsoft.CodeAnalysis.ReportDiagnostic.???);

        yield break;
    }

    protected virtual IEnumerable<DiagnosticResult> GetExpectedDiagnostics()
    {
        //we can return expected diagnostics like this:
        //yield return new Microsoft.CodeAnalysis.Testing.DiagnosticResult("CS1591", Microsoft.CodeAnalysis.DiagnosticSeverity.Error);

        yield break;
    }

    protected abstract void AddSourcesUnderTest(SourceFileList sources);

    protected virtual IEnumerable<(string Name, string SourceText)> GetExpectedAttributeSources(IReadOnlyDictionary<string, (string CompilationName, string Source)> supportedAttributes)
    {
        return supportedAttributes.Values;
    }

    protected virtual IEnumerable<(string Name, string SourceText)> GetExpectedPolyfillSources(IReadOnlyDictionary<string, (string CompilationName, string Source)> supportedPolyfills)
    {
        return supportedPolyfills.Values;
    }

    protected abstract IEnumerable<(string Name, string SourceText)> GetExpectedInterfaceImplementations();

    private List<(string Name, string SourceText)> getExpectedSources()
    {
        var result = new List<(string Name, string SourceText)>();

        addStaticSources(result);

        result.AddRange(GetExpectedInterfaceImplementations());

        return result;
    }

    private void addStaticSources(List<(string Name, string SourceText)> sources)
    {
        sources.AddRange(GetExpectedAttributeSources(StaticAttributesGeneratorData.SupportedAttributes));
        sources.AddRange(GetExpectedPolyfillSources(PolyfillsGeneratorData.SupportedPolyfills));
    }
}
