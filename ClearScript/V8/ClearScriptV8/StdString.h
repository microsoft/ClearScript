// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

#ifdef _M_CEE

//-----------------------------------------------------------------------------
// StringToUniPtr
//-----------------------------------------------------------------------------

class StringToUniPtr
{
public:

    explicit StringToUniPtr(String^ gcValue):
        m_pValue(Microsoft::ClearScript::V8::V8ProxyHelpers::AllocString(gcValue))
    {
    }

    operator const wchar_t*() const
    {
        return m_pValue;
    }

    ~StringToUniPtr()
    {
        Microsoft::ClearScript::V8::V8ProxyHelpers::FreeString(m_pValue);
    }

private:

    wchar_t* m_pValue;
};

#endif // _M_CEE

//-----------------------------------------------------------------------------
// StdString
//-----------------------------------------------------------------------------

class StdString
{
public:

    //-------------------------------------------------------------------------
    // constructors
    //-------------------------------------------------------------------------

    StdString()
    {
    }

    StdString(const StdString& that):
        m_Value(that.m_Value)
    {
    }

    StdString(StdString&& that):
        m_Value(std::move(that.m_Value))
    {
    }

    explicit StdString(const std::wstring& value):
        m_Value(value)
    {
    }

    explicit StdString(std::wstring&& value):
        m_Value(std::move(value))
    {
    }

    explicit StdString(const wchar_t* pValue):
        m_Value(EnsureNonNull(pValue))
    {
    }

    //-------------------------------------------------------------------------
    // assignment
    //-------------------------------------------------------------------------

    const StdString& operator=(const StdString& that)
    {
        m_Value = that.m_Value;
        return *this;
    }

    const StdString& operator=(StdString&& that)
    {
        m_Value = std::move(that.m_Value);
        return *this;
    }

    const StdString& operator=(const std::wstring& value)
    {
        m_Value = value;
        return *this;
    }

    const StdString& operator=(std::wstring&& value)
    {
        m_Value = std::move(value);
        return *this;
    }

    const StdString& operator=(const wchar_t* pValue)
    {
        m_Value = EnsureNonNull(pValue);
        return *this;
    }

    //-------------------------------------------------------------------------
    // concatenation
    //-------------------------------------------------------------------------

    const StdString& operator+=(const StdString& that)
    {
        m_Value += that.m_Value;
        return *this;
    }

    const StdString& operator+=(const std::wstring& value)
    {
        m_Value += value;
        return *this;
    }

    const StdString& operator+=(const wchar_t* pValue)
    {
        m_Value += EnsureNonNull(pValue);
        return *this;
    }

    const StdString& operator+=(wchar_t value)
    {
        m_Value += value;
        return *this;
    }

    //-------------------------------------------------------------------------
    // comparison
    //-------------------------------------------------------------------------

    int Compare(const StdString& that) const { return m_Value.compare(that.m_Value); }
    int Compare(const std::wstring& value) const { return m_Value.compare(value); }
    int Compare(const wchar_t* pValue) const { return m_Value.compare(pValue); }

    bool operator==(const StdString& that) const { return m_Value == that.m_Value; }
    bool operator==(const std::wstring& value) const { return m_Value == value; }
    bool operator==(const wchar_t* pValue) const { return m_Value == pValue; }

    bool operator!=(const StdString& that) const { return m_Value != that.m_Value; }
    bool operator!=(const std::wstring& value) const { return m_Value != value; }
    bool operator!=(const wchar_t* pValue) const { return m_Value != pValue; }

    bool operator<(const StdString& that) const { return m_Value < that.m_Value; }
    bool operator<(const std::wstring& value) const { return m_Value < value; }
    bool operator<(const wchar_t* pValue) const { return m_Value < pValue; }

    bool operator<=(const StdString& that) const { return m_Value <= that.m_Value; }
    bool operator<=(const std::wstring& value) const { return m_Value <= value; }
    bool operator<=(const wchar_t* pValue) const { return m_Value <= pValue; }

    bool operator>(const StdString& that) const { return m_Value > that.m_Value; }
    bool operator>(const std::wstring& value) const { return m_Value > value; }
    bool operator>(const wchar_t* pValue) const { return m_Value > pValue; }

    bool operator>=(const StdString& that) const { return m_Value >= that.m_Value; }
    bool operator>=(const std::wstring& value) const { return m_Value >= value; }
    bool operator>=(const wchar_t* pValue) const { return m_Value >= pValue; }

    //-------------------------------------------------------------------------
    // miscellaneous
    //-------------------------------------------------------------------------

    int GetLength() const
    {
        return static_cast<int>(m_Value.length());
    }

    const wchar_t* ToCString() const
    {
        return m_Value.c_str();
    }

#ifdef _M_CEE

    //-------------------------------------------------------------------------
    // managed extensions
    //-------------------------------------------------------------------------

public:

    explicit StdString(String^ gcValue):
        m_Value(StringToUniPtr(gcValue), gcValue->Length)
    {
    }

    String^ ToManagedString() const
    {
        return gcnew String(ToCString(), 0, GetLength());
    }

#else // !_M_CEE

    //-------------------------------------------------------------------------
    // V8 extensions
    //-------------------------------------------------------------------------

public:

    explicit StdString(v8::Local<v8::Value> hValue):
        m_Value(GetValue(hValue))
    {
    }

    v8::MaybeLocal<v8::String> ToV8String(v8::Isolate* pIsolate) const
    {
        return v8::String::NewFromTwoByte(pIsolate, reinterpret_cast<const uint16_t*>(ToCString()), v8::NewStringType::kNormal, GetLength());
    }

private:

    static std::wstring GetValue(v8::Local<v8::Value> hValue)
    {
        v8::String::Value value(hValue);
        return std::wstring(EnsureNonNull(*value), value.length());
    }

#endif // !_M_CEE

private:

    //-------------------------------------------------------------------------
    // internals
    //-------------------------------------------------------------------------

    static const wchar_t* EnsureNonNull(const wchar_t* pValue)
    {
        return (pValue != nullptr) ? pValue : L"";
    }

    std::wstring m_Value;
};
