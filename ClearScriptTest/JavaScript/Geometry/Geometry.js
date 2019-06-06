// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

import * as Arithmetic from '../Arithmetic/Arithmetic.js';

export class Rectangle {
    constructor(width, height) {
        this.width = width;
        this.height = height;
    }
    get Area() {
        return Arithmetic.Multiply(this.width, this.height);
    }
}

export class Square extends Rectangle {
    constructor(side) {
        super(side, side);
    }
};
