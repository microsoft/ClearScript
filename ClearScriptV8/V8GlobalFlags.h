// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8GlobalFlags
//-----------------------------------------------------------------------------

enum class V8GlobalFlags : uint32_t
{
    // IMPORTANT: maintain bitwise equivalence with managed enum V8.V8GlobalFlags
    None = 0,
    EnableTopLevelAwait = 0x00000001,
    DisableJITCompilation = 0x00000002
};
