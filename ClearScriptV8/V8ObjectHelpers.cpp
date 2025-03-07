// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// V8ObjectHelpers implementation
//-----------------------------------------------------------------------------

V8Value V8ObjectHelpers::GetProperty(const SharedPtr<V8ObjectHolder>& spHolder, const StdString& name)
{
    return spHolder.DerefAs<V8ObjectHolderImpl>().GetProperty(name);
}

//-----------------------------------------------------------------------------

bool V8ObjectHelpers::TryGetProperty(const SharedPtr<V8ObjectHolder>& spHolder, const StdString& name, V8Value& value)
{
    return spHolder.DerefAs<V8ObjectHolderImpl>().TryGetProperty(name, value);
}

//-----------------------------------------------------------------------------

void V8ObjectHelpers::SetProperty(const SharedPtr<V8ObjectHolder>& spHolder, const StdString& name, const V8Value& value)
{
    spHolder.DerefAs<V8ObjectHolderImpl>().SetProperty(name, value);
}

//-----------------------------------------------------------------------------

bool V8ObjectHelpers::DeleteProperty(const SharedPtr<V8ObjectHolder>& spHolder, const StdString& name)
{
    return spHolder.DerefAs<V8ObjectHolderImpl>().DeleteProperty(name);
}

//-----------------------------------------------------------------------------

void V8ObjectHelpers::GetPropertyNames(const SharedPtr<V8ObjectHolder>& spHolder, bool includeIndices, std::vector<StdString>& names)
{
    spHolder.DerefAs<V8ObjectHolderImpl>().GetPropertyNames(includeIndices, names);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHelpers::GetProperty(const SharedPtr<V8ObjectHolder>& spHolder, int index)
{
    return spHolder.DerefAs<V8ObjectHolderImpl>().GetProperty(index);
}

//-----------------------------------------------------------------------------

void V8ObjectHelpers::SetProperty(const SharedPtr<V8ObjectHolder>& spHolder, int index, const V8Value& value)
{
    spHolder.DerefAs<V8ObjectHolderImpl>().SetProperty(index, value);
}

//-----------------------------------------------------------------------------

bool V8ObjectHelpers::DeleteProperty(const SharedPtr<V8ObjectHolder>& spHolder, int index)
{
    return spHolder.DerefAs<V8ObjectHolderImpl>().DeleteProperty(index);
}

//-----------------------------------------------------------------------------

void V8ObjectHelpers::GetPropertyIndices(const SharedPtr<V8ObjectHolder>& spHolder, std::vector<int>& indices)
{
    spHolder.DerefAs<V8ObjectHolderImpl>().GetPropertyIndices(indices);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHelpers::Invoke(const SharedPtr<V8ObjectHolder>& spHolder, bool asConstructor, const std::vector<V8Value>& args)
{
    return spHolder.DerefAs<V8ObjectHolderImpl>().Invoke(asConstructor, args);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHelpers::InvokeMethod(const SharedPtr<V8ObjectHolder>& spHolder, const StdString& name, const std::vector<V8Value>& args)
{
    return spHolder.DerefAs<V8ObjectHolderImpl>().InvokeMethod(name, args);
}

//-----------------------------------------------------------------------------

void V8ObjectHelpers::GetArrayBufferOrViewInfo(const SharedPtr<V8ObjectHolder>& spHolder, V8Value& arrayBuffer, size_t& offset, size_t& size, size_t& length)
{
    spHolder.DerefAs<V8ObjectHolderImpl>().GetArrayBufferOrViewInfo(arrayBuffer, offset, size, length);
}

//-----------------------------------------------------------------------------

void V8ObjectHelpers::InvokeWithArrayBufferOrViewData(const SharedPtr<V8ObjectHolder>& spHolder, ArrayBufferOrViewDataCallback* pCallback, void* pvArg)
{
    spHolder.DerefAs<V8ObjectHolderImpl>().InvokeWithArrayBufferOrViewData(pCallback, pvArg);
}
