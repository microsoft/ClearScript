// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

import * as Arithmetic from '../Arithmetic/Arithmetic.js';
import * as Self from 'Geometry.js';

export const Meta = import.meta;

export class Rectangle {
    constructor(width, height) {
        this.width = width;
        this.height = height;
    }
    get Area() {
        return Self.Rectangle.CalculateArea(this.width, this.height);
    }
    static CalculateArea(width, height) {
        return Arithmetic.Multiply(width, height);
    }
}

export class Square extends Rectangle {
    constructor(side) {
        super(side, side);
    }
};
