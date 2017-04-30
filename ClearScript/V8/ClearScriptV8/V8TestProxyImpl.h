// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8TestProxyImpl
    //-------------------------------------------------------------------------

    private ref class V8TestProxyImpl : V8TestProxy
    {
    public:

        virtual V8ProxyCounters^ GetCounters() override;

        ~V8TestProxyImpl() {}
    };

}}}
