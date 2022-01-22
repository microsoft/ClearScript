---
title: Object Sharing Changes in ClearScript 7.2
---
ClearScript 7.2 includes enhancements for sharing objects across V8 script engines and runtimes.

# Introduction

In ClearScript 7.1 and earlier, passing a script object from one engine to another always results in the latter holding a "proxy to a proxy" – a special object that forwards property access to a managed proxy to the original script object.

The overhead of that arrangement is usually unavoidable, as script engines generally can't be given direct access to foreign script objects.

However, there are specific scenarios in which alternate means of sharing are not only possible but preferable. ClearScript 7.2 enables the following exceptions to the normal behavior.

# Engines That Share a V8 Runtime

Consider the following C# code:

{% highlight C# %}

using var runtime = new V8Runtime();
using var engine1 = runtime.CreateScriptEngine();
using var engine2 = runtime.CreateScriptEngine();

{% endhighlight %}

This creates a V8 runtime with two script engines – an arrangement that looks something like this:

![Two Engines One Runtime](/ClearScript/images/Two-Engines-One-Runtime.svg)

Now let's create a script object in one engine and copy a reference to the other:

{% highlight C# %}

engine1.Execute("foo = { bar: 123 }");
engine2.Script.foo = engine1.Script.foo;

{% endhighlight %}

Executing this code in ClearScript 7.1 or earlier results in the following:

![Two Engines One Runtime Double Proxy](/ClearScript/images/Two-Engines-One-Runtime-Double-Proxy.svg)

With this setup, `foo` in `engine2` is very expensive to access and may be missing some functionality. For example, [JavaScript iteration protocols](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Iteration_protocols) can't be routed through the managed proxy.

This "double proxy" construction is usually required, as different script engines generally can't access each other's objects directly. In this instance, however, they can do just that – safely and without ClearScript's involvement.

ClearScript 7.2 detects this case and produces the following configuration for the same code:

![Two Engines One Runtime Shared Object](/ClearScript/images/Two-Engines-One-Runtime-Shared-Object.svg)

All script engines in the same V8 runtime can be given direct access to each other's objects with full functionality, performance, and safety.

# V8 Shared Array Buffers and Views

Suppose you create two V8 script engines, each within its own runtime:

{% highlight C# %}

using var engine1 = new V8ScriptEngine();
using var engine2 = new V8ScriptEngine();

{% endhighlight %}

In memory, this setup looks something like this:

![Two Engines Two Runtimes](/ClearScript/images/Two-Engines-Two-Runtimes.svg)

Normally, script engines in separate runtimes can't share objects. However, [`SharedArrayBuffer`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/SharedArrayBuffer) was designed specifically for sharing memory across runtimes.

ClearScript 7.1 and earlier don't support this form of sharing. Let's say you create a shared array buffer in one script engine and copy a reference to the other:

{% highlight C# %}

engine1.Execute("sab = new SharedArrayBuffer(1024)");
engine2.Script.sab = engine1.Script.sab;

{% endhighlight %}

In ClearScript 7.1 and earlier, this code results in the following:

![Two Engines Two Runtimes Double Proxy](/ClearScript/images/Two-Engines-Two-Runtimes-Double-Proxy.svg)

In this configuration, `sab` in `engine2` is a "double proxy" that really isn't very useful. For example, you can't create [data views](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/DataView) or [typed arrays](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/TypedArray) on top of it. Additionally, it incurs a lot of overhead, as all access is routed through the host back to `engine1`.

However, the same code in ClearScript 7.2 results in this:

![Two Engines Two Runtimes Shared Backing Store](/ClearScript/images/Two-Engines-Two-Runtimes-Shared-Backing-Store.svg)

This arrangement supports the full functionality and performance of shared array buffers in both script engines, with independent access to the shared backing store.

Finally, note that shared data views and typed arrays – that is, data views and typed arrays backed by shared array buffers – are also marshaled in this manner, enabling easy memory sharing across V8 runtimes. You can use the standard [`Atomics`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Atomics) object to synchronize access to these resources.

Good luck!