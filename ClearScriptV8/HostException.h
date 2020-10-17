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

    HostException(const HostException& that):
        m_Message(that.m_Message),
        m_Exception(that.m_Exception)
    {
    }

    HostException(HostException&& that) noexcept:
        m_Message(std::move(that.m_Message)),
        m_Exception(std::move(that.m_Exception))
    {
    }

    const HostException& operator=(const HostException& that)
    {
        HostException tempException(std::move(*this));
        m_Message = that.m_Message;
        m_Exception = that.m_Exception;
        return *this;
    }

    const HostException& operator=(HostException&& that) noexcept
    {
        HostException tempException(std::move(*this));
        m_Message = std::move(that.m_Message);
        m_Exception = std::move(that.m_Exception);
        return *this;
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
