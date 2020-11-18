# Description
ClearScript is a library that makes it easy to add scripting to your .NET applications. It currently supports JavaScript (via [V8](https://developers.google.com/v8/) and [JScript](https://docs.microsoft.com/en-us/previous-versions//hbxc2t98(v=vs.85))) and [VBScript](https://docs.microsoft.com/en-us/previous-versions//t0aew7h6(v=vs.85)).

# Features
* Simple usage; create a script engine, add your objects and/or types, run scripts
* Support for several script engines: [Google's V8](https://developers.google.com/v8/), [Microsoft's JScript](https://docs.microsoft.com/en-us/previous-versions//hbxc2t98(v=vs.85)) and [VBScript](https://docs.microsoft.com/en-us/previous-versions//t0aew7h6(v=vs.85))
* Exposed resources require no modification, decoration, or special coding of any kind
* Scripts get simple access to most of the features of exposed objects and types:
  * Methods, properties, fields, events
  * (Objects) Indexers, extension methods, conversion operators, explicitly implemented interfaces
  * (Types) Constructors, nested types
* Full support for generic types and methods, including C#-like type inference and explicit type arguments
* Scripts can invoke methods with output parameters, optional parameters, and parameter arrays
* Script delegates enable callbacks into script code
* Support for exposing all the types defined in one or more assemblies in one step
* Optional support for importing types and assemblies from script code
* The host can invoke script functions and access script objects directly
* Full support for script debugging
* (V8) Support for fast data transfer to and from [JavaScript typed arrays](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Typed_arrays)
* (V8) Support for [JavaScript modules](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules)
* (JavaScript) Support for [CommonJS modules](http://wiki.commonjs.org/wiki/Modules)
* :new: Support for .NET Core 3.1 and .NET 5.0 on Windows, Linux, and macOS.

# Installation
### Composite packages
* [![ClearScript](https://img.shields.io/nuget/vpre/Microsoft.ClearScript?label=Windows&logo=Windows&logoColor=white)](https://www.nuget.org/packages/Microsoft.ClearScript)
* [![ClearScript.linux-x64](https://img.shields.io/nuget/vpre/Microsoft.ClearScript.linux-x64?label=Linux&logo=Linux&logoColor=white)](https://www.nuget.org/packages/Microsoft.ClearScript.linux-x64)
* [![ClearScript.osx-x64](https://img.shields.io/nuget/vpre/Microsoft.ClearScript.osx-x64?label=macOS&logo=Apple&logoColor=white)](https://www.nuget.org/packages/Microsoft.ClearScript.osx-x64)
### Component packages
* [![ClearScript.Core](https://img.shields.io/nuget/vpre/Microsoft.ClearScript.Core?label=ClearScript.Core&logo=NuGet&logoColor=white)](https://www.nuget.org/packages/Microsoft.ClearScript.Core)
* [![ClearScript.Windows](https://img.shields.io/nuget/vpre/Microsoft.ClearScript.Windows?label=ClearScript.Windows&logo=NuGet&logoColor=white)](https://www.nuget.org/packages/Microsoft.ClearScript.Windows)
* [![ClearScript.V8](https://img.shields.io/nuget/vpre/Microsoft.ClearScript.V8?label=ClearScript.V8&logo=NuGet&logoColor=white)](https://www.nuget.org/packages/Microsoft.ClearScript.V8)
### V8 native assembly packages
* [![ClearScript.V8.Native.win-x86](https://img.shields.io/nuget/vpre/Microsoft.ClearScript.V8.Native.win-x86?label=Windows%20(x86)&logo=V8&logoColor=white)](https://www.nuget.org/packages/Microsoft.ClearScript.V8.Native.win-x86)
* [![ClearScript.V8.Native.win-x64](https://img.shields.io/nuget/vpre/Microsoft.ClearScript.V8.Native.win-x64?label=Windows%20(x64)&logo=V8&logoColor=white)](https://www.nuget.org/packages/Microsoft.ClearScript.V8.Native.win-x64)
* [![ClearScript.V8.Native.linux-x64](https://img.shields.io/nuget/vpre/Microsoft.ClearScript.V8.Native.linux-x64?label=Linux%20(x64)&logo=V8&logoColor=white)](https://www.nuget.org/packages/Microsoft.ClearScript.V8.Native.linux-x64)
* [![ClearScript.V8.Native.osx-x64](https://img.shields.io/nuget/vpre/Microsoft.ClearScript.V8.Native.osx-x64?label=macOS%20(x64)&logo=V8&logoColor=white)](https://www.nuget.org/packages/Microsoft.ClearScript.V8.Native.osx-x64)

# Documentation
* [Examples](https://microsoft.github.io/ClearScript/Examples/Examples.html)
* [Tutorial](https://microsoft.github.io/ClearScript/Tutorial/FAQtorial.html)
* [API reference](https://microsoft.github.io/ClearScript/Reference/index.html)
* [Building, integrating, and deploying ClearScript](https://microsoft.github.io/ClearScript/Details/Build.html)
* [Old project site on CodePlex](https://clearscript.codeplex.com/)
