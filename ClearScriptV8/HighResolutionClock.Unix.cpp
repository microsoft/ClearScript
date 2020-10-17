// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"
#include <unistd.h>

//-----------------------------------------------------------------------------
// HighResolutionClock implementation
//-----------------------------------------------------------------------------

double HighResolutionClock::GetRelativeSeconds()
{
    return std::chrono::duration<double>(std::chrono::high_resolution_clock::now().time_since_epoch()).count();
}

//-----------------------------------------------------------------------------

size_t HighResolutionClock::GetHardwareConcurrency()
{
    auto result = sysconf(_SC_NPROCESSORS_ONLN);
    if (result == -1)
    {
        return 1;
    }

    return static_cast<size_t>(result);
}
