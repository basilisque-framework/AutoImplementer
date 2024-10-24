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

namespace Basilisque.AutoImplementer.CodeAnalysis.Generators;

internal static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor GenericAttributeInvalidTypeArgumentType { get { return new DiagnosticDescriptor("BAS_AUI_001", "Invalid type argument.", "The type '{0}' cannot be automatically implemented, because it is not an interface.", "Basilisque.AutoImplementer", DiagnosticSeverity.Error, true); } }
    public static DiagnosticDescriptor MemberAttributePropertyAsRequiredOnInvalidMemberType { get { return new DiagnosticDescriptor("BAS_AUI_002", "Invalid attribute usage.", "Setting 'AsRequired' is only valid on properties.", "Basilisque.AutoImplementer", DiagnosticSeverity.Error, true); } }
}
