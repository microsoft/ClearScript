// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.





namespace Microsoft.ClearScript.Windows.Core
{
    public partial class JScriptEngine
    {
        internal const string InitScript = "EngineInternal=function(){var t=this;function n(t){var n=[];if(t.GetValue)for(var o=t.Length,e=0;e<o;e++)n.push(t[e]);else for(var o=(t=new VBArray(t)).ubound(1)+1,e=0;e<o;e++)n.push(t.getItem(e));return n}function o(t,n,o,e,u,r,i,f,c,p,s,h,a,l,y,v){return new this(t,n,o,e,u,r,i,f,c,p,s,h,a,l,y,v)}return{getCommandResult:function(t){return null!==t&&('object'==typeof t||'function'==typeof t)&&'function'==typeof t.toString?t.toString():t},invokeConstructor:function(t,e){if('function'!=typeof t)throw Error('Function expected');return o.apply(t,n(e))},invokeMethod:function(t,o,e){if('function'!=typeof o)throw Error('Function expected');return o.apply(t,n(e))},isPromise:function(t){return!1},isHostObject:function(n){return!!n&&'function'!=typeof n.constructor&&n!==t},throwValue:function(t){throw t}}}();";
    }
}
