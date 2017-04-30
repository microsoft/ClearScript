// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8IsolateHeapInfo
//-----------------------------------------------------------------------------

class V8IsolateHeapInfo
{
public:

    V8IsolateHeapInfo()
    {
    }

    void Set(size_t totalHeapSize, size_t totalHeapSizeExecutable, size_t totalPhysicalSize, size_t usedHeapSize, size_t heapSizeLimit)
    {
        m_TotalHeapSize = totalHeapSize;
        m_TotalHeapSizeExecutable = totalHeapSizeExecutable;
        m_TotalPhysicalSize = totalPhysicalSize;
        m_UsedHeapSize = usedHeapSize;
        m_HeapSizeLimit = heapSizeLimit;
    }

    size_t GetTotalHeapSize() const
    {
        return m_TotalHeapSize;
    }

    size_t GetTotalHeapSizeExecutable() const
    {
        return m_TotalHeapSizeExecutable;
    }

    size_t GetTotalPhysicalSize() const
    {
        return m_TotalPhysicalSize;
    }

    size_t GetUsedHeapSize() const
    {
        return m_UsedHeapSize;
    }

    size_t GetHeapSizeLimit() const
    {
        return m_HeapSizeLimit;
    }

private:

    size_t m_TotalHeapSize;
    size_t m_TotalHeapSizeExecutable;
    size_t m_TotalPhysicalSize;
    size_t m_UsedHeapSize;
    size_t m_HeapSizeLimit;
};
