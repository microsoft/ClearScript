// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8ObjectHelpers
//-----------------------------------------------------------------------------

class V8ObjectHelpers final
{
    PROHIBIT_CONSTRUCT(V8ObjectHelpers)

public:

    static V8Value GetProperty(V8ObjectHolder* pHolder, const StdString& name);
    static void SetProperty(V8ObjectHolder* pHolder, const StdString& name, const V8Value& value);
    static bool DeleteProperty(V8ObjectHolder* pHolder, const StdString& name);
    static void GetPropertyNames(V8ObjectHolder* pHolder, std::vector<StdString>& names);

    static V8Value GetProperty(V8ObjectHolder* pHolder, int index);
    static void SetProperty(V8ObjectHolder* pHolder, int index, const V8Value& value);
    static bool DeleteProperty(V8ObjectHolder* pHolder, int index);
    static void GetPropertyIndices(V8ObjectHolder* pHolder, std::vector<int>& indices);

    static V8Value Invoke(V8ObjectHolder* pHolder, bool asConstructor, const std::vector<V8Value>& args);
    static V8Value InvokeMethod(V8ObjectHolder* pHolder, const StdString& name, const std::vector<V8Value>& args);

    typedef void ArrayBufferOrViewDataCallbackT(void* pvData, void* pvArg);
    static void GetArrayBufferOrViewInfo(V8ObjectHolder* pHolder, V8Value& arrayBuffer, size_t& offset, size_t& size, size_t& length);
    static void InvokeWithArrayBufferOrViewData(V8ObjectHolder* pHolder, ArrayBufferOrViewDataCallbackT* pCallback, void* pvArg);
};
