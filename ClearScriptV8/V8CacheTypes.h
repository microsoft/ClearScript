// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8CacheKind
//-----------------------------------------------------------------------------

enum class V8CacheKind: int32_t
{
    // IMPORTANT: maintain bitwise equivalence with managed enum V8.V8CacheKind
    None,
    Parser,
    Code
};

//-----------------------------------------------------------------------------
// V8CacheResult
//-----------------------------------------------------------------------------

enum class V8CacheResult : int32_t
{
    // IMPORTANT: maintain bitwise equivalence with managed enum V8.V8CacheResult
    Disabled,
    Accepted,
    Verified,
    Updated,
    UpdateFailed
};
