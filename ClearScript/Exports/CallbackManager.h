// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.




#pragma once

//-----------------------------------------------------------------------------
// macros
//-----------------------------------------------------------------------------

#define DEFINE_CALLBACK_MANAGER(NAME, TYPE) \
    DEFINE_CALLBACK_MANAGER_INTERNAL(NAME, TYPE, NullMutex)

#define DEFINE_CONCURRENT_CALLBACK_MANAGER(NAME, TYPE) \
    DEFINE_CALLBACK_MANAGER_INTERNAL(NAME, TYPE, RecursiveMutex)

#define DEFINE_CALLBACK_MANAGER_INTERNAL(NAME, CALLBACK_TYPE, MUTEX_TYPE) \
    struct NAME##CallbackTraits final: public CallbackTraits<NAME##CallbackTraits, CALLBACK_TYPE, MUTEX_TYPE> {};

#define CALLBACK_MANAGER(NAME) \
    CallbackManager<NAME##CallbackTraits>

//-----------------------------------------------------------------------------
// CallbackTraits
//-----------------------------------------------------------------------------

template <typename TTraits, typename TCallback, typename TMutex>
class CallbackTraits final
{
    PROHIBIT_CONSTRUCT(CallbackTraits)

public:

    typedef TCallback CallbackT;

    template <typename TResult>
    static TResult CallWithLock(const std::function<TResult()>& function)
    {
        BEGIN_MUTEX_SCOPE(*ms_pMutex)

            return function();

        END_MUTEX_SCOPE
    }

private:

    // Put the mutex on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static TMutex* ms_pMutex;
};

template <typename TTraits, typename TCallback, typename TMutex>
TMutex* CallbackTraits<TTraits, TCallback, TMutex>::ms_pMutex = new TMutex;

//-----------------------------------------------------------------------------
// CallbackSlot (unused)
//-----------------------------------------------------------------------------

template <typename TTraits, size_t NIndex, typename TCallback>
class CallbackSlot final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)
};

//-----------------------------------------------------------------------------
// CallbackSlot specializations
//-----------------------------------------------------------------------------


template <typename TTraits, size_t NIndex, typename TResult>
class CallbackSlot<TTraits, NIndex, TResult()> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT();
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback()
    {
        return GetFunctionWithLock()();
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult>
std::function<TResult()>* CallbackSlot<TTraits, NIndex, TResult()>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0>
class CallbackSlot<TTraits, NIndex, TResult(T0)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0)
    {
        return GetFunctionWithLock()(a0);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0>
std::function<TResult(T0)>* CallbackSlot<TTraits, NIndex, TResult(T0)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1)
    {
        return GetFunctionWithLock()(a0, a1);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1>
std::function<TResult(T0, T1)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2)
    {
        return GetFunctionWithLock()(a0, a1, a2);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2>
std::function<TResult(T0, T1, T2)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3>
std::function<TResult(T0, T1, T2, T3)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3, T4);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3, a4);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4>
std::function<TResult(T0, T1, T2, T3, T4)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3, T4, T5);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3, a4, a5);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5>
std::function<TResult(T0, T1, T2, T3, T4, T5)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3, T4, T5, T6);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3, a4, a5, a6);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6>
std::function<TResult(T0, T1, T2, T3, T4, T5, T6)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3, T4, T5, T6, T7);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3, a4, a5, a6, a7);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7>
std::function<TResult(T0, T1, T2, T3, T4, T5, T6, T7)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3, T4, T5, T6, T7, T8);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3, a4, a5, a6, a7, a8);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8>
std::function<TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3, a4, a5, a6, a7, a8, a9);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9>
std::function<TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9, typename T10>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9, typename T10>
std::function<TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9, typename T10, typename T11>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9, typename T10, typename T11>
std::function<TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9, typename T10, typename T11, typename T12>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9, typename T10, typename T11, typename T12>
std::function<TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9, typename T10, typename T11, typename T12, typename T13>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9, typename T10, typename T11, typename T12, typename T13>
std::function<TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9, typename T10, typename T11, typename T12, typename T13, typename T14>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9, typename T10, typename T11, typename T12, typename T13, typename T14>
std::function<TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)>::ms_pFunction = nullptr;

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9, typename T10, typename T11, typename T12, typename T13, typename T14, typename T15>
class CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)> final
{
    PROHIBIT_CONSTRUCT(CallbackSlot)

public:

