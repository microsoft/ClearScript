// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// V8Isolate implementation
//-----------------------------------------------------------------------------

V8Isolate* V8Isolate::Create(const StdString& name, const v8::ResourceConstraints* pConstraints, const Options& options)
{
    return new V8IsolateImpl(name, pConstraints, options);
}

//-----------------------------------------------------------------------------

size_t V8Isolate::GetInstanceCount()
{
    return V8IsolateImpl::GetInstanceCount();
}
