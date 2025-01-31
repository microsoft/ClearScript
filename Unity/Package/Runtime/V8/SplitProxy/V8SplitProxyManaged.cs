// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    // ReSharper disable once PartialTypeWithSinglePart
    internal static partial class V8SplitProxyManaged
    {
        public static IntPtr MethodTable => pFunctionPtrs;

        private static IntPtr pDelegatePtrs;
        private static IntPtr pFunctionPtrs;
        private static int methodCount;

        [ThreadStatic] public static Exception ScheduledException;

        public static void Initialize()
        {
            CreateMethodTable();
        }

        public static void Teardown()
        {
            DestroyMethodTable();
        }

        private static void ScheduleHostException(IntPtr pObject, Exception exception)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.HostException_Schedule(exception.GetBaseException().Message, V8ProxyHelpers.MarshalExceptionToScript(pObject, exception)));
        }

        private static void ScheduleHostException(Exception exception)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.HostException_Schedule(exception.GetBaseException().Message, ScriptEngine.Current?.MarshalToScript(exception)));
        }

        private static uint GetMaxCacheSizeForCategory(DocumentCategory category)
        {
            return Math.Max(16U, category.MaxCacheSize);
        }

        #region method delegates

        // ReSharper disable UnusedType.Local

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawScheduleForwardingException(
            [In] V8Value.Ptr pException
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawScheduleInvalidOperationException(
            [In] StdString.Ptr pMessage
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawScheduleScriptEngineException(
            [In] StdString.Ptr pEngineName,
            [In] StdString.Ptr pMessage,
            [In] StdString.Ptr pStackTrace,
            [In] [MarshalAs(UnmanagedType.I1)] bool isFatal,
            [In] [MarshalAs(UnmanagedType.I1)] bool executionStarted,
            [In] V8Value.Ptr pScriptException,
            [In] V8Value.Ptr pInnerException
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawScheduleScriptInterruptedException(
            [In] StdString.Ptr pEngineName,
            [In] StdString.Ptr pMessage,
            [In] StdString.Ptr pStackTrace,
            [In] [MarshalAs(UnmanagedType.I1)] bool isFatal,
            [In] [MarshalAs(UnmanagedType.I1)] bool executionStarted,
            [In] V8Value.Ptr pScriptException,
            [In] V8Value.Ptr pInnerException
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawInvokeAction(
            [In] IntPtr pAction
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawProcessArrayBufferOrViewData(
            [In] IntPtr pData,
            [In] IntPtr pAction
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawProcessCpuProfile(
            [In] V8CpuProfile.Ptr pProfile,
            [In] IntPtr pAction
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr RawCreateV8ObjectCache();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawCacheV8Object(
            [In] IntPtr pCache,
            [In] IntPtr pObject,
            [In] IntPtr pV8Object
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr RawGetCachedV8Object(
            [In] IntPtr pCache,
            [In] IntPtr pObject
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawGetAllCachedV8Objects(
            [In] IntPtr pCache,
            [In] StdPtrArray.Ptr pV8ObjectPtrs
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool RawRemoveV8ObjectCacheEntry(
            [In] IntPtr pCache,
            [In] IntPtr pObject
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr RawCreateDebugAgent(
            [In] StdString.Ptr pName,
            [In] StdString.Ptr pVersion,
            [In] int port,
            [In] [MarshalAs(UnmanagedType.I1)] bool remote,
            [In] V8DebugCallback.Handle hCallback
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawSendDebugMessage(
            [In] IntPtr pAgent,
            [In] StdString.Ptr pContent
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawDestroyDebugAgent(
            [In] IntPtr pAgent
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint RawGetMaxScriptCacheSize();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint RawGetMaxModuleCacheSize();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr RawAddRefHostObject(
            [In] IntPtr pObject
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawReleaseHostObject(
            [In] IntPtr pObject
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate Invocability RawGetHostObjectInvocability(
            [In] IntPtr pObject
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawGetHostObjectNamedProperty(
            [In] IntPtr pObject,
            [In] StdString.Ptr pName,
            [In] V8Value.Ptr pValue
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawGetHostObjectNamedPropertyWithCacheability(
            [In] IntPtr pObject,
            [In] StdString.Ptr pName,
            [In] V8Value.Ptr pValue,
            [Out] [MarshalAs(UnmanagedType.I1)] out bool isCacheable
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawSetHostObjectNamedProperty(
            [In] IntPtr pObject,
            [In] StdString.Ptr pName,
            [In] V8Value.Decoded.Ptr pValue
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool RawDeleteHostObjectNamedProperty(
            [In] IntPtr pObject,
            [In] StdString.Ptr pName
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawGetHostObjectPropertyNames(
            [In] IntPtr pObject,
            [In] StdStringArray.Ptr pNames
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawGetHostObjectIndexedProperty(
            [In] IntPtr pObject,
            [In] int index,
            [In] V8Value.Ptr pValue
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawSetHostObjectIndexedProperty(
            [In] IntPtr pObject,
            [In] int index,
            V8Value.Decoded.Ptr pValue
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool RawDeleteHostObjectIndexedProperty(
            [In] IntPtr pObject,
            [In] int index
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawGetHostObjectPropertyIndices(
            [In] IntPtr pObject,
            [In] StdInt32Array.Ptr pIndices
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawInvokeHostObject(
            [In] IntPtr pObject,
            [In] [MarshalAs(UnmanagedType.I1)] bool asConstructor,
            [In] int argCount,
            [In] V8Value.Decoded.Ptr pArgs,
            [In] V8Value.Ptr pResult
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawInvokeHostObjectMethod(
            [In] IntPtr pObject,
            [In] StdString.Ptr pName,
            [In] int argCount,
            [In] V8Value.Decoded.Ptr pArgs,
            [In] V8Value.Ptr pResult
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawGetHostObjectEnumerator(
            [In] IntPtr pObject,
            [In] V8Value.Ptr pResult
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawGetHostObjectAsyncEnumerator(
            [In] IntPtr pObject,
            [In] V8Value.Ptr pResult
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawQueueNativeCallback(
            [In] NativeCallback.Handle hCallback
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr RawCreateNativeCallbackTimer(
            [In] int dueTime,
            [In] int period,
            [In] NativeCallback.Handle hCallback
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool RawChangeNativeCallbackTimer(
            [In] IntPtr pTimer,
            [In] int dueTime,
            [In] int period
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawDestroyNativeCallbackTimer(
            [In] IntPtr pTimer
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawLoadModule(
            [In] IntPtr pSourceDocumentInfo,
            [In] StdString.Ptr pSpecifier,
            [In] StdString.Ptr pResourceName,
            [In] StdString.Ptr pSourceMapUrl,
            [Out] out ulong uniqueId,
            [Out] out DocumentKind documentKind,
            [In] StdString.Ptr pCode,
            [Out] out IntPtr pDocumentInfo,
            [In] V8Value.Ptr pExports
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawCreateModuleContext(
            [In] IntPtr pDocumentInfo,
            [In] StdStringArray.Ptr pNames,
            [In] StdV8ValueArray.Ptr pValues
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawWriteBytesToStream(
            [In] IntPtr pStream,
            [In] IntPtr pBytes,
            [In] int count
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate V8GlobalFlags RawGetGlobalFlags();

        // ReSharper restore UnusedType.Local

        #endregion

        #region method table construction and teardown

        private static void CreateMethodTable()
        {
            Debug.Assert(methodCount == 0);

            (IntPtr, IntPtr)[] methodPairs =
            {
                //----------------------------------------------------------------------------
                // IMPORTANT: maintain synchronization with V8_SPLIT_PROXY_MANAGED_METHOD_LIST
                //----------------------------------------------------------------------------

                GetMethodPair<RawScheduleForwardingException>(ScheduleForwardingException),
                GetMethodPair<RawScheduleInvalidOperationException>(ScheduleInvalidOperationException),
                GetMethodPair<RawScheduleScriptEngineException>(ScheduleScriptEngineException),
                GetMethodPair<RawScheduleScriptInterruptedException>(ScheduleScriptInterruptedException),

            #if NET5_0_OR_GREATER
                (IntPtr.Zero, InvokeHostActionFastMethodPtr),
            #else
                GetMethodPair<RawInvokeAction>(InvokeHostAction),
            #endif

                GetMethodPair<RawProcessArrayBufferOrViewData>(ProcessArrayBufferOrViewData),
                GetMethodPair<RawProcessCpuProfile>(ProcessCpuProfile),
                GetMethodPair<RawCreateV8ObjectCache>(CreateV8ObjectCache),

            #if NET5_0_OR_GREATER
                (IntPtr.Zero, CacheV8ObjectFastMethodPtr),
                (IntPtr.Zero, GetCachedV8ObjectFastMethodPtr),
            #else
                GetMethodPair<RawCacheV8Object>(CacheV8Object),
                GetMethodPair<RawGetCachedV8Object>(GetCachedV8Object),
            #endif

                GetMethodPair<RawGetAllCachedV8Objects>(GetAllCachedV8Objects),
                GetMethodPair<RawRemoveV8ObjectCacheEntry>(RemoveV8ObjectCacheEntry),
                GetMethodPair<RawCreateDebugAgent>(CreateDebugAgent),
                GetMethodPair<RawSendDebugMessage>(SendDebugMessage),
                GetMethodPair<RawDestroyDebugAgent>(DestroyDebugAgent),
                GetMethodPair<RawGetMaxScriptCacheSize>(GetMaxScriptCacheSize),
                GetMethodPair<RawGetMaxModuleCacheSize>(GetMaxModuleCacheSize),

            #if NET5_0_OR_GREATER
                (IntPtr.Zero, AddRefHostObjectFastMethodPtr),
                (IntPtr.Zero, ReleaseHostObjectFastMethodPtr),
                (IntPtr.Zero, GetHostObjectInvocabilityFastMethodPtr),
            #else
                GetMethodPair<RawAddRefHostObject>(AddRefHostObject),
                GetMethodPair<RawReleaseHostObject>(ReleaseHostObject),
                GetMethodPair<RawGetHostObjectInvocability>(GetHostObjectInvocability),
            #endif

            #if NET5_0_OR_GREATER
                (IntPtr.Zero, GetHostObjectNamedPropertyFastMethodPtr),
                (IntPtr.Zero, GetHostObjectNamedPropertyWithCacheabilityFastMethodPtr),
                (IntPtr.Zero, SetHostObjectNamedPropertyFastMethodPtr),
            #else
                GetMethodPair<RawGetHostObjectNamedProperty>(GetHostObjectNamedProperty),
                GetMethodPair<RawGetHostObjectNamedPropertyWithCacheability>(GetHostObjectNamedPropertyWithCacheability),
                GetMethodPair<RawSetHostObjectNamedProperty>(SetHostObjectNamedProperty),
            #endif

                GetMethodPair<RawDeleteHostObjectNamedProperty>(DeleteHostObjectNamedProperty),
                GetMethodPair<RawGetHostObjectPropertyNames>(GetHostObjectPropertyNames),

            #if NET5_0_OR_GREATER
                (IntPtr.Zero, GetHostObjectIndexedPropertyFastMethodPtr),
                (IntPtr.Zero, SetHostObjectIndexedPropertyFastMethodPtr),
            #else
                GetMethodPair<RawGetHostObjectIndexedProperty>(GetHostObjectIndexedProperty),
                GetMethodPair<RawSetHostObjectIndexedProperty>(SetHostObjectIndexedProperty),
            #endif

                GetMethodPair<RawDeleteHostObjectIndexedProperty>(DeleteHostObjectIndexedProperty),
                GetMethodPair<RawGetHostObjectPropertyIndices>(GetHostObjectPropertyIndices),

            #if NET5_0_OR_GREATER
                (IntPtr.Zero, InvokeHostObjectFastMethodPtr),
                (IntPtr.Zero, InvokeHostObjectMethodFastMethodPtr),
            #else
                GetMethodPair<RawInvokeHostObject>(InvokeHostObject),
                GetMethodPair<RawInvokeHostObjectMethod>(InvokeHostObjectMethod),
            #endif

                GetMethodPair<RawGetHostObjectEnumerator>(GetHostObjectEnumerator),
                GetMethodPair<RawGetHostObjectAsyncEnumerator>(GetHostObjectAsyncEnumerator),
                GetMethodPair<RawQueueNativeCallback>(QueueNativeCallback),
                GetMethodPair<RawCreateNativeCallbackTimer>(CreateNativeCallbackTimer),
                GetMethodPair<RawChangeNativeCallbackTimer>(ChangeNativeCallbackTimer),
                GetMethodPair<RawDestroyNativeCallbackTimer>(DestroyNativeCallbackTimer),
                GetMethodPair<RawLoadModule>(LoadModule),
                GetMethodPair<RawCreateModuleContext>(CreateModuleContext),
                GetMethodPair<RawWriteBytesToStream>(WriteBytesToStream),
                GetMethodPair<RawGetGlobalFlags>(GetGlobalFlags)
            };

            methodCount = methodPairs.Length;
            pDelegatePtrs = Marshal.AllocCoTaskMem(methodCount * IntPtr.Size);
            pFunctionPtrs = Marshal.AllocCoTaskMem(methodCount * IntPtr.Size);

            for (var index = 0; index < methodCount; index++)
            {
                var (pDelegate, pFunction) = methodPairs[index];
                Marshal.WriteIntPtr(pDelegatePtrs, index * IntPtr.Size, pDelegate);
                Marshal.WriteIntPtr(pFunctionPtrs, index * IntPtr.Size, pFunction);
            }
        }

        private static void DestroyMethodTable()
        {
            Debug.Assert(methodCount > 0);

            for (var index = 0; index < methodCount; index++)
            {
                var pDelegate = Marshal.ReadIntPtr(pDelegatePtrs, index * IntPtr.Size);
                if (pDelegate != IntPtr.Zero)
                {
                    V8ProxyHelpers.ReleaseHostObject(pDelegate);
                }
            }

            Marshal.FreeCoTaskMem(pDelegatePtrs);
            Marshal.FreeCoTaskMem(pFunctionPtrs);

            methodCount = 0;
            pDelegatePtrs = IntPtr.Zero;
            pFunctionPtrs = IntPtr.Zero;
        }

        private static (IntPtr, IntPtr) GetMethodPair<T>(T del)
        {
            return (V8ProxyHelpers.AddRefHostObject(del), Marshal.GetFunctionPointerForDelegate((Delegate)(object)del));
        }

        #endregion

        #region method table implementation

        [AOT.MonoPInvokeCallback(typeof(RawScheduleForwardingException))]
        private static void ScheduleForwardingException(V8Value.Ptr pException)
        {
            Debug.Assert(ScheduledException == null);

            var exception = V8ProxyHelpers.MarshalExceptionToHost(V8Value.Get(pException));
            if (exception is ScriptEngineException scriptEngineException)
            {
                ScheduledException = new ScriptEngineException(scriptEngineException.EngineName, scriptEngineException.Message, scriptEngineException.ErrorDetails, scriptEngineException.HResult, scriptEngineException.IsFatal, scriptEngineException.ExecutionStarted, scriptEngineException.ScriptExceptionAsObject, scriptEngineException);
            }
            else if (exception is ScriptInterruptedException scriptInterruptedException)
            {
                ScheduledException = new ScriptInterruptedException(scriptInterruptedException.EngineName, scriptInterruptedException.Message, scriptInterruptedException.ErrorDetails, scriptInterruptedException.HResult, scriptInterruptedException.IsFatal, scriptInterruptedException.ExecutionStarted, scriptInterruptedException.ScriptExceptionAsObject, scriptInterruptedException);
            }
            else
            {
                ScheduledException = exception;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawScheduleInvalidOperationException))]
        private static void ScheduleInvalidOperationException(StdString.Ptr pMessage)
        {
            Debug.Assert(ScheduledException == null);
            ScheduledException = new InvalidOperationException(StdString.GetValue(pMessage));
        }

        [AOT.MonoPInvokeCallback(typeof(RawScheduleScriptEngineException))]
        private static void ScheduleScriptEngineException(StdString.Ptr pEngineName, StdString.Ptr pMessage, StdString.Ptr pStackTrace, bool isFatal, bool executionStarted, V8Value.Ptr pScriptException, V8Value.Ptr pInnerException)
        {
            Debug.Assert(ScheduledException == null);
            var scriptException = ScriptEngine.Current?.MarshalToHost(V8Value.Get(pScriptException), false);
            var innerException = V8ProxyHelpers.MarshalExceptionToHost(V8Value.Get(pInnerException));
            ScheduledException = new ScriptEngineException(StdString.GetValue(pEngineName), StdString.GetValue(pMessage), StdString.GetValue(pStackTrace), 0, isFatal, executionStarted, scriptException, innerException);
        }

        [AOT.MonoPInvokeCallback(typeof(RawScheduleScriptInterruptedException))]
        private static void ScheduleScriptInterruptedException(StdString.Ptr pEngineName, StdString.Ptr pMessage, StdString.Ptr pStackTrace, bool isFatal, bool executionStarted, V8Value.Ptr pScriptException, V8Value.Ptr pInnerException)
        {
            Debug.Assert(ScheduledException == null);
            var scriptException = ScriptEngine.Current?.MarshalToHost(V8Value.Get(pScriptException), false);
            var innerException = V8ProxyHelpers.MarshalExceptionToHost(V8Value.Get(pInnerException));
            ScheduledException = new ScriptInterruptedException(StdString.GetValue(pEngineName), StdString.GetValue(pMessage), StdString.GetValue(pStackTrace), 0, isFatal, executionStarted, scriptException, innerException);
        }

        [AOT.MonoPInvokeCallback(typeof(RawInvokeAction))]
        private static void InvokeHostAction(IntPtr pAction)
        {
            try
            {
                object method = V8ProxyHelpers.GetHostObject(pAction);
                ((Action)method)();
            }
            catch (Exception exception)
            {
                ScheduleHostException(exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawProcessArrayBufferOrViewData))]
        private static void ProcessArrayBufferOrViewData(IntPtr pData, IntPtr pAction)
        {
            try
            {
                object method = V8ProxyHelpers.GetHostObject(pAction);
                ((Action<IntPtr>)method)(pData);
            }
            catch (Exception exception)
            {
                ScheduleHostException(exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawProcessCpuProfile))]
        private static void ProcessCpuProfile(V8CpuProfile.Ptr pProfile, IntPtr pAction)
        {
            try
            {
                object method = V8ProxyHelpers.GetHostObject(pAction);
                ((Action<V8CpuProfile.Ptr>)method)(pProfile);
            }
            catch (Exception exception)
            {
                ScheduleHostException(exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawCreateV8ObjectCache))]
        private static IntPtr CreateV8ObjectCache()
        {
            return V8ProxyHelpers.AddRefHostObject(new Dictionary<object, IntPtr>());
        }

        [AOT.MonoPInvokeCallback(typeof(RawCacheV8Object))]
        private static void CacheV8Object(IntPtr pCache, IntPtr pObject, IntPtr pV8Object)
        {
            object cache = V8ProxyHelpers.GetHostObject(pCache);
            object key = V8ProxyHelpers.GetHostObject(pObject);
            ((Dictionary<object, IntPtr>)cache).Add(key, pV8Object);
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetCachedV8Object))]
        private static IntPtr GetCachedV8Object(IntPtr pCache, IntPtr pObject)
        {
            object cache = V8ProxyHelpers.GetHostObject(pCache);
            object key = V8ProxyHelpers.GetHostObject(pObject);
            return ((Dictionary<object, IntPtr>)cache).TryGetValue(key, out IntPtr pV8Object) ? pV8Object : IntPtr.Zero;
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetAllCachedV8Objects))]
        private static void GetAllCachedV8Objects(IntPtr pCache, StdPtrArray.Ptr pV8ObjectPtrs)
        {
            object cache = V8ProxyHelpers.GetHostObject(pCache);
            StdPtrArray.CopyFromArray(pV8ObjectPtrs, ((Dictionary<object, IntPtr>)cache).Values.ToArray());
        }

        [AOT.MonoPInvokeCallback(typeof(RawRemoveV8ObjectCacheEntry))]
        private static bool RemoveV8ObjectCacheEntry(IntPtr pCache, IntPtr pObject)
        {
            object cache = V8ProxyHelpers.GetHostObject(pCache);
            object key = V8ProxyHelpers.GetHostObject(pObject);
            return ((Dictionary<object, IntPtr>)cache).Remove(key);
        }

        [AOT.MonoPInvokeCallback(typeof(RawCreateDebugAgent))]
        private static IntPtr CreateDebugAgent(StdString.Ptr pName, StdString.Ptr pVersion, int port, bool remote, V8DebugCallback.Handle hCallback)
        {
            string name = StdString.GetValue(pName);
            string version = StdString.GetValue(pVersion);
            IV8DebugListener listener = new V8DebugListenerImpl(hCallback);
            V8DebugAgent agent = new V8DebugAgent(name, version, port, remote, listener);
            return V8ProxyHelpers.AddRefHostObject(agent);
        }

        [AOT.MonoPInvokeCallback(typeof(RawSendDebugMessage))]
        private static void SendDebugMessage(IntPtr pAgent, StdString.Ptr pContent)
        {
            object agent = V8ProxyHelpers.GetHostObject(pAgent);
            string content = StdString.GetValue(pContent);
            ((V8DebugAgent)agent).SendMessage(content);
        }

        [AOT.MonoPInvokeCallback(typeof(RawDestroyDebugAgent))]
        private static void DestroyDebugAgent(IntPtr pAgent)
        {
            object agent = V8ProxyHelpers.GetHostObject(pAgent);
            ((V8DebugAgent)agent).Dispose();
            V8ProxyHelpers.ReleaseHostObject(pAgent);
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetMaxScriptCacheSize))]
        private static uint GetMaxScriptCacheSize()
        {
            return GetMaxCacheSizeForCategory(DocumentCategory.Script);
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetMaxModuleCacheSize))]
        private static uint GetMaxModuleCacheSize()
        {
            return GetMaxCacheSizeForCategory(ModuleCategory.Standard);
        }

        [AOT.MonoPInvokeCallback(typeof(RawAddRefHostObject))]
        private static IntPtr AddRefHostObject(IntPtr pObject)
        {
            object obj = V8ProxyHelpers.GetHostObject(pObject);
            return V8ProxyHelpers.AddRefHostObject(obj);
        }

        [AOT.MonoPInvokeCallback(typeof(RawReleaseHostObject))]
        private static void ReleaseHostObject(IntPtr pObject)
        {
            GCHandle handle = GCHandle.FromIntPtr(pObject);
            handle.Free();
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetHostObjectInvocability))]
        private static Invocability GetHostObjectInvocability(IntPtr pObject)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is InvokeHostObject)
                {
                    return Invocability.Delegate;
                }
                else
#endif
                {
                    return V8ProxyHelpers.GetHostObjectInvocability(obj);
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
                return default;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetHostObjectNamedProperty))]
        private static void GetHostObjectNamedProperty(IntPtr pObject, StdString.Ptr pName, V8Value.Ptr pValue)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is IV8HostObject hostObject)
                {
                    var name = new StdString(pName);
                    var value = new V8Value(pValue);
                    hostObject.GetNamedProperty(name, value, out _);
                }
                else
#endif
                {
                    string name = StdString.GetValue(pName);
                    V8Value.Set(pValue, V8ProxyHelpers.GetHostObjectProperty(obj, name));
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetHostObjectNamedPropertyWithCacheability))]
        private static void GetHostObjectNamedPropertyWithCacheability(IntPtr pObject, StdString.Ptr pName, V8Value.Ptr pValue, out bool isCacheable)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is IV8HostObject hostObject)
                {
                    var name = new StdString(pName);
                    var value = new V8Value(pValue);
                    hostObject.GetNamedProperty(name, value, out isCacheable);
                }
                else
#endif
                {
                    string name = StdString.GetValue(pName);
                    V8Value.Set(pValue, V8ProxyHelpers.GetHostObjectProperty(obj, name, out isCacheable));
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
                isCacheable = false;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawSetHostObjectNamedProperty))]
        private static void SetHostObjectNamedProperty(IntPtr pObject, StdString.Ptr pName, V8Value.Decoded.Ptr pValue)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is IV8HostObject hostObject)
                {
                    unsafe
                    {
                        var name = new StdString(pName);
                        var value = *(V8Value.Decoded*)(IntPtr)pValue;
                        hostObject.SetNamedProperty(name, value);

                        if (value.Type == V8Value.Type.V8Object)
                        {
                            V8SplitProxyNative.Instance.V8Entity_DestroyHandle((V8Entity.Handle)value.PtrOrHandle);
                        }
                    }
                }
                else
#endif
                {
                    string name = StdString.GetValue(pName);
                    object value = V8Value.Decoded.Get(pValue, 0);
                    V8ProxyHelpers.SetHostObjectProperty(obj, name, value);
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawDeleteHostObjectNamedProperty))]
        private static bool DeleteHostObjectNamedProperty(IntPtr pObject, StdString.Ptr pName)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is IV8HostObject hostObject)
                {
                    var name = new StdString(pName);
                    return hostObject.DeleteNamedProperty(name);
                }
                else
#endif
                {
                    string name = StdString.GetValue(pName);
                    return V8ProxyHelpers.DeleteHostObjectProperty(obj, name);
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
                return default;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetHostObjectPropertyNames))]
        private static void GetHostObjectPropertyNames(IntPtr pObject, StdStringArray.Ptr pNames)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is IV8HostObject hostObject)
                {
                    var names = new StdStringArray(pNames);
                    hostObject.GetNamedPropertyNames(names);
                }
                else
#endif
                {
                    string[] names = V8ProxyHelpers.GetHostObjectPropertyNames(obj);
                    StdStringArray.CopyFromArray(pNames, names);
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetHostObjectIndexedProperty))]
        private static void GetHostObjectIndexedProperty(IntPtr pObject, int index, V8Value.Ptr pValue)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is IV8HostObject hostObject)
                {
                    var value = new V8Value(pValue);
                    hostObject.GetIndexedProperty(index, value);
                }
                else
#endif
                {
                    object value = V8ProxyHelpers.GetHostObjectProperty(obj, index);
                    V8Value.Set(pValue, value);
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawSetHostObjectIndexedProperty))]
        private static void SetHostObjectIndexedProperty(IntPtr pObject, int index, V8Value.Decoded.Ptr pValue)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is IV8HostObject hostObject)
                {
                    unsafe
                    {
                        var value = *(V8Value.Decoded*)(IntPtr)pValue;
                        hostObject.SetIndexedProperty(index, value);

                        if (value.Type == V8Value.Type.V8Object)
                        {
                            V8SplitProxyNative.Instance.V8Entity_DestroyHandle((V8Entity.Handle)value.PtrOrHandle);
                        }
                    }
                }
                else
#endif
                {
                    object value = V8Value.Decoded.Get(pValue, 0);
                    V8ProxyHelpers.SetHostObjectProperty(obj, index, value);
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawDeleteHostObjectIndexedProperty))]
        private static bool DeleteHostObjectIndexedProperty(IntPtr pObject, int index)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is IV8HostObject hostObject)
                {
                    return hostObject.DeleteIndexedProperty(index);
                }
                else
#endif
                {
                    return V8ProxyHelpers.DeleteHostObjectProperty(obj, index);
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
                return default;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetHostObjectPropertyIndices))]
        private static void GetHostObjectPropertyIndices(IntPtr pObject, StdInt32Array.Ptr pIndices)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is IV8HostObject hostObject)
                {
                    var indices = new StdInt32Array(pIndices);
                    hostObject.GetIndexedPropertyIndices(indices);
                }
                else
