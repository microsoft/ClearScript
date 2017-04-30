// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// V8Context implementation
//-----------------------------------------------------------------------------

V8Context* V8Context::Create(const SharedPtr<V8Isolate>& spIsolate, const StdString& name, bool enableDebugging, bool disableGlobalMembers, int debugPort)
{
    return new V8ContextImpl(static_cast<V8IsolateImpl*>(spIsolate.GetRawPtr()), name, enableDebugging, disableGlobalMembers, debugPort);
}

//-----------------------------------------------------------------------------

size_t V8Context::GetInstanceCount()
{
    return V8ContextImpl::GetInstanceCount();
}
