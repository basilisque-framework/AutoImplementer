/*
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

namespace Basilisque.AutoImplementer.CodeAnalysis.Tests;


[AutoImplementInterface()]
public interface ICursedTypes {

    //// not sure if the compiler treats these differently, so we'll test both
    int? Nullable_Int_QuestionMark { get; set; }
    Nullable<int> Nullable_Int_WrapperStruct { get; set; }

    string? Nullable_String_QuestionMark { get; set; }
}

// If the generator chokes, we'll get a build error here.  I wish I could actually write a proper test here but I'm not sure if it's possible.
public partial class CursedTypesImpl : ICursedTypes { }
