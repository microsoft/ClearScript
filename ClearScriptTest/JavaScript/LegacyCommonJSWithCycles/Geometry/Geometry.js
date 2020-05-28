// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

var Arithmetic = require("../Arithmetic/Arithmetic");
var Self = require("Geometry");

exports.Module = module;
exports.Meta = module.meta;

exports.Rectangle = function (width, height) {
    this.width = width;
    this.height = height;
}

exports.Rectangle.prototype.getArea = function () {
    return Self.Rectangle.CalculateArea(this.width, this.height);
};

exports.Rectangle.CalculateArea = function (width, height) {
    return Arithmetic.Multiply(width, height);
};

exports.Square = function (side) {
    exports.Rectangle.call(this, side, side);
};

exports.Square.prototype.getArea = exports.Rectangle.prototype.getArea;
