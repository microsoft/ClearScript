// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// V8ObjectHolderImpl implementation
//-----------------------------------------------------------------------------

V8ObjectHolderImpl::V8ObjectHolderImpl(V8WeakContextBinding* pBinding, void* pvObject):
    m_spBinding(pBinding),
    m_pvObject(pvObject)
{
}

//-----------------------------------------------------------------------------

V8ObjectHolderImpl* V8ObjectHolderImpl::Clone() const
{
    return new V8ObjectHolderImpl(m_spBinding, m_spBinding->GetIsolateImpl()->AddRefV8Object(m_pvObject));
}

//-----------------------------------------------------------------------------

void* V8ObjectHolderImpl::GetObject() const
{
    return m_pvObject;
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHolderImpl::GetProperty(const StdString& name) const
{
    return m_spBinding->GetContextImpl()->GetV8ObjectProperty(m_pvObject, name);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::SetProperty(const StdString& name, const V8Value& value) const
{
    m_spBinding->GetContextImpl()->SetV8ObjectProperty(m_pvObject, name, value);
}

//-----------------------------------------------------------------------------

bool V8ObjectHolderImpl::DeleteProperty(const StdString& name) const
{
    return m_spBinding->GetContextImpl()->DeleteV8ObjectProperty(m_pvObject, name);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::GetPropertyNames(std::vector<StdString>& names) const
{
    m_spBinding->GetContextImpl()->GetV8ObjectPropertyNames(m_pvObject, names);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHolderImpl::GetProperty(int index) const
{
    return m_spBinding->GetContextImpl()->GetV8ObjectProperty(m_pvObject, index);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::SetProperty(int index, const V8Value& value) const
{
    m_spBinding->GetContextImpl()->SetV8ObjectProperty(m_pvObject, index, value);
}

//-----------------------------------------------------------------------------

bool V8ObjectHolderImpl::DeleteProperty(int index) const
{
    return m_spBinding->GetContextImpl()->DeleteV8ObjectProperty(m_pvObject, index);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::GetPropertyIndices(std::vector<int>& indices) const
{
    m_spBinding->GetContextImpl()->GetV8ObjectPropertyIndices(m_pvObject, indices);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHolderImpl::Invoke(bool asConstructor, const std::vector<V8Value>& args) const
{
    return m_spBinding->GetContextImpl()->InvokeV8Object(m_pvObject, asConstructor, args);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHolderImpl::InvokeMethod(const StdString& name, const std::vector<V8Value>& args) const
{
    return m_spBinding->GetContextImpl()->InvokeV8ObjectMethod(m_pvObject, name, args);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::GetArrayBufferOrViewInfo(V8Value& arrayBuffer, size_t& offset, size_t& size, size_t& length) const
{
    m_spBinding->GetContextImpl()->GetV8ObjectArrayBufferOrViewInfo(m_pvObject, arrayBuffer, offset, size, length);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::InvokeWithArrayBufferOrViewData(V8ObjectHelpers::ArrayBufferOrViewDataCallbackT* pCallback, void* pvArg) const
{
    m_spBinding->GetContextImpl()->InvokeWithV8ObjectArrayBufferOrViewData(m_pvObject, pCallback, pvArg);
}

//-----------------------------------------------------------------------------

V8ObjectHolderImpl::~V8ObjectHolderImpl()
{
    SharedPtr<V8IsolateImpl> spIsolateImpl;
    if (m_spBinding->TryGetIsolateImpl(spIsolateImpl))
    {
        spIsolateImpl->ReleaseV8Object(m_pvObject);
    }
}
