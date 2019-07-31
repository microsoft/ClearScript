// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8ScriptImpl
    //-------------------------------------------------------------------------

    private ref class V8ScriptImpl sealed : V8Script
    {
    public:

        V8ScriptImpl(ClearScript::UniqueDocumentInfo^ documentInfo, V8ScriptHolder* pHolder);

        SharedPtr<V8ScriptHolder> GetHolder();

        ~V8ScriptImpl();
        !V8ScriptImpl();

    private:

        Object^ m_gcLock;
        SharedPtr<V8ScriptHolder>* m_pspHolder;
    };

}}}
