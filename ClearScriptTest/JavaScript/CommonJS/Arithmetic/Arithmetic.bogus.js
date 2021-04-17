// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

exports.Module = module;
exports.Meta = module.meta;

exports.BogusAdd = function (a, b) {
    return a + b;
}

exports.BogusSubtract = function (a, b) {
    return a - b;
}

exports.BogusMultiply = function (a, b) {
    return a * b;
}

exports.BogusDivide = function (a, b) {
    return a / b;
}