    typedef TResult CallbackT(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15);
    typedef std::function<CallbackT> FunctionT;

    static TResult Callback(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
    {
        return GetFunctionWithLock()(a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15);
    }

    static FunctionT GetFunctionWithLock()
    {
        return TTraits::CallWithLock<FunctionT>([]
        {
            return GetFunction();
        });
    }

    static void SetFunctionWithLock(const FunctionT& function)
    {
        TTraits::CallWithLock<void>([&function]
        {
            SetFunction(function);
        });
    }

    static FunctionT GetFunction()
    {
        return (ms_pFunction != nullptr) ? *ms_pFunction : nullptr;
    }

    static void SetFunction(const FunctionT& function)
    {
        if (ms_pFunction != nullptr)
        {
            delete ms_pFunction;
        }

        ms_pFunction = new FunctionT(function);
    }

    static bool HasFunction()
    {
        return (ms_pFunction != nullptr) ? static_cast<bool>(*ms_pFunction) : false;
    }

private:

    // Put the functor on the heap. At process shutdown, static cleanup races against GC,
    // so using non-POD static data in conjunction with managed objects is problematic.

    static FunctionT* ms_pFunction;
};

template <typename TTraits, size_t NIndex, typename TResult, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8, typename T9, typename T10, typename T11, typename T12, typename T13, typename T14, typename T15>
std::function<TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)>* CallbackSlot<TTraits, NIndex, TResult(T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)>::ms_pFunction = nullptr;

//-----------------------------------------------------------------------------
// CallbackManager
//-----------------------------------------------------------------------------

template <typename TTraits>
class CallbackManager final
{
    PROHIBIT_CONSTRUCT(CallbackManager)

public:

    typedef typename TTraits::CallbackT CallbackT;
    typedef std::function<CallbackT> FunctionT;

