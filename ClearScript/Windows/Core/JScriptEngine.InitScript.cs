// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.





namespace Microsoft.ClearScript.Windows.Core
{
    public partial class JScriptEngine
    {
        internal const string InitScript = "EngineInternal=function(){var a=this;function b(b){var c=[];if(b.GetValue)for(var d=b.Length,a=0;a<d;a++)c.push(b[a]);else for(var d=(b=new VBArray(b)).ubound(1)+1,a=0;a<d;a++)c.push(b.getItem(a));return c}function c(a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p){return new this(a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p)}return{getCommandResult:function(a){return null!==a&&('object'==typeof a||'function'==typeof a)&&'function'==typeof a.toString?a.toString():a},invokeConstructor:function(a,d){if('function'!=typeof a)throw new Error('Function expected');return c.apply(a,b(d))},invokeMethod:function(c,a,d){if('function'!=typeof a)throw new Error('Function expected');return a.apply(c,b(d))},isPromise:function(a){return!1},isHostObject:function(b){return!!b&&'function'!=typeof b.constructor&&b!==a},throwValue:function(a){throw a}}}()";
    }
}
