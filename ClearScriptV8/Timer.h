// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// Timer
//-----------------------------------------------------------------------------

class Timer final: public WeakRefTarget<Timer>
{
public:

    Timer(int dueTime, int period, std::function<void(Timer*)>&& func):
        m_DueTime(dueTime),
        m_Period(period),
        m_Func(std::move(func))
    {
        auto wrTimer = CreateWeakRef();
        HostObjectUtil::NativeCallback callback = [wrTimer] ()
        {
            auto spTimer = wrTimer.GetTarget();
            if (!spTimer.IsEmpty())
            {
                spTimer->CallFunc();
            }
        };

        m_pvTimer = HostObjectUtil::CreateNativeCallbackTimer(-1, -1, std::move(callback));
    }

    void Start()
    {
        HostObjectUtil::ChangeNativeCallbackTimer(m_pvTimer, m_DueTime, m_Period);
    }

    ~Timer()
    {
        HostObjectUtil::DestroyNativeCallbackTimer(m_pvTimer);
    }

private:

    void CallFunc()
    {
        m_Func(this);
    }

    int m_DueTime;
    int m_Period;
    std::function<void(Timer*)> m_Func;
    void* m_pvTimer;
};
