// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // IV8EntityProxy
    //-------------------------------------------------------------------------

    private interface class IV8EntityProxy
    {
        String^ CreateManagedString(v8::Local<v8::Value> hValue);
    };

}}}
