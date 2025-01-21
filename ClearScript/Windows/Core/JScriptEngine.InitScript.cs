// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.





namespace Microsoft.ClearScript.Windows.Core
{
    public partial class JScriptEngine
    {
        internal const string InitScript = "EngineInternal=function(){var globalObject=this;function convertArgs(t){var n=[];if(t.GetValue)for(var o=t.Length,e=0;e<o;e++)n.push(t[e]);else for(var o=(t=new VBArray(t)).ubound(1)+1,e=0;e<o;e++)n.push(t.getItem(e));return n}function construct(t,n,o,e,r,u,i,f,c,p,s,a,h,l,y,v){return new this(t,n,o,e,r,u,i,f,c,p,s,a,h,l,y,v)}var savedJSON=globalObject.JSON;return{getCommandResult:function(t){return null!==t&&('object'==typeof t||'function'==typeof t)&&'function'==typeof t.toString?t.toString():t},invokeConstructor:function(t,n){if('function'!=typeof t)throw Error('Function expected');return construct.apply(t,convertArgs(n))},invokeMethod:function(t,n,o){if('function'!=typeof n)throw Error('Function expected');return n.apply(t,convertArgs(o))},isPromise:function(t){return!1},isHostObject:function(t){return!!t&&'function'!=typeof t.constructor&&t!==globalObject},throwValue:function(t){throw t},parseJson:function(json){return savedJSON?savedJSON.parse(json):eval('('+json+')')}}}();";
    }
}