    static CallbackT* Alloc(const FunctionT& function)
    {
        return TTraits::CallWithLock<CallbackT*>([&function]
        {
            
            if (!HasFunction<0>())
            {
                SetFunction<0>(function);
                return GetCallback<0>();
            }
            
            if (!HasFunction<1>())
            {
                SetFunction<1>(function);
                return GetCallback<1>();
            }
            
            if (!HasFunction<2>())
            {
                SetFunction<2>(function);
                return GetCallback<2>();
            }
            
            if (!HasFunction<3>())
            {
                SetFunction<3>(function);
                return GetCallback<3>();
            }
            
            if (!HasFunction<4>())
            {
                SetFunction<4>(function);
                return GetCallback<4>();
            }
            
            if (!HasFunction<5>())
            {
                SetFunction<5>(function);
                return GetCallback<5>();
            }
            
            if (!HasFunction<6>())
            {
                SetFunction<6>(function);
                return GetCallback<6>();
            }
            
            if (!HasFunction<7>())
            {
                SetFunction<7>(function);
                return GetCallback<7>();
            }
            
            if (!HasFunction<8>())
            {
                SetFunction<8>(function);
                return GetCallback<8>();
            }
            
            if (!HasFunction<9>())
            {
                SetFunction<9>(function);
                return GetCallback<9>();
            }
            
            if (!HasFunction<10>())
            {
                SetFunction<10>(function);
                return GetCallback<10>();
            }
            
            if (!HasFunction<11>())
            {
                SetFunction<11>(function);
                return GetCallback<11>();
            }
            
            if (!HasFunction<12>())
            {
                SetFunction<12>(function);
                return GetCallback<12>();
            }
            
            if (!HasFunction<13>())
            {
                SetFunction<13>(function);
                return GetCallback<13>();
            }
            
            if (!HasFunction<14>())
            {
                SetFunction<14>(function);
                return GetCallback<14>();
            }
            
            if (!HasFunction<15>())
            {
                SetFunction<15>(function);
                return GetCallback<15>();
            }
            
            if (!HasFunction<16>())
            {
                SetFunction<16>(function);
                return GetCallback<16>();
            }
            
            if (!HasFunction<17>())
            {
                SetFunction<17>(function);
                return GetCallback<17>();
            }
            
            if (!HasFunction<18>())
            {
                SetFunction<18>(function);
                return GetCallback<18>();
            }
            
            if (!HasFunction<19>())
            {
                SetFunction<19>(function);
                return GetCallback<19>();
            }
            
            if (!HasFunction<20>())
            {
                SetFunction<20>(function);
                return GetCallback<20>();
            }
            
            if (!HasFunction<21>())
            {
                SetFunction<21>(function);
                return GetCallback<21>();
            }
            
            if (!HasFunction<22>())
            {
                SetFunction<22>(function);
                return GetCallback<22>();
            }
            
            if (!HasFunction<23>())
            {
                SetFunction<23>(function);
                return GetCallback<23>();
            }
            
            if (!HasFunction<24>())
            {
                SetFunction<24>(function);
                return GetCallback<24>();
            }
            
            if (!HasFunction<25>())
            {
                SetFunction<25>(function);
                return GetCallback<25>();
            }
            
            if (!HasFunction<26>())
            {
                SetFunction<26>(function);
                return GetCallback<26>();
            }
            
            if (!HasFunction<27>())
            {
                SetFunction<27>(function);
                return GetCallback<27>();
            }
            
            if (!HasFunction<28>())
            {
                SetFunction<28>(function);
                return GetCallback<28>();
            }
            
            if (!HasFunction<29>())
            {
                SetFunction<29>(function);
                return GetCallback<29>();
            }
            
            if (!HasFunction<30>())
            {
                SetFunction<30>(function);
                return GetCallback<30>();
            }
            
            if (!HasFunction<31>())
            {
                SetFunction<31>(function);
                return GetCallback<31>();
            }
            
            if (!HasFunction<32>())
            {
                SetFunction<32>(function);
                return GetCallback<32>();
            }
            
            if (!HasFunction<33>())
            {
                SetFunction<33>(function);
                return GetCallback<33>();
            }
            
            if (!HasFunction<34>())
            {
                SetFunction<34>(function);
                return GetCallback<34>();
            }
            
            if (!HasFunction<35>())
            {
                SetFunction<35>(function);
                return GetCallback<35>();
            }
            
            if (!HasFunction<36>())
            {
                SetFunction<36>(function);
                return GetCallback<36>();
            }
            
            if (!HasFunction<37>())
            {
                SetFunction<37>(function);
                return GetCallback<37>();
            }
            
            if (!HasFunction<38>())
            {
                SetFunction<38>(function);
                return GetCallback<38>();
            }
            
            if (!HasFunction<39>())
            {
                SetFunction<39>(function);
                return GetCallback<39>();
            }
            
            if (!HasFunction<40>())
            {
                SetFunction<40>(function);
                return GetCallback<40>();
            }
            
            if (!HasFunction<41>())
            {
                SetFunction<41>(function);
                return GetCallback<41>();
            }
            
            if (!HasFunction<42>())
            {
                SetFunction<42>(function);
                return GetCallback<42>();
            }
            
            if (!HasFunction<43>())
            {
                SetFunction<43>(function);
                return GetCallback<43>();
            }
            
            if (!HasFunction<44>())
            {
                SetFunction<44>(function);
                return GetCallback<44>();
            }
            
            if (!HasFunction<45>())
            {
                SetFunction<45>(function);
                return GetCallback<45>();
            }
            
            if (!HasFunction<46>())
            {
                SetFunction<46>(function);
                return GetCallback<46>();
            }
            
            if (!HasFunction<47>())
            {
                SetFunction<47>(function);
                return GetCallback<47>();
            }
            
            if (!HasFunction<48>())
            {
                SetFunction<48>(function);
                return GetCallback<48>();
            }
            
            if (!HasFunction<49>())
            {
                SetFunction<49>(function);
                return GetCallback<49>();
            }
            
            if (!HasFunction<50>())
            {
                SetFunction<50>(function);
                return GetCallback<50>();
            }
            
            if (!HasFunction<51>())
            {
                SetFunction<51>(function);
                return GetCallback<51>();
            }
            
            if (!HasFunction<52>())
            {
                SetFunction<52>(function);
                return GetCallback<52>();
            }
            
            if (!HasFunction<53>())
            {
                SetFunction<53>(function);
                return GetCallback<53>();
            }
            
            if (!HasFunction<54>())
            {
                SetFunction<54>(function);
                return GetCallback<54>();
            }
            
            if (!HasFunction<55>())
            {
                SetFunction<55>(function);
                return GetCallback<55>();
            }
            
            if (!HasFunction<56>())
            {
                SetFunction<56>(function);
                return GetCallback<56>();
            }
            
            if (!HasFunction<57>())
            {
                SetFunction<57>(function);
                return GetCallback<57>();
            }
            
            if (!HasFunction<58>())
            {
                SetFunction<58>(function);
                return GetCallback<58>();
            }
            
            if (!HasFunction<59>())
            {
                SetFunction<59>(function);
                return GetCallback<59>();
            }
            
            if (!HasFunction<60>())
            {
                SetFunction<60>(function);
                return GetCallback<60>();
            }
            
            if (!HasFunction<61>())
            {
                SetFunction<61>(function);
                return GetCallback<61>();
            }
            
            if (!HasFunction<62>())
            {
                SetFunction<62>(function);
                return GetCallback<62>();
            }
            
            if (!HasFunction<63>())
            {
                SetFunction<63>(function);
                return GetCallback<63>();
            }
            
            return static_cast<CallbackT*>(nullptr);
        });
    }

