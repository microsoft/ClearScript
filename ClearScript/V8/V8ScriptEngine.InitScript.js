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

    const savedPromise = Promise;
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

        isPromise: bind(value => value instanceof savedPromise),

        isHostObject: bind(isHostObject),

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

        checkpoint: bind(() => {
            const value = globalObject[checkpointSymbol];
            if (value) {
                throw value;
            }
        }),

        toJson: bind((key, value) => toJson ? JSON.parse(toJson(key, value)) : value),

        asyncGenerator: (async function* () {})().constructor

    });
})(this) });
