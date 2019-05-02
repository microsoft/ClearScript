// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// HighResolutionClock
//-----------------------------------------------------------------------------

class HighResolutionClock final
{
    PROHIBIT_CONSTRUCT(HighResolutionClock)

public:

    static double GetRelativeSeconds();
    static size_t GetHardwareConcurrency();
};