    static bool Free(CallbackT* pCallback)
    {
        return TTraits::CallWithLock<bool>([pCallback]
        {
            
            if (pCallback == GetCallback<0>())
            {
                _ASSERTE(HasFunction<0>());
                SetFunction<0>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<1>())
            {
                _ASSERTE(HasFunction<1>());
                SetFunction<1>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<2>())
            {
                _ASSERTE(HasFunction<2>());
                SetFunction<2>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<3>())
            {
                _ASSERTE(HasFunction<3>());
                SetFunction<3>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<4>())
            {
                _ASSERTE(HasFunction<4>());
                SetFunction<4>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<5>())
            {
                _ASSERTE(HasFunction<5>());
                SetFunction<5>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<6>())
            {
                _ASSERTE(HasFunction<6>());
                SetFunction<6>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<7>())
            {
                _ASSERTE(HasFunction<7>());
                SetFunction<7>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<8>())
            {
                _ASSERTE(HasFunction<8>());
                SetFunction<8>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<9>())
            {
                _ASSERTE(HasFunction<9>());
                SetFunction<9>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<10>())
            {
                _ASSERTE(HasFunction<10>());
                SetFunction<10>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<11>())
            {
                _ASSERTE(HasFunction<11>());
                SetFunction<11>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<12>())
            {
                _ASSERTE(HasFunction<12>());
                SetFunction<12>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<13>())
            {
                _ASSERTE(HasFunction<13>());
                SetFunction<13>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<14>())
            {
                _ASSERTE(HasFunction<14>());
                SetFunction<14>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<15>())
            {
                _ASSERTE(HasFunction<15>());
                SetFunction<15>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<16>())
            {
                _ASSERTE(HasFunction<16>());
                SetFunction<16>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<17>())
            {
                _ASSERTE(HasFunction<17>());
                SetFunction<17>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<18>())
            {
                _ASSERTE(HasFunction<18>());
                SetFunction<18>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<19>())
            {
                _ASSERTE(HasFunction<19>());
                SetFunction<19>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<20>())
            {
                _ASSERTE(HasFunction<20>());
                SetFunction<20>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<21>())
            {
                _ASSERTE(HasFunction<21>());
                SetFunction<21>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<22>())
            {
                _ASSERTE(HasFunction<22>());
                SetFunction<22>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<23>())
            {
                _ASSERTE(HasFunction<23>());
                SetFunction<23>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<24>())
            {
                _ASSERTE(HasFunction<24>());
                SetFunction<24>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<25>())
            {
                _ASSERTE(HasFunction<25>());
                SetFunction<25>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<26>())
            {
                _ASSERTE(HasFunction<26>());
                SetFunction<26>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<27>())
            {
                _ASSERTE(HasFunction<27>());
                SetFunction<27>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<28>())
            {
                _ASSERTE(HasFunction<28>());
                SetFunction<28>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<29>())
            {
                _ASSERTE(HasFunction<29>());
                SetFunction<29>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<30>())
            {
                _ASSERTE(HasFunction<30>());
                SetFunction<30>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<31>())
            {
                _ASSERTE(HasFunction<31>());
                SetFunction<31>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<32>())
            {
                _ASSERTE(HasFunction<32>());
                SetFunction<32>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<33>())
            {
                _ASSERTE(HasFunction<33>());
                SetFunction<33>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<34>())
            {
                _ASSERTE(HasFunction<34>());
                SetFunction<34>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<35>())
            {
                _ASSERTE(HasFunction<35>());
                SetFunction<35>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<36>())
            {
                _ASSERTE(HasFunction<36>());
                SetFunction<36>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<37>())
            {
                _ASSERTE(HasFunction<37>());
                SetFunction<37>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<38>())
            {
                _ASSERTE(HasFunction<38>());
                SetFunction<38>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<39>())
            {
                _ASSERTE(HasFunction<39>());
                SetFunction<39>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<40>())
            {
                _ASSERTE(HasFunction<40>());
                SetFunction<40>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<41>())
            {
                _ASSERTE(HasFunction<41>());
                SetFunction<41>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<42>())
            {
                _ASSERTE(HasFunction<42>());
                SetFunction<42>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<43>())
            {
                _ASSERTE(HasFunction<43>());
                SetFunction<43>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<44>())
            {
                _ASSERTE(HasFunction<44>());
                SetFunction<44>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<45>())
            {
                _ASSERTE(HasFunction<45>());
                SetFunction<45>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<46>())
            {
                _ASSERTE(HasFunction<46>());
                SetFunction<46>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<47>())
            {
                _ASSERTE(HasFunction<47>());
                SetFunction<47>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<48>())
            {
                _ASSERTE(HasFunction<48>());
                SetFunction<48>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<49>())
            {
                _ASSERTE(HasFunction<49>());
                SetFunction<49>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<50>())
            {
                _ASSERTE(HasFunction<50>());
                SetFunction<50>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<51>())
            {
                _ASSERTE(HasFunction<51>());
                SetFunction<51>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<52>())
            {
                _ASSERTE(HasFunction<52>());
                SetFunction<52>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<53>())
            {
                _ASSERTE(HasFunction<53>());
                SetFunction<53>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<54>())
            {
                _ASSERTE(HasFunction<54>());
                SetFunction<54>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<55>())
            {
                _ASSERTE(HasFunction<55>());
                SetFunction<55>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<56>())
            {
                _ASSERTE(HasFunction<56>());
                SetFunction<56>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<57>())
            {
                _ASSERTE(HasFunction<57>());
                SetFunction<57>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<58>())
            {
                _ASSERTE(HasFunction<58>());
                SetFunction<58>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<59>())
            {
                _ASSERTE(HasFunction<59>());
                SetFunction<59>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<60>())
            {
                _ASSERTE(HasFunction<60>());
                SetFunction<60>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<61>())
            {
                _ASSERTE(HasFunction<61>());
                SetFunction<61>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<62>())
            {
                _ASSERTE(HasFunction<62>());
                SetFunction<62>(nullptr);
                return true;
            }
            
            if (pCallback == GetCallback<63>())
            {
                _ASSERTE(HasFunction<63>());
                SetFunction<63>(nullptr);
                return true;
            }
            
            return false;
        });
    }

private:

    template <size_t NIndex>
    static CallbackT* GetCallback()
    {
        return CallbackSlot<TTraits, NIndex, CallbackT>::Callback;
    }

    template <size_t NIndex>
    static void SetFunction(const FunctionT& function)
    {
        CallbackSlot<TTraits, NIndex, CallbackT>::SetFunction(function);
    }

    template <size_t NIndex>
    static bool HasFunction()
    {
        return CallbackSlot<TTraits, NIndex, CallbackT>::HasFunction();
    }
};
