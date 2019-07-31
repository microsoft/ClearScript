// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

#ifdef _M_CEE

//-----------------------------------------------------------------------------
// StringToUniPtr
//-----------------------------------------------------------------------------

class StringToUniPtr final
{
public:

    explicit StringToUniPtr(String^ gcValue):
        m_pValue(V8ProxyHelpers::AllocString(gcValue))
    {
    }

    operator const wchar_t*() const
    {
        return m_pValue;
    }

    ~StringToUniPtr()
    {
        V8ProxyHelpers::FreeString(m_pValue);
    }

private:

    wchar_t* m_pValue;
};

#endif // _M_CEE

//-----------------------------------------------------------------------------
// StdString
//-----------------------------------------------------------------------------

class StdString final
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

    StdString(StdString&& that) noexcept:
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

    const StdString& operator=(StdString&& that) noexcept
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

    size_t GetDigest() const
    {
        return std::hash<std::wstring>()(m_Value);
    }

    const wchar_t* ToCString() const
    {
        return m_Value.c_str();
    }

    //-------------------------------------------------------------------------
    // managed extensions
    //-------------------------------------------------------------------------

#ifdef _M_CEE

    explicit StdString(String^ gcValue)
    {
        if (gcValue != nullptr)
        {
            m_Value = std::wstring(StringToUniPtr(gcValue), gcValue->Length);
        }
    }

    String^ ToManagedString() const
    {
        return gcnew String(ToCString(), 0, GetLength());
    }

#endif // _M_CEE

    //-------------------------------------------------------------------------
    // V8 extensions
    //-------------------------------------------------------------------------

    StdString(v8::Isolate* pIsolate, v8::Local<v8::Value> hValue):
        m_Value(GetValue(pIsolate, hValue))
    {
    }

    explicit StdString(const v8_inspector::StringView& stringView):
        m_Value(GetValue(stringView))
    {
    }

    v8::MaybeLocal<v8::String> ToV8String(v8::Isolate* pIsolate, v8::NewStringType type) const
    {
        return v8::String::NewFromTwoByte(pIsolate, reinterpret_cast<const uint16_t*>(ToCString()), type, GetLength());
    }

    v8_inspector::StringView GetStringView(size_t index = 0, size_t length = SIZE_MAX) const
    {
        auto valueLength = m_Value.length();
        index = std::min(index, valueLength);
        length = std::min(length, valueLength - index);
        return v8_inspector::StringView(ToCString() + index, length);
    }

private:

    //-------------------------------------------------------------------------
    // internals
    //-------------------------------------------------------------------------

    static std::wstring GetValue(v8::Isolate* pIsolate, v8::Local<v8::Value> hValue)
    {
        v8::String::Value value(pIsolate, hValue);
        return std::wstring(EnsureNonNull(*value), value.length());
    }

    static std::wstring GetValue(const v8_inspector::StringView& stringView);

    static const wchar_t* EnsureNonNull(const wchar_t* pValue)
    {
        return (pValue != nullptr) ? pValue : L"";
    }

    std::wstring m_Value;
};
