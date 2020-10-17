// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8CacheType
//-----------------------------------------------------------------------------

enum class V8CacheType: int32_t
{
    // IMPORTANT: maintain bitwise equivalence with managed enum V8.V8CacheKind
    None,
    Parser,
    Code
};
