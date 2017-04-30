// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Managed.h"

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8TestProxyImpl implementation
    //-------------------------------------------------------------------------

    V8ProxyCounters^ V8TestProxyImpl::GetCounters()
    {
        auto gcCounters = gcnew V8ProxyCounters();
        gcCounters->IsolateCount = V8Isolate::GetInstanceCount();
        gcCounters->ContextCount = V8Context::GetInstanceCount();
        return gcCounters;
    }

    //-------------------------------------------------------------------------

    ENSURE_INTERNAL_CLASS(V8TestProxyImpl)

}}}
