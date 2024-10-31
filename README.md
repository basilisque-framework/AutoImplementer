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

[![NuGet Basilisque.AutoImplementer.CodeAnalysis](https://img.shields.io/badge/NuGet_Basilisque.AutoImplementer.CodeAnalysis-latest-blue.svg)](https://www.nuget.org/packages/Basilisque.AutoImplementer.CodeAnalysis)
[![License](https://img.shields.io/badge/License-Apache%20License%202.0-red.svg)](LICENSE.txt)

## Description
This project contains a source generator that automatically implements interface members in classes implementing the interfaces.  
The goal is to provide a workaround for C# not supporting multiple inheritance for some basic use cases.

## Getting Started
Install the NuGet package [Basilisque.AutoImplementer](https://www.nuget.org/packages/Basilisque.AutoImplementer.CodeAnalysis).  
Installing the package will add the source generator to your project.

Now you're ready to [start implementing your interfaces automatically](https://github.com/basilisque-framework/AutoImplementer/wiki/Getting-Started).


## Features
### General
- Automatic implementation of interfaces on classes
- Classes have to be marked with the `[AutoImplementInterfaces]` attribute
  - Then all interfaces marked with the `[AutoImplementInterface]` attribute will be implemented
    ```csharp
    [AutoImplementInterface]
    public interface ITitle
    {
      string Title { get; set; }
    }

    public interface ISummary
    {
      string Summary { get; set; }
    }

    [AutoImplementInterfaces]
    public partial class Book : ITitle, ISummary
    { /* will have the property 'Title' only. 'ISummary' is not marked with an attribute */ }
    ```
  - By specifying the interfaces explicitly within the attribute on the class, the interfaces dont't have to be marked  
    (`[AutoImplementInterfaces<IInterface>()]` or `[AutoImplementInterfaces(typeof(IInterface))]`)
    ```csharp
    public interface ITitle
    {
      string Title { get; set; }
    }

    public interface ISummary
    {
      string Summary { get; set; }
    }

    [AutoImplementInterfaces<ITitle, ISummary>()]
    //[AutoImplementInterfaces(typeof(ITitle), typeof(ISummary))] <- alternative syntax
    public partial class Book : ITitle, ISummary
    { /* will have the properties 'Title' and 'Summary' */ }
    ```
  
- When the interfaces are explicitly stated, they don't have to be stated as base type a second time. This is valid:
  ```csharp
  public interface ITitle
  {
    string Title { get; set; }
  }

  [AutoImplementInterfaces<ITitle>()]
  //[AutoImplementInterfaces(typeof(ITitle))] <- alternative syntax
  public partial class Book // ': ITitle' <- this is optional because 'ITitle' was specified in the attribute
  { }
  ```

### Property Implementation
- By default, all non-nullable properties are implemented with the 'required' modifier
  ```csharp
  public interface IBook
  {
    string Title { get; set; }          // required
    
    DateOnly? PublishDate { get; set; } // not required

    string Publisher { get; set; }      // required
  }
  ```
- This can be disabled by setting the 'Strict' property to 'false' in the `[AutoImplementInterface]` attribute, but it's not recommended.
  ```csharp
  [AutoImplementInterface(Strict = false)]
  public interface IMovie
  {
    [Required] string Title { get; set; } // required

    [AutoImplement(AsRequired = true)]    // required
    TimeSpan Runtime { get; set; }
    
    string Summary { get; set; } // not required (unsafe!)
  }
  ```
- Properties can also be skipped. Then they have to be implemented manually.
  ```csharp
  public interface IPublish
  {
    DateOnly PublishDate { get; set; }

    [AutoImplement(Implement = false)] // skips the 'Publisher' property
    string? Publisher { get; set; }
  }
  ```

## Examples
Create the interfaces:
```csharp
[AutoImplementInterface]
public interface ITitle
{
  [Required] string Title { get; set; }
}

public interface IDetails
{
  byte[]? Image { get; set; }  
  string Summary { get; set; }  
}
```

Create some classes implementing the interfaces:
```csharp
[AutoImplementInterfaces()] // <- no interfaces were stated explicitly. 'ITitle' will be implemented because it is marked with the 'AutoImplementInterface' attribute
public partial class Book : ITitle, IDetails
{ /* will have the properties Title only */ }

[AutoImplementInterfaces<IDetails>()] // <- 'IDetails' was stated explicitly; only this interface will be implemented
public partial class Movie : ITitle, IDetails
{ /* will have the properties Image and Summary */ }

[AutoImplementInterfaces<IDetails>()] // <- will implement 'IDetails'
[AutoImplementInterfaces()]           // <- will find 'ITitle' because it is marked with the 'AutoImplementInterface' attribute
public partial class Game : ITitle, IDetails
{ /* will have the properties Title, Image and Summary */ }

[AutoImplementInterfaces(typeof(ITitle), typeof(IDetails))]
public partial class Event
{ /* will have the properties Title, Image and Summary */ }
```

The source generator now adds the members to the corresponding classes without you having to do this on your own every time.

For details see the __[wiki](https://github.com/basilisque-framework/AutoImplementer/wiki)__.


## License
The Basilisque framework (including this repository) is licensed under the [Apache License, Version 2.0](LICENSE.txt).