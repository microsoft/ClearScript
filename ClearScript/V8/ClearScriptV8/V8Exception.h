// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8Exception
//-----------------------------------------------------------------------------

class V8Exception final
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
        m_ExecutionStarted(executionStarted),
        m_ScriptException(V8Value::Null),
        m_InnerException(V8Value::Undefined)
    {
    }

    V8Exception(Type type, const StdString& engineName, StdString&& message, StdString&& stackTrace, bool executionStarted, V8Value&& scriptException, V8Value&& innerException):
        m_Type(type),
        m_EngineName(engineName),
        m_Message(std::move(message)),
        m_StackTrace(std::move(stackTrace)),
        m_ExecutionStarted(executionStarted),
        m_ScriptException(std::move(scriptException)),
        m_InnerException(std::move(innerException))
    {
    }

    void DECLSPEC_NORETURN ThrowScriptEngineException() const;

private:

    Type m_Type;
    StdString m_EngineName;
    StdString m_Message;
    StdString m_StackTrace;
    bool m_ExecutionStarted;
    V8Value m_ScriptException;
    V8Value m_InnerException;
};
