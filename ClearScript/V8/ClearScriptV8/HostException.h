// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// HostException
//-----------------------------------------------------------------------------

class HostException final
{
public:

    HostException(StdString&& message, V8Value&& exception):
        m_Message(std::move(message)),
        m_Exception(std::move(exception))
    {
    }

    const StdString& GetMessage() const
    {
        return m_Message;
    }

    const V8Value& GetException() const
    {
        return m_Exception;
    }

private:

    StdString m_Message;
    V8Value m_Exception;
};
