---
title: Using .NET Playgrounds with ClearScript 7.3
---
ClearScript is now easier to use with LINQPad and .NET Fiddle.

# Background

.NET playgrounds such as [LINQPad](https://www.linqpad.net/) and [.NET Fiddle](https://dotnetfiddle.net/) offer a great way to create small programs and explore the .NET platform.

In the past, ClearScript was difficult to use with these environments due to its reliance on native V8 assemblies and the Windows-only .NET Framework. Now that it supports newer .NET runtimes as well as several popular operating systems and machine architectures, ClearScript is much easier to incorporate into LINQPad and .NET Fiddle.

ClearScript 7.3 includes additional changes that make it almost as easy to use in these environments as a pure .NET library.

# LINQPad

[LINQPad](https://www.linqpad.net/) is a powerful .NET playground for Windows. We've tested ClearScript 7.3 with LINQPad 7 (x86/x64 and arm64) and LINQPad 5 (x86/x64).

Use the LINQPad NuGet Manager to add the appropriate package to your query:

- For LINQPad 7 (x86/x64) and LINQPad 5, use __ClearScript Library for Windows (x86/x64)__ (Package ID: [Microsoft.ClearScript](https://www.nuget.org/packages/Microsoft.ClearScript/)).  
![LINQPad 7 NuGet Manager (x86/x64)](/ClearScript/images/LINQPad-7-NuGet-Manager-x86-x64.png)

- For LINQPad 7 (arm64), use __ClearScript Library for Windows (arm64)__ (Package ID: [Microsoft.ClearScript.win-arm64](https://www.nuget.org/packages/Microsoft.ClearScript.win-arm64/)).  
![LINQPad 7 NuGet Manager (arm64)](/ClearScript/images/LINQPad-7-NuGet-Manager-arm64.png)

Be sure to select Version 7.3.0 or later. As soon as LINQPad completes the package import procedure, ClearScript is ready to go:

![LINQPad 7 QuickTest](/ClearScript/images/LINQPad-7-QuickTest.png)

# .NET Fiddle

[.NET Fiddle](https://dotnetfiddle.net/) is a convenient online .NET playground that enables easy sharing of code snippets. ClearScript 7.3 works seamlessly with its .NET 6 compiler.

Here's how to enable ClearScript in your fiddle:

1. Select ".NET 6" in the Compiler drop-down menu:  
![.NET Fiddle Compiler Dropdown](/ClearScript/images/DotNetFiddle-Compiler-Dropdown.png)

1. Add the NuGet package __ClearScript Library for Linux (x64)__ (Package ID: [Microsoft.ClearScript.linux-x64](https://www.nuget.org/packages/Microsoft.ClearScript.linux-x64/)). Be sure to select Version 7.3.0 or later:  
![.NET Fiddle NuGet Packages](/ClearScript/images/DotNetFiddle-NuGet-Packages.png)

You are now ready to use ClearScript:

![.NET Fiddle QuickTest](/ClearScript/images/DotNetFiddle-QuickTest.png)

You can find this fiddle [here](https://dotnetfiddle.net/3daJda). Click [here](https://dotnetfiddle.net/GdGvZ5) to see the full set of [ClearScript examples](https://microsoft.github.io/ClearScript/Examples/Examples.html) in action.

Good luck!