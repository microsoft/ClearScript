// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Managed.h"

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8TestProxyImpl implementation
    //-------------------------------------------------------------------------

    UIntPtr V8TestProxyImpl::GetNativeDigest(String^ gcValue)
    {
        StdString value(gcValue);
        return (UIntPtr)value.GetDigest();
    }

    //-------------------------------------------------------------------------

    V8TestProxy::Statistics^ V8TestProxyImpl::GetStatistics()
    {
        auto gcStatistics = gcnew Statistics;
        gcStatistics->IsolateCount = V8Isolate::GetInstanceCount();
        gcStatistics->ContextCount = V8Context::GetInstanceCount();
        return gcStatistics;
    }

    //-------------------------------------------------------------------------

    ENSURE_INTERNAL_CLASS(V8TestProxyImpl)

}}}
