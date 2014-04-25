// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// V8ObjectHolderImpl implementation
//-----------------------------------------------------------------------------

V8ObjectHolderImpl::V8ObjectHolderImpl(V8WeakContextBinding* pBinding, void* pvObject):
    m_spBinding(pBinding),
    m_pvObject(pvObject)
{
}

//-----------------------------------------------------------------------------

V8ObjectHolderImpl* V8ObjectHolderImpl::Clone() const
{
    return new V8ObjectHolderImpl(m_spBinding, m_spBinding->GetIsolateImpl()->AddRefV8Object(m_pvObject));
}

//-----------------------------------------------------------------------------

void* V8ObjectHolderImpl::GetObject() const
{
    return m_pvObject;
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHolderImpl::GetProperty(const StdString& name) const
{
    return m_spBinding->GetContextImpl()->GetV8ObjectProperty(m_pvObject, name);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::SetProperty(const StdString& name, const V8Value& value) const
{
    m_spBinding->GetContextImpl()->SetV8ObjectProperty(m_pvObject, name, value);
}

//-----------------------------------------------------------------------------

bool V8ObjectHolderImpl::DeleteProperty(const StdString& name) const
{
    return m_spBinding->GetContextImpl()->DeleteV8ObjectProperty(m_pvObject, name);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::GetPropertyNames(std::vector<StdString>& names) const
{
    m_spBinding->GetContextImpl()->GetV8ObjectPropertyNames(m_pvObject, names);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHolderImpl::GetProperty(int index) const
{
    return m_spBinding->GetContextImpl()->GetV8ObjectProperty(m_pvObject, index);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::SetProperty(int index, const V8Value& value) const
{
    m_spBinding->GetContextImpl()->SetV8ObjectProperty(m_pvObject, index, value);
}

//-----------------------------------------------------------------------------

bool V8ObjectHolderImpl::DeleteProperty(int index) const
{
    return m_spBinding->GetContextImpl()->DeleteV8ObjectProperty(m_pvObject, index);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::GetPropertyIndices(std::vector<int>& indices) const
{
    m_spBinding->GetContextImpl()->GetV8ObjectPropertyIndices(m_pvObject, indices);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHolderImpl::Invoke(const std::vector<V8Value>& args, bool asConstructor) const
{
    return m_spBinding->GetContextImpl()->InvokeV8Object(m_pvObject, args, asConstructor);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHolderImpl::InvokeMethod(const StdString& name, const std::vector<V8Value>& args) const
{
    return m_spBinding->GetContextImpl()->InvokeV8ObjectMethod(m_pvObject, name, args);
}

//-----------------------------------------------------------------------------

V8ObjectHolderImpl::~V8ObjectHolderImpl()
{
    SharedPtr<V8IsolateImpl> spIsolateImpl;
    if (m_spBinding->TryGetIsolateImpl(spIsolateImpl))
    {
        spIsolateImpl->ReleaseV8Object(m_pvObject);
    }
}
