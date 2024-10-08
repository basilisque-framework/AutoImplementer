<!--
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
-->
# Basilisque - Auto Implementer

## Overview
This project provides functionality to automatically implement interfaces.

[![NuGet Basilisque.AutoImplementer](https://img.shields.io/badge/NuGet_Basilisque.AutoImplementer-latest-blue.svg)](https://www.nuget.org/packages/Basilisque.AutoImplementer)
[![License](https://img.shields.io/badge/License-Apache%20License%202.0-red.svg)](LICENSE.txt)

## Description
This project contains a source generator that automatically implements interface members in classes implementing the interfaces.  
The goal is to provide a workaround for C# not supporting multiple inheritance for some basic use cases.

## Getting Started
Install the NuGet package [Basilisque.AutoImplementer](https://www.nuget.org/packages/Basilisque.AutoImplementer).  
Installing the package will add the source generator to your project.

Now you're ready to [start implementing your interfaces automatically](https://github.com/basilisque-framework/AutoImplementer/wiki/Getting-Started).


## Features
- Properties of interfaces will be added as auto-implemented properties

## Examples
Create the interfaces:
```csharp
[AutoImplementInterface()]
public interface ITitle
{
  [Required] string Title { get; set; } // implements 'Title' as 'required' in .NET 7.0+
}

[AutoImplementInterface()]
public interface IDetails
{
  byte[]? Image { get; set; }
  string Summary { get; set; }
}
```
Create some classes implementing the interfaces:
```csharp
public partial class Book : ITitle, IDetails
{ /* will have the properties Title, Image and Summary */ }

public partial class Movie : ITitle, IDetails
{ /* will have the properties Title, Image and Summary */ }

public partial class Song: ITitle
{ /* will have the property Title */ }
```

The source generator now adds the members to the corresponding classes without you having to do this on your own every time.

For details see the __[wiki](https://github.com/basilisque-framework/AutoImplementer/wiki)__.


## License
The Basilisque framework (including this repository) is licensed under the [Apache License, Version 2.0](LICENSE.txt).