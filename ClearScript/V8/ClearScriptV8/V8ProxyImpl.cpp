// 
// Copyright © Microsoft Corporation. All rights reserved.
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

    static void InvokeAction(LPVOID pvActionRef)
    {
        (*reinterpret_cast<Action^*>(pvActionRef))();
    }

    //-------------------------------------------------------------------------
    // V8ProxyImpl implementation
    //-------------------------------------------------------------------------

    V8ProxyImpl::V8ProxyImpl(String^ gcName, Boolean enableDebugging, Int32 debugPort):
        m_gcLock(gcnew Object)
    {
        m_gcDispatchDebugMessagesAction = gcnew Action(this, &V8ProxyImpl::DispatchDebugMessages);
        m_gcProcessDebugMessagesCallback = gcnew WaitCallback(this, &V8ProxyImpl::ProcessDebugMessages);

        auto pDispatcher = (V8Context::DebugMessageDispatcher*)Marshal::GetFunctionPointerForDelegate(m_gcDispatchDebugMessagesAction).ToPointer();
        m_pContextPtr = new SharedPtr<V8Context>(V8Context::Create(StringToUniPtr(gcName), enableDebugging, pDispatcher, debugPort));
    }

    //-------------------------------------------------------------------------

    Object^ V8ProxyImpl::GetRootItem()
    {
        try
        {
            return ExportValue(GetContext()->GetRootObject());
        }
        catch (const V8Exception& exception)
        {
            throw gcnew InvalidOperationException(gcnew String(exception.GetMessage()));
        }
    }

    //-------------------------------------------------------------------------

    void V8ProxyImpl::AddGlobalItem(String^ gcName, Object^ gcItem, Boolean globalMembers)
    {
        try
        {
            GetContext()->SetGlobalProperty(StringToUniPtr(gcName), ImportValue(gcItem), globalMembers);
        }
        catch (const V8Exception& exception)
        {
            throw gcnew InvalidOperationException(gcnew String(exception.GetMessage()));
        }
    }

    //-------------------------------------------------------------------------

    Object^ V8ProxyImpl::Execute(String^ gcDocumentName, String^ gcCode, Boolean discard)
    {
        try
        {
            return ExportValue(GetContext()->Execute(StringToUniPtr(gcDocumentName), StringToUniPtr(gcCode), discard));
        }
        catch (const V8Exception& exception)
        {
            if (exception.GetType() == V8Exception::Type_Interrupt)
            {
                throw gcnew OperationCanceledException(gcnew String(exception.GetMessage()));
            }

            throw gcnew InvalidOperationException(gcnew String(exception.GetMessage()));
        }
    }

    //-------------------------------------------------------------------------

    void V8ProxyImpl::InvokeWithLock(Action^ gcAction)
    {
        GetContext()->CallWithLock(InvokeAction, &gcAction);
    }

    //-------------------------------------------------------------------------

    void V8ProxyImpl::Interrupt()
    {
        GetContext()->Interrupt();
    }

    //-------------------------------------------------------------------------

    V8ProxyImpl::~V8ProxyImpl()
    {
        SharedPtr<V8Context> psContext;

        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pContextPtr != nullptr)
            {
                // hold V8 context for destruction outside lock scope
                psContext = *m_pContextPtr;
                delete m_pContextPtr;
                m_pContextPtr = nullptr;
            }

        END_LOCK_SCOPE

        if (!psContext.IsEmpty())
        {
            // When enabled, the V8 context's debug agent holds a weak callback pointer into this
            // managed proxy. The agent must therefore be disabled before the proxy goes away.

            psContext->DisableDebugAgent();
        }
    }

    //-------------------------------------------------------------------------

    V8ProxyImpl::!V8ProxyImpl()
    {
        if (m_pContextPtr != nullptr)
        {
            delete m_pContextPtr;
        }
    }

    //-------------------------------------------------------------------------

    V8Value V8ProxyImpl::ImportValue(Object^ gcObject)
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
                return V8Value(StringToUniPtr(gcValue));
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

    Object^ V8ProxyImpl::ExportValue(const V8Value& value)
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
                if (Math::Round(result) == result)
                {
                    if ((result >= Int32::MinValue) && (result <= Int32::MaxValue))
                        return (Int32)result;

                    if ((result >= UInt32::MinValue) && (result <= UInt32::MaxValue))
                        return (UInt32)result;

                    if ((result >= Int64::MinValue) && (result <= Int64::MaxValue))
                        return (Int64)result;

                    if ((result >= UInt64::MinValue) && (result <= UInt64::MaxValue))
                        return (UInt64)result;
                }

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
            LPCWSTR pResult;
            if (value.AsString(pResult))
            {
                return gcnew String(pResult);
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

    //-------------------------------------------------------------------------

    SharedPtr<V8Context> V8ProxyImpl::GetContext()
    {
        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pContextPtr == nullptr)
            {
                throw gcnew ObjectDisposedException(ToString());
            }

            return *m_pContextPtr;

        END_LOCK_SCOPE
    }

    //-------------------------------------------------------------------------

    bool V8ProxyImpl::TryGetContext(SharedPtr<V8Context>& psContext)
    {
        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pContextPtr != nullptr)
            {
                psContext = *m_pContextPtr;
                return true;
            }

            return false;

        END_LOCK_SCOPE
    }

    //-------------------------------------------------------------------------

    void V8ProxyImpl::DispatchDebugMessages()
    {
        SharedPtr<V8Context> psContext;
        if (TryGetContext(psContext))
        {
            if (psContext->IncrementDebugMessageDispatchCount() == 1)
            {
                ThreadPool::QueueUserWorkItem(m_gcProcessDebugMessagesCallback);
            }
        }
    }

    //-------------------------------------------------------------------------

    void V8ProxyImpl::ProcessDebugMessages(Object^ /* gcState */)
    {
        SharedPtr<V8Context> psContext;
        if (TryGetContext(psContext))
        {
            psContext->ProcessDebugMessages();
        }
    }

}}}
