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
            [In] V8Value.Ptr pValue
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
            [In] V8Value.Ptr pValue
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
            [In] StdV8ValueArray.Ptr pArgs,
            [In] V8Value.Ptr pResult
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RawInvokeHostObjectMethod(
            [In] IntPtr pObject,
            [In] StdString.Ptr pName,
            [In] StdV8ValueArray.Ptr pArgs,
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
            [Out] [MarshalAs(UnmanagedType.I1)] out bool isModule,
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

        private static void ScheduleInvalidOperationException(StdString.Ptr pMessage)
        {
            Debug.Assert(ScheduledException == null);
            ScheduledException = new InvalidOperationException(StdString.GetValue(pMessage));
        }

        private static void ScheduleScriptEngineException(StdString.Ptr pEngineName, StdString.Ptr pMessage, StdString.Ptr pStackTrace, bool isFatal, bool executionStarted, V8Value.Ptr pScriptException, V8Value.Ptr pInnerException)
        {
            Debug.Assert(ScheduledException == null);
            var scriptException = ScriptEngine.Current?.MarshalToHost(V8Value.Get(pScriptException), false);
            var innerException = V8ProxyHelpers.MarshalExceptionToHost(V8Value.Get(pInnerException));
            ScheduledException = new ScriptEngineException(StdString.GetValue(pEngineName), StdString.GetValue(pMessage), StdString.GetValue(pStackTrace), 0, isFatal, executionStarted, scriptException, innerException);
        }

        private static void ScheduleScriptInterruptedException(StdString.Ptr pEngineName, StdString.Ptr pMessage, StdString.Ptr pStackTrace, bool isFatal, bool executionStarted, V8Value.Ptr pScriptException, V8Value.Ptr pInnerException)
        {
            Debug.Assert(ScheduledException == null);
            var scriptException = ScriptEngine.Current?.MarshalToHost(V8Value.Get(pScriptException), false);
            var innerException = V8ProxyHelpers.MarshalExceptionToHost(V8Value.Get(pInnerException));
            ScheduledException = new ScriptInterruptedException(StdString.GetValue(pEngineName), StdString.GetValue(pMessage), StdString.GetValue(pStackTrace), 0, isFatal, executionStarted, scriptException, innerException);
        }

        private static void InvokeHostAction(IntPtr pAction)
        {
            try
            {
                V8ProxyHelpers.GetHostObject<Action>(pAction)();
            }
            catch (Exception exception)
            {
                ScheduleHostException(exception);
            }
        }

        private static void ProcessArrayBufferOrViewData(IntPtr pData, IntPtr pAction)
        {
            try
            {
                V8ProxyHelpers.GetHostObject<Action<IntPtr>>(pAction)(pData);
            }
            catch (Exception exception)
            {
                ScheduleHostException(exception);
            }
        }

        private static void ProcessCpuProfile(V8CpuProfile.Ptr pProfile, IntPtr pAction)
        {
            try
            {
                V8ProxyHelpers.GetHostObject<Action<V8CpuProfile.Ptr>>(pAction)(pProfile);
            }
            catch (Exception exception)
            {
                ScheduleHostException(exception);
            }
        }

        private static IntPtr CreateV8ObjectCache()
        {
            return V8ProxyHelpers.AddRefHostObject(new Dictionary<object, IntPtr>());
        }

        private static void CacheV8Object(IntPtr pCache, IntPtr pObject, IntPtr pV8Object)
        {
            V8ProxyHelpers.GetHostObject<Dictionary<object, IntPtr>>(pCache).Add(V8ProxyHelpers.GetHostObject(pObject), pV8Object);
        }

        private static IntPtr GetCachedV8Object(IntPtr pCache, IntPtr pObject)
        {
            return V8ProxyHelpers.GetHostObject<Dictionary<object, IntPtr>>(pCache).TryGetValue(V8ProxyHelpers.GetHostObject(pObject), out IntPtr pV8Object) ? pV8Object : IntPtr.Zero;
        }

        private static void GetAllCachedV8Objects(IntPtr pCache, StdPtrArray.Ptr pV8ObjectPtrs)
        {
            var cache = V8ProxyHelpers.GetHostObject<Dictionary<object, IntPtr>>(pCache);
            StdPtrArray.CopyFromArray(pV8ObjectPtrs, cache.Values.ToArray());
        }

        private static bool RemoveV8ObjectCacheEntry(IntPtr pCache, IntPtr pObject)
        {
            return V8ProxyHelpers.GetHostObject<Dictionary<object, IntPtr>>(pCache).Remove(V8ProxyHelpers.GetHostObject(pObject));
        }

        private static IntPtr CreateDebugAgent(StdString.Ptr pName, StdString.Ptr pVersion, int port, bool remote, V8DebugCallback.Handle hCallback)
        {
            return V8ProxyHelpers.AddRefHostObject(new V8DebugAgent(StdString.GetValue(pName), StdString.GetValue(pVersion), port, remote, new V8DebugListenerImpl(hCallback)));
        }

        private static void SendDebugMessage(IntPtr pAgent, StdString.Ptr pContent)
        {
            V8ProxyHelpers.GetHostObject<V8DebugAgent>(pAgent).SendMessage(StdString.GetValue(pContent));
        }

        private static void DestroyDebugAgent(IntPtr pAgent)
        {
            V8ProxyHelpers.GetHostObject<V8DebugAgent>(pAgent).Dispose();
            V8ProxyHelpers.ReleaseHostObject(pAgent);
        }

        private static uint GetMaxScriptCacheSize()
        {
            return GetMaxCacheSizeForCategory(DocumentCategory.Script);
        }

        private static uint GetMaxModuleCacheSize()
        {
            return GetMaxCacheSizeForCategory(ModuleCategory.Standard);
        }

        private static IntPtr AddRefHostObject(IntPtr pObject)
        {
            return V8ProxyHelpers.AddRefHostObject(pObject);
        }

        private static void ReleaseHostObject(IntPtr pObject)
        {
            V8ProxyHelpers.ReleaseHostObject(pObject);
        }

        private static Invocability GetHostObjectInvocability(IntPtr pObject)
        {
            try
            {
                return V8ProxyHelpers.GetHostObjectInvocability(pObject);
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
                return default;
            }
        }

        private static void GetHostObjectNamedProperty(IntPtr pObject, StdString.Ptr pName, V8Value.Ptr pValue)
        {
            try
            {
                V8Value.Set(pValue, V8ProxyHelpers.GetHostObjectProperty(pObject, StdString.GetValue(pName)));
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        private static void GetHostObjectNamedPropertyWithCacheability(IntPtr pObject, StdString.Ptr pName, V8Value.Ptr pValue, out bool isCacheable)
        {
            try
            {
                V8Value.Set(pValue, V8ProxyHelpers.GetHostObjectProperty(pObject, StdString.GetValue(pName), out isCacheable));
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
                isCacheable = false;
            }
        }

        private static void SetHostObjectNamedProperty(IntPtr pObject, StdString.Ptr pName, V8Value.Ptr pValue)
        {
            try
            {
                V8ProxyHelpers.SetHostObjectProperty(pObject, StdString.GetValue(pName), V8Value.Get(pValue));
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        private static bool DeleteHostObjectNamedProperty(IntPtr pObject, StdString.Ptr pName)
        {
            try
            {
                return V8ProxyHelpers.DeleteHostObjectProperty(pObject, StdString.GetValue(pName));
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
                return default;
            }
        }

        private static void GetHostObjectPropertyNames(IntPtr pObject, StdStringArray.Ptr pNames)
        {
            string[] names;
            try
            {
                names = V8ProxyHelpers.GetHostObjectPropertyNames(pObject);
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
                return;
            }

            StdStringArray.CopyFromArray(pNames, names);
        }

        private static void GetHostObjectIndexedProperty(IntPtr pObject, int index, V8Value.Ptr pValue)
        {
            try
            {
                V8Value.Set(pValue, V8ProxyHelpers.GetHostObjectProperty(pObject, index));
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        private static void SetHostObjectIndexedProperty(IntPtr pObject, int index, V8Value.Ptr pValue)
        {
            try
            {
                V8ProxyHelpers.SetHostObjectProperty(pObject, index, V8Value.Get(pValue));
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        private static bool DeleteHostObjectIndexedProperty(IntPtr pObject, int index)
        {
            try
            {
                return V8ProxyHelpers.DeleteHostObjectProperty(pObject, index);
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
                return default;
            }
        }

        private static void GetHostObjectPropertyIndices(IntPtr pObject, StdInt32Array.Ptr pIndices)
        {
            int[] indices;
            try
            {
                indices = V8ProxyHelpers.GetHostObjectPropertyIndices(pObject);
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
                return;
            }

            StdInt32Array.CopyFromArray(pIndices, indices);
        }

        private static void InvokeHostObject(IntPtr pObject, bool asConstructor, StdV8ValueArray.Ptr pArgs, V8Value.Ptr pResult)
        {
            try
            {
                V8Value.Set(pResult, V8ProxyHelpers.InvokeHostObject(pObject, asConstructor, StdV8ValueArray.ToArray(pArgs)));
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        private static void InvokeHostObjectMethod(IntPtr pObject, StdString.Ptr pName, StdV8ValueArray.Ptr pArgs, V8Value.Ptr pResult)
        {
            try
            {
                V8Value.Set(pResult, V8ProxyHelpers.InvokeHostObjectMethod(pObject, StdString.GetValue(pName), StdV8ValueArray.ToArray(pArgs)));
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        private static void GetHostObjectEnumerator(IntPtr pObject, V8Value.Ptr pResult)
        {
            try
            {
                V8Value.Set(pResult, V8ProxyHelpers.GetHostObjectEnumerator(pObject));
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        private static void GetHostObjectAsyncEnumerator(IntPtr pObject, V8Value.Ptr pResult)
        {
            try
            {
                V8Value.Set(pResult, V8ProxyHelpers.GetHostObjectAsyncEnumerator(pObject));
            }
            catch (Exception exception)
            {
                ScheduleHostException(pObject, exception);
            }
        }

        private static void QueueNativeCallback(NativeCallback.Handle hCallback)
        {
            MiscHelpers.QueueNativeCallback(new NativeCallbackImpl(hCallback));
        }

        private static IntPtr CreateNativeCallbackTimer(int dueTime, int period, NativeCallback.Handle hCallback)
        {
            return V8ProxyHelpers.AddRefHostObject(new NativeCallbackTimer(dueTime, period, new NativeCallbackImpl(hCallback)));
        }

        private static bool ChangeNativeCallbackTimer(IntPtr pTimer, int dueTime, int period)
        {
            return V8ProxyHelpers.GetHostObject<NativeCallbackTimer>(pTimer).Change(dueTime, period);
        }

        private static void DestroyNativeCallbackTimer(IntPtr pTimer)
        {
            V8ProxyHelpers.GetHostObject<NativeCallbackTimer>(pTimer).Dispose();
            V8ProxyHelpers.ReleaseHostObject(pTimer);
        }

        private static void LoadModule(IntPtr pSourceDocumentInfo, StdString.Ptr pSpecifier, StdString.Ptr pResourceName, StdString.Ptr pSourceMapUrl, out ulong uniqueId, out bool isModule, StdString.Ptr pCode, out IntPtr pDocumentInfo, V8Value.Ptr pExports)
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
                isModule = default;
                pDocumentInfo = default;
                return;
            }

            StdString.SetValue(pResourceName, MiscHelpers.GetUrlOrPath(documentInfo.Uri, documentInfo.UniqueName));
            StdString.SetValue(pSourceMapUrl, MiscHelpers.GetUrlOrPath(documentInfo.SourceMapUri, string.Empty));
            uniqueId = documentInfo.UniqueId;
            isModule = documentInfo.Category == ModuleCategory.Standard;
            StdString.SetValue(pCode, code);
            pDocumentInfo = V8ProxyHelpers.AddRefHostObject(documentInfo);
            V8Value.Set(pExports, exports);
        }

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

        private static void WriteBytesToStream(IntPtr pStream, IntPtr pBytes, int count)
        {
            try
            {
                var bytes = new byte[count];
                Marshal.Copy(pBytes, bytes, 0, count);
                V8ProxyHelpers.GetHostObject<Stream>(pStream).Write(bytes, 0, count);
            }
            catch (Exception exception)
            {
                ScheduleHostException(exception);
            }
        }

        private static V8GlobalFlags GetGlobalFlags()
        {
            return V8Settings.GlobalFlags;
        }

        #endregion
    }
}
