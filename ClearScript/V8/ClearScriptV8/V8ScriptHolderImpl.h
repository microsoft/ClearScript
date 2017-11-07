// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8ScriptHolderImpl
//-----------------------------------------------------------------------------

class V8ScriptHolderImpl: public V8ScriptHolder
{
    PROHIBIT_COPY(V8ScriptHolderImpl)

public:

    V8ScriptHolderImpl(V8WeakContextBinding* pBinding, void* pvScript);

    virtual V8ScriptHolderImpl* Clone() const override;
    virtual bool IsSameIsolate(void* pvIsolate) const override;
    virtual void* GetScript() const override;

    ~V8ScriptHolderImpl();

private:

    SharedPtr<V8WeakContextBinding> m_spBinding;
    void* m_pvScript;
};
