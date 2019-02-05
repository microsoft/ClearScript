// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// local helper functions
//-----------------------------------------------------------------------------

V8ObjectHolderImpl* GetHolderImpl(V8ObjectHolder* pHolder)
{
    return static_cast<V8ObjectHolderImpl*>(pHolder);
}

//-----------------------------------------------------------------------------
// V8ObjectHelpers implementation
//-----------------------------------------------------------------------------

V8Value V8ObjectHelpers::GetProperty(V8ObjectHolder* pHolder, const StdString& name)
{
    return GetHolderImpl(pHolder)->GetProperty(name);
}

//-----------------------------------------------------------------------------

void V8ObjectHelpers::SetProperty(V8ObjectHolder* pHolder, const StdString& name, const V8Value& value)
{
    GetHolderImpl(pHolder)->SetProperty(name, value);
}

//-----------------------------------------------------------------------------

bool V8ObjectHelpers::DeleteProperty(V8ObjectHolder* pHolder, const StdString& name)
{
    return GetHolderImpl(pHolder)->DeleteProperty(name);
}

//-----------------------------------------------------------------------------

void V8ObjectHelpers::GetPropertyNames(V8ObjectHolder* pHolder, std::vector<StdString>& names)
{
    GetHolderImpl(pHolder)->GetPropertyNames(names);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHelpers::GetProperty(V8ObjectHolder* pHolder, int index)
{
    return GetHolderImpl(pHolder)->GetProperty(index);
}

//-----------------------------------------------------------------------------

void V8ObjectHelpers::SetProperty(V8ObjectHolder* pHolder, int index, const V8Value& value)
{
    GetHolderImpl(pHolder)->SetProperty(index, value);
}

//-----------------------------------------------------------------------------

bool V8ObjectHelpers::DeleteProperty(V8ObjectHolder* pHolder, int index)
{
    return GetHolderImpl(pHolder)->DeleteProperty(index);
}

//-----------------------------------------------------------------------------

void V8ObjectHelpers::GetPropertyIndices(V8ObjectHolder* pHolder, std::vector<int>& indices)
{
    GetHolderImpl(pHolder)->GetPropertyIndices(indices);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHelpers::Invoke(V8ObjectHolder* pHolder, bool asConstructor, const std::vector<V8Value>& args)
{
    return GetHolderImpl(pHolder)->Invoke(asConstructor, args);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHelpers::InvokeMethod(V8ObjectHolder* pHolder, const StdString& name, const std::vector<V8Value>& args)
{
    return GetHolderImpl(pHolder)->InvokeMethod(name, args);
}

//-----------------------------------------------------------------------------

void V8ObjectHelpers::GetArrayBufferOrViewInfo(V8ObjectHolder* pHolder, V8Value& arrayBuffer, size_t& offset, size_t& size, size_t& length)
{
    return GetHolderImpl(pHolder)->GetArrayBufferOrViewInfo(arrayBuffer, offset, size, length);
}

//-----------------------------------------------------------------------------

void V8ObjectHelpers::InvokeWithArrayBufferOrViewData(V8ObjectHolder* pHolder, ArrayBufferOrViewDataCallbackT* pCallback, void* pvArg)
{
    return GetHolderImpl(pHolder)->InvokeWithArrayBufferOrViewData(pCallback, pvArg);
}
