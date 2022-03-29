---
title: Custom Attribute Loaders in ClearScript 7.2.4
---
You can now add ClearScript attributes to platform and external resources, and more.

# Background

ClearScript offers several [attributes](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/attributes/) for controlling how .NET resources are exposed for scripting. They allow hosts to expose type members under substitute names, restrict or block script access to individual resources, and adjust marshaling behavior. An example is [`ScriptMemberAttribute`](https://microsoft.github.io/ClearScript/Reference/html/T_Microsoft_ClearScript_ScriptMemberAttribute.htm). 

.NET languages typically provide dedicated syntax for declaring attributes in source code, enabling fine-grained customization. However, that approach has some disadvantages:

1. New attributes can't be added to code developed elsewhere, such as .NET platform components and external libraries.

1. There's no convenient and efficient way to apply attributes broadly – e.g., for automatic renaming of all exposed type members.

# Custom Attribute Loaders

ClearScript now funnels all attribute access through a global facility, the [_custom attribute loader_](https://microsoft.github.io/ClearScript/Reference/html/P_Microsoft_ClearScript_HostSettings_CustomAttributeLoader.htm). The host can override that facility to _virtualize_ attribute retrieval, gaining the ability to add new attributes to _any_ .NET resource – as well as modify or hide conventionally declared attributes – all by overriding a single class method.

# Example: Expose .NET Type Members as Lower Camel Case

By convention, the names of public .NET type members are usually upper-camel-cased (a.k.a. Pascal-cased), whereas Javascript property names are often lower-camel-cased.

Suppose you'd like to make all exposed .NET type members accessible to script code via lower-camel-cased names. Until now, if your script API included types developed elsewhere, there was no way to achieve this without writing wrappers whose member names were under your control.

Now, by overriding ClearScript's custom attribute loader, you can write a small method to implement a _global_ transformation to lower camel case:

{% highlight C# %}

class MyAttributeLoader : CustomAttributeLoader {
    public override T[] LoadCustomAttributes<T>(ICustomAttributeProvider resource, bool inherit) {
        var declaredAttributes = base.LoadCustomAttributes<T>(resource, inherit);
        if (!declaredAttributes.Any() && typeof(T) == typeof(ScriptMemberAttribute) && resource is MemberInfo member) {
            var lowerCamelCaseName = char.ToLowerInvariant(member.Name[0]) + member.Name.Substring(1);
            return new[] { new ScriptMemberAttribute(lowerCamelCaseName) } as T[];
        }
        return declaredAttributes;
    }
}

{% endhighlight %}

And then:

{% highlight C# %}

HostSettings.CustomAttributeLoader = new MyAttributeLoader();
engine.AddHostType(typeof(Console));
engine.Execute("Console.writeLine('Hello, world!');");

{% endhighlight %}

You can also use this technique to centralize script access control and extend other ClearScript attribute capabilities to platform and external resources.

Good luck!