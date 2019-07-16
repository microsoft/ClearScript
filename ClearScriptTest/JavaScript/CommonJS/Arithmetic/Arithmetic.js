// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

exports.Module = module;
exports.Meta = module.meta;

exports.Add = function (a, b) {
    return a + b;
}

exports.Subtract = function (a, b) {
    return a - b;
}

exports.Multiply = function (a, b) {
    return a * b;
}

exports.Divide = function (a, b) {
    return a / b;
}