#endif
                {
                    int[] indices = V8ProxyHelpers.GetHostObjectPropertyIndices(obj);
                    StdInt32Array.CopyFromArray(pIndices, indices);
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawInvokeHostObject))]
        private static void InvokeHostObject(IntPtr pObject, bool asConstructor, int argCount, V8Value.Decoded.Ptr pArgs, V8Value.Ptr pResult)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is InvokeHostObject method)
                {
                    unsafe
                    {
                        var args = new ReadOnlySpan<V8Value.Decoded>((V8Value.Decoded*)(IntPtr)pArgs, argCount);
                        var result = new V8Value(pResult);
                        method(args, result);

                        for (int i = 0; i < argCount; i++)
                        {
                            if (args[i].Type == V8Value.Type.V8Object)
                            {
                                V8SplitProxyNative.Instance.V8Entity_DestroyHandle((V8Entity.Handle)args[i].PtrOrHandle);
                            }
                        }
                    }
                }
                else
#endif
                {
                    object[] args = V8Value.Decoded.ToArray(argCount, pArgs);
                    object result = V8ProxyHelpers.InvokeHostObject(obj, asConstructor, args);
                    V8Value.Set(pResult, result);
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawInvokeHostObjectMethod))]
        private static void InvokeHostObjectMethod(IntPtr pObject, StdString.Ptr pName, int argCount, V8Value.Decoded.Ptr pArgs, V8Value.Ptr pResult)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is IV8HostObject hostObject)
                {
                    unsafe
                    {
                        var name = new StdString(pName);
                        var args = new ReadOnlySpan<V8Value.Decoded>((V8Value.Decoded*)(IntPtr)pArgs, argCount);
                        var result = new V8Value(pResult);
                        hostObject.InvokeMethod(name, args, result);

                        for (int i = 0; i < argCount; i++)
                        {
                            if (args[i].Type == V8Value.Type.V8Object)
                            {
                                V8SplitProxyNative.Instance.V8Entity_DestroyHandle((V8Entity.Handle)args[i].PtrOrHandle);
                            }
                        }
                    }
                }
                else
