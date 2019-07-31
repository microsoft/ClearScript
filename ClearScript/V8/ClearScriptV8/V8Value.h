// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8Value
//-----------------------------------------------------------------------------

class V8Value final
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

    enum DateTimeInitializer
    {
        DateTime
    };

    enum class Subtype: uint16_t
    {
        None,
        ArrayBuffer,
        DataView,
        Uint8Array,
        Uint8ClampedArray,
        Int8Array,
        Uint16Array,
        Int16Array,
        Uint32Array,
        Int32Array,
        Float32Array,
        Float64Array
    };

    explicit V8Value(NonexistentInitializer):
        m_Type(Type::Nonexistent),
        m_Subtype(Subtype::None)
    {
    }

    explicit V8Value(UndefinedInitializer):
        m_Type(Type::Undefined),
        m_Subtype(Subtype::None)
    {
    }

    explicit V8Value(NullInitializer):
        m_Type(Type::Null),
        m_Subtype(Subtype::None)
    {
    }

    explicit V8Value(bool value):
        m_Type(Type::Boolean),
        m_Subtype(Subtype::None)
    {
        m_Data.BooleanValue = value;
    }

    explicit V8Value(double value):
        m_Type(Type::Number),
        m_Subtype(Subtype::None)
    {
        m_Data.DoubleValue = value;
    }

    explicit V8Value(int32_t value):
        m_Type(Type::Int32),
        m_Subtype(Subtype::None)
    {
        m_Data.Int32Value = value;
    }

    explicit V8Value(uint32_t value):
        m_Type(Type::UInt32),
        m_Subtype(Subtype::None)
    {
        m_Data.UInt32Value = value;
    }

    explicit V8Value(const StdString* pString):
        m_Type(Type::String),
        m_Subtype(Subtype::None)
    {
        m_Data.pString = pString;
    }

    V8Value(V8ObjectHolder* pV8ObjectHolder, Subtype subtype):
        m_Type(Type::V8Object),
        m_Subtype(subtype)
    {
        m_Data.pV8ObjectHolder = pV8ObjectHolder;
    }

    explicit V8Value(HostObjectHolder* pHostObjectHolder):
        m_Type(Type::HostObject),
        m_Subtype(Subtype::None)
    {
        m_Data.pHostObjectHolder = pHostObjectHolder;
    }

    V8Value(DateTimeInitializer, double value):
        m_Type(Type::DateTime),
        m_Subtype(Subtype::None)
    {
        m_Data.DoubleValue = value;
    }

    V8Value(const V8Value& that)
    {
        Copy(that);
    }

    V8Value(V8Value&& that) noexcept
    {
        Move(that);
    }

    const V8Value& operator=(const V8Value& that)
    {
        Dispose();
        Copy(that);
        return *this;
    }

    const V8Value& operator=(V8Value&& that) noexcept
    {
        Dispose();
        Move(that);
        return *this;
    }

    bool IsNonexistent() const
    {
        return m_Type == Type::Nonexistent;
    }

    bool IsUndefined() const
    {
        return m_Type == Type::Undefined;
    }

    bool IsNull() const
    {
        return m_Type == Type::Null;
    }

    bool AsBoolean(bool& result) const
    {
        if (m_Type == Type::Boolean)
        {
            result = m_Data.BooleanValue;
            return true;
        }

        return false;
    }

    bool AsNumber(double& result) const
    {
        if (m_Type == Type::Number)
        {
            result = m_Data.DoubleValue;
            return true;
        }

        return false;
    }

    bool AsInt32(int32_t& result) const
    {
        if (m_Type == Type::Int32)
        {
            result = m_Data.Int32Value;
            return true;
        }

        return false;
    }

    bool AsUInt32(uint32_t& result) const
    {
        if (m_Type == Type::UInt32)
        {
            result = m_Data.UInt32Value;
            return true;
        }

        return false;
    }

    bool AsString(const StdString*& pString) const
    {
        if (m_Type == Type::String)
        {
            pString = m_Data.pString;
            return true;
        }

        return false;
    }

    bool AsV8Object(V8ObjectHolder*& pV8ObjectHolder, Subtype& subtype) const
    {
        if (m_Type == Type::V8Object)
        {
            pV8ObjectHolder = m_Data.pV8ObjectHolder;
            subtype = m_Subtype;
            return true;
        }

        return false;
    }

    bool AsHostObject(HostObjectHolder*& pHostObjectHolder) const
    {
        if (m_Type == Type::HostObject)
        {
            pHostObjectHolder = m_Data.pHostObjectHolder;
            return true;
        }

        return false;
    }

    bool AsDateTime(double& result) const
    {
        if (m_Type == Type::DateTime)
        {
            result = m_Data.DoubleValue;
            return true;
        }

        return false;
    }

    ~V8Value()
    {
        Dispose();
    }

private:

    enum class Type: uint16_t
    {
        Nonexistent,
        Undefined,
        Null,
        Boolean,
        Number,
        Int32,
        UInt32,
        String,
        V8Object,
        HostObject,
        DateTime
    };

    union Data
    {
        bool BooleanValue;
        double DoubleValue;
        int32_t Int32Value;
        uint32_t UInt32Value;
        const StdString* pString;
        V8ObjectHolder* pV8ObjectHolder;
        HostObjectHolder* pHostObjectHolder;
    };

    void Copy(const V8Value& that)
    {
        m_Type = that.m_Type;
        m_Subtype = that.m_Subtype;

        if (m_Type == Type::Boolean)
        {
            m_Data.BooleanValue = that.m_Data.BooleanValue;
        }
        else if ((m_Type == Type::Number) || (m_Type == Type::DateTime))
        {
            m_Data.DoubleValue = that.m_Data.DoubleValue;
        }
        else if (m_Type == Type::Int32)
        {
            m_Data.Int32Value = that.m_Data.Int32Value;
        }
        else if (m_Type == Type::UInt32)
        {
            m_Data.UInt32Value = that.m_Data.UInt32Value;
        }
        else if (m_Type == Type::String)
        {
            m_Data.pString = new StdString(*that.m_Data.pString);
        }
        else if (m_Type == Type::V8Object)
        {
            m_Data.pV8ObjectHolder = that.m_Data.pV8ObjectHolder->Clone();
        }
        else if (m_Type == Type::HostObject)
        {
            m_Data.pHostObjectHolder = that.m_Data.pHostObjectHolder->Clone();
        }
    }

    void Move(V8Value& that)
    {
        m_Type = that.m_Type;
        m_Subtype = that.m_Subtype;
        m_Data = that.m_Data;
        that.m_Type = Type::Undefined;
    }

    void Dispose()
    {
        if (m_Type == Type::String)
        {
            delete m_Data.pString;
        }
        else if (m_Type == Type::V8Object)
        {
            delete m_Data.pV8ObjectHolder;
        }
        else if (m_Type == Type::HostObject)
        {
            delete m_Data.pHostObjectHolder;
        }
    }

    Type m_Type;
    Subtype m_Subtype;
    Data m_Data {};
};
