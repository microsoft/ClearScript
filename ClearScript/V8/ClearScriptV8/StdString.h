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

    explicit StdString(Handle<Value> hValue):
        m_Value(GetValue(hValue))
    {
    }

    Local<String> ToV8String(Isolate* pIsolate) const
    {
        return String::NewFromTwoByte(pIsolate, reinterpret_cast<const uint16_t*>(ToCString()), String::kNormalString, GetLength());
    }

private:

    static std::wstring GetValue(Handle<Value> hValue)
    {
        String::Value value(hValue);
        return std::wstring(*value, value.length());
    }

#endif // _M_CEE

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
