// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// HighResolutionClock
//-----------------------------------------------------------------------------

struct HighResolutionClock final: StaticBase
{
    static bool SetTimerResolution();
    static void RestoreTimerResolution();
    static void SleepMilliseconds(double delay, bool precise);
    static double GetMillisecondsSinceUnixEpoch();
    static double GetRelativeMilliseconds();
    static double GetRelativeSeconds();
    static size_t GetHardwareConcurrency();
};
