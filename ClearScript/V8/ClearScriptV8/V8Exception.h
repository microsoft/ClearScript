// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8Exception
//-----------------------------------------------------------------------------

class V8Exception
{
public:

    enum class Type
    {
        General,
        Interrupt,
        Fatal
    };

    V8Exception(Type type, const StdString& engineName, StdString&& message, bool executionStarted):
        m_Type(type),
        m_EngineName(engineName),
        m_Message(std::move(message)),
        m_InnerException(V8Value::Undefined),
        m_ExecutionStarted(executionStarted)
    {
    }

    V8Exception(Type type, const StdString& engineName, StdString&& message, StdString&& stackTrace, V8Value&& innerException, bool executionStarted):
        m_Type(type),
        m_EngineName(engineName),
        m_Message(std::move(message)),
        m_StackTrace(std::move(stackTrace)),
        m_InnerException(std::move(innerException)),
        m_ExecutionStarted(executionStarted)
    {
    }

    void DECLSPEC_NORETURN ThrowScriptEngineException() const;

private:

    Type m_Type;
    StdString m_EngineName;
    StdString m_Message;
    StdString m_StackTrace;
    V8Value m_InnerException;
    bool m_ExecutionStarted;
};
