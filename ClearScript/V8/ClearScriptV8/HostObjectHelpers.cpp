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

#include "ClearScriptV8Managed.h"

using namespace Microsoft::ClearScript::V8;

//-----------------------------------------------------------------------------
// local helper functions
//-----------------------------------------------------------------------------

static void DECLSPEC_NORETURN ThrowHostException(Exception^ gcException)
{
    throw HostException(StdString(gcException->Message), V8ContextProxyImpl::ImportValue(gcException));
}

//-----------------------------------------------------------------------------
// HostObjectHelpers implementation
//-----------------------------------------------------------------------------

void* HostObjectHelpers::AddRef(void* pvObject)
{
    return V8ProxyHelpers::AddRefHostObject(pvObject);
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::Release(void* pvObject)
{
    V8ProxyHelpers::ReleaseHostObject(pvObject);
}

//-----------------------------------------------------------------------------

V8Value HostObjectHelpers::GetProperty(void* pvObject, const StdString& name)
{
    try
    {
        return V8ContextProxyImpl::ImportValue(V8ProxyHelpers::GetHostObjectProperty(pvObject, name.ToManagedString()));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(gcException);
    }
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::SetProperty(void* pvObject, const StdString& name, const V8Value& value)
{
    try
    {
        V8ProxyHelpers::SetHostObjectProperty(pvObject, name.ToManagedString(), V8ContextProxyImpl::ExportValue(value));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(gcException);
    }
}

//-----------------------------------------------------------------------------

bool HostObjectHelpers::DeleteProperty(void* pvObject, const StdString& name)
{
    try
    {
        return V8ProxyHelpers::DeleteHostObjectProperty(pvObject, name.ToManagedString());
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(gcException);
    }
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::GetPropertyNames(void* pvObject, std::vector<StdString>& names)
{
    try
    {
        auto gcNames = V8ProxyHelpers::GetHostObjectPropertyNames(pvObject);
        auto nameCount = gcNames->Length;

        names.resize(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            names[index] = StdString(gcNames[index]);
        }
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(gcException);
    }
}

//-----------------------------------------------------------------------------

V8Value HostObjectHelpers::GetProperty(void* pvObject, int index)
{
    try
    {
        return V8ContextProxyImpl::ImportValue(V8ProxyHelpers::GetHostObjectProperty(pvObject, index));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(gcException);
    }
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::SetProperty(void* pvObject, int index, const V8Value& value)
{
    try
    {
        V8ProxyHelpers::SetHostObjectProperty(pvObject, index, V8ContextProxyImpl::ExportValue(value));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(gcException);
    }
}

//-----------------------------------------------------------------------------

bool HostObjectHelpers::DeleteProperty(void* pvObject, int index)
{
    try
    {
        return V8ProxyHelpers::DeleteHostObjectProperty(pvObject, index);
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(gcException);
    }
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::GetPropertyIndices(void* pvObject, std::vector<int>& indices)
{
    try
    {
        auto gcIndices = V8ProxyHelpers::GetHostObjectPropertyIndices(pvObject);
        auto indexCount = gcIndices->Length;

        indices.resize(indexCount);
        for (auto index = 0; index < indexCount; index++)
        {
            indices[index] = gcIndices[index];
        }
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(gcException);
    }
}

//-----------------------------------------------------------------------------

V8Value HostObjectHelpers::Invoke(void* pvObject, const std::vector<V8Value>& args, bool asConstructor)
{
    try
    {
        auto argCount = static_cast<int>(args.size());

        auto exportedArgs = gcnew array<Object^>(argCount);
        for (auto index = 0; index < argCount; index++)
        {
            exportedArgs[index] = V8ContextProxyImpl::ExportValue(args[index]);
        }

        return V8ContextProxyImpl::ImportValue(V8ProxyHelpers::InvokeHostObject(pvObject, exportedArgs, asConstructor));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(gcException);
    }
}

//-----------------------------------------------------------------------------

V8Value HostObjectHelpers::InvokeMethod(void* pvObject, const StdString& name, const std::vector<V8Value>& args)
{
    try
    {
        auto argCount = static_cast<int>(args.size());

        auto exportedArgs = gcnew array<Object^>(argCount);
        for (auto index = 0; index < argCount; index++)
        {
            exportedArgs[index] = V8ContextProxyImpl::ExportValue(args[index]);
        }

        return V8ContextProxyImpl::ImportValue(V8ProxyHelpers::InvokeHostObjectMethod(pvObject, name.ToManagedString(), exportedArgs));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(gcException);
    }
}

//-----------------------------------------------------------------------------

void* HostObjectHelpers::CreateV8ObjectCache()
{
    return V8ProxyHelpers::CreateV8ObjectCache();
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::CacheV8Object(void* pvCache, void* pvObject, void* pvV8Object)
{
    V8ProxyHelpers::CacheV8Object(pvCache, pvObject, pvV8Object);
}

//-----------------------------------------------------------------------------

void* HostObjectHelpers::GetCachedV8Object(void* pvCache, void* pvObject)
{
    return V8ProxyHelpers::GetCachedV8Object(pvCache, pvObject);
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::GetAllCachedV8Objects(void* pvCache, std::vector<void*>& v8ObjectPtrs)
{
    auto gcV8ObjectPtrs = V8ProxyHelpers::GetAllCachedV8Objects(pvCache);
    auto v8ObjectCount = gcV8ObjectPtrs->Length;

    v8ObjectPtrs.resize(v8ObjectCount);
    for (auto index = 0; index < v8ObjectCount; index++)
    {
        v8ObjectPtrs[index] = gcV8ObjectPtrs[index].ToPointer();
    }
}

//-----------------------------------------------------------------------------

bool HostObjectHelpers::RemoveV8ObjectCacheEntry(void* pvCache, void* pvObject)
{
    return V8ProxyHelpers::RemoveV8ObjectCacheEntry(pvCache, pvObject);
}

//-----------------------------------------------------------------------------

bool HostObjectHelpers::TryParseInt32(const StdString& text, int& result)
{
    return Int32::TryParse(text.ToManagedString(), NumberStyles::Integer, CultureInfo::InvariantCulture, result);
}
