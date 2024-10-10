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

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators.GenericAttributesGenerator;

internal struct GenericAttributesGeneratorInvalidTypeInfo
{
    public string TypeName { get; set; }

    public DiagnosticDescriptor DiagnosticDescriptor { get; set; }

    public Location Location { get; set; }
}
