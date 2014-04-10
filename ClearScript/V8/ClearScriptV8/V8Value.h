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

//-----------------------------------------------------------------------------
// V8Value
//-----------------------------------------------------------------------------

class V8Value
{
public:

    enum NonexistentInitializer
    {
        Nonexistent
    };

    enum UndefinedInitializer
    {
        Undefined
    };

    enum NullInitializer
    {
        Null
    };

    explicit V8Value(NonexistentInitializer):
        m_Type(Type_Nonexistent)
    {
    }

    explicit V8Value(UndefinedInitializer):
        m_Type(Type_Undefined)
    {
    }

    explicit V8Value(NullInitializer):
        m_Type(Type_Null)
    {
    }

    explicit V8Value(bool value):
        m_Type(Type_Boolean)
    {
        m_Data.BooleanValue = value;
    }

    explicit V8Value(double value):
        m_Type(Type_Number)
    {
        m_Data.DoubleValue = value;
    }

    explicit V8Value(__int32 value):
        m_Type(Type_Int32)
    {
        m_Data.Int32Value = value;
    }

    explicit V8Value(unsigned __int32 value):
        m_Type(Type_UInt32)
    {
        m_Data.UInt32Value = value;
    }

    explicit V8Value(const StdString* pString):
        m_Type(Type_String)
    {
        m_Data.pString = pString;
    }

    explicit V8Value(V8ObjectHolder* pV8ObjectHolder):
        m_Type(Type_V8Object)
    {
        m_Data.pV8ObjectHolder = pV8ObjectHolder;
    }

    explicit V8Value(HostObjectHolder* pHostObjectHolder):
        m_Type(Type_HostObject)
    {
        m_Data.pHostObjectHolder = pHostObjectHolder;
    }

    V8Value(const V8Value& that)
    {
        Copy(that);
    }

    V8Value(V8Value&& that)
    {
        Move(that);
    }

    const V8Value& operator=(const V8Value& that)
    {
        Dispose();
        Copy(that);
        return *this;
    }

    const V8Value& operator=(V8Value&& that)
    {
        Dispose();
        Move(that);
        return *this;
    }

    bool IsNonexistent() const
    {
        return m_Type == Type_Nonexistent;
    }

    bool IsUndefined() const
    {
        return m_Type == Type_Undefined;
    }

    bool IsNull() const
    {
        return m_Type == Type_Null;
    }

    bool AsBoolean(bool& result) const
    {
        if (m_Type == Type_Boolean)
        {
            result = m_Data.BooleanValue;
            return true;
        }

        return false;
    }

    bool AsNumber(double& result) const
    {
        if (m_Type == Type_Number)
        {
            result = m_Data.DoubleValue;
            return true;
        }

        return false;
    }

    bool AsInt32(__int32& result) const
    {
        if (m_Type == Type_Int32)
        {
            result = m_Data.Int32Value;
            return true;
        }

        return false;
    }

    bool AsUInt32(unsigned __int32& result) const
    {
        if (m_Type == Type_UInt32)
        {
            result = m_Data.UInt32Value;
            return true;
        }

        return false;
    }

    bool AsString(const StdString*& pString) const
    {
        if (m_Type == Type_String)
        {
            pString = m_Data.pString;
            return true;
        }

        return false;
    }

    bool AsV8Object(V8ObjectHolder*& pV8ObjectHolder) const
    {
        if (m_Type == Type_V8Object)
        {
            pV8ObjectHolder = m_Data.pV8ObjectHolder;
            return true;
        }

        return false;
    }

    bool AsHostObject(HostObjectHolder*& pHostObjectHolder) const
    {
        if (m_Type == Type_HostObject)
        {
            pHostObjectHolder = m_Data.pHostObjectHolder;
            return true;
        }

        return false;
    }

    ~V8Value()
    {
        Dispose();
    }

private:

    enum Type
    {
        Type_Nonexistent,
        Type_Undefined,
        Type_Null,
        Type_Boolean,
        Type_Number,
        Type_Int32,
        Type_UInt32,
        Type_String,
        Type_V8Object,
        Type_HostObject
    };

    union Data
    {
        bool BooleanValue;
        double DoubleValue;
        __int32 Int32Value;
        unsigned __int32 UInt32Value;
        const StdString* pString;
        V8ObjectHolder* pV8ObjectHolder;
        HostObjectHolder* pHostObjectHolder;
    };

    void Copy(const V8Value& that)
    {
        m_Type = that.m_Type;

        if (m_Type == Type_Boolean)
        {
            m_Data.BooleanValue = that.m_Data.BooleanValue;
        }
        else if (m_Type == Type_Number)
        {
            m_Data.DoubleValue = that.m_Data.DoubleValue;
        }
        else if (m_Type == Type_Int32)
        {
            m_Data.Int32Value = that.m_Data.Int32Value;
        }
        else if (m_Type == Type_UInt32)
        {
            m_Data.UInt32Value = that.m_Data.UInt32Value;
        }
        else if (m_Type == Type_String)
        {
            m_Data.pString = new StdString(*that.m_Data.pString);
        }
        else if (m_Type == Type_V8Object)
        {
            m_Data.pV8ObjectHolder = that.m_Data.pV8ObjectHolder->Clone();
        }
        else if (m_Type == Type_HostObject)
        {
            m_Data.pHostObjectHolder = that.m_Data.pHostObjectHolder->Clone();
        }
    }

    void Move(V8Value& that)
    {
        m_Type = that.m_Type;
        m_Data = that.m_Data;
        that.m_Type = Type_Undefined;
    }

    void Dispose()
    {
        if (m_Type == Type_String)
        {
            delete m_Data.pString;
        }
        else if (m_Type == Type_V8Object)
        {
            delete m_Data.pV8ObjectHolder;
        }
        else if (m_Type == Type_HostObject)
        {
            delete m_Data.pHostObjectHolder;
        }
    }

    Type m_Type;
    Data m_Data;
};
