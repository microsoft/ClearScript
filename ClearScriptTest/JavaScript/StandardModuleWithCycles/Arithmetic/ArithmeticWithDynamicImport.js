// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

// ReSharper disable once UnusedLocals
let Geometry;
(async function() {
    // ReSharper disable once UseOfImplicitGlobalInFunctionScope
    Geometry = await import('../Geometry/GeometryWithDynamicImport.js');
})();

export const Meta = import.meta;

export function Add(a, b) {
    return a + b;
}

export function Subtract(a, b) {
    return a - b;
}

export function Multiply(a, b) {
    return a * b;
}

export function Divide(a, b) {
    return a / b;
}
