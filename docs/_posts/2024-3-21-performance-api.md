---
title: Performance API in ClearScript 7.4.5
---
High-resolution timing facilities are now available for JavaScript code.

# Introduction

By default, ClearScript provides a _bare_ scripting environment. Unless the host takes explicit steps to expose .NET objects or types, scripts can access only their core language facilities, such as the standard JavaScript [built-in objects](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects).

Although ClearScript makes it easy to expose managed resources, high-resolution timing is something that benefits from a native implementation – not because .NET doesn't support it, but because the expensive transition to managed code and back can reduce its effectiveness.

ClearScript 7.4.5 introduces the `Performance` object – an optional JavaScript API for high-resolution timing, available for ClearScript's V8-based JavaScript engine.

# Setting Up

To enable the `Performance` object, specify [`V8ScriptEngineFlags.AddPerformanceObject`](https://microsoft.github.io/ClearScript/Reference/html/T_Microsoft_ClearScript_V8_V8ScriptEngineFlags.htm) when constructing a [`V8ScriptEngine`](https://microsoft.github.io/ClearScript/Reference/html/T_Microsoft_ClearScript_V8_V8ScriptEngine.htm) instance:

{% highlight C# %}

var engine = new V8ScriptEngine(V8ScriptEngineFlags.AddPerformanceObject);

{% endhighlight %}

# The API

The `Performance` object has the following members:

- `Performance.timeOrigin`: This property gets the script engine's creation time. It is a double-precision floating-point value that represents the number of milliseconds since the Unix epoch (00:00:00 UTC on Thursday, January 1, 1970).

- `Performance.now()`: This method returns a high-resolution timestamp in milliseconds. It represents the time elapsed since `Performance.timeOrigin`.

- `Performance.sleep(delay, precise)`: This method suspends script execution for at least the millisecond duration specified by `delay`. If `precise` is falsy or unspecified, the method calls an operating system sleep function. A truthy argument directs it to perform a "cooperative spin wait" instead, providing enhanced precision at the cost of mild CPU load.

# Timer Resolution

The precision of native timing facilities differs across operating systems and can in some cases be adjusted at runtime. In addition to the `Performance` object, ClearScript 7.4.5 supports a new option that sets native timers to the highest available resolution while the script engine is active:

{% highlight C# %}

var flags = V8ScriptEngineFlags.AddPerformanceObject | V8ScriptEngineFlags.SetTimerResolution;
var engine = new V8ScriptEngine(flags);

{% endhighlight %}

Note that `V8ScriptEngineFlags.SetTimerResolution` is only a hint and may be ignored on some systems. Where supported, it can degrade overall system performance or power efficiency, so caution is recommended.

Good luck!