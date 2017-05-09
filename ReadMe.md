# Description
ClearScript is a library that makes it easy to add scripting to your .NET applications. It currently supports JavaScript (via [V8](https://developers.google.com/v8/) and [JScript](http://msdn.microsoft.com/en-us/library/hbxc2t98(v=vs.84).aspx)) and [VBScript](http://msdn.microsoft.com/en-us/library/t0aew7h6(v=vs.84).aspx).

# Features
* Simple usage; create a script engine, add your objects and/or types, run scripts
* Support for several script engines: [Google's V8](https://developers.google.com/v8/), [Microsoft's JScript](http://msdn.microsoft.com/en-us/library/hbxc2t98(v=vs.84).aspx) and [VBScript](http://msdn.microsoft.com/en-us/library/t0aew7h6(v=vs.84).aspx)
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

# Examples
``` C#
using System;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;

// create a script engine
using (var engine = new V8ScriptEngine())
{
    // expose a host type
    engine.AddHostType("Console", typeof(Console));
    engine.Execute("Console.WriteLine('{0} is an interesting number.', Math.PI)");

    // expose a host object
    engine.AddHostObject("random", new Random());
    engine.Execute("Console.WriteLine(random.NextDouble())");

    // expose entire assemblies
    engine.AddHostObject("lib", new HostTypeCollection("mscorlib", "System.Core"));
    engine.Execute("Console.WriteLine(lib.System.DateTime.Now)");

    // create a host object from script
    engine.Execute(@"
        birthday = new lib.System.DateTime(2007, 5, 22);
        Console.WriteLine(birthday.ToLongDateString());
    ");

    // use a generic class from script
    engine.Execute(@"
        Dictionary = lib.System.Collections.Generic.Dictionary;
        dict = new Dictionary(lib.System.String, lib.System.Int32);
        dict.Add('foo', 123);
    ");

    // call a host method with an output parameter
    engine.AddHostObject("host", new HostFunctions());
    engine.Execute(@"
        intVar = host.newVar(lib.System.Int32);
        found = dict.TryGetValue('foo', intVar.out);
        Console.WriteLine('{0} {1}', found, intVar);
    ");

    // create and populate a host array
    engine.Execute(@"
        numbers = host.newArr(lib.System.Int32, 20);
        for (var i = 0; i < numbers.Length; i++) { numbers[i] = i; }
        Console.WriteLine(lib.System.String.Join(', ', numbers));
    ");

    // create a script delegate
    engine.Execute(@"
        Filter = lib.System.Func(lib.System.Int32, lib.System.Boolean);
        oddFilter = new Filter(function(value) {
            return (value & 1) ? true : false;
        });
    ");

    // use LINQ from script
    engine.Execute(@"
        oddNumbers = numbers.Where(oddFilter);
        Console.WriteLine(lib.System.String.Join(', ', oddNumbers));
    ");

    // use a dynamic host object
    engine.Execute(@"
        expando = new lib.System.Dynamic.ExpandoObject();
        expando.foo = 123;
        expando.bar = 'qux';
        delete expando.foo;
    ");

    // call a script function
    engine.Execute("function print(x) { Console.WriteLine(x); }");
    engine.Script.print(DateTime.Now.DayOfWeek);

    // examine a script object
    engine.Execute("person = { name: 'Fred', age: 5 }");
    Console.WriteLine(engine.Script.person.name);

    // read a JavaScript typed array
    engine.Execute("values = new Int32Array([1, 2, 3, 4, 5])");
    var values = (ITypedArray<int>)engine.Script.values;
    Console.WriteLine(string.Join(", ", values.ToArray()));
}
```

# Tutorial
View a [PDF](https://en.wikipedia.org/wiki/Portable_Document_Format) tutorial [here](https://github.com/Microsoft/ClearScript/blob/master/ClearScript/doc/FAQtorial.pdf).

Click [here](https://github.com/Microsoft/ClearScript/raw/master/ClearScript/doc/FAQtorial.docx) to download a copy in [Word](https://en.wikipedia.org/wiki/Microsoft_Word) format.

# Reference
Browse the ClearScript Library Reference [here](https://microsoft.github.io/ClearScript/Reference/index.html).

Click [here](https://github.com/Microsoft/ClearScript/raw/master/ClearScript/doc/Reference.chm) to download the reference in [CHM](https://en.wikipedia.org/wiki/Microsoft_Compiled_HTML_Help) format. If you get a security warning when you open this file, uncheck "Always ask before opening this file".

# Project Details
See [here](https://github.com/Microsoft/ClearScript/blob/master/Build.txt) for information about building, integrating, and deploying ClearScript.
