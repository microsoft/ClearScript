// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// HighResolutionClock implementation
//-----------------------------------------------------------------------------

double HighResolutionClock::SleepMilliseconds(double delay, bool precise)
{
    auto start = std::chrono::high_resolution_clock::now();
    auto end = start + std::chrono::duration<double, std::milli>(delay);
    std::chrono::high_resolution_clock::time_point final;

    if (!precise)
    {
        std::this_thread::sleep_for(std::chrono::duration<double, std::milli>(delay));
        final = std::chrono::high_resolution_clock::now();
    }
    else
    {
        while ((final = std::chrono::high_resolution_clock::now()) < end) std::this_thread::yield();
    }

    return std::chrono::duration<double, std::milli>(final - end).count();
}

//-----------------------------------------------------------------------------

double HighResolutionClock::GetMillisecondsSinceUnixEpoch()
{
    return std::chrono::duration<double, std::milli>(std::chrono::system_clock::now().time_since_epoch()).count();
}

//-----------------------------------------------------------------------------

double HighResolutionClock::GetRelativeMilliseconds()
{
    return std::chrono::duration<double, std::milli>(std::chrono::high_resolution_clock::now().time_since_epoch()).count();
}

//-----------------------------------------------------------------------------

double HighResolutionClock::GetRelativeSeconds()
{
    return std::chrono::duration<double>(std::chrono::high_resolution_clock::now().time_since_epoch()).count();
}
