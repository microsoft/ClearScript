// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

Object.defineProperty(this, 'EngineInternal', { value: (globalObject => {

    const bind = value => value.bind();

    function construct() {
        return new this(...arguments);
    }

    const isHostObjectKey = globalObject.isHostObjectKey;
    delete globalObject.isHostObjectKey;
    const isHostObject = value => !!value && (value[isHostObjectKey] === true);

    const moduleResultKey = globalObject.moduleResultKey;
    delete globalObject.moduleResultKey;

    const getPromiseState = globalObject.getPromiseState;
    delete globalObject.getPromiseState;

    const getPromiseResult = globalObject.getPromiseResult;
    delete globalObject.getPromiseResult;

    const savedPromise = Promise;
    const savedJSON = JSON;
    const checkpointSymbol = Symbol();

    const toJson = globalObject.toJson;
    delete globalObject.toJson;

    return Object.freeze({

        commandHolder: {},

        getCommandResult: bind(value => {
            if ((value === null) || (value === undefined)) {
                return value;
            }
            if (typeof(value.hasOwnProperty) !== 'function') {
                if (value[Symbol.toStringTag] === 'Module') {
                    return '[module]';
                }
                return '[external]';
            }
            if (value[isHostObjectKey] === true) {
                return value;
            }
            if (typeof(value.toString) !== 'function') {
                return '[' + typeof(value) + ']';
            }
            return value.toString();
        }),

        strictEquals: bind((left, right) => left === right),

        isPromise: bind(value => value instanceof savedPromise),

        isHostObject: bind(isHostObject),

        invokeConstructor: bind((constructor, args) => {
            if (typeof(constructor) !== 'function') {
                throw new Error('Function expected');
            }
            return construct.apply(constructor, Array.from(args));
        }),

        invokeMethod: bind((target, method, args) => {
            if (typeof(method) !== 'function') {
                throw new Error('Function expected');
            }
            return method.apply(target, Array.from(args));
        }),

        createPromise: bind(function () {
            return new savedPromise(...arguments);
        }),

        createSettledPromiseWithResult: bind(getResult => {
            try {
                return savedPromise.resolve(getResult());
            }
            catch (exception) {
                return savedPromise.reject(exception);
            }
        }),

        createSettledPromise: bind(wait => {
            try {
                wait();
                return savedPromise.resolve();
            }
            catch (exception) {
                return savedPromise.reject(exception);
            }
        }),

        completePromiseWithResult: bind((getResult, resolve, reject) => {
            try {
                resolve(getResult());
            }
            catch (exception) {
                reject(exception);
            }
            return undefined;
        }),

        completePromise: bind((wait, resolve, reject) => {
            try {
                wait();
                resolve();
            }
            catch (exception) {
                reject(exception);
            }
            return undefined;
        }),

        getPromiseState: bind(getPromiseState),

        getPromiseResult: bind(getPromiseResult),

        initializeTask: bind((promise, isPending, isRejected, onResolved, onRejected) => {
            if (isPending) {
                promise.then(onResolved, onRejected);
            }
            else if (isRejected) {
                onRejected(getPromiseResult(promise));
            }
            else {
                onResolved(getPromiseResult(promise));
            }
            return undefined;
        }),

        createArray: bind(() => []),

        throwValue: bind(value => {
            throw value;
        }),

        getStackTrace: bind(() => {
            try {
                throw new Error('[stack trace]');
            }
            catch (exception) {
                return exception.stack;
            }
        }),

        toIterator: bind(function* (enumerator) {
            try {
                while (enumerator.ScriptableMoveNext()) {
                    yield enumerator.ScriptableCurrent;
                }
            }
            finally {
                enumerator.ScriptableDispose();
            }
        }),

        toAsyncIterator: bind(async function* (asyncEnumerator) {
            try {
                while (await asyncEnumerator.ScriptableMoveNextAsync()) {
                    yield asyncEnumerator.ScriptableCurrent;
                }
            }
            finally {
                await asyncEnumerator.ScriptableDisposeAsync();
            }
        }),

        getIterator: bind(obj => obj?.[Symbol.iterator]?.()),

        getAsyncIterator: bind(obj => obj?.[Symbol.asyncIterator]?.()),

        checkpoint: bind(() => {
            const value = globalObject[checkpointSymbol];
            if (value) {
                throw value;
            }
        }),

        toJson: bind((key, value) => toJson ? savedJSON.parse(toJson(key, value)) : value),

        parseJson: bind(json => savedJSON.parse(json)),

        asyncGenerator: (async function* () {})().constructor,

        getModuleResult: bind(async (result, metaHolder) => {
            await result;
            return metaHolder[0]?.[moduleResultKey];
        })

    });
})(this) });
