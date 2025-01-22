// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"
#include <windows.h>

//-----------------------------------------------------------------------------
// HighResolutionClock implementation
//-----------------------------------------------------------------------------

static TIMECAPS s_TimeCaps {};
static auto s_GotTimeCaps = false;
 
//-----------------------------------------------------------------------------

bool HighResolutionClock::SetTimerResolution()
{
    static OnceFlag s_InitializationFlag;
    s_InitializationFlag.CallOnce([]()
    {
        s_GotTimeCaps = ::timeGetDevCaps(&s_TimeCaps, sizeof s_TimeCaps) == MMSYSERR_NOERROR;
    });

    return s_GotTimeCaps && (::timeBeginPeriod(s_TimeCaps.wPeriodMin) == TIMERR_NOERROR);
}

//-----------------------------------------------------------------------------

void HighResolutionClock::RestoreTimerResolution()
{
    if (s_GotTimeCaps)
    {
        ASSERT_EVAL(::timeEndPeriod(s_TimeCaps.wPeriodMin) == TIMERR_NOERROR);
    }
}

//-----------------------------------------------------------------------------

size_t HighResolutionClock::GetHardwareConcurrency()
{
    SYSTEM_INFO info;
    ::GetNativeSystemInfo(&info);
    return info.dwNumberOfProcessors;
}
