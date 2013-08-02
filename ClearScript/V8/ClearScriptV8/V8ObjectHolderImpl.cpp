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

V8ObjectHolderImpl::V8ObjectHolderImpl(V8ContextImpl* pContextImpl, void* pvObject):
    m_spContextImpl(pContextImpl),
    m_pvObject(pvObject)
{
}

//-----------------------------------------------------------------------------

V8ObjectHolderImpl::V8ObjectHolderImpl(const SharedPtr<V8ContextImpl>& spContextImpl, void* pvObject):
    m_spContextImpl(spContextImpl),
    m_pvObject(pvObject)
{
}

//-----------------------------------------------------------------------------

V8ObjectHolderImpl* V8ObjectHolderImpl::Clone() const
{
    return new V8ObjectHolderImpl(m_spContextImpl, m_spContextImpl->AddRefV8Object(m_pvObject));
}

//-----------------------------------------------------------------------------

void* V8ObjectHolderImpl::GetObject() const
{
    return m_pvObject;
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHolderImpl::GetProperty(const wchar_t* pName) const
{
    return m_spContextImpl->GetV8ObjectProperty(m_pvObject, pName);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::SetProperty(const wchar_t* pName, const V8Value& value) const
{
    m_spContextImpl->SetV8ObjectProperty(m_pvObject, pName, value);
}

//-----------------------------------------------------------------------------

bool V8ObjectHolderImpl::DeleteProperty(const wchar_t* pName) const
{
    return m_spContextImpl->DeleteV8ObjectProperty(m_pvObject, pName);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::GetPropertyNames(vector<wstring>& names) const
{
    m_spContextImpl->GetV8ObjectPropertyNames(m_pvObject, names);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHolderImpl::GetProperty(int index) const
{
    return m_spContextImpl->GetV8ObjectProperty(m_pvObject, index);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::SetProperty(int index, const V8Value& value) const
{
    m_spContextImpl->SetV8ObjectProperty(m_pvObject, index, value);
}

//-----------------------------------------------------------------------------

bool V8ObjectHolderImpl::DeleteProperty(int index) const
{
    return m_spContextImpl->DeleteV8ObjectProperty(m_pvObject, index);
}

//-----------------------------------------------------------------------------

void V8ObjectHolderImpl::GetPropertyIndices(vector<int>& indices) const
{
    m_spContextImpl->GetV8ObjectPropertyIndices(m_pvObject, indices);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHolderImpl::Invoke(const vector<V8Value>& args, bool asConstructor) const
{
    return m_spContextImpl->InvokeV8Object(m_pvObject, args, asConstructor);
}

//-----------------------------------------------------------------------------

V8Value V8ObjectHolderImpl::InvokeMethod(const wchar_t* pName, const vector<V8Value>& args) const
{
    return m_spContextImpl->InvokeV8ObjectMethod(m_pvObject, pName, args);
}

//-----------------------------------------------------------------------------

V8ObjectHolderImpl::~V8ObjectHolderImpl()
{
    m_spContextImpl->ReleaseV8Object(m_pvObject);
}
