// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Managed.h"

//-----------------------------------------------------------------------------
// V8Exception implementation
//-----------------------------------------------------------------------------

void DECLSPEC_NORETURN V8Exception::ThrowScriptEngineException() const
{
    auto gcEngineName = m_EngineName.ToManagedString();
    auto gcMessage = m_Message.ToManagedString();
    auto gcStackTrace = m_StackTrace.ToManagedString();
    auto gcEngine = ScriptEngine::Current;
    auto gcScriptException = (gcEngine != nullptr) ? gcEngine->MarshalToHost(V8ContextProxyImpl::ExportValue(m_ScriptException), false) : nullptr;
    auto gcInnerException = V8ProxyHelpers::MarshalExceptionToHost(V8ContextProxyImpl::ExportValue(m_InnerException));

    switch (m_Type)
    {
        case Type::General: default:
            throw gcnew ScriptEngineException(gcEngineName, gcMessage, gcStackTrace, 0, false, m_ExecutionStarted, gcScriptException, gcInnerException);

        case Type::Fatal:
            throw gcnew ScriptEngineException(gcEngineName, gcMessage, gcStackTrace, 0, true, m_ExecutionStarted, gcScriptException, gcInnerException);

        case Type::Interrupt:
            throw gcnew ScriptInterruptedException(gcEngineName, gcMessage, gcStackTrace, 0, false, m_ExecutionStarted, gcScriptException, gcInnerException);
    }
}
