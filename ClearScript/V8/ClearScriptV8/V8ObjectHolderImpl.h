// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8ObjectHolderImpl
//-----------------------------------------------------------------------------

class V8ObjectHolderImpl final: public V8ObjectHolder
{
    PROHIBIT_COPY(V8ObjectHolderImpl)

public:

    V8ObjectHolderImpl(V8WeakContextBinding* pBinding, void* pvObject);

    virtual V8ObjectHolderImpl* Clone() const override;
    virtual void* GetObject() const override;

    V8Value GetProperty(const StdString& name) const;
    void SetProperty(const StdString& name, const V8Value& value) const;
    bool DeleteProperty(const StdString& name) const;
    void GetPropertyNames(std::vector<StdString>& names) const;

    V8Value GetProperty(int index) const;
    void SetProperty(int index, const V8Value& value) const;
    bool DeleteProperty(int index) const;
    void GetPropertyIndices(std::vector<int>& indices) const;

    V8Value Invoke(bool asConstructor, const std::vector<V8Value>& args) const;
    V8Value InvokeMethod(const StdString& name, const std::vector<V8Value>& args) const;

    void GetArrayBufferOrViewInfo(V8Value& arrayBuffer, size_t& offset, size_t& size, size_t& length) const;
    void InvokeWithArrayBufferOrViewData(V8ObjectHelpers::ArrayBufferOrViewDataCallbackT* pCallback, void* pvArg) const;

    ~V8ObjectHolderImpl();

private:

    SharedPtr<V8WeakContextBinding> m_spBinding;
    void* m_pvObject;
};
