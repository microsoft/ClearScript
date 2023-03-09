// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8ObjectHelpers
//-----------------------------------------------------------------------------

struct V8ObjectHelpers final: StaticBase
{
    static V8Value GetProperty(const SharedPtr<V8ObjectHolder>& spHolder, const StdString& name);
    static bool TryGetProperty(const SharedPtr<V8ObjectHolder>& spHolder, const StdString& name, V8Value& value);
    static void SetProperty(const SharedPtr<V8ObjectHolder>& spHolder, const StdString& name, const V8Value& value);
    static bool DeleteProperty(const SharedPtr<V8ObjectHolder>& spHolder, const StdString& name);
    static void GetPropertyNames(const SharedPtr<V8ObjectHolder>& spHolder, bool includeIndices, std::vector<StdString>& names);

    static V8Value GetProperty(const SharedPtr<V8ObjectHolder>& spHolder, int index);
    static void SetProperty(const SharedPtr<V8ObjectHolder>& spHolder, int index, const V8Value& value);
    static bool DeleteProperty(const SharedPtr<V8ObjectHolder>& spHolder, int index);
    static void GetPropertyIndices(const SharedPtr<V8ObjectHolder>& spHolder, std::vector<int>& indices);

    static V8Value Invoke(const SharedPtr<V8ObjectHolder>& spHolder, bool asConstructor, const std::vector<V8Value>& args);
    static V8Value InvokeMethod(const SharedPtr<V8ObjectHolder>& spHolder, const StdString& name, const std::vector<V8Value>& args);

    typedef void ArrayBufferOrViewDataCallback(void* pvData, void* pvArg);
    static void GetArrayBufferOrViewInfo(const SharedPtr<V8ObjectHolder>& spHolder, V8Value& arrayBuffer, size_t& offset, size_t& size, size_t& length);
    static void InvokeWithArrayBufferOrViewData(const SharedPtr<V8ObjectHolder>& spHolder, ArrayBufferOrViewDataCallback* pCallback, void* pvArg);
};
