---
title: Module Interoperability in ClearScript 7.3.7
---
Standard (ES6) modules can now import CommonJS modules.

# Introduction

[JavaScript modules](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules) offer a way to split complex scripts into independent functional units with well-defined interfaces. The [`import`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/import) and [`export`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/export) declarations, as well as the [`import`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/import) operator, were introduced in ECMAScript 2015 (ES6) as a standard way for modules to share code and data.

This facility supersedes earlier module specifications such as [CommonJS](https://commonjs.org/). However, the latter remains in heavy use, and many popular libraries aren't available in any other form.

ClearScript 7.3.7 allows JavaScript modules to import resources from CommonJS libraries. In this post we'll walk through an example.

# Basic Setup

For this example, let's allow scripts to load documents and use the console:

{% highlight C# %}

engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
engine.AddHostType(typeof(Console));

{% endhighlight %}


# Document Categories

ClearScript uses [_document categories_](https://microsoft.github.io/ClearScript/Reference/html/T_Microsoft_ClearScript_DocumentCategory.htm) to distinguish between the following document types:
- JavaScript 
module – [`ModuleCategory.Standard`](https://microsoft.github.io/ClearScript/Reference/html/P_Microsoft_ClearScript_JavaScript_ModuleCategory_Standard.htm)
- CommonJS module – [`ModuleCategory.CommonJS`](https://microsoft.github.io/ClearScript/Reference/html/P_Microsoft_ClearScript_JavaScript_ModuleCategory_CommonJS.htm)
- Normal script – [`DocumentCategory.Script`](https://microsoft.github.io/ClearScript/Reference/html/P_Microsoft_ClearScript_DocumentCategory_Script.htm)

However, it has no way to _detect_ the category of a document. Instead, the host must specify the category when it initiates script execution:

{% highlight C# %}

engine.Execute(new DocumentInfo { Category = ModuleCategory.Standard }, @"
    import { Rectangle } from 'Geometry';
    Console.WriteLine('The area is {0}.', new Rectangle(3, 4).Area);
");

{% endhighlight %}

If the host doesn't provide a category, ClearScript assumes `DocumentCategory.Script`.

On the other hand, if a module is loaded on behalf of another module, it _inherits_ its category from the requesting module. In our example, **Geometry** inherits `ModuleCategory.Standard`.

# Overriding the Category

Now let's suppose that **Geometry** is actually a CommonJS module and looks something like this:

{% highlight JavaScript %}

// Geometry.js
exports.Rectangle = class {
    constructor(width, height) {
        this.width = width;
        this.height = height;
    }
    get Area() {
        return this.width * this.height;
    }
}

{% endhighlight %}

Normally, the sample code above would result in a [`SyntaxError`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/SyntaxError) with a message such as "The requested module 'Geometry' does not provide an export named 'Rectangle'".

To allow our example to work, we must _override_ **Geometry**'s document category. To do that, we can use a [document load callback](https://microsoft.github.io/ClearScript/Reference/html/P_Microsoft_ClearScript_DocumentSettings_LoadCallback.htm):

{% highlight C# %}

engine.DocumentSettings.LoadCallback = (ref DocumentInfo info) => {
    if (Path.GetFileNameWithoutExtension(info.Uri.AbsolutePath) == "Geometry") {
        info.Category = ModuleCategory.CommonJS;
    }
};

{% endhighlight %}

Note that we're using a simple file name comparison to assign the document category. A real-world host might use a more generic algorithm, basing the assignment on the document's location, its file name extension, an external manifest, or even the document's contents.

# A Final Hurdle

In ClearScript 7.3.7, `V8ScriptEngine` is capable of importing CommonJS resources via the standard `import` declaration and operator. However, by default, the document loader throws an exception if a newly loaded document is of an unexpected category.

In other words, simply overriding the category would make our example fail even earlier – at the document loading stage. ClearScript 7.3.7 retains that behavior for compatibility and safety reasons. In many cases, blocking unexpected document categories is the prudent option.

In _this_ case, however, we can use a new flag to relax that requirement:

{% highlight C# %}

engine.DocumentSettings.AccessFlags |= DocumentAccessFlags.AllowCategoryMismatch;

{% endhighlight %}

With this flag in place, the document loader allows **Geometry** to pass on to the script engine, which now supports the CommonJS module category and safely imports the requested resources.

# Putting It All Together

Here's the complete, working sample code:

{% highlight C# %}

engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading | DocumentAccessFlags.AllowCategoryMismatch;
engine.DocumentSettings.LoadCallback = (ref DocumentInfo info) => {
    if (Path.GetFileNameWithoutExtension(info.Uri.AbsolutePath) == "Geometry") {
        info.Category = ModuleCategory.CommonJS;
    }
};
engine.AddHostType(typeof(Console));
engine.Execute(new DocumentInfo { Category = ModuleCategory.Standard }, @"
    import { Rectangle } from 'Geometry';
    Console.WriteLine('The area is {0}.', new Rectangle(3, 4).Area);
");

{% endhighlight %}

Module interoperability allows newer scripts to use the standard JavaScript module facility while consuming existing CommonJS libraries.

# How About Reverse Interoperability?

Unfortunately, importing standard modules from CommonJS modules is _not_ possible. The problem has to do with synchronous vs. asynchronous execution modes.

Specifically, standard modules can be [asynchronous](https://github.com/tc39/proposal-top-level-await), so they can invoke both synchronous and asynchronous code at the top level. Even if a module doesn't use [`await`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/await), its top-level code can be effectively asynchronous if it imports any asynchronous modules.

The top-level code of a CommonJS module is executed as a normal (synchronous) function, so it cannot interoperate with asynchronous code.

Good luck!