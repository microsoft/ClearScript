// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// HighResolutionClock
//-----------------------------------------------------------------------------

struct HighResolutionClock final: StaticBase
{
    static double GetRelativeSeconds();
    static size_t GetHardwareConcurrency();
};
