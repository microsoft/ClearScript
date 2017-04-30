// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"
#include <windows.h>

//-----------------------------------------------------------------------------
// HighResolutionClock implementation
//-----------------------------------------------------------------------------

static OnceFlag s_InitializationFlag;
static LARGE_INTEGER s_TicksPerSecond;

//-----------------------------------------------------------------------------

double HighResolutionClock::GetRelativeSeconds()
{
    s_InitializationFlag.CallOnce([]
    {
        ASSERT_EVAL(::QueryPerformanceFrequency(&s_TicksPerSecond));
    });

    LARGE_INTEGER tickCount;
    ASSERT_EVAL(::QueryPerformanceCounter(&tickCount));

    auto wholeSeconds = tickCount.QuadPart / s_TicksPerSecond.QuadPart;
    auto remainingTicks = tickCount.QuadPart % s_TicksPerSecond.QuadPart;

    return wholeSeconds + (static_cast<double>(remainingTicks) / s_TicksPerSecond.QuadPart);
}

//-----------------------------------------------------------------------------

size_t HighResolutionClock::GetHardwareConcurrency()
{
    SYSTEM_INFO info;
    ::GetNativeSystemInfo(&info);
    return info.dwNumberOfProcessors;
}
