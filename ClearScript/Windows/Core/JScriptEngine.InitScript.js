// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

EngineInternal = (function () {

    var globalObject = this;

    function convertArgs(args) {
        var result = [];
        if (args.GetValue) {
            var count = args.Length;
            for (var i = 0; i < count; i++) {
                result.push(args[i]);
            }
        }
        else {
            args = new VBArray(args);
            var count = args.ubound(1) + 1;
            for (var i = 0; i < count; i++) {
                result.push(args.getItem(i));
            }
        }
        return result;
    }

    function construct(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) {
        return new this(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
    }

    var savedJSON = globalObject.JSON;

    return {

        getCommandResult: function (value) {
            if (value !== null) {
                if ((typeof(value) === 'object') || (typeof(value) === 'function')) {
                    if (typeof(value.toString) === 'function') {
                        return value.toString();
                    }
                }
            }
            return value;
        },

        invokeConstructor: function (constructor, args) {
            if (typeof(constructor) !== 'function') {
                throw new Error('Function expected');
            }
            return construct.apply(constructor, convertArgs(args));
        },

        invokeMethod: function (target, method, args) {
            if (typeof(method) !== 'function') {
                throw new Error('Function expected');
            }
            return method.apply(target, convertArgs(args));
        },

        isPromise: function (value) {
            return false;
        },

        isHostObject: function (value) {
            return !!value && (typeof(value.constructor) !== 'function') && (value !== globalObject);
        },

        throwValue: function (value) {
            throw value;
        },

        parseJson: function (json) {
            return savedJSON ? savedJSON.parse(json) : eval('(' + json + ')');
        }
    };
})();
