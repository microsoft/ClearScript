// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.





namespace Microsoft.ClearScript.V8
{
    public sealed partial class V8ScriptEngine
    {
        private const string initScript = "Object.defineProperty(this,'EngineInternal',{value:(e=>{let t=e=>e.bind();function r(){return new this(...arguments)}let o=e.isHostObjectKey;delete e.isHostObjectKey;let n=e=>!!e&&!0===e[o],i=e.moduleResultKey;delete e.moduleResultKey;let c=e.getPromiseState;delete e.getPromiseState;let s=e.getPromiseResult;delete e.getPromiseResult;let a=Promise,l=JSON,u=Symbol(),y=e.toJson;return delete e.toJson,Object.freeze({commandHolder:{},getCommandResult:t(e=>null==e?e:'function'!=typeof e.hasOwnProperty?'Module'===e[Symbol.toStringTag]?'[module]':'[external]':!0===e[o]?e:'function'!=typeof e.toString?'['+typeof e+']':e.toString()),strictEquals:t((e,t)=>e===t),isPromise:t(e=>e instanceof a),isHostObject:t(n),invokeConstructor:t((e,t)=>{if('function'!=typeof e)throw Error('Function expected');return r.apply(e,Array.from(t))}),invokeMethod:t((e,t,r)=>{if('function'!=typeof t)throw Error('Function expected');return t.apply(e,Array.from(r))}),createPromise:t(function(){return new a(...arguments)}),createSettledPromiseWithResult:t(e=>{try{return a.resolve(e())}catch(t){return a.reject(t)}}),createSettledPromise:t(e=>{try{return e(),a.resolve()}catch(t){return a.reject(t)}}),completePromiseWithResult:t((e,t,r)=>{try{t(e())}catch(o){r(o)}}),completePromise:t((e,t,r)=>{try{e(),t()}catch(o){r(o)}}),getPromiseState:t(c),getPromiseResult:t(s),initializeTask:t((e,t,r,o,n)=>{t?e.then(o,n):r?n(s(e)):o(s(e))}),createArray:t(()=>[]),throwValue:t(e=>{throw e}),getStackTrace:t(()=>{try{throw Error('[stack trace]')}catch(e){return e.stack}}),toIterator:t(function*(e){try{for(;e.ScriptableMoveNext();)yield e.ScriptableCurrent}finally{e.ScriptableDispose()}}),toAsyncIterator:t(async function*(e){try{for(;await e.ScriptableMoveNextAsync();)yield e.ScriptableCurrent}finally{await e.ScriptableDisposeAsync()}}),getIterator:t(e=>e?.[Symbol.iterator]?.()),getAsyncIterator:t(e=>e?.[Symbol.asyncIterator]?.()),checkpoint:t(()=>{let t=e[u];if(t)throw t}),toJson:t((e,t)=>y?l.parse(y(e,t)):t),parseJson:t(e=>l.parse(e)),asyncGenerator:async function*(){}().constructor,getModuleResult:t(async(e,t)=>(await e,t[0]?.[i]))})})(this)});";
    }
}
