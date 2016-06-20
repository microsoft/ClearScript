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

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // local helper functions
    //-------------------------------------------------------------------------

    static void ArrayBufferOrViewDataCallback(void* pvData, void* pvArg)
    {
        (*static_cast<Action<IntPtr>^*>(pvArg))(IntPtr(pvData));
    }

    //-------------------------------------------------------------------------
    // V8ObjectImpl implementation
    //-------------------------------------------------------------------------

    V8ObjectImpl::V8ObjectImpl(V8ObjectHolder* pHolder, V8Value::Subtype subtype):
        m_gcLock(gcnew Object),
        m_pspHolder(new SharedPtr<V8ObjectHolder>(pHolder)),
        m_Subtype(subtype)
    {
    }

    //-------------------------------------------------------------------------

    Object^ V8ObjectImpl::GetProperty(String^ gcName)
    {
        try
        {
            return V8ContextProxyImpl::ExportValue(V8ObjectHelpers::GetProperty(GetHolder(), StdString(gcName)));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    Object^ V8ObjectImpl::GetProperty(String^ gcName, [Out] Boolean% isCacheable)
    {
        isCacheable = false;
        return GetProperty(gcName);
    }

    //-------------------------------------------------------------------------

    void V8ObjectImpl::SetProperty(String^ gcName, Object^ gcValue)
    {
        try
        {
            V8ObjectHelpers::SetProperty(GetHolder(), StdString(gcName), V8ContextProxyImpl::ImportValue(gcValue));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    bool V8ObjectImpl::DeleteProperty(String^ gcName)
    {
        try
        {
            return V8ObjectHelpers::DeleteProperty(GetHolder(), StdString(gcName));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    array<String^>^ V8ObjectImpl::GetPropertyNames()
    {
        try
        {
            std::vector<StdString> names;
            V8ObjectHelpers::GetPropertyNames(GetHolder(), names);
            auto nameCount = static_cast<int>(names.size());

            auto gcNames = gcnew array<String^>(nameCount);
            for (auto index = 0; index < nameCount; index++)
            {
                gcNames[index] = names[index].ToManagedString();
            }

            return gcNames;
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    Object^ V8ObjectImpl::GetProperty(int index)
    {
        try
        {
            return V8ContextProxyImpl::ExportValue(V8ObjectHelpers::GetProperty(GetHolder(), index));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    void V8ObjectImpl::SetProperty(int index, Object^ gcValue)
    {
        try
        {
            V8ObjectHelpers::SetProperty(GetHolder(), index, V8ContextProxyImpl::ImportValue(gcValue));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    bool V8ObjectImpl::DeleteProperty(int index)
    {
        try
        {
            return V8ObjectHelpers::DeleteProperty(GetHolder(), index);
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    array<int>^ V8ObjectImpl::GetPropertyIndices()
    {
        try
        {
            std::vector<int> indices;
            V8ObjectHelpers::GetPropertyIndices(GetHolder(), indices);
            auto indexCount = static_cast<int>(indices.size());

            auto gcIndices = gcnew array<int>(indexCount);
            for (auto index = 0; index < indexCount; index++)
            {
                gcIndices[index] = indices[index];
            }

            return gcIndices;
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    Object^ V8ObjectImpl::Invoke(array<Object^>^ gcArgs, bool asConstructor)
    {
        try
        {
            std::vector<V8Value> importedArgs;
            ImportValues(gcArgs, importedArgs);

            return V8ContextProxyImpl::ExportValue(V8ObjectHelpers::Invoke(GetHolder(), importedArgs, asConstructor));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    Object^ V8ObjectImpl::InvokeMethod(String^ gcName, array<Object^>^ gcArgs)
    {
        try
        {
            std::vector<V8Value> importedArgs;
            ImportValues(gcArgs, importedArgs);

            return V8ContextProxyImpl::ExportValue(V8ObjectHelpers::InvokeMethod(GetHolder(), StdString(gcName), importedArgs));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    bool V8ObjectImpl::IsArrayBufferOrView()
    {
        return m_Subtype != V8Value::Subtype::None;
    }

    //-------------------------------------------------------------------------

    V8ArrayBufferOrViewKind V8ObjectImpl::GetArrayBufferOrViewKind()
    {
        auto kind = V8ArrayBufferOrViewKind::None;

        if (m_Subtype == V8Value::Subtype::ArrayBuffer)
            kind = V8ArrayBufferOrViewKind::ArrayBuffer;
        else if (m_Subtype == V8Value::Subtype::DataView)
            kind = V8ArrayBufferOrViewKind::DataView;
        else if (m_Subtype == V8Value::Subtype::Uint8Array)
            kind = V8ArrayBufferOrViewKind::Uint8Array;
        else if (m_Subtype == V8Value::Subtype::Uint8ClampedArray)
            kind = V8ArrayBufferOrViewKind::Uint8ClampedArray;
        else if (m_Subtype == V8Value::Subtype::Int8Array)
            kind = V8ArrayBufferOrViewKind::Int8Array;
        else if (m_Subtype == V8Value::Subtype::Uint16Array)
            kind = V8ArrayBufferOrViewKind::Uint16Array;
        else if (m_Subtype == V8Value::Subtype::Int16Array)
            kind = V8ArrayBufferOrViewKind::Int16Array;
        else if (m_Subtype == V8Value::Subtype::Uint32Array)
            kind = V8ArrayBufferOrViewKind::Uint32Array;
        else if (m_Subtype == V8Value::Subtype::Int32Array)
            kind = V8ArrayBufferOrViewKind::Int32Array;
        else if (m_Subtype == V8Value::Subtype::Float32Array)
            kind = V8ArrayBufferOrViewKind::Float32Array;
        else if (m_Subtype == V8Value::Subtype::Float64Array)
            kind = V8ArrayBufferOrViewKind::Float64Array;

        return kind;
    }
    
    //-------------------------------------------------------------------------

    V8ArrayBufferOrViewInfo^ V8ObjectImpl::GetArrayBufferOrViewInfo()
    {
        try
        {
            auto kind = GetArrayBufferOrViewKind();
            if (kind != V8ArrayBufferOrViewKind::None)
            {
                V8Value arrayBuffer(V8Value::Null);
                size_t offset;
                size_t size;
                size_t length;

                V8ObjectHelpers::GetArrayBufferOrViewInfo(GetHolder(), arrayBuffer, offset, size, length);
                return gcnew V8ArrayBufferOrViewInfo(kind, (IV8Object^)V8ContextProxyImpl::ExportValue(arrayBuffer), offset, size, length);
            }

            return nullptr;
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    void V8ObjectImpl::InvokeWithArrayBufferOrViewData(Action<IntPtr>^ gcAction)
    {
        try
        {
            V8ObjectHelpers::InvokeWithArrayBufferOrViewData(GetHolder(), ArrayBufferOrViewDataCallback, &gcAction);
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    SharedPtr<V8ObjectHolder> V8ObjectImpl::GetHolder()
    {
        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pspHolder == nullptr)
            {
                throw gcnew ObjectDisposedException(ToString());
            }

            return *m_pspHolder;

        END_LOCK_SCOPE
    }

    //-------------------------------------------------------------------------

    V8Value::Subtype V8ObjectImpl::GetSubtype()
    {
        return m_Subtype;
    }

    //-------------------------------------------------------------------------

    V8ObjectImpl::~V8ObjectImpl()
    {
        SharedPtr<V8ObjectHolder> spHolder;

        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pspHolder != nullptr)
            {
                // hold V8 object holder for destruction outside lock scope
                spHolder = *m_pspHolder;
                delete m_pspHolder;
                m_pspHolder = nullptr;
            }

        END_LOCK_SCOPE

        if (!spHolder.IsEmpty())
        {
            GC::SuppressFinalize(this);
        }
    }

    //-------------------------------------------------------------------------

    V8ObjectImpl::!V8ObjectImpl()
    {
        if (m_pspHolder != nullptr)
        {
            delete m_pspHolder;
            m_pspHolder = nullptr;
        }
    }

    //-------------------------------------------------------------------------

    void V8ObjectImpl::ImportValues(array<Object^>^ gcValues, std::vector<V8Value>& importedValues)
    {
        importedValues.clear();
        if (gcValues != nullptr)
        {
            auto valueCount = gcValues->Length;
            importedValues.reserve(valueCount);

            for (auto index = 0; index < valueCount; index++)
            {
                importedValues.push_back(V8ContextProxyImpl::ImportValue(gcValues[index]));
            }
        }
    }

}}}
