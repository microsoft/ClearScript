// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8Value::WireData
//-----------------------------------------------------------------------------

struct V8Value::WireData
{
    // IMPORTANT: maintain bitwise equivalence with managed struct V8.SplitProxy.V8Value.WireData

    Type Type;
    Subtype Subtype;

    union
    {
        Flags Flags;
        int16_t SignBit;
    };

    union
    {
        int32_t Length;
        int32_t IdentityHash;
    };

    union
    {
        int32_t Int32Value;
        double DoubleValue;
        const StdChar* pStringData;
        const uint64_t* pBigIntData;
        const V8ObjectHandle* pV8ObjectHandle;
        void* pvHostObject;
    };
};

//-----------------------------------------------------------------------------

static_assert(sizeof(V8Value::WireData) == 16, "The managed SplitProxy code assumes that sizeof(V8Value::WireData) is 16 on all platforms.");
static_assert(offsetof(V8Value::WireData, Type) == 0, "The managed SplitProxy code assumes that offsetof(V8Value::WireData, Type) is 0 on all platforms.");
static_assert(offsetof(V8Value::WireData, Subtype) == 1, "The managed SplitProxy code assumes that offsetof(V8Value::WireData, Subtype) is 1 on all platforms.");
static_assert(offsetof(V8Value::WireData, Flags) == 2, "The managed SplitProxy code assumes that offsetof(V8Value::WireData, Flags) is 2 on all platforms.");
static_assert(offsetof(V8Value::WireData, SignBit) == 2, "The managed SplitProxy code assumes that offsetof(V8Value::WireData, SignBit) is 2 on all platforms.");
static_assert(offsetof(V8Value::WireData, Length) == 4, "The managed SplitProxy code assumes that offsetof(V8Value::WireData, Length) is 4 on all platforms.");
static_assert(offsetof(V8Value::WireData, IdentityHash) == 4, "The managed SplitProxy code assumes that offsetof(V8Value::WireData, IdentityHash) is 4 on all platforms.");
static_assert(offsetof(V8Value::WireData, Int32Value) == 8, "The managed SplitProxy code assumes that offsetof(V8Value::WireData, Int32Value) is 8 on all platforms.");
static_assert(offsetof(V8Value::WireData, DoubleValue) == 8, "The managed SplitProxy code assumes that offsetof(V8Value::WireData, DoubleValue) is 8 on all platforms.");
static_assert(offsetof(V8Value::WireData, pStringData) == 8, "The managed SplitProxy code assumes that offsetof(V8Value::WireData, pStringData) is 8 on all platforms.");
static_assert(offsetof(V8Value::WireData, pBigIntData) == 8, "The managed SplitProxy code assumes that offsetof(V8Value::WireData, pBigIntData) is 8 on all platforms.");
static_assert(offsetof(V8Value::WireData, pV8ObjectHandle) == 8, "The managed SplitProxy code assumes that offsetof(V8Value::WireData, pV8ObjectHandle) is 8 on all platforms.");
static_assert(offsetof(V8Value::WireData, pvHostObject) == 8, "The managed SplitProxy code assumes that offsetof(V8Value::WireData, pvHostObject) is 8 on all platforms.");

//-----------------------------------------------------------------------------
// V8Value::Decoded
//-----------------------------------------------------------------------------

struct V8Value::Decoded: WireData
{
    // IMPORTANT: maintain bitwise equivalence with managed struct V8.SplitProxy.V8Value.Decoded
};

//-----------------------------------------------------------------------------

static_assert(sizeof(V8Value::Decoded) == sizeof(V8Value::WireData));

//-----------------------------------------------------------------------------
// V8Value::FastArg
//-----------------------------------------------------------------------------

struct V8Value::FastArg final: Decoded
{
    // IMPORTANT: maintain bitwise equivalence with managed struct V8.SplitProxy.V8Value.FastArg

    ~FastArg()
    {
        switch (Type)
        {
            case Type::V8Object:
                delete pV8ObjectHandle;
                break;

            default:
                break;
        }
    }
};

//-----------------------------------------------------------------------------

static_assert(sizeof(V8Value::FastArg) == sizeof(V8Value::Decoded));

//-----------------------------------------------------------------------------
// V8Value::FastResult
//-----------------------------------------------------------------------------

struct V8Value::FastResult final: WireData
{
    // IMPORTANT: maintain bitwise equivalence with managed struct V8.SplitProxy.V8Value.FastResult

    FastResult()
    {
        Type = Type::Nonexistent;
    }

    ~FastResult()
    {
        switch (Type)
        {
            case Type::String:
                ::Memory_Free(pStringData);
                break;

            case Type::BigInt:
                ::Memory_Free(pBigIntData);
                break;

            case Type::V8Object:
                delete pV8ObjectHandle;
                break;

            default:
                break;
        }
    }
};

//-----------------------------------------------------------------------------

static_assert(sizeof(V8Value::FastResult) == sizeof(V8Value::WireData));
