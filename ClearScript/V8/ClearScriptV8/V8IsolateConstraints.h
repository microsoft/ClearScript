// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8IsolateConstraints
//-----------------------------------------------------------------------------

class V8IsolateConstraints
{
public:

    V8IsolateConstraints()
    {
    }

    void Set(int maxNewSpaceSize, int maxOldSpaceSize, int maxExecutableSize)
    {
        m_MaxNewSpaceSize = maxNewSpaceSize;
        m_MaxOldSpaceSize = maxOldSpaceSize;
        m_MaxExecutableSize = maxExecutableSize;
    }

    int GetMaxNewSpaceSize() const
    {
        return m_MaxNewSpaceSize;
    }

    int GetMaxOldSpaceSize() const
    {
        return m_MaxOldSpaceSize;
    }

    int GetMaxExecutableSize() const
    {
        return m_MaxExecutableSize;
    }

private:

    int m_MaxNewSpaceSize;
    int m_MaxOldSpaceSize;
    int m_MaxExecutableSize;
};
