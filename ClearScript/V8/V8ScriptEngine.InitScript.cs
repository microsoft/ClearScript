// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.





namespace Microsoft.ClearScript.V8
{
    public sealed partial class V8ScriptEngine
    {
        private const string initScript = "Object.defineProperty(this,'EngineInternal',{value:(b=>{let a=a=>a.bind();function d(){return new this(...arguments)}let e=b.isHostObjectKey;delete b.isHostObjectKey;let c=a=>!!a&& !0===a[e],f=Promise,g=Symbol(),h=b.toJson;return delete b.toJson,Object.freeze({commandHolder:{},getCommandResult:a(a=>null==a?a:'function'!=typeof a.hasOwnProperty?'Module'===a[Symbol.toStringTag]?'[module]':'[external]':!0===a[e]?a:'function'!=typeof a.toString?'['+typeof a+']':a.toString()),invokeConstructor:a((a,b)=>{if('function'!=typeof a)throw new Error('Function expected');return d.apply(a,Array.from(b))}),invokeMethod:a((b,a,c)=>{if('function'!=typeof a)throw new Error('Function expected');return a.apply(b,Array.from(c))}),createPromise:a(function(){return new f(...arguments)}),isPromise:a(a=>a instanceof f),isHostObject:a(c),completePromiseWithResult:a((a,b,c)=>{try{b(a())}catch(d){c(d)}}),completePromise:a((a,b,c)=>{try{a(),b()}catch(d){c(d)}}),throwValue:a(a=>{throw a}),getStackTrace:a(()=>{try{throw new Error('[stack trace]')}catch(a){return a.stack}}),toIterator:a(function*(a){try{for(;a.MoveNext();)yield a.Current}finally{a.Dispose()}}),toAsyncIterator:a(async function*(a){try{for(;await a.MoveNextPromise();)yield a.Current}finally{await a.DisposePromise()}}),checkpoint:a(()=>{let a=b[g];if(a)throw a}),toJson:a((b,a)=>h?JSON.parse(h(b,a)):a)})})(this)})";
    }
}
