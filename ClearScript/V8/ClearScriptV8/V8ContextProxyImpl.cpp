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

    static void InvokeAction(void* pvActionRef)
    {
        (*static_cast<Action^*>(pvActionRef))();
    }

    //-------------------------------------------------------------------------
    // V8ContextProxyImpl implementation
    //-------------------------------------------------------------------------

    V8ContextProxyImpl::V8ContextProxyImpl(V8IsolateProxy^ gcIsolateProxy, String^ gcName, Boolean enableDebugging, Boolean disableGlobalMembers, Int32 debugPort):
        m_gcLock(gcnew Object)
    {
        try
        {
            auto gcIsolateProxyImpl = dynamic_cast<V8IsolateProxyImpl^>(gcIsolateProxy);
            m_pspContext = new SharedPtr<V8Context>(V8Context::Create(gcIsolateProxyImpl->GetIsolate(), StdString(gcName), enableDebugging, disableGlobalMembers, debugPort));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    UIntPtr V8ContextProxyImpl::MaxRuntimeStackUsage::get()
    {
        return (UIntPtr)GetContext()->GetMaxIsolateStackUsage();
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::MaxRuntimeStackUsage::set(UIntPtr value)
    {
        GetContext()->SetMaxIsolateStackUsage(static_cast<size_t>(value));
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::InvokeWithLock(Action^ gcAction)
    {
        try
        {
            GetContext()->CallWithLock(InvokeAction, &gcAction);
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    Object^ V8ContextProxyImpl::GetRootItem()
    {
        try
        {
            return ExportValue(GetContext()->GetRootObject());
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::AddGlobalItem(String^ gcName, Object^ gcItem, Boolean globalMembers)
    {
        try
        {
            GetContext()->SetGlobalProperty(StdString(gcName), ImportValue(gcItem), globalMembers);
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    Object^ V8ContextProxyImpl::Execute(String^ gcDocumentName, String^ gcCode, Boolean evaluate, Boolean discard)
    {
        try
        {
            return ExportValue(GetContext()->Execute(StdString(gcDocumentName), StdString(gcCode), evaluate, discard));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    V8Script^ V8ContextProxyImpl::Compile(String^ gcDocumentName, String^ gcCode)
    {
        try
        {
            return gcnew V8ScriptImpl(gcDocumentName, GetContext()->Compile(StdString(gcDocumentName), StdString(gcCode)));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    Object^ V8ContextProxyImpl::Execute(V8Script^ gcScript, Boolean evaluate)
    {
        try
        {
            auto gcScriptImpl = dynamic_cast<V8ScriptImpl^>(gcScript);
            if (gcScriptImpl == nullptr)
            {
                throw gcnew ArgumentException(L"Invalid compiled script", L"script");
            }

            auto spContext = GetContext();
            auto spHolder = gcScriptImpl->GetHolder();
            if (!spContext->CanExecute(spHolder))
            {
                throw gcnew ArgumentException(L"Invalid compiled script", L"script");
            }

            return ExportValue(spContext->Execute(spHolder, evaluate));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::Interrupt()
    {
        GetContext()->Interrupt();
    }

    //-------------------------------------------------------------------------

    V8RuntimeHeapInfo^ V8ContextProxyImpl::GetRuntimeHeapInfo()
    {
        V8IsolateHeapInfo heapInfo;
        GetContext()->GetIsolateHeapInfo(heapInfo);

        auto gcHeapInfo = gcnew V8RuntimeHeapInfo();
        gcHeapInfo->TotalHeapSize = heapInfo.GetTotalHeapSize();
        gcHeapInfo->TotalHeapSizeExecutable = heapInfo.GetTotalHeapSizeExecutable();
        gcHeapInfo->TotalPhysicalSize = heapInfo.GetTotalPhysicalSize();
        gcHeapInfo->UsedHeapSize = heapInfo.GetUsedHeapSize();
        gcHeapInfo->HeapSizeLimit = heapInfo.GetHeapSizeLimit();
        return gcHeapInfo;
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::CollectGarbage(bool exhaustive)
    {
        GetContext()->CollectGarbage(exhaustive);
    }

    //-------------------------------------------------------------------------

    V8ContextProxyImpl::~V8ContextProxyImpl()
    {
        SharedPtr<V8Context> spContext;

        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pspContext != nullptr)
            {
                // hold V8 context for destruction outside lock scope
                spContext = *m_pspContext;
                delete m_pspContext;
                m_pspContext = nullptr;
            }

        END_LOCK_SCOPE
    }

    //-------------------------------------------------------------------------

    V8ContextProxyImpl::!V8ContextProxyImpl()
    {
        if (m_pspContext != nullptr)
        {
            delete m_pspContext;
            m_pspContext = nullptr;
        }
    }

    //-------------------------------------------------------------------------

    V8Value V8ContextProxyImpl::ImportValue(Object^ gcObject)
    {
        if (dynamic_cast<Nonexistent^>(gcObject) != nullptr)
        {
            return V8Value(V8Value::Nonexistent);
        }

        if (gcObject == nullptr)
        {
            return V8Value(V8Value::Undefined);
        }

        if (dynamic_cast<DBNull^>(gcObject) != nullptr)
        {
            return V8Value(V8Value::Null);
        }

        {
            auto gcValue = dynamic_cast<Char^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<SByte^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Byte^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Int16^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<UInt16^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Int32^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<UInt32^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Int64^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value((double)*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<UInt64^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value((double)*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Boolean^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Single^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Double^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Decimal^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value((double)*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<String^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(new StdString(gcValue));
            }
        }

        {
            auto gcValue = dynamic_cast<V8ObjectImpl^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(gcValue->GetHolder()->Clone());
            }
        }

        return V8Value(new HostObjectHolderImpl(V8ProxyHelpers::AddRefHostObject(gcObject)));
    }

    //-------------------------------------------------------------------------

    Object^ V8ContextProxyImpl::ExportValue(const V8Value& value)
    {
        if (value.IsNonexistent())
        {
            return Nonexistent::Value;
        }

        if (value.IsUndefined())
        {
            return nullptr;
        }

        if (value.IsNull())
        {
            return DBNull::Value;
        }

        {
            bool result;
            if (value.AsBoolean(result))
            {
                return result;
            }
        }

        {
            double result;
            if (value.AsNumber(result))
            {
                return result;
            }
        }

        {
            __int32 result;
            if (value.AsInt32(result))
            {
                return result;
            }
        }

        {
            unsigned __int32 result;
            if (value.AsUInt32(result))
            {
                return result;
            }
        }

        {
            const StdString* pString;
            if (value.AsString(pString))
            {
                return pString->ToManagedString();
            }
        }

        {
            V8ObjectHolder* pHolder;
            if (value.AsV8Object(pHolder))
            {
                return gcnew V8ObjectImpl(pHolder->Clone());
            }
        }

        {
            HostObjectHolder* pHolder;
            if (value.AsHostObject(pHolder))
            {
                return V8ProxyHelpers::GetHostObject(pHolder->GetObject());
            }
        }

        return nullptr;
    }

//-----------------------------------------------------------------------------

    SharedPtr<V8Context> V8ContextProxyImpl::GetContext()
    {
        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pspContext == nullptr)
            {
                throw gcnew ObjectDisposedException(ToString());
            }

            return *m_pspContext;

        END_LOCK_SCOPE
    }

}}}
