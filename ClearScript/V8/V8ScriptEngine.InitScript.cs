// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.





namespace Microsoft.ClearScript.V8
{
    public sealed partial class V8ScriptEngine
    {
        private const string initScript = "Object.defineProperty(this,'EngineInternal',{value:(t=>{let e=t=>t.bind();function r(){return new this(...arguments)}let o=t.isHostObjectKey;delete t.isHostObjectKey;let n=t=>!!t&&!0===t[o],c=Promise,i=JSON,a=Symbol(),s=t.toJson;return delete t.toJson,Object.freeze({commandHolder:{},getCommandResult:e(t=>null==t?t:'function'!=typeof t.hasOwnProperty?'Module'===t[Symbol.toStringTag]?'[module]':'[external]':!0===t[o]?t:'function'!=typeof t.toString?'['+typeof t+']':t.toString()),strictEquals:e((t,e)=>t===e),invokeConstructor:e((t,e)=>{if('function'!=typeof t)throw Error('Function expected');return r.apply(t,Array.from(e))}),invokeMethod:e((t,e,r)=>{if('function'!=typeof e)throw Error('Function expected');return e.apply(t,Array.from(r))}),createPromise:e(function(){return new c(...arguments)}),isPromise:e(t=>t instanceof c),isHostObject:e(n),completePromiseWithResult:e((t,e,r)=>{try{e(t())}catch(o){r(o)}}),completePromise:e((t,e,r)=>{try{t(),e()}catch(o){r(o)}}),throwValue:e(t=>{throw t}),getStackTrace:e(()=>{try{throw Error('[stack trace]')}catch(t){return t.stack}}),toIterator:e(function*(t){try{for(;t.ScriptableMoveNext();)yield t.ScriptableCurrent}finally{t.ScriptableDispose()}}),toAsyncIterator:e(async function*(t){try{for(;await t.ScriptableMoveNextAsync();)yield t.ScriptableCurrent}finally{await t.ScriptableDisposeAsync()}}),getIterator:e(t=>t?.[Symbol.iterator]?.()),getAsyncIterator:e(t=>t?.[Symbol.asyncIterator]?.()),checkpoint:e(()=>{let e=t[a];if(e)throw e}),toJson:e((t,e)=>s?i.parse(s(t,e)):e),parseJson:e(t=>i.parse(t)),asyncGenerator:async function*(){}().constructor})})(this)});";
    }
}
