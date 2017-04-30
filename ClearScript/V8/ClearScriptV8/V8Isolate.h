// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8Isolate
//-----------------------------------------------------------------------------

class V8Isolate: public WeakRefTarget<V8Isolate>
{
public:

    static V8Isolate* Create(const StdString& name, const V8IsolateConstraints* pConstraints, bool enableDebugging, int debugPort);
    static size_t GetInstanceCount();

    virtual size_t GetMaxHeapSize() = 0;
    virtual void SetMaxHeapSize(size_t value) = 0;
    virtual double GetHeapSizeSampleInterval() = 0;
    virtual void SetHeapSizeSampleInterval(double value) = 0;

    virtual size_t GetMaxStackUsage() = 0;
    virtual void SetMaxStackUsage(size_t value) = 0;

    virtual V8ScriptHolder* Compile(const StdString& documentName, const StdString& code) = 0;
    virtual V8ScriptHolder* Compile(const StdString& documentName, const StdString& code, V8CacheType cacheType, std::vector<std::uint8_t>& cacheBytes) = 0;
    virtual V8ScriptHolder* Compile(const StdString& documentName, const StdString& code, V8CacheType cacheType, const std::vector<std::uint8_t>& cacheBytes, bool& cacheAccepted) = 0;
    virtual void GetHeapInfo(V8IsolateHeapInfo& heapInfo) = 0;
    virtual void CollectGarbage(bool exhaustive) = 0;

    virtual ~V8Isolate() {}
};
