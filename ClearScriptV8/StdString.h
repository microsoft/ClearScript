// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// StdChar
//-----------------------------------------------------------------------------

using StdChar = char16_t;
static_assert(sizeof(StdChar) == sizeof(uint16_t));

//-----------------------------------------------------------------------------
// StdString
//-----------------------------------------------------------------------------

class StdString final
{
    //-------------------------------------------------------------------------
    // types
    //-------------------------------------------------------------------------

    using Value = std::basic_string<StdChar>;
    using UTF8Converter = std::wstring_convert<std::codecvt_utf8_utf16<StdChar>, StdChar>;
    
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

    explicit StdString(const Value& value):
        m_Value(value)
    {
    }

    explicit StdString(Value&& value):
        m_Value(std::move(value))
    {
    }

    explicit StdString(const StdChar* pValue):
        m_Value(EnsureNonNull(pValue))
    {
    }

    StdString(const StdChar* pValue, int32_t length):
        StdString(Value(EnsureNonNull(pValue), length))
    {
    }

    explicit StdString(const std::string& value):
        m_Value(UTF8Converter().from_bytes(value))
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

    const StdString& operator=(const Value& value)
    {
        m_Value = value;
        return *this;
    }

    const StdString& operator=(Value&& value)
    {
        m_Value = std::move(value);
        return *this;
    }

    const StdString& operator=(const StdChar* pValue)
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

    const StdString& operator+=(const Value& value)
    {
        m_Value += value;
        return *this;
    }

    const StdString& operator+=(const StdChar* pValue)
    {
        m_Value += EnsureNonNull(pValue);
        return *this;
    }

    const StdString& operator+=(StdChar value)
    {
        m_Value += value;
        return *this;
    }

    //-------------------------------------------------------------------------
    // comparison
    //-------------------------------------------------------------------------

    int Compare(const StdString& that) const { return m_Value.compare(that.m_Value); }
    int Compare(const Value& value) const { return m_Value.compare(value); }
    int Compare(const StdChar* pValue) const { return m_Value.compare(pValue); }

    bool operator==(const StdString& that) const { return m_Value == that.m_Value; }
    bool operator==(const Value& value) const { return m_Value == value; }
    bool operator==(const StdChar* pValue) const { return m_Value == pValue; }

    bool operator!=(const StdString& that) const { return m_Value != that.m_Value; }
    bool operator!=(const Value& value) const { return m_Value != value; }
    bool operator!=(const StdChar* pValue) const { return m_Value != pValue; }

    bool operator<(const StdString& that) const { return m_Value < that.m_Value; }
    bool operator<(const Value& value) const { return m_Value < value; }
    bool operator<(const StdChar* pValue) const { return m_Value < pValue; }

    bool operator<=(const StdString& that) const { return m_Value <= that.m_Value; }
    bool operator<=(const Value& value) const { return m_Value <= value; }
    bool operator<=(const StdChar* pValue) const { return m_Value <= pValue; }

    bool operator>(const StdString& that) const { return m_Value > that.m_Value; }
    bool operator>(const Value& value) const { return m_Value > value; }
    bool operator>(const StdChar* pValue) const { return m_Value > pValue; }

    bool operator>=(const StdString& that) const { return m_Value >= that.m_Value; }
    bool operator>=(const Value& value) const { return m_Value >= value; }
    bool operator>=(const StdChar* pValue) const { return m_Value >= pValue; }

    //-------------------------------------------------------------------------
    // miscellaneous
    //-------------------------------------------------------------------------

    int GetLength() const
    {
        return static_cast<int>(m_Value.length());
    }

    size_t GetDigest() const;

    const StdChar* ToCString() const
    {
        return m_Value.c_str();
    }

    std::string ToUTF8() const
    {
        return UTF8Converter().to_bytes(m_Value);
    }

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
        return v8_inspector::StringView(reinterpret_cast<const uint16_t*>(ToCString() + index), length);
    }

private:

    //-------------------------------------------------------------------------
    // internals
    //-------------------------------------------------------------------------

    uint32_t GetDigestAsUInt32() const;
    uint64_t GetDigestAsUInt64() const;

    static Value GetValue(v8::Isolate* pIsolate, v8::Local<v8::Value> hValue)
    {
        Value value;
        if (hValue.IsEmpty())
        {
            return value;
        }

        v8::Local<v8::String> hString;
        if (hValue->IsString())
        {
            hString = hValue.As<v8::String>();
        }
        else if (!hValue->ToString(pIsolate->GetCurrentContext()).ToLocal(&hString))
        {
            return value;
        }

        auto length = hString->Length();
        value.resize(length);

        hString->WriteV2(pIsolate, 0, length, reinterpret_cast<uint16_t*>(value.data()));
        return value;
    }

    static Value GetValue(const v8_inspector::StringView& stringView);
    static const StdChar* EnsureNonNull(const StdChar* pValue);

    Value m_Value;
};

//-----------------------------------------------------------------------------
// string and character literal support
//-----------------------------------------------------------------------------

#define UTF8_LITERAL(LITERAL) (u ## LITERAL)
#define SL(LITERAL) UTF8_LITERAL(LITERAL)

//-----------------------------------------------------------------------------
// StdString implementation
//-----------------------------------------------------------------------------

inline const StdChar* StdString::EnsureNonNull(const StdChar* pValue)
{
    return (pValue != nullptr) ? pValue : SL("");
}
