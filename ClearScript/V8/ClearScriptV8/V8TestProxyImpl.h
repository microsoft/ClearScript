// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8TestProxyImpl
    //-------------------------------------------------------------------------

    private ref class V8TestProxyImpl sealed : V8TestProxy
    {
    public:

        virtual UIntPtr GetNativeDigest(String^ gcValue) override;

        virtual Statistics^ GetStatistics() override;

        ~V8TestProxyImpl() {}
    };

}}}
