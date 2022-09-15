// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.





namespace Microsoft.ClearScript.V8
{
    public sealed partial class V8ScriptEngine
    {
        private const string initScript = "Object.defineProperty(this,'EngineInternal',{value:(t=>{let e=t=>t.bind();function o(){return new this(...arguments)}let r=t.isHostObjectKey;delete t.isHostObjectKey;let n=t=>!!t&&!0===t[r],i=Promise,c=Symbol(),s=t.toJson;return delete t.toJson,Object.freeze({commandHolder:{},getCommandResult:e(t=>null==t?t:'function'!=typeof t.hasOwnProperty?'Module'===t[Symbol.toStringTag]?'[module]':'[external]':!0===t[r]?t:'function'!=typeof t.toString?'['+typeof t+']':t.toString()),invokeConstructor:e((t,e)=>{if('function'!=typeof t)throw Error('Function expected');return o.apply(t,Array.from(e))}),invokeMethod:e((t,e,o)=>{if('function'!=typeof e)throw Error('Function expected');return e.apply(t,Array.from(o))}),createPromise:e(function(){return new i(...arguments)}),isPromise:e(t=>t instanceof i),isHostObject:e(n),completePromiseWithResult:e((t,e,o)=>{try{e(t())}catch(r){o(r)}}),completePromise:e((t,e,o)=>{try{t(),e()}catch(r){o(r)}}),throwValue:e(t=>{throw t}),getStackTrace:e(()=>{try{throw Error('[stack trace]')}catch(t){return t.stack}}),toIterator:e(function*(t){try{for(;t.MoveNext();)yield t.Current}finally{t.Dispose()}}),toAsyncIterator:e(async function*(t){try{for(;await t.MoveNextPromise();)yield t.Current}finally{await t.DisposePromise()}}),checkpoint:e(()=>{let e=t[c];if(e)throw e}),toJson:e((t,e)=>s?JSON.parse(s(t,e)):e)})})(this)});";
    }
}
