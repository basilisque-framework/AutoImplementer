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

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.AutoImplementerGenerator;

internal class AutoImplementerGeneratorInfo
{
    private List<INamedTypeSymbol>? _interfaces = null;
    private List<Diagnostic>? _diagnostics = null;

    public ClassDeclarationSyntax ClassDeclaration { get; set; }

    public bool HasData
    {
        get
        {
            return HasInterfaces || HasDiagnostics;
        }
    }

    public bool HasInterfaces
    {
        get
        {
            return _interfaces?.Any() == true;
        }
    }

    public bool HasDiagnostics
    {
        get
        {
            return _diagnostics?.Any() == true;
        }
    }

    public List<INamedTypeSymbol> Interfaces
    {
        get
        {
            if (_interfaces is null)
                _interfaces = new List<INamedTypeSymbol>();

            return _interfaces;
        }
    }

    public List<Diagnostic> Diagnostics
    {
        get
        {
            if (_diagnostics is null)
                _diagnostics = new List<Diagnostic>();

            return _diagnostics;
        }
    }

    public AutoImplementerGeneratorInfo(ClassDeclarationSyntax classDeclaration)
    {
        ClassDeclaration = classDeclaration;
    }
}