#endif
                {
                    string name = StdString.GetValue(pName);
                    object[] args = V8Value.Decoded.ToArray(argCount, pArgs);
                    object result = V8ProxyHelpers.InvokeHostObjectMethod(obj, name, args);
                    V8Value.Set(pResult, result);
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetHostObjectEnumerator))]
        private static void GetHostObjectEnumerator(IntPtr pObject, V8Value.Ptr pResult)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is IV8HostObject hostObject)
                {
                    var result = new V8Value(pResult);
                    hostObject.GetEnumerator(result);
                }
                else
#endif
                {
                    object result = V8ProxyHelpers.GetHostObjectEnumerator(obj);
                    V8Value.Set(pResult, result);
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetHostObjectAsyncEnumerator))]
        private static void GetHostObjectAsyncEnumerator(IntPtr pObject, V8Value.Ptr pResult)
        {
            try
            {
                object obj = V8ProxyHelpers.GetHostObject(pObject);

#if NETCOREAPP || NETSTANDARD
                if (obj is IV8HostObject hostObject)
                {
                    var result = new V8Value(pResult);
                    hostObject.GetAsyncEnumerator(result);
                }
                else
#endif
                {
                    object result = V8ProxyHelpers.GetHostObjectAsyncEnumerator(obj);
                    V8Value.Set(pResult, result);
                }
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawQueueNativeCallback))]
        private static void QueueNativeCallback(NativeCallback.Handle hCallback)
        {
            MiscHelpers.QueueNativeCallback(new NativeCallbackImpl(hCallback));
        }

        [AOT.MonoPInvokeCallback(typeof(RawCreateNativeCallbackTimer))]
        private static IntPtr CreateNativeCallbackTimer(int dueTime, int period, NativeCallback.Handle hCallback)
        {
            return V8ProxyHelpers.AddRefHostObject(new NativeCallbackTimer(dueTime, period, new NativeCallbackImpl(hCallback)));
        }

        [AOT.MonoPInvokeCallback(typeof(RawChangeNativeCallbackTimer))]
        private static bool ChangeNativeCallbackTimer(IntPtr pTimer, int dueTime, int period)
        {
            return V8ProxyHelpers.GetHostObject<NativeCallbackTimer>(pTimer).Change(dueTime, period);
        }

        [AOT.MonoPInvokeCallback(typeof(RawDestroyNativeCallbackTimer))]
        private static void DestroyNativeCallbackTimer(IntPtr pTimer)
        {
            V8ProxyHelpers.GetHostObject<NativeCallbackTimer>(pTimer).Dispose();
            V8ProxyHelpers.ReleaseHostObject(pTimer);
        }

        [AOT.MonoPInvokeCallback(typeof(RawLoadModule))]
        private static void LoadModule(IntPtr pSourceDocumentInfo, StdString.Ptr pSpecifier, StdString.Ptr pResourceName, StdString.Ptr pSourceMapUrl, out ulong uniqueId, out DocumentKind documentKind, StdString.Ptr pCode, out IntPtr pDocumentInfo, V8Value.Ptr pExports)
        {
            string code;
            UniqueDocumentInfo documentInfo;
            object exports;

            try
            {
                code = V8ProxyHelpers.LoadModule(pSourceDocumentInfo, StdString.GetValue(pSpecifier), out documentInfo, out exports);
            }
            catch (Exception exception)
            {
                ScheduleHostException(exception);
                uniqueId = default;
                documentKind = default;
                pDocumentInfo = default;
                return;
            }

            StdString.SetValue(pResourceName, MiscHelpers.GetUrlOrPath(documentInfo.Uri, documentInfo.UniqueName));
            StdString.SetValue(pSourceMapUrl, MiscHelpers.GetUrlOrPath(documentInfo.SourceMapUri, string.Empty));
            uniqueId = documentInfo.UniqueId;
            documentKind = documentInfo.Category.Kind;
            StdString.SetValue(pCode, code);
            pDocumentInfo = V8ProxyHelpers.AddRefHostObject(documentInfo);
            V8Value.Set(pExports, exports);
        }

        [AOT.MonoPInvokeCallback(typeof(RawCreateModuleContext))]
        private static void CreateModuleContext(IntPtr pDocumentInfo, StdStringArray.Ptr pNames, StdV8ValueArray.Ptr pValues)
        {
            IDictionary<string, object> context;
            try
            {
                context = V8ProxyHelpers.CreateModuleContext(pDocumentInfo);
            }
            catch (Exception exception)
            {
                ScheduleHostException(exception);
                return;
            }

            if (context == null)
            {
                StdStringArray.SetElementCount(pNames, 0);
                StdV8ValueArray.SetElementCount(pValues, 0);
            }
            else
            {
                StdStringArray.CopyFromArray(pNames, context.Keys.ToArray());
                StdV8ValueArray.CopyFromArray(pValues, context.Values.ToArray());
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawWriteBytesToStream))]
        private static void WriteBytesToStream(IntPtr pStream, IntPtr pBytes, int count)
        {
            try
            {
                Stream stream = V8ProxyHelpers.GetHostObject<Stream>(pStream);

#if NETCOREAPP || NETSTANDARD
                unsafe
                {
                    var bytes = new ReadOnlySpan<byte>((byte*)pBytes, count);
                    stream.Write(bytes);
                }
#else
                var bytes = new byte[count];
                Marshal.Copy(pBytes, bytes, 0, count);
#endif
            }
            catch (Exception exception)
            {
                ScheduleHostException(exception);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RawGetGlobalFlags))]
        private static V8GlobalFlags GetGlobalFlags()
        {
            return V8Settings.GlobalFlags;
        }

        #endregion
    }
}
