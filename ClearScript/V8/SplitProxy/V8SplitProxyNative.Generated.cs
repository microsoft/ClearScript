// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.









// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal static partial class V8SplitProxyNative
    {
        private static IV8SplitProxyNative CreateInstance()
        {
            var architecture = RuntimeInformation.ProcessArchitecture;

            

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                

                if (architecture == Architecture.X86)
                {
                    return new Impl_Windows_X86();
                }

                

                if (architecture == Architecture.X64)
                {
                    return new Impl_Windows_X64();
                }

                

                if (architecture == Architecture.Arm64)
                {
                    return new Impl_Windows_Arm64();
                }

                

                throw new PlatformNotSupportedException("Unsupported process architecture");
            }

            

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                

                if (architecture == Architecture.X64)
                {
                    return new Impl_Linux_X64();
                }

                

                if (architecture == Architecture.Arm64)
                {
                    return new Impl_Linux_Arm64();
                }

                

                if (architecture == Architecture.Arm)
                {
                    return new Impl_Linux_Arm();
                }

                

                throw new PlatformNotSupportedException("Unsupported process architecture");
            }

            

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                

                if (architecture == Architecture.X64)
                {
                    return new Impl_OSX_X64();
                }

                

                if (architecture == Architecture.Arm64)
                {
                    return new Impl_OSX_Arm64();
                }

                

                throw new PlatformNotSupportedException("Unsupported process architecture");
            }

            

            throw new PlatformNotSupportedException("Unsupported operating system");
        }

        

        #region Nested type: Impl_Windows_X86

        private sealed class Impl_Windows_X86 : IV8SplitProxyNative
        {
            public static readonly IV8SplitProxyNative Instance = new Impl_Windows_X86();

            #region IV8SplitProxyNative implementation

            #region initialization

            IntPtr IV8SplitProxyNative.V8SplitProxyManaged_SetMethodTable(IntPtr pMethodTable)
            {
                return V8SplitProxyManaged_SetMethodTable(pMethodTable);
            }

            string IV8SplitProxyNative.V8SplitProxyNative_GetVersion()
            {
                return Marshal.PtrToStringUni(V8SplitProxyNative_GetVersion());
            }

            void IV8SplitProxyNative.V8Environment_InitializeICU(IntPtr pICUData, uint size)
            {
                V8Environment_InitializeICU(pICUData, size);
            }

            #endregion

            #region memory methods

            IntPtr IV8SplitProxyNative.Memory_Allocate(UIntPtr size)
            {
                return Memory_Allocate(size);
            }

            IntPtr IV8SplitProxyNative.Memory_AllocateZeroed(UIntPtr size)
            {
                return Memory_AllocateZeroed(size);
            }

            void IV8SplitProxyNative.Memory_Free(IntPtr pMemory)
            {
                Memory_Free(pMemory);
            }

            #endregion

            #region StdString methods

            StdString.Ptr IV8SplitProxyNative.StdString_New(string value)
            {
                return StdString_New(value, value.Length);
            }

            string IV8SplitProxyNative.StdString_GetValue(StdString.Ptr pString)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue>(StdString.Ptr pString, Func<IntPtr, int, TValue> factory)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue, TArg>(StdString.Ptr pString, Func<IntPtr, int, TArg, TValue> factory, in TArg arg)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length, arg);
            }

            void IV8SplitProxyNative.StdString_SetValue(StdString.Ptr pString, string value)
            {
                StdString_SetValue(pString, value, value.Length);
            }

            void IV8SplitProxyNative.StdString_Delete(StdString.Ptr pString)
            {
                StdString_Delete(pString);
            }

            #endregion

            #region StdStringArray methods

            StdStringArray.Ptr IV8SplitProxyNative.StdStringArray_New(int elementCount)
            {
                return StdStringArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdStringArray_GetElementCount(StdStringArray.Ptr pArray)
            {
                return StdStringArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdStringArray_SetElementCount(StdStringArray.Ptr pArray, int elementCount)
            {
                StdStringArray_SetElementCount(pArray, elementCount);
            }

            string IV8SplitProxyNative.StdStringArray_GetElement(StdStringArray.Ptr pArray, int index)
            {
                var pValue = StdStringArray_GetElement(pArray, index, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            void IV8SplitProxyNative.StdStringArray_SetElement(StdStringArray.Ptr pArray, int index, string value)
            {
                StdStringArray_SetElement(pArray, index, value, value.Length);
            }

            void IV8SplitProxyNative.StdStringArray_Delete(StdStringArray.Ptr pArray)
            {
                StdStringArray_Delete(pArray);
            }

            #endregion

            #region StdByteArray methods

            StdByteArray.Ptr IV8SplitProxyNative.StdByteArray_New(int elementCount)
            {
                return StdByteArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdByteArray_GetElementCount(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_SetElementCount(StdByteArray.Ptr pArray, int elementCount)
            {
                StdByteArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdByteArray_GetData(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_Delete(StdByteArray.Ptr pArray)
            {
                StdByteArray_Delete(pArray);
            }

            #endregion

            #region StdInt32Array methods

            StdInt32Array.Ptr IV8SplitProxyNative.StdInt32Array_New(int elementCount)
            {
                return StdInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdInt32Array_GetElementCount(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_SetElementCount(StdInt32Array.Ptr pArray, int elementCount)
            {
                StdInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdInt32Array_GetData(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_Delete(StdInt32Array.Ptr pArray)
            {
                StdInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt32Array methods

            StdUInt32Array.Ptr IV8SplitProxyNative.StdUInt32Array_New(int elementCount)
            {
                return StdUInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt32Array_GetElementCount(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_SetElementCount(StdUInt32Array.Ptr pArray, int elementCount)
            {
                StdUInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt32Array_GetData(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_Delete(StdUInt32Array.Ptr pArray)
            {
                StdUInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt64Array methods

            StdUInt64Array.Ptr IV8SplitProxyNative.StdUInt64Array_New(int elementCount)
            {
                return StdUInt64Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt64Array_GetElementCount(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_SetElementCount(StdUInt64Array.Ptr pArray, int elementCount)
            {
                StdUInt64Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt64Array_GetData(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_Delete(StdUInt64Array.Ptr pArray)
            {
                StdUInt64Array_Delete(pArray);
            }

            #endregion

            #region StdPtrArray methods

            StdPtrArray.Ptr IV8SplitProxyNative.StdPtrArray_New(int elementCount)
            {
                return StdPtrArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdPtrArray_GetElementCount(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_SetElementCount(StdPtrArray.Ptr pArray, int elementCount)
            {
                StdPtrArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdPtrArray_GetData(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_Delete(StdPtrArray.Ptr pArray)
            {
                StdPtrArray_Delete(pArray);
            }

            #endregion

            #region StdV8ValueArray methods

            StdV8ValueArray.Ptr IV8SplitProxyNative.StdV8ValueArray_New(int elementCount)
            {
                return StdV8ValueArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdV8ValueArray_GetElementCount(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_SetElementCount(StdV8ValueArray.Ptr pArray, int elementCount)
            {
                StdV8ValueArray_SetElementCount(pArray, elementCount);
            }

            V8Value.Ptr IV8SplitProxyNative.StdV8ValueArray_GetData(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_Delete(StdV8ValueArray.Ptr pArray)
            {
                StdV8ValueArray_Delete(pArray);
            }

            #endregion

            #region V8Value methods

            V8Value.Ptr IV8SplitProxyNative.V8Value_New()
            {
                return V8Value_New();
            }

            void IV8SplitProxyNative.V8Value_SetNonexistent(V8Value.Ptr pV8Value)
            {
                V8Value_SetNonexistent(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetUndefined(V8Value.Ptr pV8Value)
            {
                V8Value_SetUndefined(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetNull(V8Value.Ptr pV8Value)
            {
                V8Value_SetNull(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetBoolean(V8Value.Ptr pV8Value, bool value)
            {
                V8Value_SetBoolean(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetNumber(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetNumber(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetString(V8Value.Ptr pV8Value, string value)
            {
                V8Value_SetString(pV8Value, value, value.Length);
            }

            void IV8SplitProxyNative.V8Value_SetDateTime(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetDateTime(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetBigInt(V8Value.Ptr pV8Value, int signBit, byte[] bytes)
            {
                V8Value_SetBigInt(pV8Value, signBit, bytes, bytes.Length);
            }

            void IV8SplitProxyNative.V8Value_SetV8Object(V8Value.Ptr pV8Value, V8Object.Handle hObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetV8Object(pV8Value, hObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_SetHostObject(V8Value.Ptr pV8Value, IntPtr pObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetHostObject(pV8Value, pObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_Decode(V8Value.Ptr pV8Value, out V8Value.Decoded decoded)
            {
                V8Value_Decode(pV8Value, out decoded);
            }

            void IV8SplitProxyNative.V8Value_Delete(V8Value.Ptr pV8Value)
            {
                V8Value_Delete(pV8Value);
            }

            #endregion

            #region V8CpuProfile methods

            void IV8SplitProxyNative.V8CpuProfile_GetInfo(V8CpuProfile.Ptr pProfile, V8Entity.Handle hEntity, out string name, out ulong startTimestamp, out ulong endTimestamp, out int sampleCount, out V8CpuProfile.Node.Ptr pRootNode)
            {
                using (var nameScope = StdString.CreateScope())
                {
                    V8CpuProfile_GetInfo(pProfile, hEntity, nameScope.Value, out startTimestamp, out endTimestamp, out sampleCount, out pRootNode);
                    name = StdString.GetValue(nameScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8CpuProfile_GetSample(V8CpuProfile.Ptr pProfile, int index, out ulong nodeId, out ulong timestamp)
            {
                return V8CpuProfile_GetSample(pProfile, index, out nodeId, out timestamp);
            }

            void IV8SplitProxyNative.V8CpuProfileNode_GetInfo(V8CpuProfile.Node.Ptr pNode, V8Entity.Handle hEntity, out ulong nodeId, out long scriptId, out string scriptName, out string functionName, out string bailoutReason, out long lineNumber, out long columnNumber, out ulong hitCount, out uint hitLineCount, out int childCount)
            {
                using (var scriptNameScope = StdString.CreateScope())
                {
                    using (var functionNameScope = StdString.CreateScope())
                    {
                        using (var bailoutReasonScope = StdString.CreateScope())
                        {
                            V8CpuProfileNode_GetInfo(pNode, hEntity, out nodeId, out scriptId, scriptNameScope.Value, functionNameScope.Value, bailoutReasonScope.Value, out lineNumber, out columnNumber, out hitCount, out hitLineCount, out childCount);
                            scriptName = StdString.GetValue(scriptNameScope.Value);
                            functionName = StdString.GetValue(functionNameScope.Value);
                            bailoutReason = StdString.GetValue(bailoutReasonScope.Value);

                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8CpuProfileNode_GetHitLines(V8CpuProfile.Node.Ptr pNode, out int[] lineNumbers, out uint[] hitCounts)
            {
                using (var lineNumbersScope = StdInt32Array.CreateScope())
                {
                    using (var hitCountsScope = StdUInt32Array.CreateScope())
                    {
                        var result = V8CpuProfileNode_GetHitLines(pNode, lineNumbersScope.Value, hitCountsScope.Value);
                        lineNumbers = StdInt32Array.ToArray(lineNumbersScope.Value);
                        hitCounts = StdUInt32Array.ToArray(hitCountsScope.Value);
                        return result;
                    }
                }
            }

            V8CpuProfile.Node.Ptr IV8SplitProxyNative.V8CpuProfileNode_GetChildNode(V8CpuProfile.Node.Ptr pNode, int index)
            {
                return V8CpuProfileNode_GetChildNode(pNode, index);
            }

            #endregion

            #region V8 isolate methods

            V8Isolate.Handle IV8SplitProxyNative.V8Isolate_Create(string name, int maxNewSpaceSize, int maxOldSpaceSize, double heapExpansionMultiplier, ulong maxArrayBufferAllocation, V8RuntimeFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_Create(nameScope.Value, maxNewSpaceSize, maxOldSpaceSize, heapExpansionMultiplier, maxArrayBufferAllocation, flags, debugPort);
                }
            }

            V8Context.Handle IV8SplitProxyNative.V8Isolate_CreateContext(V8Isolate.Handle hIsolate, string name, V8ScriptEngineFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_CreateContext(hIsolate, nameScope.Value, flags, debugPort);
                }
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxHeapSize(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxHeapSize(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxHeapSize(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxHeapSize(hIsolate, size);
            }

            double IV8SplitProxyNative.V8Isolate_GetHeapSizeSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetHeapSizeSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetHeapSizeSampleInterval(V8Isolate.Handle hIsolate, double milliseconds)
            {
                V8Isolate_SetHeapSizeSampleInterval(hIsolate, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxStackUsage(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxStackUsage(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxStackUsage(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxStackUsage(hIsolate, size);
            }

            void IV8SplitProxyNative.V8Isolate_AwaitDebuggerAndPause(V8Isolate.Handle hIsolate)
            {
                V8Isolate_AwaitDebuggerAndPause(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_CancelAwaitDebugger(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CancelAwaitDebugger(hIsolate);
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_Compile(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Isolate_Compile(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileProducingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Isolate_CompileProducingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileConsumingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Isolate_CompileConsumingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileUpdatingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Isolate_CompileUpdatingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }

                                return hScript;
                            }
                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8Isolate_GetEnableInterruptPropagation(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetEnableInterruptPropagation(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetEnableInterruptPropagation(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetEnableInterruptPropagation(hIsolate, value);
            }

            bool IV8SplitProxyNative.V8Isolate_GetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetDisableHeapSizeViolationInterrupt(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetDisableHeapSizeViolationInterrupt(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_GetHeapStatistics(V8Isolate.Handle hIsolate, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Isolate_GetHeapStatistics(hIsolate, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Isolate_GetStatistics(V8Isolate.Handle hIsolate, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Isolate_GetStatistics(hIsolate, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectGarbage(V8Isolate.Handle hIsolate, bool exhaustive)
            {
                V8Isolate_CollectGarbage(hIsolate, exhaustive);
            }

            bool IV8SplitProxyNative.V8Isolate_BeginCpuProfile(V8Isolate.Handle hIsolate, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_BeginCpuProfile(hIsolate, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Isolate_EndCpuProfile(V8Isolate.Handle hIsolate, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Isolate_EndCpuProfile(hIsolate, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectCpuProfileSample(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CollectCpuProfileSample(hIsolate);
            }

            uint IV8SplitProxyNative.V8Isolate_GetCpuProfileSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetCpuProfileSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetCpuProfileSampleInterval(V8Isolate.Handle hIsolate, uint value)
            {
                V8Isolate_SetCpuProfileSampleInterval(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_WriteHeapSnapshot(V8Isolate.Handle hIsolate, IntPtr pStream)
            {
                V8Isolate_WriteHeapSnapshot(hIsolate, pStream);
            }

            #endregion

            #region V8 context methods

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateHeapSize(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateHeapSize(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateHeapSize(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateHeapSize(hContext, size);
            }

            double IV8SplitProxyNative.V8Context_GetIsolateHeapSizeSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetIsolateHeapSizeSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetIsolateHeapSizeSampleInterval(V8Context.Handle hContext, double milliseconds)
            {
                V8Context_SetIsolateHeapSizeSampleInterval(hContext, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateStackUsage(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateStackUsage(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateStackUsage(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateStackUsage(hContext, size);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLock(V8Context.Handle hContext, IntPtr pAction)
            {
                V8Context_InvokeWithLock(hContext, pAction);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLockWithArg(V8Context.Handle hContext, IntPtr pAction, IntPtr pArg)
            {
                V8Context_InvokeWithLockWithArg(hContext, pAction, pArg);
            }

            object IV8SplitProxyNative.V8Context_GetRootItem(V8Context.Handle hContext)
            {
                using (var itemScope = V8Value.CreateScope())
                {
                    V8Context_GetRootItem(hContext, itemScope.Value);
                    return V8Value.Get(itemScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_AddGlobalItem(V8Context.Handle hContext, string name, object value, bool globalMembers)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Context_AddGlobalItem(hContext, nameScope.Value, valueScope.Value, globalMembers);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_AwaitDebuggerAndPause(V8Context.Handle hContext)
            {
                V8Context_AwaitDebuggerAndPause(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelAwaitDebugger(V8Context.Handle hContext)
            {
                V8Context_CancelAwaitDebugger(hContext);
            }

            object IV8SplitProxyNative.V8Context_ExecuteCode(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var resultScope = V8Value.CreateScope())
                            {
                                V8Context_ExecuteCode(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, evaluate, resultScope.Value);
                                return V8Value.Get(resultScope.Value);
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Context_ExecuteScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest, evaluate, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_Compile(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Context_Compile(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        return V8Context_CompileScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest);
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileProducingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Context_CompileProducingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileConsumingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Context_CompileConsumingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileUpdatingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Context_CompileUpdatingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }
                
                                return hScript;
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScript(V8Context.Handle hContext, V8Script.Handle hScript, bool evaluate)
            {
                using (var resultScope = V8Value.CreateScope())
                {
                    V8Context_ExecuteScript(hContext, hScript, evaluate, resultScope.Value);
                    return V8Value.Get(resultScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_Interrupt(V8Context.Handle hContext)
            {
                V8Context_Interrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelInterrupt(V8Context.Handle hContext)
            {
                V8Context_CancelInterrupt(hContext);
            }

            bool IV8SplitProxyNative.V8Context_GetEnableIsolateInterruptPropagation(V8Context.Handle hContext)
            {
                return V8Context_GetEnableIsolateInterruptPropagation(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetEnableIsolateInterruptPropagation(V8Context.Handle hContext, bool value)
            {
                V8Context_SetEnableIsolateInterruptPropagation(hContext, value);
            }

            bool IV8SplitProxyNative.V8Context_GetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext)
            {
                return V8Context_GetDisableIsolateHeapSizeViolationInterrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext, bool value)
            {
                V8Context_SetDisableIsolateHeapSizeViolationInterrupt(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateHeapStatistics(V8Context.Handle hContext, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Context_GetIsolateHeapStatistics(hContext, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Context_GetIsolateStatistics(hContext, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_GetStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong moduleCount, out ulong moduleCacheSize)
            {
                V8Context_GetStatistics(hContext, out scriptCount, out moduleCount, out moduleCacheSize);
            }

            void IV8SplitProxyNative.V8Context_CollectGarbage(V8Context.Handle hContext, bool exhaustive)
            {
                V8Context_CollectGarbage(hContext, exhaustive);
            }

            void IV8SplitProxyNative.V8Context_OnAccessSettingsChanged(V8Context.Handle hContext)
            {
                V8Context_OnAccessSettingsChanged(hContext);
            }

            bool IV8SplitProxyNative.V8Context_BeginCpuProfile(V8Context.Handle hContext, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Context_BeginCpuProfile(hContext, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Context_EndCpuProfile(V8Context.Handle hContext, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Context_EndCpuProfile(hContext, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Context_CollectCpuProfileSample(V8Context.Handle hContext)
            {
                V8Context_CollectCpuProfileSample(hContext);
            }

            uint IV8SplitProxyNative.V8Context_GetCpuProfileSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetCpuProfileSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetCpuProfileSampleInterval(V8Context.Handle hContext, uint value)
            {
                V8Context_SetCpuProfileSampleInterval(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_WriteIsolateHeapSnapshot(V8Context.Handle hContext, IntPtr pStream)
            {
                V8Context_WriteIsolateHeapSnapshot(hContext, pStream);
            }

            #endregion

            #region V8 object methods

            object IV8SplitProxyNative.V8Object_GetNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        V8Object_GetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                        return V8Value.Get(valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_TryGetNamedProperty(V8Object.Handle hObject, string name, out object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        if (V8Object_TryGetNamedProperty(hObject, nameScope.Value, valueScope.Value))
                        {
                            value = V8Value.Get(valueScope.Value);
                            return true;
                        }

                        value = null;
                        return false;
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_SetNamedProperty(V8Object.Handle hObject, string name, object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Object_SetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Object_DeleteNamedProperty(hObject, nameScope.Value);
                }
            }

            string[] IV8SplitProxyNative.V8Object_GetPropertyNames(V8Object.Handle hObject, bool includeIndices)
            {
                using (var namesScope = StdStringArray.CreateScope())
                {
                    V8Object_GetPropertyNames(hObject, includeIndices, namesScope.Value);
                    return StdStringArray.ToArray(namesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_GetIndexedProperty(V8Object.Handle hObject, int index)
            {
                using (var valueScope = V8Value.CreateScope())
                {
                    V8Object_GetIndexedProperty(hObject, index, valueScope.Value);
                    return V8Value.Get(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_SetIndexedProperty(V8Object.Handle hObject, int index, object value)
            {
                using (var valueScope = V8Value.CreateScope(value))
                {
                    V8Object_SetIndexedProperty(hObject, index, valueScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteIndexedProperty(V8Object.Handle hObject, int index)
            {
                return V8Object_DeleteIndexedProperty(hObject, index);
            }

            int[] IV8SplitProxyNative.V8Object_GetPropertyIndices(V8Object.Handle hObject)
            {
                using (var indicesScope = StdInt32Array.CreateScope())
                {
                    V8Object_GetPropertyIndices(hObject, indicesScope.Value);
                    return StdInt32Array.ToArray(indicesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_Invoke(V8Object.Handle hObject, bool asConstructor, object[] args)
            {
                using (var argsScope = StdV8ValueArray.CreateScope(args))
                {
                    using (var resultScope = V8Value.CreateScope())
                    {
                        V8Object_Invoke(hObject, asConstructor, argsScope.Value, resultScope.Value);
                        return V8Value.Get(resultScope.Value);
                    }
                }
            }

            object IV8SplitProxyNative.V8Object_InvokeMethod(V8Object.Handle hObject, string name, object[] args)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var argsScope = StdV8ValueArray.CreateScope(args))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Object_InvokeMethod(hObject, nameScope.Value, argsScope.Value, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_GetArrayBufferOrViewInfo(V8Object.Handle hObject, out IV8Object arrayBuffer, out ulong offset, out ulong size, out ulong length)
            {
                using (var arrayBufferScope = V8Value.CreateScope())
                {
                    V8Object_GetArrayBufferOrViewInfo(hObject, arrayBufferScope.Value, out offset, out size, out length);
                    arrayBuffer = (IV8Object)V8Value.Get(arrayBufferScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewData(V8Object.Handle hObject, IntPtr pAction)
            {
                V8Object_InvokeWithArrayBufferOrViewData(hObject, pAction);
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewDataWithArg(V8Object.Handle hObject, IntPtr pAction, IntPtr pArg)
            {
                V8Object_InvokeWithArrayBufferOrViewDataWithArg(hObject, pAction, pArg);
            }

            #endregion

            #region V8 debug callback methods

            void IV8SplitProxyNative.V8DebugCallback_ConnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_ConnectClient(hCallback);
            }

            void IV8SplitProxyNative.V8DebugCallback_SendCommand(V8DebugCallback.Handle hCallback, string command)
            {
                using (var commandScope = StdString.CreateScope(command))
                {
                    V8DebugCallback_SendCommand(hCallback, commandScope.Value);
                }
            }

            void IV8SplitProxyNative.V8DebugCallback_DisconnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_DisconnectClient(hCallback);
            }

            #endregion

            #region native callback methods

            void IV8SplitProxyNative.NativeCallback_Invoke(NativeCallback.Handle hCallback)
            {
                NativeCallback_Invoke(hCallback);
            }

            #endregion

            #region V8 entity methods

            void IV8SplitProxyNative.V8Entity_Release(V8Entity.Handle hEntity)
            {
                V8Entity_Release(hEntity);
            }

            V8Entity.Handle IV8SplitProxyNative.V8Entity_CloneHandle(V8Entity.Handle hEntity)
            {
                return V8Entity_CloneHandle(hEntity);
            }

            void IV8SplitProxyNative.V8Entity_DestroyHandle(V8Entity.Handle hEntity)
            {
                V8Entity_DestroyHandle(hEntity);
            }

            #endregion

            #region error handling

            void IV8SplitProxyNative.HostException_Schedule(string message, object exception)
            {
                using (var messageScope = StdString.CreateScope(message))
                {
                    using (var exceptionScope = V8Value.CreateScope(exception))
                    {
                        HostException_Schedule(messageScope.Value, exceptionScope.Value);
                    }
                }
            }

            #endregion

            #region unit test support

            UIntPtr IV8SplitProxyNative.V8UnitTestSupport_GetTextDigest(string value)
            {
                using (var valueScope = StdString.CreateScope(value))
                {
                    return V8UnitTestSupport_GetTextDigest(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8UnitTestSupport_GetStatistics(out ulong isolateCount, out ulong contextCount)
            {
                V8UnitTestSupport_GetStatistics(out isolateCount, out contextCount);
            }

            #endregion

            #endregion

            #region native methods

            #region initialization

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyManaged_SetMethodTable(
                [In] IntPtr pMethodTable
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyNative_GetVersion();

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Environment_InitializeICU(
                [In] IntPtr pICUData,
                [In] uint size
            );

            #endregion

            #region memory methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_Allocate(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_AllocateZeroed(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void Memory_Free(
                [In] IntPtr pMemory
            );

            #endregion

            #region StdString methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdString.Ptr StdString_New(
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdString_GetValue(
                [In] StdString.Ptr pString,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_SetValue(
                [In] StdString.Ptr pString,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_Delete(
                [In] StdString.Ptr pString
            );

            #endregion

            #region StdStringArray methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdStringArray.Ptr StdStringArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdStringArray_GetElementCount(
                [In] StdStringArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElementCount(
                [In] StdStringArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdStringArray_GetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_Delete(
                [In] StdStringArray.Ptr pArray
            );

            #endregion

            #region StdByteArray methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdByteArray.Ptr StdByteArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdByteArray_GetElementCount(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_SetElementCount(
                [In] StdByteArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdByteArray_GetData(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_Delete(
                [In] StdByteArray.Ptr pArray
            );

            #endregion

            #region StdInt32Array methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdInt32Array.Ptr StdInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdInt32Array_GetElementCount(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_SetElementCount(
                [In] StdInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdInt32Array_GetData(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_Delete(
                [In] StdInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt32Array methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt32Array.Ptr StdUInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt32Array_GetElementCount(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_SetElementCount(
                [In] StdUInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt32Array_GetData(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_Delete(
                [In] StdUInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt64Array methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt64Array.Ptr StdUInt64Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt64Array_GetElementCount(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_SetElementCount(
                [In] StdUInt64Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt64Array_GetData(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_Delete(
                [In] StdUInt64Array.Ptr pArray
            );

            #endregion

            #region StdPtrArray methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdPtrArray.Ptr StdPtrArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdPtrArray_GetElementCount(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_SetElementCount(
                [In] StdPtrArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdPtrArray_GetData(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_Delete(
                [In] StdPtrArray.Ptr pArray
            );

            #endregion

            #region StdV8ValueArray methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdV8ValueArray.Ptr StdV8ValueArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdV8ValueArray_GetElementCount(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_SetElementCount(
                [In] StdV8ValueArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr StdV8ValueArray_GetData(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_Delete(
                [In] StdV8ValueArray.Ptr pArray
            );

            #endregion

            #region V8Value methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr V8Value_New();

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNonexistent(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetUndefined(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNull(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBoolean(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNumber(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetString(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetDateTime(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBigInt(
                [In] V8Value.Ptr pV8Value,
                [In] int signBit,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] bytes,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetV8Object(
                [In] V8Value.Ptr pV8Value,
                [In] V8Object.Handle hObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetHostObject(
                [In] V8Value.Ptr pV8Value,
                [In] IntPtr pObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Decode(
                [In] V8Value.Ptr pV8Value,
                [Out] out V8Value.Decoded decoded
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Delete(
                [In] V8Value.Ptr pV8Value
            );

            #endregion

            #region V8CpuProfile methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfile_GetInfo(
                [In] V8CpuProfile.Ptr pProfile,
                [In] V8Entity.Handle hEntity,
                [In] StdString.Ptr pName,
                [Out] out ulong startTimestamp,
                [Out] out ulong endTimestamp,
                [Out] out int sampleCount,
                [Out] out V8CpuProfile.Node.Ptr pRootNode
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfile_GetSample(
                [In] V8CpuProfile.Ptr pProfile,
                [In] int index,
                [Out] out ulong nodeId,
                [Out] out ulong timestamp
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfileNode_GetInfo(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] V8Entity.Handle hEntity,
                [Out] out ulong nodeId,
                [Out] out long scriptId,
                [In] StdString.Ptr pScriptName,
                [In] StdString.Ptr pFunctionName,
                [In] StdString.Ptr pBailoutReason,
                [Out] out long lineNumber,
                [Out] out long columnNumber,
                [Out] out ulong hitCount,
                [Out] out uint hitLineCount,
                [Out] out int childCount
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfileNode_GetHitLines(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] StdInt32Array.Ptr pLineNumbers,
                [In] StdUInt32Array.Ptr pHitCounts
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8CpuProfile.Node.Ptr V8CpuProfileNode_GetChildNode(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] int index
            );

            #endregion

            #region V8 isolate methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Isolate.Handle V8Isolate_Create(
                [In] StdString.Ptr pName,
                [In] int maxNewSpaceSize,
                [In] int maxOldSpaceSize,
                [In] double heapExpansionMultiplier,
                [In] ulong maxArrayBufferAllocation,
                [In] V8RuntimeFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Context.Handle V8Isolate_CreateContext(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] V8ScriptEngineFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Isolate_GetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_AwaitDebuggerAndPause(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CancelAwaitDebugger(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_Compile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileProducingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileConsumingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileUpdatingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetHeapStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectGarbage(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_BeginCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_EndCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectCpuProfileSample(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Isolate_GetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] uint value
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_WriteHeapSnapshot(
                [In] V8Isolate.Handle hIsolate,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 context methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Context_GetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLock(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLockWithArg(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetRootItem(
                [In] V8Context.Handle hContext,
                [In] V8Value.Ptr pItem
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AddGlobalItem(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue,
                [In] [MarshalAs(UnmanagedType.I1)] bool globalMembers
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AwaitDebuggerAndPause(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelAwaitDebugger(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteCode(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_Compile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileProducingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileConsumingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileUpdatingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScript(
                [In] V8Context.Handle hContext,
                [In] V8Script.Handle hScript,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_Interrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateHeapStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong moduleCount,
                [Out] out ulong moduleCacheSize
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectGarbage(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_OnAccessSettingsChanged(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_BeginCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_EndCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectCpuProfileSample(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Context_GetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext,
                [In] uint value
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_WriteIsolateHeapSnapshot(
                [In] V8Context.Handle hContext,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 object methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_TryGetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyNames(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool includeIndices,
                [In] StdStringArray.Ptr pNames
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyIndices(
                [In] V8Object.Handle hObject,
                [In] StdInt32Array.Ptr pIndices
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_Invoke(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool asConstructor,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeMethod(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetArrayBufferOrViewInfo(
                [In] V8Object.Handle hObject,
                [In] V8Value.Ptr pArrayBuffer,
                [Out] out ulong offset,
                [Out] out ulong size,
                [Out] out ulong length
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewData(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewDataWithArg(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            #endregion

            #region V8 debug callback methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_ConnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_SendCommand(
                [In] V8DebugCallback.Handle hCallback,
                [In] StdString.Ptr pCommand
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_DisconnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            #endregion

            #region native callback methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void NativeCallback_Invoke(
                [In] NativeCallback.Handle hCallback
            );

            #endregion

            #region V8 entity methods

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_Release(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Entity.Handle V8Entity_CloneHandle(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_DestroyHandle(
                [In] V8Entity.Handle hEntity
            );

            #endregion

            #region error handling

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void HostException_Schedule(
                [In] StdString.Ptr pMessage,
                [In] V8Value.Ptr pException
            );

            #endregion

            #region unit test support

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8UnitTestSupport_GetTextDigest(
                [In] StdString.Ptr pString
            );

            [DllImport("ClearScriptV8.win-x86.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8UnitTestSupport_GetStatistics(
                [Out] out ulong isolateCount,
                [Out] out ulong contextCount
            );

            #endregion

            #endregion
        }

        #endregion

        

        #region Nested type: Impl_Windows_X64

        private sealed class Impl_Windows_X64 : IV8SplitProxyNative
        {
            public static readonly IV8SplitProxyNative Instance = new Impl_Windows_X64();

            #region IV8SplitProxyNative implementation

            #region initialization

            IntPtr IV8SplitProxyNative.V8SplitProxyManaged_SetMethodTable(IntPtr pMethodTable)
            {
                return V8SplitProxyManaged_SetMethodTable(pMethodTable);
            }

            string IV8SplitProxyNative.V8SplitProxyNative_GetVersion()
            {
                return Marshal.PtrToStringUni(V8SplitProxyNative_GetVersion());
            }

            void IV8SplitProxyNative.V8Environment_InitializeICU(IntPtr pICUData, uint size)
            {
                V8Environment_InitializeICU(pICUData, size);
            }

            #endregion

            #region memory methods

            IntPtr IV8SplitProxyNative.Memory_Allocate(UIntPtr size)
            {
                return Memory_Allocate(size);
            }

            IntPtr IV8SplitProxyNative.Memory_AllocateZeroed(UIntPtr size)
            {
                return Memory_AllocateZeroed(size);
            }

            void IV8SplitProxyNative.Memory_Free(IntPtr pMemory)
            {
                Memory_Free(pMemory);
            }

            #endregion

            #region StdString methods

            StdString.Ptr IV8SplitProxyNative.StdString_New(string value)
            {
                return StdString_New(value, value.Length);
            }

            string IV8SplitProxyNative.StdString_GetValue(StdString.Ptr pString)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue>(StdString.Ptr pString, Func<IntPtr, int, TValue> factory)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue, TArg>(StdString.Ptr pString, Func<IntPtr, int, TArg, TValue> factory, in TArg arg)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length, arg);
            }

            void IV8SplitProxyNative.StdString_SetValue(StdString.Ptr pString, string value)
            {
                StdString_SetValue(pString, value, value.Length);
            }

            void IV8SplitProxyNative.StdString_Delete(StdString.Ptr pString)
            {
                StdString_Delete(pString);
            }

            #endregion

            #region StdStringArray methods

            StdStringArray.Ptr IV8SplitProxyNative.StdStringArray_New(int elementCount)
            {
                return StdStringArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdStringArray_GetElementCount(StdStringArray.Ptr pArray)
            {
                return StdStringArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdStringArray_SetElementCount(StdStringArray.Ptr pArray, int elementCount)
            {
                StdStringArray_SetElementCount(pArray, elementCount);
            }

            string IV8SplitProxyNative.StdStringArray_GetElement(StdStringArray.Ptr pArray, int index)
            {
                var pValue = StdStringArray_GetElement(pArray, index, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            void IV8SplitProxyNative.StdStringArray_SetElement(StdStringArray.Ptr pArray, int index, string value)
            {
                StdStringArray_SetElement(pArray, index, value, value.Length);
            }

            void IV8SplitProxyNative.StdStringArray_Delete(StdStringArray.Ptr pArray)
            {
                StdStringArray_Delete(pArray);
            }

            #endregion

            #region StdByteArray methods

            StdByteArray.Ptr IV8SplitProxyNative.StdByteArray_New(int elementCount)
            {
                return StdByteArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdByteArray_GetElementCount(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_SetElementCount(StdByteArray.Ptr pArray, int elementCount)
            {
                StdByteArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdByteArray_GetData(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_Delete(StdByteArray.Ptr pArray)
            {
                StdByteArray_Delete(pArray);
            }

            #endregion

            #region StdInt32Array methods

            StdInt32Array.Ptr IV8SplitProxyNative.StdInt32Array_New(int elementCount)
            {
                return StdInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdInt32Array_GetElementCount(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_SetElementCount(StdInt32Array.Ptr pArray, int elementCount)
            {
                StdInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdInt32Array_GetData(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_Delete(StdInt32Array.Ptr pArray)
            {
                StdInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt32Array methods

            StdUInt32Array.Ptr IV8SplitProxyNative.StdUInt32Array_New(int elementCount)
            {
                return StdUInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt32Array_GetElementCount(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_SetElementCount(StdUInt32Array.Ptr pArray, int elementCount)
            {
                StdUInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt32Array_GetData(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_Delete(StdUInt32Array.Ptr pArray)
            {
                StdUInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt64Array methods

            StdUInt64Array.Ptr IV8SplitProxyNative.StdUInt64Array_New(int elementCount)
            {
                return StdUInt64Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt64Array_GetElementCount(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_SetElementCount(StdUInt64Array.Ptr pArray, int elementCount)
            {
                StdUInt64Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt64Array_GetData(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_Delete(StdUInt64Array.Ptr pArray)
            {
                StdUInt64Array_Delete(pArray);
            }

            #endregion

            #region StdPtrArray methods

            StdPtrArray.Ptr IV8SplitProxyNative.StdPtrArray_New(int elementCount)
            {
                return StdPtrArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdPtrArray_GetElementCount(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_SetElementCount(StdPtrArray.Ptr pArray, int elementCount)
            {
                StdPtrArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdPtrArray_GetData(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_Delete(StdPtrArray.Ptr pArray)
            {
                StdPtrArray_Delete(pArray);
            }

            #endregion

            #region StdV8ValueArray methods

            StdV8ValueArray.Ptr IV8SplitProxyNative.StdV8ValueArray_New(int elementCount)
            {
                return StdV8ValueArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdV8ValueArray_GetElementCount(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_SetElementCount(StdV8ValueArray.Ptr pArray, int elementCount)
            {
                StdV8ValueArray_SetElementCount(pArray, elementCount);
            }

            V8Value.Ptr IV8SplitProxyNative.StdV8ValueArray_GetData(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_Delete(StdV8ValueArray.Ptr pArray)
            {
                StdV8ValueArray_Delete(pArray);
            }

            #endregion

            #region V8Value methods

            V8Value.Ptr IV8SplitProxyNative.V8Value_New()
            {
                return V8Value_New();
            }

            void IV8SplitProxyNative.V8Value_SetNonexistent(V8Value.Ptr pV8Value)
            {
                V8Value_SetNonexistent(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetUndefined(V8Value.Ptr pV8Value)
            {
                V8Value_SetUndefined(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetNull(V8Value.Ptr pV8Value)
            {
                V8Value_SetNull(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetBoolean(V8Value.Ptr pV8Value, bool value)
            {
                V8Value_SetBoolean(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetNumber(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetNumber(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetString(V8Value.Ptr pV8Value, string value)
            {
                V8Value_SetString(pV8Value, value, value.Length);
            }

            void IV8SplitProxyNative.V8Value_SetDateTime(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetDateTime(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetBigInt(V8Value.Ptr pV8Value, int signBit, byte[] bytes)
            {
                V8Value_SetBigInt(pV8Value, signBit, bytes, bytes.Length);
            }

            void IV8SplitProxyNative.V8Value_SetV8Object(V8Value.Ptr pV8Value, V8Object.Handle hObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetV8Object(pV8Value, hObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_SetHostObject(V8Value.Ptr pV8Value, IntPtr pObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetHostObject(pV8Value, pObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_Decode(V8Value.Ptr pV8Value, out V8Value.Decoded decoded)
            {
                V8Value_Decode(pV8Value, out decoded);
            }

            void IV8SplitProxyNative.V8Value_Delete(V8Value.Ptr pV8Value)
            {
                V8Value_Delete(pV8Value);
            }

            #endregion

            #region V8CpuProfile methods

            void IV8SplitProxyNative.V8CpuProfile_GetInfo(V8CpuProfile.Ptr pProfile, V8Entity.Handle hEntity, out string name, out ulong startTimestamp, out ulong endTimestamp, out int sampleCount, out V8CpuProfile.Node.Ptr pRootNode)
            {
                using (var nameScope = StdString.CreateScope())
                {
                    V8CpuProfile_GetInfo(pProfile, hEntity, nameScope.Value, out startTimestamp, out endTimestamp, out sampleCount, out pRootNode);
                    name = StdString.GetValue(nameScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8CpuProfile_GetSample(V8CpuProfile.Ptr pProfile, int index, out ulong nodeId, out ulong timestamp)
            {
                return V8CpuProfile_GetSample(pProfile, index, out nodeId, out timestamp);
            }

            void IV8SplitProxyNative.V8CpuProfileNode_GetInfo(V8CpuProfile.Node.Ptr pNode, V8Entity.Handle hEntity, out ulong nodeId, out long scriptId, out string scriptName, out string functionName, out string bailoutReason, out long lineNumber, out long columnNumber, out ulong hitCount, out uint hitLineCount, out int childCount)
            {
                using (var scriptNameScope = StdString.CreateScope())
                {
                    using (var functionNameScope = StdString.CreateScope())
                    {
                        using (var bailoutReasonScope = StdString.CreateScope())
                        {
                            V8CpuProfileNode_GetInfo(pNode, hEntity, out nodeId, out scriptId, scriptNameScope.Value, functionNameScope.Value, bailoutReasonScope.Value, out lineNumber, out columnNumber, out hitCount, out hitLineCount, out childCount);
                            scriptName = StdString.GetValue(scriptNameScope.Value);
                            functionName = StdString.GetValue(functionNameScope.Value);
                            bailoutReason = StdString.GetValue(bailoutReasonScope.Value);

                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8CpuProfileNode_GetHitLines(V8CpuProfile.Node.Ptr pNode, out int[] lineNumbers, out uint[] hitCounts)
            {
                using (var lineNumbersScope = StdInt32Array.CreateScope())
                {
                    using (var hitCountsScope = StdUInt32Array.CreateScope())
                    {
                        var result = V8CpuProfileNode_GetHitLines(pNode, lineNumbersScope.Value, hitCountsScope.Value);
                        lineNumbers = StdInt32Array.ToArray(lineNumbersScope.Value);
                        hitCounts = StdUInt32Array.ToArray(hitCountsScope.Value);
                        return result;
                    }
                }
            }

            V8CpuProfile.Node.Ptr IV8SplitProxyNative.V8CpuProfileNode_GetChildNode(V8CpuProfile.Node.Ptr pNode, int index)
            {
                return V8CpuProfileNode_GetChildNode(pNode, index);
            }

            #endregion

            #region V8 isolate methods

            V8Isolate.Handle IV8SplitProxyNative.V8Isolate_Create(string name, int maxNewSpaceSize, int maxOldSpaceSize, double heapExpansionMultiplier, ulong maxArrayBufferAllocation, V8RuntimeFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_Create(nameScope.Value, maxNewSpaceSize, maxOldSpaceSize, heapExpansionMultiplier, maxArrayBufferAllocation, flags, debugPort);
                }
            }

            V8Context.Handle IV8SplitProxyNative.V8Isolate_CreateContext(V8Isolate.Handle hIsolate, string name, V8ScriptEngineFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_CreateContext(hIsolate, nameScope.Value, flags, debugPort);
                }
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxHeapSize(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxHeapSize(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxHeapSize(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxHeapSize(hIsolate, size);
            }

            double IV8SplitProxyNative.V8Isolate_GetHeapSizeSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetHeapSizeSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetHeapSizeSampleInterval(V8Isolate.Handle hIsolate, double milliseconds)
            {
                V8Isolate_SetHeapSizeSampleInterval(hIsolate, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxStackUsage(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxStackUsage(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxStackUsage(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxStackUsage(hIsolate, size);
            }

            void IV8SplitProxyNative.V8Isolate_AwaitDebuggerAndPause(V8Isolate.Handle hIsolate)
            {
                V8Isolate_AwaitDebuggerAndPause(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_CancelAwaitDebugger(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CancelAwaitDebugger(hIsolate);
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_Compile(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Isolate_Compile(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileProducingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Isolate_CompileProducingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileConsumingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Isolate_CompileConsumingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileUpdatingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Isolate_CompileUpdatingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }

                                return hScript;
                            }
                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8Isolate_GetEnableInterruptPropagation(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetEnableInterruptPropagation(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetEnableInterruptPropagation(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetEnableInterruptPropagation(hIsolate, value);
            }

            bool IV8SplitProxyNative.V8Isolate_GetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetDisableHeapSizeViolationInterrupt(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetDisableHeapSizeViolationInterrupt(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_GetHeapStatistics(V8Isolate.Handle hIsolate, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Isolate_GetHeapStatistics(hIsolate, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Isolate_GetStatistics(V8Isolate.Handle hIsolate, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Isolate_GetStatistics(hIsolate, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectGarbage(V8Isolate.Handle hIsolate, bool exhaustive)
            {
                V8Isolate_CollectGarbage(hIsolate, exhaustive);
            }

            bool IV8SplitProxyNative.V8Isolate_BeginCpuProfile(V8Isolate.Handle hIsolate, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_BeginCpuProfile(hIsolate, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Isolate_EndCpuProfile(V8Isolate.Handle hIsolate, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Isolate_EndCpuProfile(hIsolate, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectCpuProfileSample(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CollectCpuProfileSample(hIsolate);
            }

            uint IV8SplitProxyNative.V8Isolate_GetCpuProfileSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetCpuProfileSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetCpuProfileSampleInterval(V8Isolate.Handle hIsolate, uint value)
            {
                V8Isolate_SetCpuProfileSampleInterval(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_WriteHeapSnapshot(V8Isolate.Handle hIsolate, IntPtr pStream)
            {
                V8Isolate_WriteHeapSnapshot(hIsolate, pStream);
            }

            #endregion

            #region V8 context methods

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateHeapSize(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateHeapSize(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateHeapSize(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateHeapSize(hContext, size);
            }

            double IV8SplitProxyNative.V8Context_GetIsolateHeapSizeSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetIsolateHeapSizeSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetIsolateHeapSizeSampleInterval(V8Context.Handle hContext, double milliseconds)
            {
                V8Context_SetIsolateHeapSizeSampleInterval(hContext, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateStackUsage(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateStackUsage(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateStackUsage(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateStackUsage(hContext, size);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLock(V8Context.Handle hContext, IntPtr pAction)
            {
                V8Context_InvokeWithLock(hContext, pAction);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLockWithArg(V8Context.Handle hContext, IntPtr pAction, IntPtr pArg)
            {
                V8Context_InvokeWithLockWithArg(hContext, pAction, pArg);
            }

            object IV8SplitProxyNative.V8Context_GetRootItem(V8Context.Handle hContext)
            {
                using (var itemScope = V8Value.CreateScope())
                {
                    V8Context_GetRootItem(hContext, itemScope.Value);
                    return V8Value.Get(itemScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_AddGlobalItem(V8Context.Handle hContext, string name, object value, bool globalMembers)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Context_AddGlobalItem(hContext, nameScope.Value, valueScope.Value, globalMembers);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_AwaitDebuggerAndPause(V8Context.Handle hContext)
            {
                V8Context_AwaitDebuggerAndPause(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelAwaitDebugger(V8Context.Handle hContext)
            {
                V8Context_CancelAwaitDebugger(hContext);
            }

            object IV8SplitProxyNative.V8Context_ExecuteCode(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var resultScope = V8Value.CreateScope())
                            {
                                V8Context_ExecuteCode(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, evaluate, resultScope.Value);
                                return V8Value.Get(resultScope.Value);
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Context_ExecuteScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest, evaluate, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_Compile(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Context_Compile(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        return V8Context_CompileScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest);
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileProducingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Context_CompileProducingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileConsumingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Context_CompileConsumingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileUpdatingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Context_CompileUpdatingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }
                
                                return hScript;
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScript(V8Context.Handle hContext, V8Script.Handle hScript, bool evaluate)
            {
                using (var resultScope = V8Value.CreateScope())
                {
                    V8Context_ExecuteScript(hContext, hScript, evaluate, resultScope.Value);
                    return V8Value.Get(resultScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_Interrupt(V8Context.Handle hContext)
            {
                V8Context_Interrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelInterrupt(V8Context.Handle hContext)
            {
                V8Context_CancelInterrupt(hContext);
            }

            bool IV8SplitProxyNative.V8Context_GetEnableIsolateInterruptPropagation(V8Context.Handle hContext)
            {
                return V8Context_GetEnableIsolateInterruptPropagation(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetEnableIsolateInterruptPropagation(V8Context.Handle hContext, bool value)
            {
                V8Context_SetEnableIsolateInterruptPropagation(hContext, value);
            }

            bool IV8SplitProxyNative.V8Context_GetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext)
            {
                return V8Context_GetDisableIsolateHeapSizeViolationInterrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext, bool value)
            {
                V8Context_SetDisableIsolateHeapSizeViolationInterrupt(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateHeapStatistics(V8Context.Handle hContext, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Context_GetIsolateHeapStatistics(hContext, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Context_GetIsolateStatistics(hContext, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_GetStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong moduleCount, out ulong moduleCacheSize)
            {
                V8Context_GetStatistics(hContext, out scriptCount, out moduleCount, out moduleCacheSize);
            }

            void IV8SplitProxyNative.V8Context_CollectGarbage(V8Context.Handle hContext, bool exhaustive)
            {
                V8Context_CollectGarbage(hContext, exhaustive);
            }

            void IV8SplitProxyNative.V8Context_OnAccessSettingsChanged(V8Context.Handle hContext)
            {
                V8Context_OnAccessSettingsChanged(hContext);
            }

            bool IV8SplitProxyNative.V8Context_BeginCpuProfile(V8Context.Handle hContext, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Context_BeginCpuProfile(hContext, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Context_EndCpuProfile(V8Context.Handle hContext, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Context_EndCpuProfile(hContext, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Context_CollectCpuProfileSample(V8Context.Handle hContext)
            {
                V8Context_CollectCpuProfileSample(hContext);
            }

            uint IV8SplitProxyNative.V8Context_GetCpuProfileSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetCpuProfileSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetCpuProfileSampleInterval(V8Context.Handle hContext, uint value)
            {
                V8Context_SetCpuProfileSampleInterval(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_WriteIsolateHeapSnapshot(V8Context.Handle hContext, IntPtr pStream)
            {
                V8Context_WriteIsolateHeapSnapshot(hContext, pStream);
            }

            #endregion

            #region V8 object methods

            object IV8SplitProxyNative.V8Object_GetNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        V8Object_GetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                        return V8Value.Get(valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_TryGetNamedProperty(V8Object.Handle hObject, string name, out object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        if (V8Object_TryGetNamedProperty(hObject, nameScope.Value, valueScope.Value))
                        {
                            value = V8Value.Get(valueScope.Value);
                            return true;
                        }

                        value = null;
                        return false;
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_SetNamedProperty(V8Object.Handle hObject, string name, object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Object_SetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Object_DeleteNamedProperty(hObject, nameScope.Value);
                }
            }

            string[] IV8SplitProxyNative.V8Object_GetPropertyNames(V8Object.Handle hObject, bool includeIndices)
            {
                using (var namesScope = StdStringArray.CreateScope())
                {
                    V8Object_GetPropertyNames(hObject, includeIndices, namesScope.Value);
                    return StdStringArray.ToArray(namesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_GetIndexedProperty(V8Object.Handle hObject, int index)
            {
                using (var valueScope = V8Value.CreateScope())
                {
                    V8Object_GetIndexedProperty(hObject, index, valueScope.Value);
                    return V8Value.Get(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_SetIndexedProperty(V8Object.Handle hObject, int index, object value)
            {
                using (var valueScope = V8Value.CreateScope(value))
                {
                    V8Object_SetIndexedProperty(hObject, index, valueScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteIndexedProperty(V8Object.Handle hObject, int index)
            {
                return V8Object_DeleteIndexedProperty(hObject, index);
            }

            int[] IV8SplitProxyNative.V8Object_GetPropertyIndices(V8Object.Handle hObject)
            {
                using (var indicesScope = StdInt32Array.CreateScope())
                {
                    V8Object_GetPropertyIndices(hObject, indicesScope.Value);
                    return StdInt32Array.ToArray(indicesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_Invoke(V8Object.Handle hObject, bool asConstructor, object[] args)
            {
                using (var argsScope = StdV8ValueArray.CreateScope(args))
                {
                    using (var resultScope = V8Value.CreateScope())
                    {
                        V8Object_Invoke(hObject, asConstructor, argsScope.Value, resultScope.Value);
                        return V8Value.Get(resultScope.Value);
                    }
                }
            }

            object IV8SplitProxyNative.V8Object_InvokeMethod(V8Object.Handle hObject, string name, object[] args)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var argsScope = StdV8ValueArray.CreateScope(args))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Object_InvokeMethod(hObject, nameScope.Value, argsScope.Value, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_GetArrayBufferOrViewInfo(V8Object.Handle hObject, out IV8Object arrayBuffer, out ulong offset, out ulong size, out ulong length)
            {
                using (var arrayBufferScope = V8Value.CreateScope())
                {
                    V8Object_GetArrayBufferOrViewInfo(hObject, arrayBufferScope.Value, out offset, out size, out length);
                    arrayBuffer = (IV8Object)V8Value.Get(arrayBufferScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewData(V8Object.Handle hObject, IntPtr pAction)
            {
                V8Object_InvokeWithArrayBufferOrViewData(hObject, pAction);
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewDataWithArg(V8Object.Handle hObject, IntPtr pAction, IntPtr pArg)
            {
                V8Object_InvokeWithArrayBufferOrViewDataWithArg(hObject, pAction, pArg);
            }

            #endregion

            #region V8 debug callback methods

            void IV8SplitProxyNative.V8DebugCallback_ConnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_ConnectClient(hCallback);
            }

            void IV8SplitProxyNative.V8DebugCallback_SendCommand(V8DebugCallback.Handle hCallback, string command)
            {
                using (var commandScope = StdString.CreateScope(command))
                {
                    V8DebugCallback_SendCommand(hCallback, commandScope.Value);
                }
            }

            void IV8SplitProxyNative.V8DebugCallback_DisconnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_DisconnectClient(hCallback);
            }

            #endregion

            #region native callback methods

            void IV8SplitProxyNative.NativeCallback_Invoke(NativeCallback.Handle hCallback)
            {
                NativeCallback_Invoke(hCallback);
            }

            #endregion

            #region V8 entity methods

            void IV8SplitProxyNative.V8Entity_Release(V8Entity.Handle hEntity)
            {
                V8Entity_Release(hEntity);
            }

            V8Entity.Handle IV8SplitProxyNative.V8Entity_CloneHandle(V8Entity.Handle hEntity)
            {
                return V8Entity_CloneHandle(hEntity);
            }

            void IV8SplitProxyNative.V8Entity_DestroyHandle(V8Entity.Handle hEntity)
            {
                V8Entity_DestroyHandle(hEntity);
            }

            #endregion

            #region error handling

            void IV8SplitProxyNative.HostException_Schedule(string message, object exception)
            {
                using (var messageScope = StdString.CreateScope(message))
                {
                    using (var exceptionScope = V8Value.CreateScope(exception))
                    {
                        HostException_Schedule(messageScope.Value, exceptionScope.Value);
                    }
                }
            }

            #endregion

            #region unit test support

            UIntPtr IV8SplitProxyNative.V8UnitTestSupport_GetTextDigest(string value)
            {
                using (var valueScope = StdString.CreateScope(value))
                {
                    return V8UnitTestSupport_GetTextDigest(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8UnitTestSupport_GetStatistics(out ulong isolateCount, out ulong contextCount)
            {
                V8UnitTestSupport_GetStatistics(out isolateCount, out contextCount);
            }

            #endregion

            #endregion

            #region native methods

            #region initialization

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyManaged_SetMethodTable(
                [In] IntPtr pMethodTable
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyNative_GetVersion();

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Environment_InitializeICU(
                [In] IntPtr pICUData,
                [In] uint size
            );

            #endregion

            #region memory methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_Allocate(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_AllocateZeroed(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void Memory_Free(
                [In] IntPtr pMemory
            );

            #endregion

            #region StdString methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdString.Ptr StdString_New(
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdString_GetValue(
                [In] StdString.Ptr pString,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_SetValue(
                [In] StdString.Ptr pString,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_Delete(
                [In] StdString.Ptr pString
            );

            #endregion

            #region StdStringArray methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdStringArray.Ptr StdStringArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdStringArray_GetElementCount(
                [In] StdStringArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElementCount(
                [In] StdStringArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdStringArray_GetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_Delete(
                [In] StdStringArray.Ptr pArray
            );

            #endregion

            #region StdByteArray methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdByteArray.Ptr StdByteArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdByteArray_GetElementCount(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_SetElementCount(
                [In] StdByteArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdByteArray_GetData(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_Delete(
                [In] StdByteArray.Ptr pArray
            );

            #endregion

            #region StdInt32Array methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdInt32Array.Ptr StdInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdInt32Array_GetElementCount(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_SetElementCount(
                [In] StdInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdInt32Array_GetData(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_Delete(
                [In] StdInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt32Array methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt32Array.Ptr StdUInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt32Array_GetElementCount(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_SetElementCount(
                [In] StdUInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt32Array_GetData(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_Delete(
                [In] StdUInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt64Array methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt64Array.Ptr StdUInt64Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt64Array_GetElementCount(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_SetElementCount(
                [In] StdUInt64Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt64Array_GetData(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_Delete(
                [In] StdUInt64Array.Ptr pArray
            );

            #endregion

            #region StdPtrArray methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdPtrArray.Ptr StdPtrArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdPtrArray_GetElementCount(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_SetElementCount(
                [In] StdPtrArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdPtrArray_GetData(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_Delete(
                [In] StdPtrArray.Ptr pArray
            );

            #endregion

            #region StdV8ValueArray methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdV8ValueArray.Ptr StdV8ValueArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdV8ValueArray_GetElementCount(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_SetElementCount(
                [In] StdV8ValueArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr StdV8ValueArray_GetData(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_Delete(
                [In] StdV8ValueArray.Ptr pArray
            );

            #endregion

            #region V8Value methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr V8Value_New();

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNonexistent(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetUndefined(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNull(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBoolean(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNumber(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetString(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetDateTime(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBigInt(
                [In] V8Value.Ptr pV8Value,
                [In] int signBit,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] bytes,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetV8Object(
                [In] V8Value.Ptr pV8Value,
                [In] V8Object.Handle hObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetHostObject(
                [In] V8Value.Ptr pV8Value,
                [In] IntPtr pObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Decode(
                [In] V8Value.Ptr pV8Value,
                [Out] out V8Value.Decoded decoded
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Delete(
                [In] V8Value.Ptr pV8Value
            );

            #endregion

            #region V8CpuProfile methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfile_GetInfo(
                [In] V8CpuProfile.Ptr pProfile,
                [In] V8Entity.Handle hEntity,
                [In] StdString.Ptr pName,
                [Out] out ulong startTimestamp,
                [Out] out ulong endTimestamp,
                [Out] out int sampleCount,
                [Out] out V8CpuProfile.Node.Ptr pRootNode
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfile_GetSample(
                [In] V8CpuProfile.Ptr pProfile,
                [In] int index,
                [Out] out ulong nodeId,
                [Out] out ulong timestamp
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfileNode_GetInfo(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] V8Entity.Handle hEntity,
                [Out] out ulong nodeId,
                [Out] out long scriptId,
                [In] StdString.Ptr pScriptName,
                [In] StdString.Ptr pFunctionName,
                [In] StdString.Ptr pBailoutReason,
                [Out] out long lineNumber,
                [Out] out long columnNumber,
                [Out] out ulong hitCount,
                [Out] out uint hitLineCount,
                [Out] out int childCount
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfileNode_GetHitLines(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] StdInt32Array.Ptr pLineNumbers,
                [In] StdUInt32Array.Ptr pHitCounts
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8CpuProfile.Node.Ptr V8CpuProfileNode_GetChildNode(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] int index
            );

            #endregion

            #region V8 isolate methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Isolate.Handle V8Isolate_Create(
                [In] StdString.Ptr pName,
                [In] int maxNewSpaceSize,
                [In] int maxOldSpaceSize,
                [In] double heapExpansionMultiplier,
                [In] ulong maxArrayBufferAllocation,
                [In] V8RuntimeFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Context.Handle V8Isolate_CreateContext(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] V8ScriptEngineFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Isolate_GetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_AwaitDebuggerAndPause(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CancelAwaitDebugger(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_Compile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileProducingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileConsumingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileUpdatingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetHeapStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectGarbage(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_BeginCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_EndCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectCpuProfileSample(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Isolate_GetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] uint value
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_WriteHeapSnapshot(
                [In] V8Isolate.Handle hIsolate,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 context methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Context_GetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLock(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLockWithArg(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetRootItem(
                [In] V8Context.Handle hContext,
                [In] V8Value.Ptr pItem
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AddGlobalItem(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue,
                [In] [MarshalAs(UnmanagedType.I1)] bool globalMembers
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AwaitDebuggerAndPause(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelAwaitDebugger(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteCode(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_Compile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileProducingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileConsumingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileUpdatingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScript(
                [In] V8Context.Handle hContext,
                [In] V8Script.Handle hScript,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_Interrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateHeapStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong moduleCount,
                [Out] out ulong moduleCacheSize
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectGarbage(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_OnAccessSettingsChanged(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_BeginCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_EndCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectCpuProfileSample(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Context_GetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext,
                [In] uint value
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_WriteIsolateHeapSnapshot(
                [In] V8Context.Handle hContext,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 object methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_TryGetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyNames(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool includeIndices,
                [In] StdStringArray.Ptr pNames
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyIndices(
                [In] V8Object.Handle hObject,
                [In] StdInt32Array.Ptr pIndices
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_Invoke(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool asConstructor,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeMethod(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetArrayBufferOrViewInfo(
                [In] V8Object.Handle hObject,
                [In] V8Value.Ptr pArrayBuffer,
                [Out] out ulong offset,
                [Out] out ulong size,
                [Out] out ulong length
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewData(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewDataWithArg(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            #endregion

            #region V8 debug callback methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_ConnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_SendCommand(
                [In] V8DebugCallback.Handle hCallback,
                [In] StdString.Ptr pCommand
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_DisconnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            #endregion

            #region native callback methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void NativeCallback_Invoke(
                [In] NativeCallback.Handle hCallback
            );

            #endregion

            #region V8 entity methods

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_Release(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Entity.Handle V8Entity_CloneHandle(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_DestroyHandle(
                [In] V8Entity.Handle hEntity
            );

            #endregion

            #region error handling

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void HostException_Schedule(
                [In] StdString.Ptr pMessage,
                [In] V8Value.Ptr pException
            );

            #endregion

            #region unit test support

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8UnitTestSupport_GetTextDigest(
                [In] StdString.Ptr pString
            );

            [DllImport("ClearScriptV8.win-x64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8UnitTestSupport_GetStatistics(
                [Out] out ulong isolateCount,
                [Out] out ulong contextCount
            );

            #endregion

            #endregion
        }

        #endregion

        

        #region Nested type: Impl_Windows_Arm64

        private sealed class Impl_Windows_Arm64 : IV8SplitProxyNative
        {
            public static readonly IV8SplitProxyNative Instance = new Impl_Windows_Arm64();

            #region IV8SplitProxyNative implementation

            #region initialization

            IntPtr IV8SplitProxyNative.V8SplitProxyManaged_SetMethodTable(IntPtr pMethodTable)
            {
                return V8SplitProxyManaged_SetMethodTable(pMethodTable);
            }

            string IV8SplitProxyNative.V8SplitProxyNative_GetVersion()
            {
                return Marshal.PtrToStringUni(V8SplitProxyNative_GetVersion());
            }

            void IV8SplitProxyNative.V8Environment_InitializeICU(IntPtr pICUData, uint size)
            {
                V8Environment_InitializeICU(pICUData, size);
            }

            #endregion

            #region memory methods

            IntPtr IV8SplitProxyNative.Memory_Allocate(UIntPtr size)
            {
                return Memory_Allocate(size);
            }

            IntPtr IV8SplitProxyNative.Memory_AllocateZeroed(UIntPtr size)
            {
                return Memory_AllocateZeroed(size);
            }

            void IV8SplitProxyNative.Memory_Free(IntPtr pMemory)
            {
                Memory_Free(pMemory);
            }

            #endregion

            #region StdString methods

            StdString.Ptr IV8SplitProxyNative.StdString_New(string value)
            {
                return StdString_New(value, value.Length);
            }

            string IV8SplitProxyNative.StdString_GetValue(StdString.Ptr pString)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue>(StdString.Ptr pString, Func<IntPtr, int, TValue> factory)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue, TArg>(StdString.Ptr pString, Func<IntPtr, int, TArg, TValue> factory, in TArg arg)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length, arg);
            }

            void IV8SplitProxyNative.StdString_SetValue(StdString.Ptr pString, string value)
            {
                StdString_SetValue(pString, value, value.Length);
            }

            void IV8SplitProxyNative.StdString_Delete(StdString.Ptr pString)
            {
                StdString_Delete(pString);
            }

            #endregion

            #region StdStringArray methods

            StdStringArray.Ptr IV8SplitProxyNative.StdStringArray_New(int elementCount)
            {
                return StdStringArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdStringArray_GetElementCount(StdStringArray.Ptr pArray)
            {
                return StdStringArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdStringArray_SetElementCount(StdStringArray.Ptr pArray, int elementCount)
            {
                StdStringArray_SetElementCount(pArray, elementCount);
            }

            string IV8SplitProxyNative.StdStringArray_GetElement(StdStringArray.Ptr pArray, int index)
            {
                var pValue = StdStringArray_GetElement(pArray, index, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            void IV8SplitProxyNative.StdStringArray_SetElement(StdStringArray.Ptr pArray, int index, string value)
            {
                StdStringArray_SetElement(pArray, index, value, value.Length);
            }

            void IV8SplitProxyNative.StdStringArray_Delete(StdStringArray.Ptr pArray)
            {
                StdStringArray_Delete(pArray);
            }

            #endregion

            #region StdByteArray methods

            StdByteArray.Ptr IV8SplitProxyNative.StdByteArray_New(int elementCount)
            {
                return StdByteArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdByteArray_GetElementCount(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_SetElementCount(StdByteArray.Ptr pArray, int elementCount)
            {
                StdByteArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdByteArray_GetData(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_Delete(StdByteArray.Ptr pArray)
            {
                StdByteArray_Delete(pArray);
            }

            #endregion

            #region StdInt32Array methods

            StdInt32Array.Ptr IV8SplitProxyNative.StdInt32Array_New(int elementCount)
            {
                return StdInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdInt32Array_GetElementCount(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_SetElementCount(StdInt32Array.Ptr pArray, int elementCount)
            {
                StdInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdInt32Array_GetData(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_Delete(StdInt32Array.Ptr pArray)
            {
                StdInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt32Array methods

            StdUInt32Array.Ptr IV8SplitProxyNative.StdUInt32Array_New(int elementCount)
            {
                return StdUInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt32Array_GetElementCount(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_SetElementCount(StdUInt32Array.Ptr pArray, int elementCount)
            {
                StdUInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt32Array_GetData(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_Delete(StdUInt32Array.Ptr pArray)
            {
                StdUInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt64Array methods

            StdUInt64Array.Ptr IV8SplitProxyNative.StdUInt64Array_New(int elementCount)
            {
                return StdUInt64Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt64Array_GetElementCount(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_SetElementCount(StdUInt64Array.Ptr pArray, int elementCount)
            {
                StdUInt64Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt64Array_GetData(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_Delete(StdUInt64Array.Ptr pArray)
            {
                StdUInt64Array_Delete(pArray);
            }

            #endregion

            #region StdPtrArray methods

            StdPtrArray.Ptr IV8SplitProxyNative.StdPtrArray_New(int elementCount)
            {
                return StdPtrArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdPtrArray_GetElementCount(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_SetElementCount(StdPtrArray.Ptr pArray, int elementCount)
            {
                StdPtrArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdPtrArray_GetData(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_Delete(StdPtrArray.Ptr pArray)
            {
                StdPtrArray_Delete(pArray);
            }

            #endregion

            #region StdV8ValueArray methods

            StdV8ValueArray.Ptr IV8SplitProxyNative.StdV8ValueArray_New(int elementCount)
            {
                return StdV8ValueArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdV8ValueArray_GetElementCount(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_SetElementCount(StdV8ValueArray.Ptr pArray, int elementCount)
            {
                StdV8ValueArray_SetElementCount(pArray, elementCount);
            }

            V8Value.Ptr IV8SplitProxyNative.StdV8ValueArray_GetData(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_Delete(StdV8ValueArray.Ptr pArray)
            {
                StdV8ValueArray_Delete(pArray);
            }

            #endregion

            #region V8Value methods

            V8Value.Ptr IV8SplitProxyNative.V8Value_New()
            {
                return V8Value_New();
            }

            void IV8SplitProxyNative.V8Value_SetNonexistent(V8Value.Ptr pV8Value)
            {
                V8Value_SetNonexistent(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetUndefined(V8Value.Ptr pV8Value)
            {
                V8Value_SetUndefined(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetNull(V8Value.Ptr pV8Value)
            {
                V8Value_SetNull(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetBoolean(V8Value.Ptr pV8Value, bool value)
            {
                V8Value_SetBoolean(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetNumber(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetNumber(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetString(V8Value.Ptr pV8Value, string value)
            {
                V8Value_SetString(pV8Value, value, value.Length);
            }

            void IV8SplitProxyNative.V8Value_SetDateTime(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetDateTime(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetBigInt(V8Value.Ptr pV8Value, int signBit, byte[] bytes)
            {
                V8Value_SetBigInt(pV8Value, signBit, bytes, bytes.Length);
            }

            void IV8SplitProxyNative.V8Value_SetV8Object(V8Value.Ptr pV8Value, V8Object.Handle hObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetV8Object(pV8Value, hObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_SetHostObject(V8Value.Ptr pV8Value, IntPtr pObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetHostObject(pV8Value, pObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_Decode(V8Value.Ptr pV8Value, out V8Value.Decoded decoded)
            {
                V8Value_Decode(pV8Value, out decoded);
            }

            void IV8SplitProxyNative.V8Value_Delete(V8Value.Ptr pV8Value)
            {
                V8Value_Delete(pV8Value);
            }

            #endregion

            #region V8CpuProfile methods

            void IV8SplitProxyNative.V8CpuProfile_GetInfo(V8CpuProfile.Ptr pProfile, V8Entity.Handle hEntity, out string name, out ulong startTimestamp, out ulong endTimestamp, out int sampleCount, out V8CpuProfile.Node.Ptr pRootNode)
            {
                using (var nameScope = StdString.CreateScope())
                {
                    V8CpuProfile_GetInfo(pProfile, hEntity, nameScope.Value, out startTimestamp, out endTimestamp, out sampleCount, out pRootNode);
                    name = StdString.GetValue(nameScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8CpuProfile_GetSample(V8CpuProfile.Ptr pProfile, int index, out ulong nodeId, out ulong timestamp)
            {
                return V8CpuProfile_GetSample(pProfile, index, out nodeId, out timestamp);
            }

            void IV8SplitProxyNative.V8CpuProfileNode_GetInfo(V8CpuProfile.Node.Ptr pNode, V8Entity.Handle hEntity, out ulong nodeId, out long scriptId, out string scriptName, out string functionName, out string bailoutReason, out long lineNumber, out long columnNumber, out ulong hitCount, out uint hitLineCount, out int childCount)
            {
                using (var scriptNameScope = StdString.CreateScope())
                {
                    using (var functionNameScope = StdString.CreateScope())
                    {
                        using (var bailoutReasonScope = StdString.CreateScope())
                        {
                            V8CpuProfileNode_GetInfo(pNode, hEntity, out nodeId, out scriptId, scriptNameScope.Value, functionNameScope.Value, bailoutReasonScope.Value, out lineNumber, out columnNumber, out hitCount, out hitLineCount, out childCount);
                            scriptName = StdString.GetValue(scriptNameScope.Value);
                            functionName = StdString.GetValue(functionNameScope.Value);
                            bailoutReason = StdString.GetValue(bailoutReasonScope.Value);

                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8CpuProfileNode_GetHitLines(V8CpuProfile.Node.Ptr pNode, out int[] lineNumbers, out uint[] hitCounts)
            {
                using (var lineNumbersScope = StdInt32Array.CreateScope())
                {
                    using (var hitCountsScope = StdUInt32Array.CreateScope())
                    {
                        var result = V8CpuProfileNode_GetHitLines(pNode, lineNumbersScope.Value, hitCountsScope.Value);
                        lineNumbers = StdInt32Array.ToArray(lineNumbersScope.Value);
                        hitCounts = StdUInt32Array.ToArray(hitCountsScope.Value);
                        return result;
                    }
                }
            }

            V8CpuProfile.Node.Ptr IV8SplitProxyNative.V8CpuProfileNode_GetChildNode(V8CpuProfile.Node.Ptr pNode, int index)
            {
                return V8CpuProfileNode_GetChildNode(pNode, index);
            }

            #endregion

            #region V8 isolate methods

            V8Isolate.Handle IV8SplitProxyNative.V8Isolate_Create(string name, int maxNewSpaceSize, int maxOldSpaceSize, double heapExpansionMultiplier, ulong maxArrayBufferAllocation, V8RuntimeFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_Create(nameScope.Value, maxNewSpaceSize, maxOldSpaceSize, heapExpansionMultiplier, maxArrayBufferAllocation, flags, debugPort);
                }
            }

            V8Context.Handle IV8SplitProxyNative.V8Isolate_CreateContext(V8Isolate.Handle hIsolate, string name, V8ScriptEngineFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_CreateContext(hIsolate, nameScope.Value, flags, debugPort);
                }
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxHeapSize(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxHeapSize(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxHeapSize(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxHeapSize(hIsolate, size);
            }

            double IV8SplitProxyNative.V8Isolate_GetHeapSizeSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetHeapSizeSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetHeapSizeSampleInterval(V8Isolate.Handle hIsolate, double milliseconds)
            {
                V8Isolate_SetHeapSizeSampleInterval(hIsolate, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxStackUsage(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxStackUsage(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxStackUsage(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxStackUsage(hIsolate, size);
            }

            void IV8SplitProxyNative.V8Isolate_AwaitDebuggerAndPause(V8Isolate.Handle hIsolate)
            {
                V8Isolate_AwaitDebuggerAndPause(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_CancelAwaitDebugger(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CancelAwaitDebugger(hIsolate);
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_Compile(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Isolate_Compile(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileProducingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Isolate_CompileProducingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileConsumingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Isolate_CompileConsumingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileUpdatingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Isolate_CompileUpdatingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }

                                return hScript;
                            }
                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8Isolate_GetEnableInterruptPropagation(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetEnableInterruptPropagation(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetEnableInterruptPropagation(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetEnableInterruptPropagation(hIsolate, value);
            }

            bool IV8SplitProxyNative.V8Isolate_GetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetDisableHeapSizeViolationInterrupt(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetDisableHeapSizeViolationInterrupt(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_GetHeapStatistics(V8Isolate.Handle hIsolate, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Isolate_GetHeapStatistics(hIsolate, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Isolate_GetStatistics(V8Isolate.Handle hIsolate, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Isolate_GetStatistics(hIsolate, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectGarbage(V8Isolate.Handle hIsolate, bool exhaustive)
            {
                V8Isolate_CollectGarbage(hIsolate, exhaustive);
            }

            bool IV8SplitProxyNative.V8Isolate_BeginCpuProfile(V8Isolate.Handle hIsolate, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_BeginCpuProfile(hIsolate, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Isolate_EndCpuProfile(V8Isolate.Handle hIsolate, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Isolate_EndCpuProfile(hIsolate, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectCpuProfileSample(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CollectCpuProfileSample(hIsolate);
            }

            uint IV8SplitProxyNative.V8Isolate_GetCpuProfileSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetCpuProfileSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetCpuProfileSampleInterval(V8Isolate.Handle hIsolate, uint value)
            {
                V8Isolate_SetCpuProfileSampleInterval(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_WriteHeapSnapshot(V8Isolate.Handle hIsolate, IntPtr pStream)
            {
                V8Isolate_WriteHeapSnapshot(hIsolate, pStream);
            }

            #endregion

            #region V8 context methods

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateHeapSize(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateHeapSize(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateHeapSize(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateHeapSize(hContext, size);
            }

            double IV8SplitProxyNative.V8Context_GetIsolateHeapSizeSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetIsolateHeapSizeSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetIsolateHeapSizeSampleInterval(V8Context.Handle hContext, double milliseconds)
            {
                V8Context_SetIsolateHeapSizeSampleInterval(hContext, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateStackUsage(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateStackUsage(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateStackUsage(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateStackUsage(hContext, size);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLock(V8Context.Handle hContext, IntPtr pAction)
            {
                V8Context_InvokeWithLock(hContext, pAction);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLockWithArg(V8Context.Handle hContext, IntPtr pAction, IntPtr pArg)
            {
                V8Context_InvokeWithLockWithArg(hContext, pAction, pArg);
            }

            object IV8SplitProxyNative.V8Context_GetRootItem(V8Context.Handle hContext)
            {
                using (var itemScope = V8Value.CreateScope())
                {
                    V8Context_GetRootItem(hContext, itemScope.Value);
                    return V8Value.Get(itemScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_AddGlobalItem(V8Context.Handle hContext, string name, object value, bool globalMembers)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Context_AddGlobalItem(hContext, nameScope.Value, valueScope.Value, globalMembers);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_AwaitDebuggerAndPause(V8Context.Handle hContext)
            {
                V8Context_AwaitDebuggerAndPause(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelAwaitDebugger(V8Context.Handle hContext)
            {
                V8Context_CancelAwaitDebugger(hContext);
            }

            object IV8SplitProxyNative.V8Context_ExecuteCode(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var resultScope = V8Value.CreateScope())
                            {
                                V8Context_ExecuteCode(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, evaluate, resultScope.Value);
                                return V8Value.Get(resultScope.Value);
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Context_ExecuteScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest, evaluate, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_Compile(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Context_Compile(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        return V8Context_CompileScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest);
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileProducingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Context_CompileProducingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileConsumingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Context_CompileConsumingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileUpdatingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Context_CompileUpdatingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }
                
                                return hScript;
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScript(V8Context.Handle hContext, V8Script.Handle hScript, bool evaluate)
            {
                using (var resultScope = V8Value.CreateScope())
                {
                    V8Context_ExecuteScript(hContext, hScript, evaluate, resultScope.Value);
                    return V8Value.Get(resultScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_Interrupt(V8Context.Handle hContext)
            {
                V8Context_Interrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelInterrupt(V8Context.Handle hContext)
            {
                V8Context_CancelInterrupt(hContext);
            }

            bool IV8SplitProxyNative.V8Context_GetEnableIsolateInterruptPropagation(V8Context.Handle hContext)
            {
                return V8Context_GetEnableIsolateInterruptPropagation(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetEnableIsolateInterruptPropagation(V8Context.Handle hContext, bool value)
            {
                V8Context_SetEnableIsolateInterruptPropagation(hContext, value);
            }

            bool IV8SplitProxyNative.V8Context_GetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext)
            {
                return V8Context_GetDisableIsolateHeapSizeViolationInterrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext, bool value)
            {
                V8Context_SetDisableIsolateHeapSizeViolationInterrupt(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateHeapStatistics(V8Context.Handle hContext, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Context_GetIsolateHeapStatistics(hContext, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Context_GetIsolateStatistics(hContext, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_GetStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong moduleCount, out ulong moduleCacheSize)
            {
                V8Context_GetStatistics(hContext, out scriptCount, out moduleCount, out moduleCacheSize);
            }

            void IV8SplitProxyNative.V8Context_CollectGarbage(V8Context.Handle hContext, bool exhaustive)
            {
                V8Context_CollectGarbage(hContext, exhaustive);
            }

            void IV8SplitProxyNative.V8Context_OnAccessSettingsChanged(V8Context.Handle hContext)
            {
                V8Context_OnAccessSettingsChanged(hContext);
            }

            bool IV8SplitProxyNative.V8Context_BeginCpuProfile(V8Context.Handle hContext, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Context_BeginCpuProfile(hContext, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Context_EndCpuProfile(V8Context.Handle hContext, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Context_EndCpuProfile(hContext, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Context_CollectCpuProfileSample(V8Context.Handle hContext)
            {
                V8Context_CollectCpuProfileSample(hContext);
            }

            uint IV8SplitProxyNative.V8Context_GetCpuProfileSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetCpuProfileSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetCpuProfileSampleInterval(V8Context.Handle hContext, uint value)
            {
                V8Context_SetCpuProfileSampleInterval(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_WriteIsolateHeapSnapshot(V8Context.Handle hContext, IntPtr pStream)
            {
                V8Context_WriteIsolateHeapSnapshot(hContext, pStream);
            }

            #endregion

            #region V8 object methods

            object IV8SplitProxyNative.V8Object_GetNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        V8Object_GetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                        return V8Value.Get(valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_TryGetNamedProperty(V8Object.Handle hObject, string name, out object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        if (V8Object_TryGetNamedProperty(hObject, nameScope.Value, valueScope.Value))
                        {
                            value = V8Value.Get(valueScope.Value);
                            return true;
                        }

                        value = null;
                        return false;
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_SetNamedProperty(V8Object.Handle hObject, string name, object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Object_SetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Object_DeleteNamedProperty(hObject, nameScope.Value);
                }
            }

            string[] IV8SplitProxyNative.V8Object_GetPropertyNames(V8Object.Handle hObject, bool includeIndices)
            {
                using (var namesScope = StdStringArray.CreateScope())
                {
                    V8Object_GetPropertyNames(hObject, includeIndices, namesScope.Value);
                    return StdStringArray.ToArray(namesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_GetIndexedProperty(V8Object.Handle hObject, int index)
            {
                using (var valueScope = V8Value.CreateScope())
                {
                    V8Object_GetIndexedProperty(hObject, index, valueScope.Value);
                    return V8Value.Get(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_SetIndexedProperty(V8Object.Handle hObject, int index, object value)
            {
                using (var valueScope = V8Value.CreateScope(value))
                {
                    V8Object_SetIndexedProperty(hObject, index, valueScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteIndexedProperty(V8Object.Handle hObject, int index)
            {
                return V8Object_DeleteIndexedProperty(hObject, index);
            }

            int[] IV8SplitProxyNative.V8Object_GetPropertyIndices(V8Object.Handle hObject)
            {
                using (var indicesScope = StdInt32Array.CreateScope())
                {
                    V8Object_GetPropertyIndices(hObject, indicesScope.Value);
                    return StdInt32Array.ToArray(indicesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_Invoke(V8Object.Handle hObject, bool asConstructor, object[] args)
            {
                using (var argsScope = StdV8ValueArray.CreateScope(args))
                {
                    using (var resultScope = V8Value.CreateScope())
                    {
                        V8Object_Invoke(hObject, asConstructor, argsScope.Value, resultScope.Value);
                        return V8Value.Get(resultScope.Value);
                    }
                }
            }

            object IV8SplitProxyNative.V8Object_InvokeMethod(V8Object.Handle hObject, string name, object[] args)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var argsScope = StdV8ValueArray.CreateScope(args))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Object_InvokeMethod(hObject, nameScope.Value, argsScope.Value, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_GetArrayBufferOrViewInfo(V8Object.Handle hObject, out IV8Object arrayBuffer, out ulong offset, out ulong size, out ulong length)
            {
                using (var arrayBufferScope = V8Value.CreateScope())
                {
                    V8Object_GetArrayBufferOrViewInfo(hObject, arrayBufferScope.Value, out offset, out size, out length);
                    arrayBuffer = (IV8Object)V8Value.Get(arrayBufferScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewData(V8Object.Handle hObject, IntPtr pAction)
            {
                V8Object_InvokeWithArrayBufferOrViewData(hObject, pAction);
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewDataWithArg(V8Object.Handle hObject, IntPtr pAction, IntPtr pArg)
            {
                V8Object_InvokeWithArrayBufferOrViewDataWithArg(hObject, pAction, pArg);
            }

            #endregion

            #region V8 debug callback methods

            void IV8SplitProxyNative.V8DebugCallback_ConnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_ConnectClient(hCallback);
            }

            void IV8SplitProxyNative.V8DebugCallback_SendCommand(V8DebugCallback.Handle hCallback, string command)
            {
                using (var commandScope = StdString.CreateScope(command))
                {
                    V8DebugCallback_SendCommand(hCallback, commandScope.Value);
                }
            }

            void IV8SplitProxyNative.V8DebugCallback_DisconnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_DisconnectClient(hCallback);
            }

            #endregion

            #region native callback methods

            void IV8SplitProxyNative.NativeCallback_Invoke(NativeCallback.Handle hCallback)
            {
                NativeCallback_Invoke(hCallback);
            }

            #endregion

            #region V8 entity methods

            void IV8SplitProxyNative.V8Entity_Release(V8Entity.Handle hEntity)
            {
                V8Entity_Release(hEntity);
            }

            V8Entity.Handle IV8SplitProxyNative.V8Entity_CloneHandle(V8Entity.Handle hEntity)
            {
                return V8Entity_CloneHandle(hEntity);
            }

            void IV8SplitProxyNative.V8Entity_DestroyHandle(V8Entity.Handle hEntity)
            {
                V8Entity_DestroyHandle(hEntity);
            }

            #endregion

            #region error handling

            void IV8SplitProxyNative.HostException_Schedule(string message, object exception)
            {
                using (var messageScope = StdString.CreateScope(message))
                {
                    using (var exceptionScope = V8Value.CreateScope(exception))
                    {
                        HostException_Schedule(messageScope.Value, exceptionScope.Value);
                    }
                }
            }

            #endregion

            #region unit test support

            UIntPtr IV8SplitProxyNative.V8UnitTestSupport_GetTextDigest(string value)
            {
                using (var valueScope = StdString.CreateScope(value))
                {
                    return V8UnitTestSupport_GetTextDigest(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8UnitTestSupport_GetStatistics(out ulong isolateCount, out ulong contextCount)
            {
                V8UnitTestSupport_GetStatistics(out isolateCount, out contextCount);
            }

            #endregion

            #endregion

            #region native methods

            #region initialization

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyManaged_SetMethodTable(
                [In] IntPtr pMethodTable
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyNative_GetVersion();

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Environment_InitializeICU(
                [In] IntPtr pICUData,
                [In] uint size
            );

            #endregion

            #region memory methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_Allocate(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_AllocateZeroed(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void Memory_Free(
                [In] IntPtr pMemory
            );

            #endregion

            #region StdString methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdString.Ptr StdString_New(
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdString_GetValue(
                [In] StdString.Ptr pString,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_SetValue(
                [In] StdString.Ptr pString,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_Delete(
                [In] StdString.Ptr pString
            );

            #endregion

            #region StdStringArray methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdStringArray.Ptr StdStringArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdStringArray_GetElementCount(
                [In] StdStringArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElementCount(
                [In] StdStringArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdStringArray_GetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_Delete(
                [In] StdStringArray.Ptr pArray
            );

            #endregion

            #region StdByteArray methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdByteArray.Ptr StdByteArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdByteArray_GetElementCount(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_SetElementCount(
                [In] StdByteArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdByteArray_GetData(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_Delete(
                [In] StdByteArray.Ptr pArray
            );

            #endregion

            #region StdInt32Array methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdInt32Array.Ptr StdInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdInt32Array_GetElementCount(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_SetElementCount(
                [In] StdInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdInt32Array_GetData(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_Delete(
                [In] StdInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt32Array methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt32Array.Ptr StdUInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt32Array_GetElementCount(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_SetElementCount(
                [In] StdUInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt32Array_GetData(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_Delete(
                [In] StdUInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt64Array methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt64Array.Ptr StdUInt64Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt64Array_GetElementCount(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_SetElementCount(
                [In] StdUInt64Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt64Array_GetData(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_Delete(
                [In] StdUInt64Array.Ptr pArray
            );

            #endregion

            #region StdPtrArray methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdPtrArray.Ptr StdPtrArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdPtrArray_GetElementCount(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_SetElementCount(
                [In] StdPtrArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdPtrArray_GetData(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_Delete(
                [In] StdPtrArray.Ptr pArray
            );

            #endregion

            #region StdV8ValueArray methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern StdV8ValueArray.Ptr StdV8ValueArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdV8ValueArray_GetElementCount(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_SetElementCount(
                [In] StdV8ValueArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr StdV8ValueArray_GetData(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_Delete(
                [In] StdV8ValueArray.Ptr pArray
            );

            #endregion

            #region V8Value methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr V8Value_New();

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNonexistent(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetUndefined(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNull(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBoolean(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNumber(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetString(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetDateTime(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBigInt(
                [In] V8Value.Ptr pV8Value,
                [In] int signBit,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] bytes,
                [In] int length
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetV8Object(
                [In] V8Value.Ptr pV8Value,
                [In] V8Object.Handle hObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetHostObject(
                [In] V8Value.Ptr pV8Value,
                [In] IntPtr pObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Decode(
                [In] V8Value.Ptr pV8Value,
                [Out] out V8Value.Decoded decoded
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Delete(
                [In] V8Value.Ptr pV8Value
            );

            #endregion

            #region V8CpuProfile methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfile_GetInfo(
                [In] V8CpuProfile.Ptr pProfile,
                [In] V8Entity.Handle hEntity,
                [In] StdString.Ptr pName,
                [Out] out ulong startTimestamp,
                [Out] out ulong endTimestamp,
                [Out] out int sampleCount,
                [Out] out V8CpuProfile.Node.Ptr pRootNode
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfile_GetSample(
                [In] V8CpuProfile.Ptr pProfile,
                [In] int index,
                [Out] out ulong nodeId,
                [Out] out ulong timestamp
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfileNode_GetInfo(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] V8Entity.Handle hEntity,
                [Out] out ulong nodeId,
                [Out] out long scriptId,
                [In] StdString.Ptr pScriptName,
                [In] StdString.Ptr pFunctionName,
                [In] StdString.Ptr pBailoutReason,
                [Out] out long lineNumber,
                [Out] out long columnNumber,
                [Out] out ulong hitCount,
                [Out] out uint hitLineCount,
                [Out] out int childCount
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfileNode_GetHitLines(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] StdInt32Array.Ptr pLineNumbers,
                [In] StdUInt32Array.Ptr pHitCounts
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8CpuProfile.Node.Ptr V8CpuProfileNode_GetChildNode(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] int index
            );

            #endregion

            #region V8 isolate methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Isolate.Handle V8Isolate_Create(
                [In] StdString.Ptr pName,
                [In] int maxNewSpaceSize,
                [In] int maxOldSpaceSize,
                [In] double heapExpansionMultiplier,
                [In] ulong maxArrayBufferAllocation,
                [In] V8RuntimeFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Context.Handle V8Isolate_CreateContext(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] V8ScriptEngineFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Isolate_GetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_AwaitDebuggerAndPause(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CancelAwaitDebugger(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_Compile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileProducingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileConsumingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileUpdatingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetHeapStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectGarbage(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_BeginCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_EndCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectCpuProfileSample(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Isolate_GetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] uint value
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_WriteHeapSnapshot(
                [In] V8Isolate.Handle hIsolate,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 context methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Context_GetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLock(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLockWithArg(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetRootItem(
                [In] V8Context.Handle hContext,
                [In] V8Value.Ptr pItem
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AddGlobalItem(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue,
                [In] [MarshalAs(UnmanagedType.I1)] bool globalMembers
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AwaitDebuggerAndPause(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelAwaitDebugger(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteCode(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_Compile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileProducingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileConsumingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileUpdatingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScript(
                [In] V8Context.Handle hContext,
                [In] V8Script.Handle hScript,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_Interrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateHeapStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong moduleCount,
                [Out] out ulong moduleCacheSize
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectGarbage(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_OnAccessSettingsChanged(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_BeginCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_EndCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectCpuProfileSample(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Context_GetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext,
                [In] uint value
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_WriteIsolateHeapSnapshot(
                [In] V8Context.Handle hContext,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 object methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_TryGetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyNames(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool includeIndices,
                [In] StdStringArray.Ptr pNames
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyIndices(
                [In] V8Object.Handle hObject,
                [In] StdInt32Array.Ptr pIndices
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_Invoke(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool asConstructor,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeMethod(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetArrayBufferOrViewInfo(
                [In] V8Object.Handle hObject,
                [In] V8Value.Ptr pArrayBuffer,
                [Out] out ulong offset,
                [Out] out ulong size,
                [Out] out ulong length
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewData(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewDataWithArg(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            #endregion

            #region V8 debug callback methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_ConnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_SendCommand(
                [In] V8DebugCallback.Handle hCallback,
                [In] StdString.Ptr pCommand
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_DisconnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            #endregion

            #region native callback methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void NativeCallback_Invoke(
                [In] NativeCallback.Handle hCallback
            );

            #endregion

            #region V8 entity methods

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_Release(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Entity.Handle V8Entity_CloneHandle(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_DestroyHandle(
                [In] V8Entity.Handle hEntity
            );

            #endregion

            #region error handling

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void HostException_Schedule(
                [In] StdString.Ptr pMessage,
                [In] V8Value.Ptr pException
            );

            #endregion

            #region unit test support

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8UnitTestSupport_GetTextDigest(
                [In] StdString.Ptr pString
            );

            [DllImport("ClearScriptV8.win-arm64.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8UnitTestSupport_GetStatistics(
                [Out] out ulong isolateCount,
                [Out] out ulong contextCount
            );

            #endregion

            #endregion
        }

        #endregion

        

        #region Nested type: Impl_Linux_X64

        private sealed class Impl_Linux_X64 : IV8SplitProxyNative
        {
            public static readonly IV8SplitProxyNative Instance = new Impl_Linux_X64();

            #region IV8SplitProxyNative implementation

            #region initialization

            IntPtr IV8SplitProxyNative.V8SplitProxyManaged_SetMethodTable(IntPtr pMethodTable)
            {
                return V8SplitProxyManaged_SetMethodTable(pMethodTable);
            }

            string IV8SplitProxyNative.V8SplitProxyNative_GetVersion()
            {
                return Marshal.PtrToStringUni(V8SplitProxyNative_GetVersion());
            }

            void IV8SplitProxyNative.V8Environment_InitializeICU(IntPtr pICUData, uint size)
            {
                V8Environment_InitializeICU(pICUData, size);
            }

            #endregion

            #region memory methods

            IntPtr IV8SplitProxyNative.Memory_Allocate(UIntPtr size)
            {
                return Memory_Allocate(size);
            }

            IntPtr IV8SplitProxyNative.Memory_AllocateZeroed(UIntPtr size)
            {
                return Memory_AllocateZeroed(size);
            }

            void IV8SplitProxyNative.Memory_Free(IntPtr pMemory)
            {
                Memory_Free(pMemory);
            }

            #endregion

            #region StdString methods

            StdString.Ptr IV8SplitProxyNative.StdString_New(string value)
            {
                return StdString_New(value, value.Length);
            }

            string IV8SplitProxyNative.StdString_GetValue(StdString.Ptr pString)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue>(StdString.Ptr pString, Func<IntPtr, int, TValue> factory)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue, TArg>(StdString.Ptr pString, Func<IntPtr, int, TArg, TValue> factory, in TArg arg)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length, arg);
            }

            void IV8SplitProxyNative.StdString_SetValue(StdString.Ptr pString, string value)
            {
                StdString_SetValue(pString, value, value.Length);
            }

            void IV8SplitProxyNative.StdString_Delete(StdString.Ptr pString)
            {
                StdString_Delete(pString);
            }

            #endregion

            #region StdStringArray methods

            StdStringArray.Ptr IV8SplitProxyNative.StdStringArray_New(int elementCount)
            {
                return StdStringArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdStringArray_GetElementCount(StdStringArray.Ptr pArray)
            {
                return StdStringArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdStringArray_SetElementCount(StdStringArray.Ptr pArray, int elementCount)
            {
                StdStringArray_SetElementCount(pArray, elementCount);
            }

            string IV8SplitProxyNative.StdStringArray_GetElement(StdStringArray.Ptr pArray, int index)
            {
                var pValue = StdStringArray_GetElement(pArray, index, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            void IV8SplitProxyNative.StdStringArray_SetElement(StdStringArray.Ptr pArray, int index, string value)
            {
                StdStringArray_SetElement(pArray, index, value, value.Length);
            }

            void IV8SplitProxyNative.StdStringArray_Delete(StdStringArray.Ptr pArray)
            {
                StdStringArray_Delete(pArray);
            }

            #endregion

            #region StdByteArray methods

            StdByteArray.Ptr IV8SplitProxyNative.StdByteArray_New(int elementCount)
            {
                return StdByteArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdByteArray_GetElementCount(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_SetElementCount(StdByteArray.Ptr pArray, int elementCount)
            {
                StdByteArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdByteArray_GetData(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_Delete(StdByteArray.Ptr pArray)
            {
                StdByteArray_Delete(pArray);
            }

            #endregion

            #region StdInt32Array methods

            StdInt32Array.Ptr IV8SplitProxyNative.StdInt32Array_New(int elementCount)
            {
                return StdInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdInt32Array_GetElementCount(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_SetElementCount(StdInt32Array.Ptr pArray, int elementCount)
            {
                StdInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdInt32Array_GetData(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_Delete(StdInt32Array.Ptr pArray)
            {
                StdInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt32Array methods

            StdUInt32Array.Ptr IV8SplitProxyNative.StdUInt32Array_New(int elementCount)
            {
                return StdUInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt32Array_GetElementCount(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_SetElementCount(StdUInt32Array.Ptr pArray, int elementCount)
            {
                StdUInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt32Array_GetData(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_Delete(StdUInt32Array.Ptr pArray)
            {
                StdUInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt64Array methods

            StdUInt64Array.Ptr IV8SplitProxyNative.StdUInt64Array_New(int elementCount)
            {
                return StdUInt64Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt64Array_GetElementCount(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_SetElementCount(StdUInt64Array.Ptr pArray, int elementCount)
            {
                StdUInt64Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt64Array_GetData(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_Delete(StdUInt64Array.Ptr pArray)
            {
                StdUInt64Array_Delete(pArray);
            }

            #endregion

            #region StdPtrArray methods

            StdPtrArray.Ptr IV8SplitProxyNative.StdPtrArray_New(int elementCount)
            {
                return StdPtrArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdPtrArray_GetElementCount(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_SetElementCount(StdPtrArray.Ptr pArray, int elementCount)
            {
                StdPtrArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdPtrArray_GetData(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_Delete(StdPtrArray.Ptr pArray)
            {
                StdPtrArray_Delete(pArray);
            }

            #endregion

            #region StdV8ValueArray methods

            StdV8ValueArray.Ptr IV8SplitProxyNative.StdV8ValueArray_New(int elementCount)
            {
                return StdV8ValueArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdV8ValueArray_GetElementCount(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_SetElementCount(StdV8ValueArray.Ptr pArray, int elementCount)
            {
                StdV8ValueArray_SetElementCount(pArray, elementCount);
            }

            V8Value.Ptr IV8SplitProxyNative.StdV8ValueArray_GetData(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_Delete(StdV8ValueArray.Ptr pArray)
            {
                StdV8ValueArray_Delete(pArray);
            }

            #endregion

            #region V8Value methods

            V8Value.Ptr IV8SplitProxyNative.V8Value_New()
            {
                return V8Value_New();
            }

            void IV8SplitProxyNative.V8Value_SetNonexistent(V8Value.Ptr pV8Value)
            {
                V8Value_SetNonexistent(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetUndefined(V8Value.Ptr pV8Value)
            {
                V8Value_SetUndefined(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetNull(V8Value.Ptr pV8Value)
            {
                V8Value_SetNull(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetBoolean(V8Value.Ptr pV8Value, bool value)
            {
                V8Value_SetBoolean(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetNumber(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetNumber(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetString(V8Value.Ptr pV8Value, string value)
            {
                V8Value_SetString(pV8Value, value, value.Length);
            }

            void IV8SplitProxyNative.V8Value_SetDateTime(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetDateTime(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetBigInt(V8Value.Ptr pV8Value, int signBit, byte[] bytes)
            {
                V8Value_SetBigInt(pV8Value, signBit, bytes, bytes.Length);
            }

            void IV8SplitProxyNative.V8Value_SetV8Object(V8Value.Ptr pV8Value, V8Object.Handle hObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetV8Object(pV8Value, hObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_SetHostObject(V8Value.Ptr pV8Value, IntPtr pObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetHostObject(pV8Value, pObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_Decode(V8Value.Ptr pV8Value, out V8Value.Decoded decoded)
            {
                V8Value_Decode(pV8Value, out decoded);
            }

            void IV8SplitProxyNative.V8Value_Delete(V8Value.Ptr pV8Value)
            {
                V8Value_Delete(pV8Value);
            }

            #endregion

            #region V8CpuProfile methods

            void IV8SplitProxyNative.V8CpuProfile_GetInfo(V8CpuProfile.Ptr pProfile, V8Entity.Handle hEntity, out string name, out ulong startTimestamp, out ulong endTimestamp, out int sampleCount, out V8CpuProfile.Node.Ptr pRootNode)
            {
                using (var nameScope = StdString.CreateScope())
                {
                    V8CpuProfile_GetInfo(pProfile, hEntity, nameScope.Value, out startTimestamp, out endTimestamp, out sampleCount, out pRootNode);
                    name = StdString.GetValue(nameScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8CpuProfile_GetSample(V8CpuProfile.Ptr pProfile, int index, out ulong nodeId, out ulong timestamp)
            {
                return V8CpuProfile_GetSample(pProfile, index, out nodeId, out timestamp);
            }

            void IV8SplitProxyNative.V8CpuProfileNode_GetInfo(V8CpuProfile.Node.Ptr pNode, V8Entity.Handle hEntity, out ulong nodeId, out long scriptId, out string scriptName, out string functionName, out string bailoutReason, out long lineNumber, out long columnNumber, out ulong hitCount, out uint hitLineCount, out int childCount)
            {
                using (var scriptNameScope = StdString.CreateScope())
                {
                    using (var functionNameScope = StdString.CreateScope())
                    {
                        using (var bailoutReasonScope = StdString.CreateScope())
                        {
                            V8CpuProfileNode_GetInfo(pNode, hEntity, out nodeId, out scriptId, scriptNameScope.Value, functionNameScope.Value, bailoutReasonScope.Value, out lineNumber, out columnNumber, out hitCount, out hitLineCount, out childCount);
                            scriptName = StdString.GetValue(scriptNameScope.Value);
                            functionName = StdString.GetValue(functionNameScope.Value);
                            bailoutReason = StdString.GetValue(bailoutReasonScope.Value);

                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8CpuProfileNode_GetHitLines(V8CpuProfile.Node.Ptr pNode, out int[] lineNumbers, out uint[] hitCounts)
            {
                using (var lineNumbersScope = StdInt32Array.CreateScope())
                {
                    using (var hitCountsScope = StdUInt32Array.CreateScope())
                    {
                        var result = V8CpuProfileNode_GetHitLines(pNode, lineNumbersScope.Value, hitCountsScope.Value);
                        lineNumbers = StdInt32Array.ToArray(lineNumbersScope.Value);
                        hitCounts = StdUInt32Array.ToArray(hitCountsScope.Value);
                        return result;
                    }
                }
            }

            V8CpuProfile.Node.Ptr IV8SplitProxyNative.V8CpuProfileNode_GetChildNode(V8CpuProfile.Node.Ptr pNode, int index)
            {
                return V8CpuProfileNode_GetChildNode(pNode, index);
            }

            #endregion

            #region V8 isolate methods

            V8Isolate.Handle IV8SplitProxyNative.V8Isolate_Create(string name, int maxNewSpaceSize, int maxOldSpaceSize, double heapExpansionMultiplier, ulong maxArrayBufferAllocation, V8RuntimeFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_Create(nameScope.Value, maxNewSpaceSize, maxOldSpaceSize, heapExpansionMultiplier, maxArrayBufferAllocation, flags, debugPort);
                }
            }

            V8Context.Handle IV8SplitProxyNative.V8Isolate_CreateContext(V8Isolate.Handle hIsolate, string name, V8ScriptEngineFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_CreateContext(hIsolate, nameScope.Value, flags, debugPort);
                }
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxHeapSize(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxHeapSize(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxHeapSize(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxHeapSize(hIsolate, size);
            }

            double IV8SplitProxyNative.V8Isolate_GetHeapSizeSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetHeapSizeSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetHeapSizeSampleInterval(V8Isolate.Handle hIsolate, double milliseconds)
            {
                V8Isolate_SetHeapSizeSampleInterval(hIsolate, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxStackUsage(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxStackUsage(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxStackUsage(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxStackUsage(hIsolate, size);
            }

            void IV8SplitProxyNative.V8Isolate_AwaitDebuggerAndPause(V8Isolate.Handle hIsolate)
            {
                V8Isolate_AwaitDebuggerAndPause(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_CancelAwaitDebugger(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CancelAwaitDebugger(hIsolate);
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_Compile(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Isolate_Compile(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileProducingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Isolate_CompileProducingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileConsumingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Isolate_CompileConsumingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileUpdatingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Isolate_CompileUpdatingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }

                                return hScript;
                            }
                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8Isolate_GetEnableInterruptPropagation(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetEnableInterruptPropagation(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetEnableInterruptPropagation(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetEnableInterruptPropagation(hIsolate, value);
            }

            bool IV8SplitProxyNative.V8Isolate_GetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetDisableHeapSizeViolationInterrupt(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetDisableHeapSizeViolationInterrupt(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_GetHeapStatistics(V8Isolate.Handle hIsolate, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Isolate_GetHeapStatistics(hIsolate, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Isolate_GetStatistics(V8Isolate.Handle hIsolate, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Isolate_GetStatistics(hIsolate, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectGarbage(V8Isolate.Handle hIsolate, bool exhaustive)
            {
                V8Isolate_CollectGarbage(hIsolate, exhaustive);
            }

            bool IV8SplitProxyNative.V8Isolate_BeginCpuProfile(V8Isolate.Handle hIsolate, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_BeginCpuProfile(hIsolate, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Isolate_EndCpuProfile(V8Isolate.Handle hIsolate, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Isolate_EndCpuProfile(hIsolate, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectCpuProfileSample(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CollectCpuProfileSample(hIsolate);
            }

            uint IV8SplitProxyNative.V8Isolate_GetCpuProfileSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetCpuProfileSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetCpuProfileSampleInterval(V8Isolate.Handle hIsolate, uint value)
            {
                V8Isolate_SetCpuProfileSampleInterval(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_WriteHeapSnapshot(V8Isolate.Handle hIsolate, IntPtr pStream)
            {
                V8Isolate_WriteHeapSnapshot(hIsolate, pStream);
            }

            #endregion

            #region V8 context methods

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateHeapSize(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateHeapSize(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateHeapSize(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateHeapSize(hContext, size);
            }

            double IV8SplitProxyNative.V8Context_GetIsolateHeapSizeSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetIsolateHeapSizeSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetIsolateHeapSizeSampleInterval(V8Context.Handle hContext, double milliseconds)
            {
                V8Context_SetIsolateHeapSizeSampleInterval(hContext, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateStackUsage(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateStackUsage(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateStackUsage(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateStackUsage(hContext, size);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLock(V8Context.Handle hContext, IntPtr pAction)
            {
                V8Context_InvokeWithLock(hContext, pAction);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLockWithArg(V8Context.Handle hContext, IntPtr pAction, IntPtr pArg)
            {
                V8Context_InvokeWithLockWithArg(hContext, pAction, pArg);
            }

            object IV8SplitProxyNative.V8Context_GetRootItem(V8Context.Handle hContext)
            {
                using (var itemScope = V8Value.CreateScope())
                {
                    V8Context_GetRootItem(hContext, itemScope.Value);
                    return V8Value.Get(itemScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_AddGlobalItem(V8Context.Handle hContext, string name, object value, bool globalMembers)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Context_AddGlobalItem(hContext, nameScope.Value, valueScope.Value, globalMembers);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_AwaitDebuggerAndPause(V8Context.Handle hContext)
            {
                V8Context_AwaitDebuggerAndPause(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelAwaitDebugger(V8Context.Handle hContext)
            {
                V8Context_CancelAwaitDebugger(hContext);
            }

            object IV8SplitProxyNative.V8Context_ExecuteCode(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var resultScope = V8Value.CreateScope())
                            {
                                V8Context_ExecuteCode(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, evaluate, resultScope.Value);
                                return V8Value.Get(resultScope.Value);
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Context_ExecuteScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest, evaluate, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_Compile(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Context_Compile(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        return V8Context_CompileScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest);
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileProducingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Context_CompileProducingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileConsumingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Context_CompileConsumingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileUpdatingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Context_CompileUpdatingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }
                
                                return hScript;
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScript(V8Context.Handle hContext, V8Script.Handle hScript, bool evaluate)
            {
                using (var resultScope = V8Value.CreateScope())
                {
                    V8Context_ExecuteScript(hContext, hScript, evaluate, resultScope.Value);
                    return V8Value.Get(resultScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_Interrupt(V8Context.Handle hContext)
            {
                V8Context_Interrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelInterrupt(V8Context.Handle hContext)
            {
                V8Context_CancelInterrupt(hContext);
            }

            bool IV8SplitProxyNative.V8Context_GetEnableIsolateInterruptPropagation(V8Context.Handle hContext)
            {
                return V8Context_GetEnableIsolateInterruptPropagation(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetEnableIsolateInterruptPropagation(V8Context.Handle hContext, bool value)
            {
                V8Context_SetEnableIsolateInterruptPropagation(hContext, value);
            }

            bool IV8SplitProxyNative.V8Context_GetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext)
            {
                return V8Context_GetDisableIsolateHeapSizeViolationInterrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext, bool value)
            {
                V8Context_SetDisableIsolateHeapSizeViolationInterrupt(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateHeapStatistics(V8Context.Handle hContext, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Context_GetIsolateHeapStatistics(hContext, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Context_GetIsolateStatistics(hContext, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_GetStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong moduleCount, out ulong moduleCacheSize)
            {
                V8Context_GetStatistics(hContext, out scriptCount, out moduleCount, out moduleCacheSize);
            }

            void IV8SplitProxyNative.V8Context_CollectGarbage(V8Context.Handle hContext, bool exhaustive)
            {
                V8Context_CollectGarbage(hContext, exhaustive);
            }

            void IV8SplitProxyNative.V8Context_OnAccessSettingsChanged(V8Context.Handle hContext)
            {
                V8Context_OnAccessSettingsChanged(hContext);
            }

            bool IV8SplitProxyNative.V8Context_BeginCpuProfile(V8Context.Handle hContext, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Context_BeginCpuProfile(hContext, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Context_EndCpuProfile(V8Context.Handle hContext, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Context_EndCpuProfile(hContext, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Context_CollectCpuProfileSample(V8Context.Handle hContext)
            {
                V8Context_CollectCpuProfileSample(hContext);
            }

            uint IV8SplitProxyNative.V8Context_GetCpuProfileSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetCpuProfileSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetCpuProfileSampleInterval(V8Context.Handle hContext, uint value)
            {
                V8Context_SetCpuProfileSampleInterval(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_WriteIsolateHeapSnapshot(V8Context.Handle hContext, IntPtr pStream)
            {
                V8Context_WriteIsolateHeapSnapshot(hContext, pStream);
            }

            #endregion

            #region V8 object methods

            object IV8SplitProxyNative.V8Object_GetNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        V8Object_GetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                        return V8Value.Get(valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_TryGetNamedProperty(V8Object.Handle hObject, string name, out object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        if (V8Object_TryGetNamedProperty(hObject, nameScope.Value, valueScope.Value))
                        {
                            value = V8Value.Get(valueScope.Value);
                            return true;
                        }

                        value = null;
                        return false;
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_SetNamedProperty(V8Object.Handle hObject, string name, object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Object_SetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Object_DeleteNamedProperty(hObject, nameScope.Value);
                }
            }

            string[] IV8SplitProxyNative.V8Object_GetPropertyNames(V8Object.Handle hObject, bool includeIndices)
            {
                using (var namesScope = StdStringArray.CreateScope())
                {
                    V8Object_GetPropertyNames(hObject, includeIndices, namesScope.Value);
                    return StdStringArray.ToArray(namesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_GetIndexedProperty(V8Object.Handle hObject, int index)
            {
                using (var valueScope = V8Value.CreateScope())
                {
                    V8Object_GetIndexedProperty(hObject, index, valueScope.Value);
                    return V8Value.Get(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_SetIndexedProperty(V8Object.Handle hObject, int index, object value)
            {
                using (var valueScope = V8Value.CreateScope(value))
                {
                    V8Object_SetIndexedProperty(hObject, index, valueScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteIndexedProperty(V8Object.Handle hObject, int index)
            {
                return V8Object_DeleteIndexedProperty(hObject, index);
            }

            int[] IV8SplitProxyNative.V8Object_GetPropertyIndices(V8Object.Handle hObject)
            {
                using (var indicesScope = StdInt32Array.CreateScope())
                {
                    V8Object_GetPropertyIndices(hObject, indicesScope.Value);
                    return StdInt32Array.ToArray(indicesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_Invoke(V8Object.Handle hObject, bool asConstructor, object[] args)
            {
                using (var argsScope = StdV8ValueArray.CreateScope(args))
                {
                    using (var resultScope = V8Value.CreateScope())
                    {
                        V8Object_Invoke(hObject, asConstructor, argsScope.Value, resultScope.Value);
                        return V8Value.Get(resultScope.Value);
                    }
                }
            }

            object IV8SplitProxyNative.V8Object_InvokeMethod(V8Object.Handle hObject, string name, object[] args)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var argsScope = StdV8ValueArray.CreateScope(args))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Object_InvokeMethod(hObject, nameScope.Value, argsScope.Value, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_GetArrayBufferOrViewInfo(V8Object.Handle hObject, out IV8Object arrayBuffer, out ulong offset, out ulong size, out ulong length)
            {
                using (var arrayBufferScope = V8Value.CreateScope())
                {
                    V8Object_GetArrayBufferOrViewInfo(hObject, arrayBufferScope.Value, out offset, out size, out length);
                    arrayBuffer = (IV8Object)V8Value.Get(arrayBufferScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewData(V8Object.Handle hObject, IntPtr pAction)
            {
                V8Object_InvokeWithArrayBufferOrViewData(hObject, pAction);
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewDataWithArg(V8Object.Handle hObject, IntPtr pAction, IntPtr pArg)
            {
                V8Object_InvokeWithArrayBufferOrViewDataWithArg(hObject, pAction, pArg);
            }

            #endregion

            #region V8 debug callback methods

            void IV8SplitProxyNative.V8DebugCallback_ConnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_ConnectClient(hCallback);
            }

            void IV8SplitProxyNative.V8DebugCallback_SendCommand(V8DebugCallback.Handle hCallback, string command)
            {
                using (var commandScope = StdString.CreateScope(command))
                {
                    V8DebugCallback_SendCommand(hCallback, commandScope.Value);
                }
            }

            void IV8SplitProxyNative.V8DebugCallback_DisconnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_DisconnectClient(hCallback);
            }

            #endregion

            #region native callback methods

            void IV8SplitProxyNative.NativeCallback_Invoke(NativeCallback.Handle hCallback)
            {
                NativeCallback_Invoke(hCallback);
            }

            #endregion

            #region V8 entity methods

            void IV8SplitProxyNative.V8Entity_Release(V8Entity.Handle hEntity)
            {
                V8Entity_Release(hEntity);
            }

            V8Entity.Handle IV8SplitProxyNative.V8Entity_CloneHandle(V8Entity.Handle hEntity)
            {
                return V8Entity_CloneHandle(hEntity);
            }

            void IV8SplitProxyNative.V8Entity_DestroyHandle(V8Entity.Handle hEntity)
            {
                V8Entity_DestroyHandle(hEntity);
            }

            #endregion

            #region error handling

            void IV8SplitProxyNative.HostException_Schedule(string message, object exception)
            {
                using (var messageScope = StdString.CreateScope(message))
                {
                    using (var exceptionScope = V8Value.CreateScope(exception))
                    {
                        HostException_Schedule(messageScope.Value, exceptionScope.Value);
                    }
                }
            }

            #endregion

            #region unit test support

            UIntPtr IV8SplitProxyNative.V8UnitTestSupport_GetTextDigest(string value)
            {
                using (var valueScope = StdString.CreateScope(value))
                {
                    return V8UnitTestSupport_GetTextDigest(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8UnitTestSupport_GetStatistics(out ulong isolateCount, out ulong contextCount)
            {
                V8UnitTestSupport_GetStatistics(out isolateCount, out contextCount);
            }

            #endregion

            #endregion

            #region native methods

            #region initialization

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyManaged_SetMethodTable(
                [In] IntPtr pMethodTable
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyNative_GetVersion();

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Environment_InitializeICU(
                [In] IntPtr pICUData,
                [In] uint size
            );

            #endregion

            #region memory methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_Allocate(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_AllocateZeroed(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void Memory_Free(
                [In] IntPtr pMemory
            );

            #endregion

            #region StdString methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdString.Ptr StdString_New(
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdString_GetValue(
                [In] StdString.Ptr pString,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_SetValue(
                [In] StdString.Ptr pString,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_Delete(
                [In] StdString.Ptr pString
            );

            #endregion

            #region StdStringArray methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdStringArray.Ptr StdStringArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdStringArray_GetElementCount(
                [In] StdStringArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElementCount(
                [In] StdStringArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdStringArray_GetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_Delete(
                [In] StdStringArray.Ptr pArray
            );

            #endregion

            #region StdByteArray methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdByteArray.Ptr StdByteArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdByteArray_GetElementCount(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_SetElementCount(
                [In] StdByteArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdByteArray_GetData(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_Delete(
                [In] StdByteArray.Ptr pArray
            );

            #endregion

            #region StdInt32Array methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdInt32Array.Ptr StdInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdInt32Array_GetElementCount(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_SetElementCount(
                [In] StdInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdInt32Array_GetData(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_Delete(
                [In] StdInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt32Array methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt32Array.Ptr StdUInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt32Array_GetElementCount(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_SetElementCount(
                [In] StdUInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt32Array_GetData(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_Delete(
                [In] StdUInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt64Array methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt64Array.Ptr StdUInt64Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt64Array_GetElementCount(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_SetElementCount(
                [In] StdUInt64Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt64Array_GetData(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_Delete(
                [In] StdUInt64Array.Ptr pArray
            );

            #endregion

            #region StdPtrArray methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdPtrArray.Ptr StdPtrArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdPtrArray_GetElementCount(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_SetElementCount(
                [In] StdPtrArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdPtrArray_GetData(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_Delete(
                [In] StdPtrArray.Ptr pArray
            );

            #endregion

            #region StdV8ValueArray methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdV8ValueArray.Ptr StdV8ValueArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdV8ValueArray_GetElementCount(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_SetElementCount(
                [In] StdV8ValueArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr StdV8ValueArray_GetData(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_Delete(
                [In] StdV8ValueArray.Ptr pArray
            );

            #endregion

            #region V8Value methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr V8Value_New();

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNonexistent(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetUndefined(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNull(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBoolean(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNumber(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetString(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetDateTime(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBigInt(
                [In] V8Value.Ptr pV8Value,
                [In] int signBit,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] bytes,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetV8Object(
                [In] V8Value.Ptr pV8Value,
                [In] V8Object.Handle hObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetHostObject(
                [In] V8Value.Ptr pV8Value,
                [In] IntPtr pObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Decode(
                [In] V8Value.Ptr pV8Value,
                [Out] out V8Value.Decoded decoded
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Delete(
                [In] V8Value.Ptr pV8Value
            );

            #endregion

            #region V8CpuProfile methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfile_GetInfo(
                [In] V8CpuProfile.Ptr pProfile,
                [In] V8Entity.Handle hEntity,
                [In] StdString.Ptr pName,
                [Out] out ulong startTimestamp,
                [Out] out ulong endTimestamp,
                [Out] out int sampleCount,
                [Out] out V8CpuProfile.Node.Ptr pRootNode
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfile_GetSample(
                [In] V8CpuProfile.Ptr pProfile,
                [In] int index,
                [Out] out ulong nodeId,
                [Out] out ulong timestamp
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfileNode_GetInfo(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] V8Entity.Handle hEntity,
                [Out] out ulong nodeId,
                [Out] out long scriptId,
                [In] StdString.Ptr pScriptName,
                [In] StdString.Ptr pFunctionName,
                [In] StdString.Ptr pBailoutReason,
                [Out] out long lineNumber,
                [Out] out long columnNumber,
                [Out] out ulong hitCount,
                [Out] out uint hitLineCount,
                [Out] out int childCount
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfileNode_GetHitLines(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] StdInt32Array.Ptr pLineNumbers,
                [In] StdUInt32Array.Ptr pHitCounts
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8CpuProfile.Node.Ptr V8CpuProfileNode_GetChildNode(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] int index
            );

            #endregion

            #region V8 isolate methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Isolate.Handle V8Isolate_Create(
                [In] StdString.Ptr pName,
                [In] int maxNewSpaceSize,
                [In] int maxOldSpaceSize,
                [In] double heapExpansionMultiplier,
                [In] ulong maxArrayBufferAllocation,
                [In] V8RuntimeFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Context.Handle V8Isolate_CreateContext(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] V8ScriptEngineFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Isolate_GetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_AwaitDebuggerAndPause(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CancelAwaitDebugger(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_Compile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileProducingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileConsumingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileUpdatingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetHeapStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectGarbage(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_BeginCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_EndCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectCpuProfileSample(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Isolate_GetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] uint value
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_WriteHeapSnapshot(
                [In] V8Isolate.Handle hIsolate,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 context methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Context_GetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLock(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLockWithArg(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetRootItem(
                [In] V8Context.Handle hContext,
                [In] V8Value.Ptr pItem
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AddGlobalItem(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue,
                [In] [MarshalAs(UnmanagedType.I1)] bool globalMembers
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AwaitDebuggerAndPause(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelAwaitDebugger(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteCode(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_Compile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileProducingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileConsumingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileUpdatingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScript(
                [In] V8Context.Handle hContext,
                [In] V8Script.Handle hScript,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_Interrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateHeapStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong moduleCount,
                [Out] out ulong moduleCacheSize
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectGarbage(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_OnAccessSettingsChanged(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_BeginCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_EndCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectCpuProfileSample(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Context_GetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext,
                [In] uint value
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_WriteIsolateHeapSnapshot(
                [In] V8Context.Handle hContext,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 object methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_TryGetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyNames(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool includeIndices,
                [In] StdStringArray.Ptr pNames
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyIndices(
                [In] V8Object.Handle hObject,
                [In] StdInt32Array.Ptr pIndices
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_Invoke(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool asConstructor,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeMethod(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetArrayBufferOrViewInfo(
                [In] V8Object.Handle hObject,
                [In] V8Value.Ptr pArrayBuffer,
                [Out] out ulong offset,
                [Out] out ulong size,
                [Out] out ulong length
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewData(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewDataWithArg(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            #endregion

            #region V8 debug callback methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_ConnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_SendCommand(
                [In] V8DebugCallback.Handle hCallback,
                [In] StdString.Ptr pCommand
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_DisconnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            #endregion

            #region native callback methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void NativeCallback_Invoke(
                [In] NativeCallback.Handle hCallback
            );

            #endregion

            #region V8 entity methods

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_Release(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Entity.Handle V8Entity_CloneHandle(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_DestroyHandle(
                [In] V8Entity.Handle hEntity
            );

            #endregion

            #region error handling

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void HostException_Schedule(
                [In] StdString.Ptr pMessage,
                [In] V8Value.Ptr pException
            );

            #endregion

            #region unit test support

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8UnitTestSupport_GetTextDigest(
                [In] StdString.Ptr pString
            );

            [DllImport("ClearScriptV8.linux-x64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8UnitTestSupport_GetStatistics(
                [Out] out ulong isolateCount,
                [Out] out ulong contextCount
            );

            #endregion

            #endregion
        }

        #endregion

        

        #region Nested type: Impl_Linux_Arm64

        private sealed class Impl_Linux_Arm64 : IV8SplitProxyNative
        {
            public static readonly IV8SplitProxyNative Instance = new Impl_Linux_Arm64();

            #region IV8SplitProxyNative implementation

            #region initialization

            IntPtr IV8SplitProxyNative.V8SplitProxyManaged_SetMethodTable(IntPtr pMethodTable)
            {
                return V8SplitProxyManaged_SetMethodTable(pMethodTable);
            }

            string IV8SplitProxyNative.V8SplitProxyNative_GetVersion()
            {
                return Marshal.PtrToStringUni(V8SplitProxyNative_GetVersion());
            }

            void IV8SplitProxyNative.V8Environment_InitializeICU(IntPtr pICUData, uint size)
            {
                V8Environment_InitializeICU(pICUData, size);
            }

            #endregion

            #region memory methods

            IntPtr IV8SplitProxyNative.Memory_Allocate(UIntPtr size)
            {
                return Memory_Allocate(size);
            }

            IntPtr IV8SplitProxyNative.Memory_AllocateZeroed(UIntPtr size)
            {
                return Memory_AllocateZeroed(size);
            }

            void IV8SplitProxyNative.Memory_Free(IntPtr pMemory)
            {
                Memory_Free(pMemory);
            }

            #endregion

            #region StdString methods

            StdString.Ptr IV8SplitProxyNative.StdString_New(string value)
            {
                return StdString_New(value, value.Length);
            }

            string IV8SplitProxyNative.StdString_GetValue(StdString.Ptr pString)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue>(StdString.Ptr pString, Func<IntPtr, int, TValue> factory)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue, TArg>(StdString.Ptr pString, Func<IntPtr, int, TArg, TValue> factory, in TArg arg)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length, arg);
            }

            void IV8SplitProxyNative.StdString_SetValue(StdString.Ptr pString, string value)
            {
                StdString_SetValue(pString, value, value.Length);
            }

            void IV8SplitProxyNative.StdString_Delete(StdString.Ptr pString)
            {
                StdString_Delete(pString);
            }

            #endregion

            #region StdStringArray methods

            StdStringArray.Ptr IV8SplitProxyNative.StdStringArray_New(int elementCount)
            {
                return StdStringArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdStringArray_GetElementCount(StdStringArray.Ptr pArray)
            {
                return StdStringArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdStringArray_SetElementCount(StdStringArray.Ptr pArray, int elementCount)
            {
                StdStringArray_SetElementCount(pArray, elementCount);
            }

            string IV8SplitProxyNative.StdStringArray_GetElement(StdStringArray.Ptr pArray, int index)
            {
                var pValue = StdStringArray_GetElement(pArray, index, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            void IV8SplitProxyNative.StdStringArray_SetElement(StdStringArray.Ptr pArray, int index, string value)
            {
                StdStringArray_SetElement(pArray, index, value, value.Length);
            }

            void IV8SplitProxyNative.StdStringArray_Delete(StdStringArray.Ptr pArray)
            {
                StdStringArray_Delete(pArray);
            }

            #endregion

            #region StdByteArray methods

            StdByteArray.Ptr IV8SplitProxyNative.StdByteArray_New(int elementCount)
            {
                return StdByteArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdByteArray_GetElementCount(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_SetElementCount(StdByteArray.Ptr pArray, int elementCount)
            {
                StdByteArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdByteArray_GetData(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_Delete(StdByteArray.Ptr pArray)
            {
                StdByteArray_Delete(pArray);
            }

            #endregion

            #region StdInt32Array methods

            StdInt32Array.Ptr IV8SplitProxyNative.StdInt32Array_New(int elementCount)
            {
                return StdInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdInt32Array_GetElementCount(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_SetElementCount(StdInt32Array.Ptr pArray, int elementCount)
            {
                StdInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdInt32Array_GetData(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_Delete(StdInt32Array.Ptr pArray)
            {
                StdInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt32Array methods

            StdUInt32Array.Ptr IV8SplitProxyNative.StdUInt32Array_New(int elementCount)
            {
                return StdUInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt32Array_GetElementCount(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_SetElementCount(StdUInt32Array.Ptr pArray, int elementCount)
            {
                StdUInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt32Array_GetData(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_Delete(StdUInt32Array.Ptr pArray)
            {
                StdUInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt64Array methods

            StdUInt64Array.Ptr IV8SplitProxyNative.StdUInt64Array_New(int elementCount)
            {
                return StdUInt64Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt64Array_GetElementCount(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_SetElementCount(StdUInt64Array.Ptr pArray, int elementCount)
            {
                StdUInt64Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt64Array_GetData(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_Delete(StdUInt64Array.Ptr pArray)
            {
                StdUInt64Array_Delete(pArray);
            }

            #endregion

            #region StdPtrArray methods

            StdPtrArray.Ptr IV8SplitProxyNative.StdPtrArray_New(int elementCount)
            {
                return StdPtrArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdPtrArray_GetElementCount(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_SetElementCount(StdPtrArray.Ptr pArray, int elementCount)
            {
                StdPtrArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdPtrArray_GetData(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_Delete(StdPtrArray.Ptr pArray)
            {
                StdPtrArray_Delete(pArray);
            }

            #endregion

            #region StdV8ValueArray methods

            StdV8ValueArray.Ptr IV8SplitProxyNative.StdV8ValueArray_New(int elementCount)
            {
                return StdV8ValueArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdV8ValueArray_GetElementCount(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_SetElementCount(StdV8ValueArray.Ptr pArray, int elementCount)
            {
                StdV8ValueArray_SetElementCount(pArray, elementCount);
            }

            V8Value.Ptr IV8SplitProxyNative.StdV8ValueArray_GetData(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_Delete(StdV8ValueArray.Ptr pArray)
            {
                StdV8ValueArray_Delete(pArray);
            }

            #endregion

            #region V8Value methods

            V8Value.Ptr IV8SplitProxyNative.V8Value_New()
            {
                return V8Value_New();
            }

            void IV8SplitProxyNative.V8Value_SetNonexistent(V8Value.Ptr pV8Value)
            {
                V8Value_SetNonexistent(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetUndefined(V8Value.Ptr pV8Value)
            {
                V8Value_SetUndefined(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetNull(V8Value.Ptr pV8Value)
            {
                V8Value_SetNull(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetBoolean(V8Value.Ptr pV8Value, bool value)
            {
                V8Value_SetBoolean(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetNumber(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetNumber(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetString(V8Value.Ptr pV8Value, string value)
            {
                V8Value_SetString(pV8Value, value, value.Length);
            }

            void IV8SplitProxyNative.V8Value_SetDateTime(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetDateTime(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetBigInt(V8Value.Ptr pV8Value, int signBit, byte[] bytes)
            {
                V8Value_SetBigInt(pV8Value, signBit, bytes, bytes.Length);
            }

            void IV8SplitProxyNative.V8Value_SetV8Object(V8Value.Ptr pV8Value, V8Object.Handle hObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetV8Object(pV8Value, hObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_SetHostObject(V8Value.Ptr pV8Value, IntPtr pObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetHostObject(pV8Value, pObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_Decode(V8Value.Ptr pV8Value, out V8Value.Decoded decoded)
            {
                V8Value_Decode(pV8Value, out decoded);
            }

            void IV8SplitProxyNative.V8Value_Delete(V8Value.Ptr pV8Value)
            {
                V8Value_Delete(pV8Value);
            }

            #endregion

            #region V8CpuProfile methods

            void IV8SplitProxyNative.V8CpuProfile_GetInfo(V8CpuProfile.Ptr pProfile, V8Entity.Handle hEntity, out string name, out ulong startTimestamp, out ulong endTimestamp, out int sampleCount, out V8CpuProfile.Node.Ptr pRootNode)
            {
                using (var nameScope = StdString.CreateScope())
                {
                    V8CpuProfile_GetInfo(pProfile, hEntity, nameScope.Value, out startTimestamp, out endTimestamp, out sampleCount, out pRootNode);
                    name = StdString.GetValue(nameScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8CpuProfile_GetSample(V8CpuProfile.Ptr pProfile, int index, out ulong nodeId, out ulong timestamp)
            {
                return V8CpuProfile_GetSample(pProfile, index, out nodeId, out timestamp);
            }

            void IV8SplitProxyNative.V8CpuProfileNode_GetInfo(V8CpuProfile.Node.Ptr pNode, V8Entity.Handle hEntity, out ulong nodeId, out long scriptId, out string scriptName, out string functionName, out string bailoutReason, out long lineNumber, out long columnNumber, out ulong hitCount, out uint hitLineCount, out int childCount)
            {
                using (var scriptNameScope = StdString.CreateScope())
                {
                    using (var functionNameScope = StdString.CreateScope())
                    {
                        using (var bailoutReasonScope = StdString.CreateScope())
                        {
                            V8CpuProfileNode_GetInfo(pNode, hEntity, out nodeId, out scriptId, scriptNameScope.Value, functionNameScope.Value, bailoutReasonScope.Value, out lineNumber, out columnNumber, out hitCount, out hitLineCount, out childCount);
                            scriptName = StdString.GetValue(scriptNameScope.Value);
                            functionName = StdString.GetValue(functionNameScope.Value);
                            bailoutReason = StdString.GetValue(bailoutReasonScope.Value);

                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8CpuProfileNode_GetHitLines(V8CpuProfile.Node.Ptr pNode, out int[] lineNumbers, out uint[] hitCounts)
            {
                using (var lineNumbersScope = StdInt32Array.CreateScope())
                {
                    using (var hitCountsScope = StdUInt32Array.CreateScope())
                    {
                        var result = V8CpuProfileNode_GetHitLines(pNode, lineNumbersScope.Value, hitCountsScope.Value);
                        lineNumbers = StdInt32Array.ToArray(lineNumbersScope.Value);
                        hitCounts = StdUInt32Array.ToArray(hitCountsScope.Value);
                        return result;
                    }
                }
            }

            V8CpuProfile.Node.Ptr IV8SplitProxyNative.V8CpuProfileNode_GetChildNode(V8CpuProfile.Node.Ptr pNode, int index)
            {
                return V8CpuProfileNode_GetChildNode(pNode, index);
            }

            #endregion

            #region V8 isolate methods

            V8Isolate.Handle IV8SplitProxyNative.V8Isolate_Create(string name, int maxNewSpaceSize, int maxOldSpaceSize, double heapExpansionMultiplier, ulong maxArrayBufferAllocation, V8RuntimeFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_Create(nameScope.Value, maxNewSpaceSize, maxOldSpaceSize, heapExpansionMultiplier, maxArrayBufferAllocation, flags, debugPort);
                }
            }

            V8Context.Handle IV8SplitProxyNative.V8Isolate_CreateContext(V8Isolate.Handle hIsolate, string name, V8ScriptEngineFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_CreateContext(hIsolate, nameScope.Value, flags, debugPort);
                }
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxHeapSize(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxHeapSize(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxHeapSize(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxHeapSize(hIsolate, size);
            }

            double IV8SplitProxyNative.V8Isolate_GetHeapSizeSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetHeapSizeSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetHeapSizeSampleInterval(V8Isolate.Handle hIsolate, double milliseconds)
            {
                V8Isolate_SetHeapSizeSampleInterval(hIsolate, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxStackUsage(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxStackUsage(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxStackUsage(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxStackUsage(hIsolate, size);
            }

            void IV8SplitProxyNative.V8Isolate_AwaitDebuggerAndPause(V8Isolate.Handle hIsolate)
            {
                V8Isolate_AwaitDebuggerAndPause(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_CancelAwaitDebugger(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CancelAwaitDebugger(hIsolate);
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_Compile(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Isolate_Compile(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileProducingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Isolate_CompileProducingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileConsumingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Isolate_CompileConsumingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileUpdatingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Isolate_CompileUpdatingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }

                                return hScript;
                            }
                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8Isolate_GetEnableInterruptPropagation(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetEnableInterruptPropagation(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetEnableInterruptPropagation(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetEnableInterruptPropagation(hIsolate, value);
            }

            bool IV8SplitProxyNative.V8Isolate_GetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetDisableHeapSizeViolationInterrupt(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetDisableHeapSizeViolationInterrupt(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_GetHeapStatistics(V8Isolate.Handle hIsolate, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Isolate_GetHeapStatistics(hIsolate, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Isolate_GetStatistics(V8Isolate.Handle hIsolate, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Isolate_GetStatistics(hIsolate, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectGarbage(V8Isolate.Handle hIsolate, bool exhaustive)
            {
                V8Isolate_CollectGarbage(hIsolate, exhaustive);
            }

            bool IV8SplitProxyNative.V8Isolate_BeginCpuProfile(V8Isolate.Handle hIsolate, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_BeginCpuProfile(hIsolate, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Isolate_EndCpuProfile(V8Isolate.Handle hIsolate, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Isolate_EndCpuProfile(hIsolate, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectCpuProfileSample(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CollectCpuProfileSample(hIsolate);
            }

            uint IV8SplitProxyNative.V8Isolate_GetCpuProfileSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetCpuProfileSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetCpuProfileSampleInterval(V8Isolate.Handle hIsolate, uint value)
            {
                V8Isolate_SetCpuProfileSampleInterval(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_WriteHeapSnapshot(V8Isolate.Handle hIsolate, IntPtr pStream)
            {
                V8Isolate_WriteHeapSnapshot(hIsolate, pStream);
            }

            #endregion

            #region V8 context methods

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateHeapSize(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateHeapSize(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateHeapSize(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateHeapSize(hContext, size);
            }

            double IV8SplitProxyNative.V8Context_GetIsolateHeapSizeSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetIsolateHeapSizeSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetIsolateHeapSizeSampleInterval(V8Context.Handle hContext, double milliseconds)
            {
                V8Context_SetIsolateHeapSizeSampleInterval(hContext, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateStackUsage(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateStackUsage(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateStackUsage(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateStackUsage(hContext, size);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLock(V8Context.Handle hContext, IntPtr pAction)
            {
                V8Context_InvokeWithLock(hContext, pAction);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLockWithArg(V8Context.Handle hContext, IntPtr pAction, IntPtr pArg)
            {
                V8Context_InvokeWithLockWithArg(hContext, pAction, pArg);
            }

            object IV8SplitProxyNative.V8Context_GetRootItem(V8Context.Handle hContext)
            {
                using (var itemScope = V8Value.CreateScope())
                {
                    V8Context_GetRootItem(hContext, itemScope.Value);
                    return V8Value.Get(itemScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_AddGlobalItem(V8Context.Handle hContext, string name, object value, bool globalMembers)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Context_AddGlobalItem(hContext, nameScope.Value, valueScope.Value, globalMembers);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_AwaitDebuggerAndPause(V8Context.Handle hContext)
            {
                V8Context_AwaitDebuggerAndPause(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelAwaitDebugger(V8Context.Handle hContext)
            {
                V8Context_CancelAwaitDebugger(hContext);
            }

            object IV8SplitProxyNative.V8Context_ExecuteCode(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var resultScope = V8Value.CreateScope())
                            {
                                V8Context_ExecuteCode(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, evaluate, resultScope.Value);
                                return V8Value.Get(resultScope.Value);
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Context_ExecuteScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest, evaluate, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_Compile(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Context_Compile(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        return V8Context_CompileScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest);
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileProducingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Context_CompileProducingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileConsumingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Context_CompileConsumingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileUpdatingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Context_CompileUpdatingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }
                
                                return hScript;
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScript(V8Context.Handle hContext, V8Script.Handle hScript, bool evaluate)
            {
                using (var resultScope = V8Value.CreateScope())
                {
                    V8Context_ExecuteScript(hContext, hScript, evaluate, resultScope.Value);
                    return V8Value.Get(resultScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_Interrupt(V8Context.Handle hContext)
            {
                V8Context_Interrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelInterrupt(V8Context.Handle hContext)
            {
                V8Context_CancelInterrupt(hContext);
            }

            bool IV8SplitProxyNative.V8Context_GetEnableIsolateInterruptPropagation(V8Context.Handle hContext)
            {
                return V8Context_GetEnableIsolateInterruptPropagation(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetEnableIsolateInterruptPropagation(V8Context.Handle hContext, bool value)
            {
                V8Context_SetEnableIsolateInterruptPropagation(hContext, value);
            }

            bool IV8SplitProxyNative.V8Context_GetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext)
            {
                return V8Context_GetDisableIsolateHeapSizeViolationInterrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext, bool value)
            {
                V8Context_SetDisableIsolateHeapSizeViolationInterrupt(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateHeapStatistics(V8Context.Handle hContext, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Context_GetIsolateHeapStatistics(hContext, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Context_GetIsolateStatistics(hContext, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_GetStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong moduleCount, out ulong moduleCacheSize)
            {
                V8Context_GetStatistics(hContext, out scriptCount, out moduleCount, out moduleCacheSize);
            }

            void IV8SplitProxyNative.V8Context_CollectGarbage(V8Context.Handle hContext, bool exhaustive)
            {
                V8Context_CollectGarbage(hContext, exhaustive);
            }

            void IV8SplitProxyNative.V8Context_OnAccessSettingsChanged(V8Context.Handle hContext)
            {
                V8Context_OnAccessSettingsChanged(hContext);
            }

            bool IV8SplitProxyNative.V8Context_BeginCpuProfile(V8Context.Handle hContext, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Context_BeginCpuProfile(hContext, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Context_EndCpuProfile(V8Context.Handle hContext, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Context_EndCpuProfile(hContext, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Context_CollectCpuProfileSample(V8Context.Handle hContext)
            {
                V8Context_CollectCpuProfileSample(hContext);
            }

            uint IV8SplitProxyNative.V8Context_GetCpuProfileSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetCpuProfileSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetCpuProfileSampleInterval(V8Context.Handle hContext, uint value)
            {
                V8Context_SetCpuProfileSampleInterval(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_WriteIsolateHeapSnapshot(V8Context.Handle hContext, IntPtr pStream)
            {
                V8Context_WriteIsolateHeapSnapshot(hContext, pStream);
            }

            #endregion

            #region V8 object methods

            object IV8SplitProxyNative.V8Object_GetNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        V8Object_GetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                        return V8Value.Get(valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_TryGetNamedProperty(V8Object.Handle hObject, string name, out object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        if (V8Object_TryGetNamedProperty(hObject, nameScope.Value, valueScope.Value))
                        {
                            value = V8Value.Get(valueScope.Value);
                            return true;
                        }

                        value = null;
                        return false;
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_SetNamedProperty(V8Object.Handle hObject, string name, object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Object_SetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Object_DeleteNamedProperty(hObject, nameScope.Value);
                }
            }

            string[] IV8SplitProxyNative.V8Object_GetPropertyNames(V8Object.Handle hObject, bool includeIndices)
            {
                using (var namesScope = StdStringArray.CreateScope())
                {
                    V8Object_GetPropertyNames(hObject, includeIndices, namesScope.Value);
                    return StdStringArray.ToArray(namesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_GetIndexedProperty(V8Object.Handle hObject, int index)
            {
                using (var valueScope = V8Value.CreateScope())
                {
                    V8Object_GetIndexedProperty(hObject, index, valueScope.Value);
                    return V8Value.Get(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_SetIndexedProperty(V8Object.Handle hObject, int index, object value)
            {
                using (var valueScope = V8Value.CreateScope(value))
                {
                    V8Object_SetIndexedProperty(hObject, index, valueScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteIndexedProperty(V8Object.Handle hObject, int index)
            {
                return V8Object_DeleteIndexedProperty(hObject, index);
            }

            int[] IV8SplitProxyNative.V8Object_GetPropertyIndices(V8Object.Handle hObject)
            {
                using (var indicesScope = StdInt32Array.CreateScope())
                {
                    V8Object_GetPropertyIndices(hObject, indicesScope.Value);
                    return StdInt32Array.ToArray(indicesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_Invoke(V8Object.Handle hObject, bool asConstructor, object[] args)
            {
                using (var argsScope = StdV8ValueArray.CreateScope(args))
                {
                    using (var resultScope = V8Value.CreateScope())
                    {
                        V8Object_Invoke(hObject, asConstructor, argsScope.Value, resultScope.Value);
                        return V8Value.Get(resultScope.Value);
                    }
                }
            }

            object IV8SplitProxyNative.V8Object_InvokeMethod(V8Object.Handle hObject, string name, object[] args)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var argsScope = StdV8ValueArray.CreateScope(args))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Object_InvokeMethod(hObject, nameScope.Value, argsScope.Value, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_GetArrayBufferOrViewInfo(V8Object.Handle hObject, out IV8Object arrayBuffer, out ulong offset, out ulong size, out ulong length)
            {
                using (var arrayBufferScope = V8Value.CreateScope())
                {
                    V8Object_GetArrayBufferOrViewInfo(hObject, arrayBufferScope.Value, out offset, out size, out length);
                    arrayBuffer = (IV8Object)V8Value.Get(arrayBufferScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewData(V8Object.Handle hObject, IntPtr pAction)
            {
                V8Object_InvokeWithArrayBufferOrViewData(hObject, pAction);
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewDataWithArg(V8Object.Handle hObject, IntPtr pAction, IntPtr pArg)
            {
                V8Object_InvokeWithArrayBufferOrViewDataWithArg(hObject, pAction, pArg);
            }

            #endregion

            #region V8 debug callback methods

            void IV8SplitProxyNative.V8DebugCallback_ConnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_ConnectClient(hCallback);
            }

            void IV8SplitProxyNative.V8DebugCallback_SendCommand(V8DebugCallback.Handle hCallback, string command)
            {
                using (var commandScope = StdString.CreateScope(command))
                {
                    V8DebugCallback_SendCommand(hCallback, commandScope.Value);
                }
            }

            void IV8SplitProxyNative.V8DebugCallback_DisconnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_DisconnectClient(hCallback);
            }

            #endregion

            #region native callback methods

            void IV8SplitProxyNative.NativeCallback_Invoke(NativeCallback.Handle hCallback)
            {
                NativeCallback_Invoke(hCallback);
            }

            #endregion

            #region V8 entity methods

            void IV8SplitProxyNative.V8Entity_Release(V8Entity.Handle hEntity)
            {
                V8Entity_Release(hEntity);
            }

            V8Entity.Handle IV8SplitProxyNative.V8Entity_CloneHandle(V8Entity.Handle hEntity)
            {
                return V8Entity_CloneHandle(hEntity);
            }

            void IV8SplitProxyNative.V8Entity_DestroyHandle(V8Entity.Handle hEntity)
            {
                V8Entity_DestroyHandle(hEntity);
            }

            #endregion

            #region error handling

            void IV8SplitProxyNative.HostException_Schedule(string message, object exception)
            {
                using (var messageScope = StdString.CreateScope(message))
                {
                    using (var exceptionScope = V8Value.CreateScope(exception))
                    {
                        HostException_Schedule(messageScope.Value, exceptionScope.Value);
                    }
                }
            }

            #endregion

            #region unit test support

            UIntPtr IV8SplitProxyNative.V8UnitTestSupport_GetTextDigest(string value)
            {
                using (var valueScope = StdString.CreateScope(value))
                {
                    return V8UnitTestSupport_GetTextDigest(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8UnitTestSupport_GetStatistics(out ulong isolateCount, out ulong contextCount)
            {
                V8UnitTestSupport_GetStatistics(out isolateCount, out contextCount);
            }

            #endregion

            #endregion

            #region native methods

            #region initialization

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyManaged_SetMethodTable(
                [In] IntPtr pMethodTable
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyNative_GetVersion();

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Environment_InitializeICU(
                [In] IntPtr pICUData,
                [In] uint size
            );

            #endregion

            #region memory methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_Allocate(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_AllocateZeroed(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void Memory_Free(
                [In] IntPtr pMemory
            );

            #endregion

            #region StdString methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdString.Ptr StdString_New(
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdString_GetValue(
                [In] StdString.Ptr pString,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_SetValue(
                [In] StdString.Ptr pString,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_Delete(
                [In] StdString.Ptr pString
            );

            #endregion

            #region StdStringArray methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdStringArray.Ptr StdStringArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdStringArray_GetElementCount(
                [In] StdStringArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElementCount(
                [In] StdStringArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdStringArray_GetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_Delete(
                [In] StdStringArray.Ptr pArray
            );

            #endregion

            #region StdByteArray methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdByteArray.Ptr StdByteArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdByteArray_GetElementCount(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_SetElementCount(
                [In] StdByteArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdByteArray_GetData(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_Delete(
                [In] StdByteArray.Ptr pArray
            );

            #endregion

            #region StdInt32Array methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdInt32Array.Ptr StdInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdInt32Array_GetElementCount(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_SetElementCount(
                [In] StdInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdInt32Array_GetData(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_Delete(
                [In] StdInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt32Array methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt32Array.Ptr StdUInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt32Array_GetElementCount(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_SetElementCount(
                [In] StdUInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt32Array_GetData(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_Delete(
                [In] StdUInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt64Array methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt64Array.Ptr StdUInt64Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt64Array_GetElementCount(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_SetElementCount(
                [In] StdUInt64Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt64Array_GetData(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_Delete(
                [In] StdUInt64Array.Ptr pArray
            );

            #endregion

            #region StdPtrArray methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdPtrArray.Ptr StdPtrArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdPtrArray_GetElementCount(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_SetElementCount(
                [In] StdPtrArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdPtrArray_GetData(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_Delete(
                [In] StdPtrArray.Ptr pArray
            );

            #endregion

            #region StdV8ValueArray methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdV8ValueArray.Ptr StdV8ValueArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdV8ValueArray_GetElementCount(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_SetElementCount(
                [In] StdV8ValueArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr StdV8ValueArray_GetData(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_Delete(
                [In] StdV8ValueArray.Ptr pArray
            );

            #endregion

            #region V8Value methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr V8Value_New();

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNonexistent(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetUndefined(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNull(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBoolean(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNumber(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetString(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetDateTime(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBigInt(
                [In] V8Value.Ptr pV8Value,
                [In] int signBit,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] bytes,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetV8Object(
                [In] V8Value.Ptr pV8Value,
                [In] V8Object.Handle hObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetHostObject(
                [In] V8Value.Ptr pV8Value,
                [In] IntPtr pObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Decode(
                [In] V8Value.Ptr pV8Value,
                [Out] out V8Value.Decoded decoded
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Delete(
                [In] V8Value.Ptr pV8Value
            );

            #endregion

            #region V8CpuProfile methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfile_GetInfo(
                [In] V8CpuProfile.Ptr pProfile,
                [In] V8Entity.Handle hEntity,
                [In] StdString.Ptr pName,
                [Out] out ulong startTimestamp,
                [Out] out ulong endTimestamp,
                [Out] out int sampleCount,
                [Out] out V8CpuProfile.Node.Ptr pRootNode
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfile_GetSample(
                [In] V8CpuProfile.Ptr pProfile,
                [In] int index,
                [Out] out ulong nodeId,
                [Out] out ulong timestamp
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfileNode_GetInfo(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] V8Entity.Handle hEntity,
                [Out] out ulong nodeId,
                [Out] out long scriptId,
                [In] StdString.Ptr pScriptName,
                [In] StdString.Ptr pFunctionName,
                [In] StdString.Ptr pBailoutReason,
                [Out] out long lineNumber,
                [Out] out long columnNumber,
                [Out] out ulong hitCount,
                [Out] out uint hitLineCount,
                [Out] out int childCount
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfileNode_GetHitLines(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] StdInt32Array.Ptr pLineNumbers,
                [In] StdUInt32Array.Ptr pHitCounts
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8CpuProfile.Node.Ptr V8CpuProfileNode_GetChildNode(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] int index
            );

            #endregion

            #region V8 isolate methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Isolate.Handle V8Isolate_Create(
                [In] StdString.Ptr pName,
                [In] int maxNewSpaceSize,
                [In] int maxOldSpaceSize,
                [In] double heapExpansionMultiplier,
                [In] ulong maxArrayBufferAllocation,
                [In] V8RuntimeFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Context.Handle V8Isolate_CreateContext(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] V8ScriptEngineFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Isolate_GetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_AwaitDebuggerAndPause(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CancelAwaitDebugger(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_Compile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileProducingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileConsumingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileUpdatingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetHeapStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectGarbage(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_BeginCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_EndCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectCpuProfileSample(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Isolate_GetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] uint value
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_WriteHeapSnapshot(
                [In] V8Isolate.Handle hIsolate,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 context methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Context_GetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLock(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLockWithArg(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetRootItem(
                [In] V8Context.Handle hContext,
                [In] V8Value.Ptr pItem
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AddGlobalItem(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue,
                [In] [MarshalAs(UnmanagedType.I1)] bool globalMembers
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AwaitDebuggerAndPause(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelAwaitDebugger(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteCode(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_Compile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileProducingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileConsumingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileUpdatingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScript(
                [In] V8Context.Handle hContext,
                [In] V8Script.Handle hScript,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_Interrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateHeapStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong moduleCount,
                [Out] out ulong moduleCacheSize
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectGarbage(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_OnAccessSettingsChanged(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_BeginCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_EndCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectCpuProfileSample(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Context_GetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext,
                [In] uint value
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_WriteIsolateHeapSnapshot(
                [In] V8Context.Handle hContext,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 object methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_TryGetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyNames(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool includeIndices,
                [In] StdStringArray.Ptr pNames
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyIndices(
                [In] V8Object.Handle hObject,
                [In] StdInt32Array.Ptr pIndices
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_Invoke(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool asConstructor,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeMethod(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetArrayBufferOrViewInfo(
                [In] V8Object.Handle hObject,
                [In] V8Value.Ptr pArrayBuffer,
                [Out] out ulong offset,
                [Out] out ulong size,
                [Out] out ulong length
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewData(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewDataWithArg(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            #endregion

            #region V8 debug callback methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_ConnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_SendCommand(
                [In] V8DebugCallback.Handle hCallback,
                [In] StdString.Ptr pCommand
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_DisconnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            #endregion

            #region native callback methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void NativeCallback_Invoke(
                [In] NativeCallback.Handle hCallback
            );

            #endregion

            #region V8 entity methods

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_Release(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Entity.Handle V8Entity_CloneHandle(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_DestroyHandle(
                [In] V8Entity.Handle hEntity
            );

            #endregion

            #region error handling

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void HostException_Schedule(
                [In] StdString.Ptr pMessage,
                [In] V8Value.Ptr pException
            );

            #endregion

            #region unit test support

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8UnitTestSupport_GetTextDigest(
                [In] StdString.Ptr pString
            );

            [DllImport("ClearScriptV8.linux-arm64.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8UnitTestSupport_GetStatistics(
                [Out] out ulong isolateCount,
                [Out] out ulong contextCount
            );

            #endregion

            #endregion
        }

        #endregion

        

        #region Nested type: Impl_Linux_Arm

        private sealed class Impl_Linux_Arm : IV8SplitProxyNative
        {
            public static readonly IV8SplitProxyNative Instance = new Impl_Linux_Arm();

            #region IV8SplitProxyNative implementation

            #region initialization

            IntPtr IV8SplitProxyNative.V8SplitProxyManaged_SetMethodTable(IntPtr pMethodTable)
            {
                return V8SplitProxyManaged_SetMethodTable(pMethodTable);
            }

            string IV8SplitProxyNative.V8SplitProxyNative_GetVersion()
            {
                return Marshal.PtrToStringUni(V8SplitProxyNative_GetVersion());
            }

            void IV8SplitProxyNative.V8Environment_InitializeICU(IntPtr pICUData, uint size)
            {
                V8Environment_InitializeICU(pICUData, size);
            }

            #endregion

            #region memory methods

            IntPtr IV8SplitProxyNative.Memory_Allocate(UIntPtr size)
            {
                return Memory_Allocate(size);
            }

            IntPtr IV8SplitProxyNative.Memory_AllocateZeroed(UIntPtr size)
            {
                return Memory_AllocateZeroed(size);
            }

            void IV8SplitProxyNative.Memory_Free(IntPtr pMemory)
            {
                Memory_Free(pMemory);
            }

            #endregion

            #region StdString methods

            StdString.Ptr IV8SplitProxyNative.StdString_New(string value)
            {
                return StdString_New(value, value.Length);
            }

            string IV8SplitProxyNative.StdString_GetValue(StdString.Ptr pString)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue>(StdString.Ptr pString, Func<IntPtr, int, TValue> factory)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue, TArg>(StdString.Ptr pString, Func<IntPtr, int, TArg, TValue> factory, in TArg arg)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length, arg);
            }

            void IV8SplitProxyNative.StdString_SetValue(StdString.Ptr pString, string value)
            {
                StdString_SetValue(pString, value, value.Length);
            }

            void IV8SplitProxyNative.StdString_Delete(StdString.Ptr pString)
            {
                StdString_Delete(pString);
            }

            #endregion

            #region StdStringArray methods

            StdStringArray.Ptr IV8SplitProxyNative.StdStringArray_New(int elementCount)
            {
                return StdStringArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdStringArray_GetElementCount(StdStringArray.Ptr pArray)
            {
                return StdStringArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdStringArray_SetElementCount(StdStringArray.Ptr pArray, int elementCount)
            {
                StdStringArray_SetElementCount(pArray, elementCount);
            }

            string IV8SplitProxyNative.StdStringArray_GetElement(StdStringArray.Ptr pArray, int index)
            {
                var pValue = StdStringArray_GetElement(pArray, index, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            void IV8SplitProxyNative.StdStringArray_SetElement(StdStringArray.Ptr pArray, int index, string value)
            {
                StdStringArray_SetElement(pArray, index, value, value.Length);
            }

            void IV8SplitProxyNative.StdStringArray_Delete(StdStringArray.Ptr pArray)
            {
                StdStringArray_Delete(pArray);
            }

            #endregion

            #region StdByteArray methods

            StdByteArray.Ptr IV8SplitProxyNative.StdByteArray_New(int elementCount)
            {
                return StdByteArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdByteArray_GetElementCount(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_SetElementCount(StdByteArray.Ptr pArray, int elementCount)
            {
                StdByteArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdByteArray_GetData(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_Delete(StdByteArray.Ptr pArray)
            {
                StdByteArray_Delete(pArray);
            }

            #endregion

            #region StdInt32Array methods

            StdInt32Array.Ptr IV8SplitProxyNative.StdInt32Array_New(int elementCount)
            {
                return StdInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdInt32Array_GetElementCount(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_SetElementCount(StdInt32Array.Ptr pArray, int elementCount)
            {
                StdInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdInt32Array_GetData(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_Delete(StdInt32Array.Ptr pArray)
            {
                StdInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt32Array methods

            StdUInt32Array.Ptr IV8SplitProxyNative.StdUInt32Array_New(int elementCount)
            {
                return StdUInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt32Array_GetElementCount(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_SetElementCount(StdUInt32Array.Ptr pArray, int elementCount)
            {
                StdUInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt32Array_GetData(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_Delete(StdUInt32Array.Ptr pArray)
            {
                StdUInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt64Array methods

            StdUInt64Array.Ptr IV8SplitProxyNative.StdUInt64Array_New(int elementCount)
            {
                return StdUInt64Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt64Array_GetElementCount(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_SetElementCount(StdUInt64Array.Ptr pArray, int elementCount)
            {
                StdUInt64Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt64Array_GetData(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_Delete(StdUInt64Array.Ptr pArray)
            {
                StdUInt64Array_Delete(pArray);
            }

            #endregion

            #region StdPtrArray methods

            StdPtrArray.Ptr IV8SplitProxyNative.StdPtrArray_New(int elementCount)
            {
                return StdPtrArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdPtrArray_GetElementCount(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_SetElementCount(StdPtrArray.Ptr pArray, int elementCount)
            {
                StdPtrArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdPtrArray_GetData(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_Delete(StdPtrArray.Ptr pArray)
            {
                StdPtrArray_Delete(pArray);
            }

            #endregion

            #region StdV8ValueArray methods

            StdV8ValueArray.Ptr IV8SplitProxyNative.StdV8ValueArray_New(int elementCount)
            {
                return StdV8ValueArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdV8ValueArray_GetElementCount(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_SetElementCount(StdV8ValueArray.Ptr pArray, int elementCount)
            {
                StdV8ValueArray_SetElementCount(pArray, elementCount);
            }

            V8Value.Ptr IV8SplitProxyNative.StdV8ValueArray_GetData(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_Delete(StdV8ValueArray.Ptr pArray)
            {
                StdV8ValueArray_Delete(pArray);
            }

            #endregion

            #region V8Value methods

            V8Value.Ptr IV8SplitProxyNative.V8Value_New()
            {
                return V8Value_New();
            }

            void IV8SplitProxyNative.V8Value_SetNonexistent(V8Value.Ptr pV8Value)
            {
                V8Value_SetNonexistent(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetUndefined(V8Value.Ptr pV8Value)
            {
                V8Value_SetUndefined(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetNull(V8Value.Ptr pV8Value)
            {
                V8Value_SetNull(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetBoolean(V8Value.Ptr pV8Value, bool value)
            {
                V8Value_SetBoolean(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetNumber(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetNumber(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetString(V8Value.Ptr pV8Value, string value)
            {
                V8Value_SetString(pV8Value, value, value.Length);
            }

            void IV8SplitProxyNative.V8Value_SetDateTime(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetDateTime(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetBigInt(V8Value.Ptr pV8Value, int signBit, byte[] bytes)
            {
                V8Value_SetBigInt(pV8Value, signBit, bytes, bytes.Length);
            }

            void IV8SplitProxyNative.V8Value_SetV8Object(V8Value.Ptr pV8Value, V8Object.Handle hObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetV8Object(pV8Value, hObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_SetHostObject(V8Value.Ptr pV8Value, IntPtr pObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetHostObject(pV8Value, pObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_Decode(V8Value.Ptr pV8Value, out V8Value.Decoded decoded)
            {
                V8Value_Decode(pV8Value, out decoded);
            }

            void IV8SplitProxyNative.V8Value_Delete(V8Value.Ptr pV8Value)
            {
                V8Value_Delete(pV8Value);
            }

            #endregion

            #region V8CpuProfile methods

            void IV8SplitProxyNative.V8CpuProfile_GetInfo(V8CpuProfile.Ptr pProfile, V8Entity.Handle hEntity, out string name, out ulong startTimestamp, out ulong endTimestamp, out int sampleCount, out V8CpuProfile.Node.Ptr pRootNode)
            {
                using (var nameScope = StdString.CreateScope())
                {
                    V8CpuProfile_GetInfo(pProfile, hEntity, nameScope.Value, out startTimestamp, out endTimestamp, out sampleCount, out pRootNode);
                    name = StdString.GetValue(nameScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8CpuProfile_GetSample(V8CpuProfile.Ptr pProfile, int index, out ulong nodeId, out ulong timestamp)
            {
                return V8CpuProfile_GetSample(pProfile, index, out nodeId, out timestamp);
            }

            void IV8SplitProxyNative.V8CpuProfileNode_GetInfo(V8CpuProfile.Node.Ptr pNode, V8Entity.Handle hEntity, out ulong nodeId, out long scriptId, out string scriptName, out string functionName, out string bailoutReason, out long lineNumber, out long columnNumber, out ulong hitCount, out uint hitLineCount, out int childCount)
            {
                using (var scriptNameScope = StdString.CreateScope())
                {
                    using (var functionNameScope = StdString.CreateScope())
                    {
                        using (var bailoutReasonScope = StdString.CreateScope())
                        {
                            V8CpuProfileNode_GetInfo(pNode, hEntity, out nodeId, out scriptId, scriptNameScope.Value, functionNameScope.Value, bailoutReasonScope.Value, out lineNumber, out columnNumber, out hitCount, out hitLineCount, out childCount);
                            scriptName = StdString.GetValue(scriptNameScope.Value);
                            functionName = StdString.GetValue(functionNameScope.Value);
                            bailoutReason = StdString.GetValue(bailoutReasonScope.Value);

                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8CpuProfileNode_GetHitLines(V8CpuProfile.Node.Ptr pNode, out int[] lineNumbers, out uint[] hitCounts)
            {
                using (var lineNumbersScope = StdInt32Array.CreateScope())
                {
                    using (var hitCountsScope = StdUInt32Array.CreateScope())
                    {
                        var result = V8CpuProfileNode_GetHitLines(pNode, lineNumbersScope.Value, hitCountsScope.Value);
                        lineNumbers = StdInt32Array.ToArray(lineNumbersScope.Value);
                        hitCounts = StdUInt32Array.ToArray(hitCountsScope.Value);
                        return result;
                    }
                }
            }

            V8CpuProfile.Node.Ptr IV8SplitProxyNative.V8CpuProfileNode_GetChildNode(V8CpuProfile.Node.Ptr pNode, int index)
            {
                return V8CpuProfileNode_GetChildNode(pNode, index);
            }

            #endregion

            #region V8 isolate methods

            V8Isolate.Handle IV8SplitProxyNative.V8Isolate_Create(string name, int maxNewSpaceSize, int maxOldSpaceSize, double heapExpansionMultiplier, ulong maxArrayBufferAllocation, V8RuntimeFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_Create(nameScope.Value, maxNewSpaceSize, maxOldSpaceSize, heapExpansionMultiplier, maxArrayBufferAllocation, flags, debugPort);
                }
            }

            V8Context.Handle IV8SplitProxyNative.V8Isolate_CreateContext(V8Isolate.Handle hIsolate, string name, V8ScriptEngineFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_CreateContext(hIsolate, nameScope.Value, flags, debugPort);
                }
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxHeapSize(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxHeapSize(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxHeapSize(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxHeapSize(hIsolate, size);
            }

            double IV8SplitProxyNative.V8Isolate_GetHeapSizeSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetHeapSizeSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetHeapSizeSampleInterval(V8Isolate.Handle hIsolate, double milliseconds)
            {
                V8Isolate_SetHeapSizeSampleInterval(hIsolate, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxStackUsage(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxStackUsage(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxStackUsage(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxStackUsage(hIsolate, size);
            }

            void IV8SplitProxyNative.V8Isolate_AwaitDebuggerAndPause(V8Isolate.Handle hIsolate)
            {
                V8Isolate_AwaitDebuggerAndPause(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_CancelAwaitDebugger(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CancelAwaitDebugger(hIsolate);
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_Compile(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Isolate_Compile(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileProducingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Isolate_CompileProducingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileConsumingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Isolate_CompileConsumingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileUpdatingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Isolate_CompileUpdatingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }

                                return hScript;
                            }
                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8Isolate_GetEnableInterruptPropagation(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetEnableInterruptPropagation(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetEnableInterruptPropagation(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetEnableInterruptPropagation(hIsolate, value);
            }

            bool IV8SplitProxyNative.V8Isolate_GetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetDisableHeapSizeViolationInterrupt(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetDisableHeapSizeViolationInterrupt(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_GetHeapStatistics(V8Isolate.Handle hIsolate, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Isolate_GetHeapStatistics(hIsolate, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Isolate_GetStatistics(V8Isolate.Handle hIsolate, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Isolate_GetStatistics(hIsolate, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectGarbage(V8Isolate.Handle hIsolate, bool exhaustive)
            {
                V8Isolate_CollectGarbage(hIsolate, exhaustive);
            }

            bool IV8SplitProxyNative.V8Isolate_BeginCpuProfile(V8Isolate.Handle hIsolate, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_BeginCpuProfile(hIsolate, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Isolate_EndCpuProfile(V8Isolate.Handle hIsolate, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Isolate_EndCpuProfile(hIsolate, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectCpuProfileSample(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CollectCpuProfileSample(hIsolate);
            }

            uint IV8SplitProxyNative.V8Isolate_GetCpuProfileSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetCpuProfileSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetCpuProfileSampleInterval(V8Isolate.Handle hIsolate, uint value)
            {
                V8Isolate_SetCpuProfileSampleInterval(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_WriteHeapSnapshot(V8Isolate.Handle hIsolate, IntPtr pStream)
            {
                V8Isolate_WriteHeapSnapshot(hIsolate, pStream);
            }

            #endregion

            #region V8 context methods

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateHeapSize(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateHeapSize(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateHeapSize(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateHeapSize(hContext, size);
            }

            double IV8SplitProxyNative.V8Context_GetIsolateHeapSizeSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetIsolateHeapSizeSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetIsolateHeapSizeSampleInterval(V8Context.Handle hContext, double milliseconds)
            {
                V8Context_SetIsolateHeapSizeSampleInterval(hContext, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateStackUsage(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateStackUsage(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateStackUsage(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateStackUsage(hContext, size);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLock(V8Context.Handle hContext, IntPtr pAction)
            {
                V8Context_InvokeWithLock(hContext, pAction);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLockWithArg(V8Context.Handle hContext, IntPtr pAction, IntPtr pArg)
            {
                V8Context_InvokeWithLockWithArg(hContext, pAction, pArg);
            }

            object IV8SplitProxyNative.V8Context_GetRootItem(V8Context.Handle hContext)
            {
                using (var itemScope = V8Value.CreateScope())
                {
                    V8Context_GetRootItem(hContext, itemScope.Value);
                    return V8Value.Get(itemScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_AddGlobalItem(V8Context.Handle hContext, string name, object value, bool globalMembers)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Context_AddGlobalItem(hContext, nameScope.Value, valueScope.Value, globalMembers);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_AwaitDebuggerAndPause(V8Context.Handle hContext)
            {
                V8Context_AwaitDebuggerAndPause(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelAwaitDebugger(V8Context.Handle hContext)
            {
                V8Context_CancelAwaitDebugger(hContext);
            }

            object IV8SplitProxyNative.V8Context_ExecuteCode(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var resultScope = V8Value.CreateScope())
                            {
                                V8Context_ExecuteCode(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, evaluate, resultScope.Value);
                                return V8Value.Get(resultScope.Value);
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Context_ExecuteScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest, evaluate, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_Compile(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Context_Compile(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        return V8Context_CompileScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest);
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileProducingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Context_CompileProducingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileConsumingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Context_CompileConsumingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileUpdatingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Context_CompileUpdatingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }
                
                                return hScript;
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScript(V8Context.Handle hContext, V8Script.Handle hScript, bool evaluate)
            {
                using (var resultScope = V8Value.CreateScope())
                {
                    V8Context_ExecuteScript(hContext, hScript, evaluate, resultScope.Value);
                    return V8Value.Get(resultScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_Interrupt(V8Context.Handle hContext)
            {
                V8Context_Interrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelInterrupt(V8Context.Handle hContext)
            {
                V8Context_CancelInterrupt(hContext);
            }

            bool IV8SplitProxyNative.V8Context_GetEnableIsolateInterruptPropagation(V8Context.Handle hContext)
            {
                return V8Context_GetEnableIsolateInterruptPropagation(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetEnableIsolateInterruptPropagation(V8Context.Handle hContext, bool value)
            {
                V8Context_SetEnableIsolateInterruptPropagation(hContext, value);
            }

            bool IV8SplitProxyNative.V8Context_GetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext)
            {
                return V8Context_GetDisableIsolateHeapSizeViolationInterrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext, bool value)
            {
                V8Context_SetDisableIsolateHeapSizeViolationInterrupt(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateHeapStatistics(V8Context.Handle hContext, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Context_GetIsolateHeapStatistics(hContext, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Context_GetIsolateStatistics(hContext, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_GetStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong moduleCount, out ulong moduleCacheSize)
            {
                V8Context_GetStatistics(hContext, out scriptCount, out moduleCount, out moduleCacheSize);
            }

            void IV8SplitProxyNative.V8Context_CollectGarbage(V8Context.Handle hContext, bool exhaustive)
            {
                V8Context_CollectGarbage(hContext, exhaustive);
            }

            void IV8SplitProxyNative.V8Context_OnAccessSettingsChanged(V8Context.Handle hContext)
            {
                V8Context_OnAccessSettingsChanged(hContext);
            }

            bool IV8SplitProxyNative.V8Context_BeginCpuProfile(V8Context.Handle hContext, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Context_BeginCpuProfile(hContext, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Context_EndCpuProfile(V8Context.Handle hContext, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Context_EndCpuProfile(hContext, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Context_CollectCpuProfileSample(V8Context.Handle hContext)
            {
                V8Context_CollectCpuProfileSample(hContext);
            }

            uint IV8SplitProxyNative.V8Context_GetCpuProfileSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetCpuProfileSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetCpuProfileSampleInterval(V8Context.Handle hContext, uint value)
            {
                V8Context_SetCpuProfileSampleInterval(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_WriteIsolateHeapSnapshot(V8Context.Handle hContext, IntPtr pStream)
            {
                V8Context_WriteIsolateHeapSnapshot(hContext, pStream);
            }

            #endregion

            #region V8 object methods

            object IV8SplitProxyNative.V8Object_GetNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        V8Object_GetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                        return V8Value.Get(valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_TryGetNamedProperty(V8Object.Handle hObject, string name, out object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        if (V8Object_TryGetNamedProperty(hObject, nameScope.Value, valueScope.Value))
                        {
                            value = V8Value.Get(valueScope.Value);
                            return true;
                        }

                        value = null;
                        return false;
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_SetNamedProperty(V8Object.Handle hObject, string name, object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Object_SetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Object_DeleteNamedProperty(hObject, nameScope.Value);
                }
            }

            string[] IV8SplitProxyNative.V8Object_GetPropertyNames(V8Object.Handle hObject, bool includeIndices)
            {
                using (var namesScope = StdStringArray.CreateScope())
                {
                    V8Object_GetPropertyNames(hObject, includeIndices, namesScope.Value);
                    return StdStringArray.ToArray(namesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_GetIndexedProperty(V8Object.Handle hObject, int index)
            {
                using (var valueScope = V8Value.CreateScope())
                {
                    V8Object_GetIndexedProperty(hObject, index, valueScope.Value);
                    return V8Value.Get(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_SetIndexedProperty(V8Object.Handle hObject, int index, object value)
            {
                using (var valueScope = V8Value.CreateScope(value))
                {
                    V8Object_SetIndexedProperty(hObject, index, valueScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteIndexedProperty(V8Object.Handle hObject, int index)
            {
                return V8Object_DeleteIndexedProperty(hObject, index);
            }

            int[] IV8SplitProxyNative.V8Object_GetPropertyIndices(V8Object.Handle hObject)
            {
                using (var indicesScope = StdInt32Array.CreateScope())
                {
                    V8Object_GetPropertyIndices(hObject, indicesScope.Value);
                    return StdInt32Array.ToArray(indicesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_Invoke(V8Object.Handle hObject, bool asConstructor, object[] args)
            {
                using (var argsScope = StdV8ValueArray.CreateScope(args))
                {
                    using (var resultScope = V8Value.CreateScope())
                    {
                        V8Object_Invoke(hObject, asConstructor, argsScope.Value, resultScope.Value);
                        return V8Value.Get(resultScope.Value);
                    }
                }
            }

            object IV8SplitProxyNative.V8Object_InvokeMethod(V8Object.Handle hObject, string name, object[] args)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var argsScope = StdV8ValueArray.CreateScope(args))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Object_InvokeMethod(hObject, nameScope.Value, argsScope.Value, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_GetArrayBufferOrViewInfo(V8Object.Handle hObject, out IV8Object arrayBuffer, out ulong offset, out ulong size, out ulong length)
            {
                using (var arrayBufferScope = V8Value.CreateScope())
                {
                    V8Object_GetArrayBufferOrViewInfo(hObject, arrayBufferScope.Value, out offset, out size, out length);
                    arrayBuffer = (IV8Object)V8Value.Get(arrayBufferScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewData(V8Object.Handle hObject, IntPtr pAction)
            {
                V8Object_InvokeWithArrayBufferOrViewData(hObject, pAction);
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewDataWithArg(V8Object.Handle hObject, IntPtr pAction, IntPtr pArg)
            {
                V8Object_InvokeWithArrayBufferOrViewDataWithArg(hObject, pAction, pArg);
            }

            #endregion

            #region V8 debug callback methods

            void IV8SplitProxyNative.V8DebugCallback_ConnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_ConnectClient(hCallback);
            }

            void IV8SplitProxyNative.V8DebugCallback_SendCommand(V8DebugCallback.Handle hCallback, string command)
            {
                using (var commandScope = StdString.CreateScope(command))
                {
                    V8DebugCallback_SendCommand(hCallback, commandScope.Value);
                }
            }

            void IV8SplitProxyNative.V8DebugCallback_DisconnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_DisconnectClient(hCallback);
            }

            #endregion

            #region native callback methods

            void IV8SplitProxyNative.NativeCallback_Invoke(NativeCallback.Handle hCallback)
            {
                NativeCallback_Invoke(hCallback);
            }

            #endregion

            #region V8 entity methods

            void IV8SplitProxyNative.V8Entity_Release(V8Entity.Handle hEntity)
            {
                V8Entity_Release(hEntity);
            }

            V8Entity.Handle IV8SplitProxyNative.V8Entity_CloneHandle(V8Entity.Handle hEntity)
            {
                return V8Entity_CloneHandle(hEntity);
            }

            void IV8SplitProxyNative.V8Entity_DestroyHandle(V8Entity.Handle hEntity)
            {
                V8Entity_DestroyHandle(hEntity);
            }

            #endregion

            #region error handling

            void IV8SplitProxyNative.HostException_Schedule(string message, object exception)
            {
                using (var messageScope = StdString.CreateScope(message))
                {
                    using (var exceptionScope = V8Value.CreateScope(exception))
                    {
                        HostException_Schedule(messageScope.Value, exceptionScope.Value);
                    }
                }
            }

            #endregion

            #region unit test support

            UIntPtr IV8SplitProxyNative.V8UnitTestSupport_GetTextDigest(string value)
            {
                using (var valueScope = StdString.CreateScope(value))
                {
                    return V8UnitTestSupport_GetTextDigest(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8UnitTestSupport_GetStatistics(out ulong isolateCount, out ulong contextCount)
            {
                V8UnitTestSupport_GetStatistics(out isolateCount, out contextCount);
            }

            #endregion

            #endregion

            #region native methods

            #region initialization

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyManaged_SetMethodTable(
                [In] IntPtr pMethodTable
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyNative_GetVersion();

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Environment_InitializeICU(
                [In] IntPtr pICUData,
                [In] uint size
            );

            #endregion

            #region memory methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_Allocate(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_AllocateZeroed(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void Memory_Free(
                [In] IntPtr pMemory
            );

            #endregion

            #region StdString methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdString.Ptr StdString_New(
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdString_GetValue(
                [In] StdString.Ptr pString,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_SetValue(
                [In] StdString.Ptr pString,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_Delete(
                [In] StdString.Ptr pString
            );

            #endregion

            #region StdStringArray methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdStringArray.Ptr StdStringArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdStringArray_GetElementCount(
                [In] StdStringArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElementCount(
                [In] StdStringArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdStringArray_GetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_Delete(
                [In] StdStringArray.Ptr pArray
            );

            #endregion

            #region StdByteArray methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdByteArray.Ptr StdByteArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdByteArray_GetElementCount(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_SetElementCount(
                [In] StdByteArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdByteArray_GetData(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_Delete(
                [In] StdByteArray.Ptr pArray
            );

            #endregion

            #region StdInt32Array methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdInt32Array.Ptr StdInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdInt32Array_GetElementCount(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_SetElementCount(
                [In] StdInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdInt32Array_GetData(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_Delete(
                [In] StdInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt32Array methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt32Array.Ptr StdUInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt32Array_GetElementCount(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_SetElementCount(
                [In] StdUInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt32Array_GetData(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_Delete(
                [In] StdUInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt64Array methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt64Array.Ptr StdUInt64Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt64Array_GetElementCount(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_SetElementCount(
                [In] StdUInt64Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt64Array_GetData(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_Delete(
                [In] StdUInt64Array.Ptr pArray
            );

            #endregion

            #region StdPtrArray methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdPtrArray.Ptr StdPtrArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdPtrArray_GetElementCount(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_SetElementCount(
                [In] StdPtrArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdPtrArray_GetData(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_Delete(
                [In] StdPtrArray.Ptr pArray
            );

            #endregion

            #region StdV8ValueArray methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern StdV8ValueArray.Ptr StdV8ValueArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdV8ValueArray_GetElementCount(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_SetElementCount(
                [In] StdV8ValueArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr StdV8ValueArray_GetData(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_Delete(
                [In] StdV8ValueArray.Ptr pArray
            );

            #endregion

            #region V8Value methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr V8Value_New();

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNonexistent(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetUndefined(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNull(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBoolean(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNumber(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetString(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetDateTime(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBigInt(
                [In] V8Value.Ptr pV8Value,
                [In] int signBit,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] bytes,
                [In] int length
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetV8Object(
                [In] V8Value.Ptr pV8Value,
                [In] V8Object.Handle hObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetHostObject(
                [In] V8Value.Ptr pV8Value,
                [In] IntPtr pObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Decode(
                [In] V8Value.Ptr pV8Value,
                [Out] out V8Value.Decoded decoded
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Delete(
                [In] V8Value.Ptr pV8Value
            );

            #endregion

            #region V8CpuProfile methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfile_GetInfo(
                [In] V8CpuProfile.Ptr pProfile,
                [In] V8Entity.Handle hEntity,
                [In] StdString.Ptr pName,
                [Out] out ulong startTimestamp,
                [Out] out ulong endTimestamp,
                [Out] out int sampleCount,
                [Out] out V8CpuProfile.Node.Ptr pRootNode
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfile_GetSample(
                [In] V8CpuProfile.Ptr pProfile,
                [In] int index,
                [Out] out ulong nodeId,
                [Out] out ulong timestamp
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfileNode_GetInfo(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] V8Entity.Handle hEntity,
                [Out] out ulong nodeId,
                [Out] out long scriptId,
                [In] StdString.Ptr pScriptName,
                [In] StdString.Ptr pFunctionName,
                [In] StdString.Ptr pBailoutReason,
                [Out] out long lineNumber,
                [Out] out long columnNumber,
                [Out] out ulong hitCount,
                [Out] out uint hitLineCount,
                [Out] out int childCount
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfileNode_GetHitLines(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] StdInt32Array.Ptr pLineNumbers,
                [In] StdUInt32Array.Ptr pHitCounts
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8CpuProfile.Node.Ptr V8CpuProfileNode_GetChildNode(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] int index
            );

            #endregion

            #region V8 isolate methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Isolate.Handle V8Isolate_Create(
                [In] StdString.Ptr pName,
                [In] int maxNewSpaceSize,
                [In] int maxOldSpaceSize,
                [In] double heapExpansionMultiplier,
                [In] ulong maxArrayBufferAllocation,
                [In] V8RuntimeFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Context.Handle V8Isolate_CreateContext(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] V8ScriptEngineFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Isolate_GetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_AwaitDebuggerAndPause(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CancelAwaitDebugger(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_Compile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileProducingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileConsumingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileUpdatingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetHeapStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectGarbage(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_BeginCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_EndCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectCpuProfileSample(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Isolate_GetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] uint value
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_WriteHeapSnapshot(
                [In] V8Isolate.Handle hIsolate,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 context methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Context_GetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLock(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLockWithArg(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetRootItem(
                [In] V8Context.Handle hContext,
                [In] V8Value.Ptr pItem
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AddGlobalItem(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue,
                [In] [MarshalAs(UnmanagedType.I1)] bool globalMembers
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AwaitDebuggerAndPause(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelAwaitDebugger(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteCode(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_Compile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileProducingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileConsumingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileUpdatingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScript(
                [In] V8Context.Handle hContext,
                [In] V8Script.Handle hScript,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_Interrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateHeapStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong moduleCount,
                [Out] out ulong moduleCacheSize
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectGarbage(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_OnAccessSettingsChanged(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_BeginCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_EndCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectCpuProfileSample(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Context_GetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext,
                [In] uint value
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_WriteIsolateHeapSnapshot(
                [In] V8Context.Handle hContext,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 object methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_TryGetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyNames(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool includeIndices,
                [In] StdStringArray.Ptr pNames
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyIndices(
                [In] V8Object.Handle hObject,
                [In] StdInt32Array.Ptr pIndices
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_Invoke(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool asConstructor,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeMethod(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetArrayBufferOrViewInfo(
                [In] V8Object.Handle hObject,
                [In] V8Value.Ptr pArrayBuffer,
                [Out] out ulong offset,
                [Out] out ulong size,
                [Out] out ulong length
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewData(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewDataWithArg(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            #endregion

            #region V8 debug callback methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_ConnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_SendCommand(
                [In] V8DebugCallback.Handle hCallback,
                [In] StdString.Ptr pCommand
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_DisconnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            #endregion

            #region native callback methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void NativeCallback_Invoke(
                [In] NativeCallback.Handle hCallback
            );

            #endregion

            #region V8 entity methods

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_Release(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Entity.Handle V8Entity_CloneHandle(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_DestroyHandle(
                [In] V8Entity.Handle hEntity
            );

            #endregion

            #region error handling

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void HostException_Schedule(
                [In] StdString.Ptr pMessage,
                [In] V8Value.Ptr pException
            );

            #endregion

            #region unit test support

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8UnitTestSupport_GetTextDigest(
                [In] StdString.Ptr pString
            );

            [DllImport("ClearScriptV8.linux-arm.so", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8UnitTestSupport_GetStatistics(
                [Out] out ulong isolateCount,
                [Out] out ulong contextCount
            );

            #endregion

            #endregion
        }

        #endregion

        

        #region Nested type: Impl_OSX_X64

        private sealed class Impl_OSX_X64 : IV8SplitProxyNative
        {
            public static readonly IV8SplitProxyNative Instance = new Impl_OSX_X64();

            #region IV8SplitProxyNative implementation

            #region initialization

            IntPtr IV8SplitProxyNative.V8SplitProxyManaged_SetMethodTable(IntPtr pMethodTable)
            {
                return V8SplitProxyManaged_SetMethodTable(pMethodTable);
            }

            string IV8SplitProxyNative.V8SplitProxyNative_GetVersion()
            {
                return Marshal.PtrToStringUni(V8SplitProxyNative_GetVersion());
            }

            void IV8SplitProxyNative.V8Environment_InitializeICU(IntPtr pICUData, uint size)
            {
                V8Environment_InitializeICU(pICUData, size);
            }

            #endregion

            #region memory methods

            IntPtr IV8SplitProxyNative.Memory_Allocate(UIntPtr size)
            {
                return Memory_Allocate(size);
            }

            IntPtr IV8SplitProxyNative.Memory_AllocateZeroed(UIntPtr size)
            {
                return Memory_AllocateZeroed(size);
            }

            void IV8SplitProxyNative.Memory_Free(IntPtr pMemory)
            {
                Memory_Free(pMemory);
            }

            #endregion

            #region StdString methods

            StdString.Ptr IV8SplitProxyNative.StdString_New(string value)
            {
                return StdString_New(value, value.Length);
            }

            string IV8SplitProxyNative.StdString_GetValue(StdString.Ptr pString)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue>(StdString.Ptr pString, Func<IntPtr, int, TValue> factory)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue, TArg>(StdString.Ptr pString, Func<IntPtr, int, TArg, TValue> factory, in TArg arg)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length, arg);
            }

            void IV8SplitProxyNative.StdString_SetValue(StdString.Ptr pString, string value)
            {
                StdString_SetValue(pString, value, value.Length);
            }

            void IV8SplitProxyNative.StdString_Delete(StdString.Ptr pString)
            {
                StdString_Delete(pString);
            }

            #endregion

            #region StdStringArray methods

            StdStringArray.Ptr IV8SplitProxyNative.StdStringArray_New(int elementCount)
            {
                return StdStringArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdStringArray_GetElementCount(StdStringArray.Ptr pArray)
            {
                return StdStringArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdStringArray_SetElementCount(StdStringArray.Ptr pArray, int elementCount)
            {
                StdStringArray_SetElementCount(pArray, elementCount);
            }

            string IV8SplitProxyNative.StdStringArray_GetElement(StdStringArray.Ptr pArray, int index)
            {
                var pValue = StdStringArray_GetElement(pArray, index, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            void IV8SplitProxyNative.StdStringArray_SetElement(StdStringArray.Ptr pArray, int index, string value)
            {
                StdStringArray_SetElement(pArray, index, value, value.Length);
            }

            void IV8SplitProxyNative.StdStringArray_Delete(StdStringArray.Ptr pArray)
            {
                StdStringArray_Delete(pArray);
            }

            #endregion

            #region StdByteArray methods

            StdByteArray.Ptr IV8SplitProxyNative.StdByteArray_New(int elementCount)
            {
                return StdByteArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdByteArray_GetElementCount(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_SetElementCount(StdByteArray.Ptr pArray, int elementCount)
            {
                StdByteArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdByteArray_GetData(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_Delete(StdByteArray.Ptr pArray)
            {
                StdByteArray_Delete(pArray);
            }

            #endregion

            #region StdInt32Array methods

            StdInt32Array.Ptr IV8SplitProxyNative.StdInt32Array_New(int elementCount)
            {
                return StdInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdInt32Array_GetElementCount(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_SetElementCount(StdInt32Array.Ptr pArray, int elementCount)
            {
                StdInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdInt32Array_GetData(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_Delete(StdInt32Array.Ptr pArray)
            {
                StdInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt32Array methods

            StdUInt32Array.Ptr IV8SplitProxyNative.StdUInt32Array_New(int elementCount)
            {
                return StdUInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt32Array_GetElementCount(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_SetElementCount(StdUInt32Array.Ptr pArray, int elementCount)
            {
                StdUInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt32Array_GetData(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_Delete(StdUInt32Array.Ptr pArray)
            {
                StdUInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt64Array methods

            StdUInt64Array.Ptr IV8SplitProxyNative.StdUInt64Array_New(int elementCount)
            {
                return StdUInt64Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt64Array_GetElementCount(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_SetElementCount(StdUInt64Array.Ptr pArray, int elementCount)
            {
                StdUInt64Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt64Array_GetData(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_Delete(StdUInt64Array.Ptr pArray)
            {
                StdUInt64Array_Delete(pArray);
            }

            #endregion

            #region StdPtrArray methods

            StdPtrArray.Ptr IV8SplitProxyNative.StdPtrArray_New(int elementCount)
            {
                return StdPtrArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdPtrArray_GetElementCount(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_SetElementCount(StdPtrArray.Ptr pArray, int elementCount)
            {
                StdPtrArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdPtrArray_GetData(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_Delete(StdPtrArray.Ptr pArray)
            {
                StdPtrArray_Delete(pArray);
            }

            #endregion

            #region StdV8ValueArray methods

            StdV8ValueArray.Ptr IV8SplitProxyNative.StdV8ValueArray_New(int elementCount)
            {
                return StdV8ValueArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdV8ValueArray_GetElementCount(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_SetElementCount(StdV8ValueArray.Ptr pArray, int elementCount)
            {
                StdV8ValueArray_SetElementCount(pArray, elementCount);
            }

            V8Value.Ptr IV8SplitProxyNative.StdV8ValueArray_GetData(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_Delete(StdV8ValueArray.Ptr pArray)
            {
                StdV8ValueArray_Delete(pArray);
            }

            #endregion

            #region V8Value methods

            V8Value.Ptr IV8SplitProxyNative.V8Value_New()
            {
                return V8Value_New();
            }

            void IV8SplitProxyNative.V8Value_SetNonexistent(V8Value.Ptr pV8Value)
            {
                V8Value_SetNonexistent(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetUndefined(V8Value.Ptr pV8Value)
            {
                V8Value_SetUndefined(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetNull(V8Value.Ptr pV8Value)
            {
                V8Value_SetNull(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetBoolean(V8Value.Ptr pV8Value, bool value)
            {
                V8Value_SetBoolean(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetNumber(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetNumber(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetString(V8Value.Ptr pV8Value, string value)
            {
                V8Value_SetString(pV8Value, value, value.Length);
            }

            void IV8SplitProxyNative.V8Value_SetDateTime(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetDateTime(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetBigInt(V8Value.Ptr pV8Value, int signBit, byte[] bytes)
            {
                V8Value_SetBigInt(pV8Value, signBit, bytes, bytes.Length);
            }

            void IV8SplitProxyNative.V8Value_SetV8Object(V8Value.Ptr pV8Value, V8Object.Handle hObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetV8Object(pV8Value, hObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_SetHostObject(V8Value.Ptr pV8Value, IntPtr pObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetHostObject(pV8Value, pObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_Decode(V8Value.Ptr pV8Value, out V8Value.Decoded decoded)
            {
                V8Value_Decode(pV8Value, out decoded);
            }

            void IV8SplitProxyNative.V8Value_Delete(V8Value.Ptr pV8Value)
            {
                V8Value_Delete(pV8Value);
            }

            #endregion

            #region V8CpuProfile methods

            void IV8SplitProxyNative.V8CpuProfile_GetInfo(V8CpuProfile.Ptr pProfile, V8Entity.Handle hEntity, out string name, out ulong startTimestamp, out ulong endTimestamp, out int sampleCount, out V8CpuProfile.Node.Ptr pRootNode)
            {
                using (var nameScope = StdString.CreateScope())
                {
                    V8CpuProfile_GetInfo(pProfile, hEntity, nameScope.Value, out startTimestamp, out endTimestamp, out sampleCount, out pRootNode);
                    name = StdString.GetValue(nameScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8CpuProfile_GetSample(V8CpuProfile.Ptr pProfile, int index, out ulong nodeId, out ulong timestamp)
            {
                return V8CpuProfile_GetSample(pProfile, index, out nodeId, out timestamp);
            }

            void IV8SplitProxyNative.V8CpuProfileNode_GetInfo(V8CpuProfile.Node.Ptr pNode, V8Entity.Handle hEntity, out ulong nodeId, out long scriptId, out string scriptName, out string functionName, out string bailoutReason, out long lineNumber, out long columnNumber, out ulong hitCount, out uint hitLineCount, out int childCount)
            {
                using (var scriptNameScope = StdString.CreateScope())
                {
                    using (var functionNameScope = StdString.CreateScope())
                    {
                        using (var bailoutReasonScope = StdString.CreateScope())
                        {
                            V8CpuProfileNode_GetInfo(pNode, hEntity, out nodeId, out scriptId, scriptNameScope.Value, functionNameScope.Value, bailoutReasonScope.Value, out lineNumber, out columnNumber, out hitCount, out hitLineCount, out childCount);
                            scriptName = StdString.GetValue(scriptNameScope.Value);
                            functionName = StdString.GetValue(functionNameScope.Value);
                            bailoutReason = StdString.GetValue(bailoutReasonScope.Value);

                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8CpuProfileNode_GetHitLines(V8CpuProfile.Node.Ptr pNode, out int[] lineNumbers, out uint[] hitCounts)
            {
                using (var lineNumbersScope = StdInt32Array.CreateScope())
                {
                    using (var hitCountsScope = StdUInt32Array.CreateScope())
                    {
                        var result = V8CpuProfileNode_GetHitLines(pNode, lineNumbersScope.Value, hitCountsScope.Value);
                        lineNumbers = StdInt32Array.ToArray(lineNumbersScope.Value);
                        hitCounts = StdUInt32Array.ToArray(hitCountsScope.Value);
                        return result;
                    }
                }
            }

            V8CpuProfile.Node.Ptr IV8SplitProxyNative.V8CpuProfileNode_GetChildNode(V8CpuProfile.Node.Ptr pNode, int index)
            {
                return V8CpuProfileNode_GetChildNode(pNode, index);
            }

            #endregion

            #region V8 isolate methods

            V8Isolate.Handle IV8SplitProxyNative.V8Isolate_Create(string name, int maxNewSpaceSize, int maxOldSpaceSize, double heapExpansionMultiplier, ulong maxArrayBufferAllocation, V8RuntimeFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_Create(nameScope.Value, maxNewSpaceSize, maxOldSpaceSize, heapExpansionMultiplier, maxArrayBufferAllocation, flags, debugPort);
                }
            }

            V8Context.Handle IV8SplitProxyNative.V8Isolate_CreateContext(V8Isolate.Handle hIsolate, string name, V8ScriptEngineFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_CreateContext(hIsolate, nameScope.Value, flags, debugPort);
                }
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxHeapSize(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxHeapSize(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxHeapSize(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxHeapSize(hIsolate, size);
            }

            double IV8SplitProxyNative.V8Isolate_GetHeapSizeSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetHeapSizeSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetHeapSizeSampleInterval(V8Isolate.Handle hIsolate, double milliseconds)
            {
                V8Isolate_SetHeapSizeSampleInterval(hIsolate, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxStackUsage(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxStackUsage(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxStackUsage(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxStackUsage(hIsolate, size);
            }

            void IV8SplitProxyNative.V8Isolate_AwaitDebuggerAndPause(V8Isolate.Handle hIsolate)
            {
                V8Isolate_AwaitDebuggerAndPause(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_CancelAwaitDebugger(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CancelAwaitDebugger(hIsolate);
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_Compile(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Isolate_Compile(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileProducingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Isolate_CompileProducingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileConsumingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Isolate_CompileConsumingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileUpdatingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Isolate_CompileUpdatingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }

                                return hScript;
                            }
                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8Isolate_GetEnableInterruptPropagation(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetEnableInterruptPropagation(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetEnableInterruptPropagation(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetEnableInterruptPropagation(hIsolate, value);
            }

            bool IV8SplitProxyNative.V8Isolate_GetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetDisableHeapSizeViolationInterrupt(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetDisableHeapSizeViolationInterrupt(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_GetHeapStatistics(V8Isolate.Handle hIsolate, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Isolate_GetHeapStatistics(hIsolate, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Isolate_GetStatistics(V8Isolate.Handle hIsolate, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Isolate_GetStatistics(hIsolate, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectGarbage(V8Isolate.Handle hIsolate, bool exhaustive)
            {
                V8Isolate_CollectGarbage(hIsolate, exhaustive);
            }

            bool IV8SplitProxyNative.V8Isolate_BeginCpuProfile(V8Isolate.Handle hIsolate, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_BeginCpuProfile(hIsolate, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Isolate_EndCpuProfile(V8Isolate.Handle hIsolate, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Isolate_EndCpuProfile(hIsolate, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectCpuProfileSample(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CollectCpuProfileSample(hIsolate);
            }

            uint IV8SplitProxyNative.V8Isolate_GetCpuProfileSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetCpuProfileSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetCpuProfileSampleInterval(V8Isolate.Handle hIsolate, uint value)
            {
                V8Isolate_SetCpuProfileSampleInterval(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_WriteHeapSnapshot(V8Isolate.Handle hIsolate, IntPtr pStream)
            {
                V8Isolate_WriteHeapSnapshot(hIsolate, pStream);
            }

            #endregion

            #region V8 context methods

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateHeapSize(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateHeapSize(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateHeapSize(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateHeapSize(hContext, size);
            }

            double IV8SplitProxyNative.V8Context_GetIsolateHeapSizeSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetIsolateHeapSizeSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetIsolateHeapSizeSampleInterval(V8Context.Handle hContext, double milliseconds)
            {
                V8Context_SetIsolateHeapSizeSampleInterval(hContext, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateStackUsage(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateStackUsage(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateStackUsage(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateStackUsage(hContext, size);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLock(V8Context.Handle hContext, IntPtr pAction)
            {
                V8Context_InvokeWithLock(hContext, pAction);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLockWithArg(V8Context.Handle hContext, IntPtr pAction, IntPtr pArg)
            {
                V8Context_InvokeWithLockWithArg(hContext, pAction, pArg);
            }

            object IV8SplitProxyNative.V8Context_GetRootItem(V8Context.Handle hContext)
            {
                using (var itemScope = V8Value.CreateScope())
                {
                    V8Context_GetRootItem(hContext, itemScope.Value);
                    return V8Value.Get(itemScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_AddGlobalItem(V8Context.Handle hContext, string name, object value, bool globalMembers)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Context_AddGlobalItem(hContext, nameScope.Value, valueScope.Value, globalMembers);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_AwaitDebuggerAndPause(V8Context.Handle hContext)
            {
                V8Context_AwaitDebuggerAndPause(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelAwaitDebugger(V8Context.Handle hContext)
            {
                V8Context_CancelAwaitDebugger(hContext);
            }

            object IV8SplitProxyNative.V8Context_ExecuteCode(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var resultScope = V8Value.CreateScope())
                            {
                                V8Context_ExecuteCode(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, evaluate, resultScope.Value);
                                return V8Value.Get(resultScope.Value);
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Context_ExecuteScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest, evaluate, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_Compile(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Context_Compile(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        return V8Context_CompileScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest);
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileProducingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Context_CompileProducingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileConsumingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Context_CompileConsumingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileUpdatingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Context_CompileUpdatingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }
                
                                return hScript;
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScript(V8Context.Handle hContext, V8Script.Handle hScript, bool evaluate)
            {
                using (var resultScope = V8Value.CreateScope())
                {
                    V8Context_ExecuteScript(hContext, hScript, evaluate, resultScope.Value);
                    return V8Value.Get(resultScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_Interrupt(V8Context.Handle hContext)
            {
                V8Context_Interrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelInterrupt(V8Context.Handle hContext)
            {
                V8Context_CancelInterrupt(hContext);
            }

            bool IV8SplitProxyNative.V8Context_GetEnableIsolateInterruptPropagation(V8Context.Handle hContext)
            {
                return V8Context_GetEnableIsolateInterruptPropagation(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetEnableIsolateInterruptPropagation(V8Context.Handle hContext, bool value)
            {
                V8Context_SetEnableIsolateInterruptPropagation(hContext, value);
            }

            bool IV8SplitProxyNative.V8Context_GetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext)
            {
                return V8Context_GetDisableIsolateHeapSizeViolationInterrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext, bool value)
            {
                V8Context_SetDisableIsolateHeapSizeViolationInterrupt(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateHeapStatistics(V8Context.Handle hContext, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Context_GetIsolateHeapStatistics(hContext, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Context_GetIsolateStatistics(hContext, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_GetStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong moduleCount, out ulong moduleCacheSize)
            {
                V8Context_GetStatistics(hContext, out scriptCount, out moduleCount, out moduleCacheSize);
            }

            void IV8SplitProxyNative.V8Context_CollectGarbage(V8Context.Handle hContext, bool exhaustive)
            {
                V8Context_CollectGarbage(hContext, exhaustive);
            }

            void IV8SplitProxyNative.V8Context_OnAccessSettingsChanged(V8Context.Handle hContext)
            {
                V8Context_OnAccessSettingsChanged(hContext);
            }

            bool IV8SplitProxyNative.V8Context_BeginCpuProfile(V8Context.Handle hContext, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Context_BeginCpuProfile(hContext, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Context_EndCpuProfile(V8Context.Handle hContext, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Context_EndCpuProfile(hContext, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Context_CollectCpuProfileSample(V8Context.Handle hContext)
            {
                V8Context_CollectCpuProfileSample(hContext);
            }

            uint IV8SplitProxyNative.V8Context_GetCpuProfileSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetCpuProfileSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetCpuProfileSampleInterval(V8Context.Handle hContext, uint value)
            {
                V8Context_SetCpuProfileSampleInterval(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_WriteIsolateHeapSnapshot(V8Context.Handle hContext, IntPtr pStream)
            {
                V8Context_WriteIsolateHeapSnapshot(hContext, pStream);
            }

            #endregion

            #region V8 object methods

            object IV8SplitProxyNative.V8Object_GetNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        V8Object_GetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                        return V8Value.Get(valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_TryGetNamedProperty(V8Object.Handle hObject, string name, out object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        if (V8Object_TryGetNamedProperty(hObject, nameScope.Value, valueScope.Value))
                        {
                            value = V8Value.Get(valueScope.Value);
                            return true;
                        }

                        value = null;
                        return false;
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_SetNamedProperty(V8Object.Handle hObject, string name, object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Object_SetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Object_DeleteNamedProperty(hObject, nameScope.Value);
                }
            }

            string[] IV8SplitProxyNative.V8Object_GetPropertyNames(V8Object.Handle hObject, bool includeIndices)
            {
                using (var namesScope = StdStringArray.CreateScope())
                {
                    V8Object_GetPropertyNames(hObject, includeIndices, namesScope.Value);
                    return StdStringArray.ToArray(namesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_GetIndexedProperty(V8Object.Handle hObject, int index)
            {
                using (var valueScope = V8Value.CreateScope())
                {
                    V8Object_GetIndexedProperty(hObject, index, valueScope.Value);
                    return V8Value.Get(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_SetIndexedProperty(V8Object.Handle hObject, int index, object value)
            {
                using (var valueScope = V8Value.CreateScope(value))
                {
                    V8Object_SetIndexedProperty(hObject, index, valueScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteIndexedProperty(V8Object.Handle hObject, int index)
            {
                return V8Object_DeleteIndexedProperty(hObject, index);
            }

            int[] IV8SplitProxyNative.V8Object_GetPropertyIndices(V8Object.Handle hObject)
            {
                using (var indicesScope = StdInt32Array.CreateScope())
                {
                    V8Object_GetPropertyIndices(hObject, indicesScope.Value);
                    return StdInt32Array.ToArray(indicesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_Invoke(V8Object.Handle hObject, bool asConstructor, object[] args)
            {
                using (var argsScope = StdV8ValueArray.CreateScope(args))
                {
                    using (var resultScope = V8Value.CreateScope())
                    {
                        V8Object_Invoke(hObject, asConstructor, argsScope.Value, resultScope.Value);
                        return V8Value.Get(resultScope.Value);
                    }
                }
            }

            object IV8SplitProxyNative.V8Object_InvokeMethod(V8Object.Handle hObject, string name, object[] args)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var argsScope = StdV8ValueArray.CreateScope(args))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Object_InvokeMethod(hObject, nameScope.Value, argsScope.Value, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_GetArrayBufferOrViewInfo(V8Object.Handle hObject, out IV8Object arrayBuffer, out ulong offset, out ulong size, out ulong length)
            {
                using (var arrayBufferScope = V8Value.CreateScope())
                {
                    V8Object_GetArrayBufferOrViewInfo(hObject, arrayBufferScope.Value, out offset, out size, out length);
                    arrayBuffer = (IV8Object)V8Value.Get(arrayBufferScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewData(V8Object.Handle hObject, IntPtr pAction)
            {
                V8Object_InvokeWithArrayBufferOrViewData(hObject, pAction);
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewDataWithArg(V8Object.Handle hObject, IntPtr pAction, IntPtr pArg)
            {
                V8Object_InvokeWithArrayBufferOrViewDataWithArg(hObject, pAction, pArg);
            }

            #endregion

            #region V8 debug callback methods

            void IV8SplitProxyNative.V8DebugCallback_ConnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_ConnectClient(hCallback);
            }

            void IV8SplitProxyNative.V8DebugCallback_SendCommand(V8DebugCallback.Handle hCallback, string command)
            {
                using (var commandScope = StdString.CreateScope(command))
                {
                    V8DebugCallback_SendCommand(hCallback, commandScope.Value);
                }
            }

            void IV8SplitProxyNative.V8DebugCallback_DisconnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_DisconnectClient(hCallback);
            }

            #endregion

            #region native callback methods

            void IV8SplitProxyNative.NativeCallback_Invoke(NativeCallback.Handle hCallback)
            {
                NativeCallback_Invoke(hCallback);
            }

            #endregion

            #region V8 entity methods

            void IV8SplitProxyNative.V8Entity_Release(V8Entity.Handle hEntity)
            {
                V8Entity_Release(hEntity);
            }

            V8Entity.Handle IV8SplitProxyNative.V8Entity_CloneHandle(V8Entity.Handle hEntity)
            {
                return V8Entity_CloneHandle(hEntity);
            }

            void IV8SplitProxyNative.V8Entity_DestroyHandle(V8Entity.Handle hEntity)
            {
                V8Entity_DestroyHandle(hEntity);
            }

            #endregion

            #region error handling

            void IV8SplitProxyNative.HostException_Schedule(string message, object exception)
            {
                using (var messageScope = StdString.CreateScope(message))
                {
                    using (var exceptionScope = V8Value.CreateScope(exception))
                    {
                        HostException_Schedule(messageScope.Value, exceptionScope.Value);
                    }
                }
            }

            #endregion

            #region unit test support

            UIntPtr IV8SplitProxyNative.V8UnitTestSupport_GetTextDigest(string value)
            {
                using (var valueScope = StdString.CreateScope(value))
                {
                    return V8UnitTestSupport_GetTextDigest(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8UnitTestSupport_GetStatistics(out ulong isolateCount, out ulong contextCount)
            {
                V8UnitTestSupport_GetStatistics(out isolateCount, out contextCount);
            }

            #endregion

            #endregion

            #region native methods

            #region initialization

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyManaged_SetMethodTable(
                [In] IntPtr pMethodTable
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyNative_GetVersion();

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Environment_InitializeICU(
                [In] IntPtr pICUData,
                [In] uint size
            );

            #endregion

            #region memory methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_Allocate(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_AllocateZeroed(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void Memory_Free(
                [In] IntPtr pMemory
            );

            #endregion

            #region StdString methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdString.Ptr StdString_New(
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdString_GetValue(
                [In] StdString.Ptr pString,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_SetValue(
                [In] StdString.Ptr pString,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_Delete(
                [In] StdString.Ptr pString
            );

            #endregion

            #region StdStringArray methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdStringArray.Ptr StdStringArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdStringArray_GetElementCount(
                [In] StdStringArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElementCount(
                [In] StdStringArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdStringArray_GetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_Delete(
                [In] StdStringArray.Ptr pArray
            );

            #endregion

            #region StdByteArray methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdByteArray.Ptr StdByteArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdByteArray_GetElementCount(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_SetElementCount(
                [In] StdByteArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdByteArray_GetData(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_Delete(
                [In] StdByteArray.Ptr pArray
            );

            #endregion

            #region StdInt32Array methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdInt32Array.Ptr StdInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdInt32Array_GetElementCount(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_SetElementCount(
                [In] StdInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdInt32Array_GetData(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_Delete(
                [In] StdInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt32Array methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt32Array.Ptr StdUInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt32Array_GetElementCount(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_SetElementCount(
                [In] StdUInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt32Array_GetData(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_Delete(
                [In] StdUInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt64Array methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt64Array.Ptr StdUInt64Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt64Array_GetElementCount(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_SetElementCount(
                [In] StdUInt64Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt64Array_GetData(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_Delete(
                [In] StdUInt64Array.Ptr pArray
            );

            #endregion

            #region StdPtrArray methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdPtrArray.Ptr StdPtrArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdPtrArray_GetElementCount(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_SetElementCount(
                [In] StdPtrArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdPtrArray_GetData(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_Delete(
                [In] StdPtrArray.Ptr pArray
            );

            #endregion

            #region StdV8ValueArray methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdV8ValueArray.Ptr StdV8ValueArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdV8ValueArray_GetElementCount(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_SetElementCount(
                [In] StdV8ValueArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr StdV8ValueArray_GetData(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_Delete(
                [In] StdV8ValueArray.Ptr pArray
            );

            #endregion

            #region V8Value methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr V8Value_New();

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNonexistent(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetUndefined(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNull(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBoolean(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNumber(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetString(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetDateTime(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBigInt(
                [In] V8Value.Ptr pV8Value,
                [In] int signBit,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] bytes,
                [In] int length
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetV8Object(
                [In] V8Value.Ptr pV8Value,
                [In] V8Object.Handle hObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetHostObject(
                [In] V8Value.Ptr pV8Value,
                [In] IntPtr pObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Decode(
                [In] V8Value.Ptr pV8Value,
                [Out] out V8Value.Decoded decoded
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Delete(
                [In] V8Value.Ptr pV8Value
            );

            #endregion

            #region V8CpuProfile methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfile_GetInfo(
                [In] V8CpuProfile.Ptr pProfile,
                [In] V8Entity.Handle hEntity,
                [In] StdString.Ptr pName,
                [Out] out ulong startTimestamp,
                [Out] out ulong endTimestamp,
                [Out] out int sampleCount,
                [Out] out V8CpuProfile.Node.Ptr pRootNode
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfile_GetSample(
                [In] V8CpuProfile.Ptr pProfile,
                [In] int index,
                [Out] out ulong nodeId,
                [Out] out ulong timestamp
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfileNode_GetInfo(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] V8Entity.Handle hEntity,
                [Out] out ulong nodeId,
                [Out] out long scriptId,
                [In] StdString.Ptr pScriptName,
                [In] StdString.Ptr pFunctionName,
                [In] StdString.Ptr pBailoutReason,
                [Out] out long lineNumber,
                [Out] out long columnNumber,
                [Out] out ulong hitCount,
                [Out] out uint hitLineCount,
                [Out] out int childCount
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfileNode_GetHitLines(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] StdInt32Array.Ptr pLineNumbers,
                [In] StdUInt32Array.Ptr pHitCounts
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8CpuProfile.Node.Ptr V8CpuProfileNode_GetChildNode(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] int index
            );

            #endregion

            #region V8 isolate methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Isolate.Handle V8Isolate_Create(
                [In] StdString.Ptr pName,
                [In] int maxNewSpaceSize,
                [In] int maxOldSpaceSize,
                [In] double heapExpansionMultiplier,
                [In] ulong maxArrayBufferAllocation,
                [In] V8RuntimeFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Context.Handle V8Isolate_CreateContext(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] V8ScriptEngineFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Isolate_GetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_AwaitDebuggerAndPause(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CancelAwaitDebugger(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_Compile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileProducingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileConsumingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileUpdatingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetHeapStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectGarbage(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_BeginCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_EndCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectCpuProfileSample(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Isolate_GetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] uint value
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_WriteHeapSnapshot(
                [In] V8Isolate.Handle hIsolate,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 context methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Context_GetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLock(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLockWithArg(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetRootItem(
                [In] V8Context.Handle hContext,
                [In] V8Value.Ptr pItem
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AddGlobalItem(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue,
                [In] [MarshalAs(UnmanagedType.I1)] bool globalMembers
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AwaitDebuggerAndPause(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelAwaitDebugger(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteCode(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_Compile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileProducingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileConsumingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileUpdatingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScript(
                [In] V8Context.Handle hContext,
                [In] V8Script.Handle hScript,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_Interrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateHeapStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong moduleCount,
                [Out] out ulong moduleCacheSize
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectGarbage(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_OnAccessSettingsChanged(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_BeginCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_EndCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectCpuProfileSample(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Context_GetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext,
                [In] uint value
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_WriteIsolateHeapSnapshot(
                [In] V8Context.Handle hContext,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 object methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_TryGetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyNames(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool includeIndices,
                [In] StdStringArray.Ptr pNames
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyIndices(
                [In] V8Object.Handle hObject,
                [In] StdInt32Array.Ptr pIndices
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_Invoke(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool asConstructor,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeMethod(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetArrayBufferOrViewInfo(
                [In] V8Object.Handle hObject,
                [In] V8Value.Ptr pArrayBuffer,
                [Out] out ulong offset,
                [Out] out ulong size,
                [Out] out ulong length
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewData(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewDataWithArg(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            #endregion

            #region V8 debug callback methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_ConnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_SendCommand(
                [In] V8DebugCallback.Handle hCallback,
                [In] StdString.Ptr pCommand
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_DisconnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            #endregion

            #region native callback methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void NativeCallback_Invoke(
                [In] NativeCallback.Handle hCallback
            );

            #endregion

            #region V8 entity methods

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_Release(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Entity.Handle V8Entity_CloneHandle(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_DestroyHandle(
                [In] V8Entity.Handle hEntity
            );

            #endregion

            #region error handling

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void HostException_Schedule(
                [In] StdString.Ptr pMessage,
                [In] V8Value.Ptr pException
            );

            #endregion

            #region unit test support

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8UnitTestSupport_GetTextDigest(
                [In] StdString.Ptr pString
            );

            [DllImport("ClearScriptV8.osx-x64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8UnitTestSupport_GetStatistics(
                [Out] out ulong isolateCount,
                [Out] out ulong contextCount
            );

            #endregion

            #endregion
        }

        #endregion

        

        #region Nested type: Impl_OSX_Arm64

        private sealed class Impl_OSX_Arm64 : IV8SplitProxyNative
        {
            public static readonly IV8SplitProxyNative Instance = new Impl_OSX_Arm64();

            #region IV8SplitProxyNative implementation

            #region initialization

            IntPtr IV8SplitProxyNative.V8SplitProxyManaged_SetMethodTable(IntPtr pMethodTable)
            {
                return V8SplitProxyManaged_SetMethodTable(pMethodTable);
            }

            string IV8SplitProxyNative.V8SplitProxyNative_GetVersion()
            {
                return Marshal.PtrToStringUni(V8SplitProxyNative_GetVersion());
            }

            void IV8SplitProxyNative.V8Environment_InitializeICU(IntPtr pICUData, uint size)
            {
                V8Environment_InitializeICU(pICUData, size);
            }

            #endregion

            #region memory methods

            IntPtr IV8SplitProxyNative.Memory_Allocate(UIntPtr size)
            {
                return Memory_Allocate(size);
            }

            IntPtr IV8SplitProxyNative.Memory_AllocateZeroed(UIntPtr size)
            {
                return Memory_AllocateZeroed(size);
            }

            void IV8SplitProxyNative.Memory_Free(IntPtr pMemory)
            {
                Memory_Free(pMemory);
            }

            #endregion

            #region StdString methods

            StdString.Ptr IV8SplitProxyNative.StdString_New(string value)
            {
                return StdString_New(value, value.Length);
            }

            string IV8SplitProxyNative.StdString_GetValue(StdString.Ptr pString)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue>(StdString.Ptr pString, Func<IntPtr, int, TValue> factory)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length);
            }

            TValue IV8SplitProxyNative.StdString_GetValue<TValue, TArg>(StdString.Ptr pString, Func<IntPtr, int, TArg, TValue> factory, in TArg arg)
            {
                var pValue = StdString_GetValue(pString, out var length);
                return factory(pValue, length, arg);
            }

            void IV8SplitProxyNative.StdString_SetValue(StdString.Ptr pString, string value)
            {
                StdString_SetValue(pString, value, value.Length);
            }

            void IV8SplitProxyNative.StdString_Delete(StdString.Ptr pString)
            {
                StdString_Delete(pString);
            }

            #endregion

            #region StdStringArray methods

            StdStringArray.Ptr IV8SplitProxyNative.StdStringArray_New(int elementCount)
            {
                return StdStringArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdStringArray_GetElementCount(StdStringArray.Ptr pArray)
            {
                return StdStringArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdStringArray_SetElementCount(StdStringArray.Ptr pArray, int elementCount)
            {
                StdStringArray_SetElementCount(pArray, elementCount);
            }

            string IV8SplitProxyNative.StdStringArray_GetElement(StdStringArray.Ptr pArray, int index)
            {
                var pValue = StdStringArray_GetElement(pArray, index, out var length);
                return Marshal.PtrToStringUni(pValue, length);
            }

            void IV8SplitProxyNative.StdStringArray_SetElement(StdStringArray.Ptr pArray, int index, string value)
            {
                StdStringArray_SetElement(pArray, index, value, value.Length);
            }

            void IV8SplitProxyNative.StdStringArray_Delete(StdStringArray.Ptr pArray)
            {
                StdStringArray_Delete(pArray);
            }

            #endregion

            #region StdByteArray methods

            StdByteArray.Ptr IV8SplitProxyNative.StdByteArray_New(int elementCount)
            {
                return StdByteArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdByteArray_GetElementCount(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_SetElementCount(StdByteArray.Ptr pArray, int elementCount)
            {
                StdByteArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdByteArray_GetData(StdByteArray.Ptr pArray)
            {
                return StdByteArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdByteArray_Delete(StdByteArray.Ptr pArray)
            {
                StdByteArray_Delete(pArray);
            }

            #endregion

            #region StdInt32Array methods

            StdInt32Array.Ptr IV8SplitProxyNative.StdInt32Array_New(int elementCount)
            {
                return StdInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdInt32Array_GetElementCount(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_SetElementCount(StdInt32Array.Ptr pArray, int elementCount)
            {
                StdInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdInt32Array_GetData(StdInt32Array.Ptr pArray)
            {
                return StdInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdInt32Array_Delete(StdInt32Array.Ptr pArray)
            {
                StdInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt32Array methods

            StdUInt32Array.Ptr IV8SplitProxyNative.StdUInt32Array_New(int elementCount)
            {
                return StdUInt32Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt32Array_GetElementCount(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_SetElementCount(StdUInt32Array.Ptr pArray, int elementCount)
            {
                StdUInt32Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt32Array_GetData(StdUInt32Array.Ptr pArray)
            {
                return StdUInt32Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt32Array_Delete(StdUInt32Array.Ptr pArray)
            {
                StdUInt32Array_Delete(pArray);
            }

            #endregion

            #region StdUInt64Array methods

            StdUInt64Array.Ptr IV8SplitProxyNative.StdUInt64Array_New(int elementCount)
            {
                return StdUInt64Array_New(elementCount);
            }

            int IV8SplitProxyNative.StdUInt64Array_GetElementCount(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_SetElementCount(StdUInt64Array.Ptr pArray, int elementCount)
            {
                StdUInt64Array_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdUInt64Array_GetData(StdUInt64Array.Ptr pArray)
            {
                return StdUInt64Array_GetData(pArray);
            }

            void IV8SplitProxyNative.StdUInt64Array_Delete(StdUInt64Array.Ptr pArray)
            {
                StdUInt64Array_Delete(pArray);
            }

            #endregion

            #region StdPtrArray methods

            StdPtrArray.Ptr IV8SplitProxyNative.StdPtrArray_New(int elementCount)
            {
                return StdPtrArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdPtrArray_GetElementCount(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_SetElementCount(StdPtrArray.Ptr pArray, int elementCount)
            {
                StdPtrArray_SetElementCount(pArray, elementCount);
            }

            IntPtr IV8SplitProxyNative.StdPtrArray_GetData(StdPtrArray.Ptr pArray)
            {
                return StdPtrArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdPtrArray_Delete(StdPtrArray.Ptr pArray)
            {
                StdPtrArray_Delete(pArray);
            }

            #endregion

            #region StdV8ValueArray methods

            StdV8ValueArray.Ptr IV8SplitProxyNative.StdV8ValueArray_New(int elementCount)
            {
                return StdV8ValueArray_New(elementCount);
            }

            int IV8SplitProxyNative.StdV8ValueArray_GetElementCount(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetElementCount(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_SetElementCount(StdV8ValueArray.Ptr pArray, int elementCount)
            {
                StdV8ValueArray_SetElementCount(pArray, elementCount);
            }

            V8Value.Ptr IV8SplitProxyNative.StdV8ValueArray_GetData(StdV8ValueArray.Ptr pArray)
            {
                return StdV8ValueArray_GetData(pArray);
            }

            void IV8SplitProxyNative.StdV8ValueArray_Delete(StdV8ValueArray.Ptr pArray)
            {
                StdV8ValueArray_Delete(pArray);
            }

            #endregion

            #region V8Value methods

            V8Value.Ptr IV8SplitProxyNative.V8Value_New()
            {
                return V8Value_New();
            }

            void IV8SplitProxyNative.V8Value_SetNonexistent(V8Value.Ptr pV8Value)
            {
                V8Value_SetNonexistent(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetUndefined(V8Value.Ptr pV8Value)
            {
                V8Value_SetUndefined(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetNull(V8Value.Ptr pV8Value)
            {
                V8Value_SetNull(pV8Value);
            }

            void IV8SplitProxyNative.V8Value_SetBoolean(V8Value.Ptr pV8Value, bool value)
            {
                V8Value_SetBoolean(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetNumber(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetNumber(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetString(V8Value.Ptr pV8Value, string value)
            {
                V8Value_SetString(pV8Value, value, value.Length);
            }

            void IV8SplitProxyNative.V8Value_SetDateTime(V8Value.Ptr pV8Value, double value)
            {
                V8Value_SetDateTime(pV8Value, value);
            }

            void IV8SplitProxyNative.V8Value_SetBigInt(V8Value.Ptr pV8Value, int signBit, byte[] bytes)
            {
                V8Value_SetBigInt(pV8Value, signBit, bytes, bytes.Length);
            }

            void IV8SplitProxyNative.V8Value_SetV8Object(V8Value.Ptr pV8Value, V8Object.Handle hObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetV8Object(pV8Value, hObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_SetHostObject(V8Value.Ptr pV8Value, IntPtr pObject, V8Value.Subtype subtype, V8Value.Flags flags)
            {
                V8Value_SetHostObject(pV8Value, pObject, subtype, flags);
            }

            void IV8SplitProxyNative.V8Value_Decode(V8Value.Ptr pV8Value, out V8Value.Decoded decoded)
            {
                V8Value_Decode(pV8Value, out decoded);
            }

            void IV8SplitProxyNative.V8Value_Delete(V8Value.Ptr pV8Value)
            {
                V8Value_Delete(pV8Value);
            }

            #endregion

            #region V8CpuProfile methods

            void IV8SplitProxyNative.V8CpuProfile_GetInfo(V8CpuProfile.Ptr pProfile, V8Entity.Handle hEntity, out string name, out ulong startTimestamp, out ulong endTimestamp, out int sampleCount, out V8CpuProfile.Node.Ptr pRootNode)
            {
                using (var nameScope = StdString.CreateScope())
                {
                    V8CpuProfile_GetInfo(pProfile, hEntity, nameScope.Value, out startTimestamp, out endTimestamp, out sampleCount, out pRootNode);
                    name = StdString.GetValue(nameScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8CpuProfile_GetSample(V8CpuProfile.Ptr pProfile, int index, out ulong nodeId, out ulong timestamp)
            {
                return V8CpuProfile_GetSample(pProfile, index, out nodeId, out timestamp);
            }

            void IV8SplitProxyNative.V8CpuProfileNode_GetInfo(V8CpuProfile.Node.Ptr pNode, V8Entity.Handle hEntity, out ulong nodeId, out long scriptId, out string scriptName, out string functionName, out string bailoutReason, out long lineNumber, out long columnNumber, out ulong hitCount, out uint hitLineCount, out int childCount)
            {
                using (var scriptNameScope = StdString.CreateScope())
                {
                    using (var functionNameScope = StdString.CreateScope())
                    {
                        using (var bailoutReasonScope = StdString.CreateScope())
                        {
                            V8CpuProfileNode_GetInfo(pNode, hEntity, out nodeId, out scriptId, scriptNameScope.Value, functionNameScope.Value, bailoutReasonScope.Value, out lineNumber, out columnNumber, out hitCount, out hitLineCount, out childCount);
                            scriptName = StdString.GetValue(scriptNameScope.Value);
                            functionName = StdString.GetValue(functionNameScope.Value);
                            bailoutReason = StdString.GetValue(bailoutReasonScope.Value);

                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8CpuProfileNode_GetHitLines(V8CpuProfile.Node.Ptr pNode, out int[] lineNumbers, out uint[] hitCounts)
            {
                using (var lineNumbersScope = StdInt32Array.CreateScope())
                {
                    using (var hitCountsScope = StdUInt32Array.CreateScope())
                    {
                        var result = V8CpuProfileNode_GetHitLines(pNode, lineNumbersScope.Value, hitCountsScope.Value);
                        lineNumbers = StdInt32Array.ToArray(lineNumbersScope.Value);
                        hitCounts = StdUInt32Array.ToArray(hitCountsScope.Value);
                        return result;
                    }
                }
            }

            V8CpuProfile.Node.Ptr IV8SplitProxyNative.V8CpuProfileNode_GetChildNode(V8CpuProfile.Node.Ptr pNode, int index)
            {
                return V8CpuProfileNode_GetChildNode(pNode, index);
            }

            #endregion

            #region V8 isolate methods

            V8Isolate.Handle IV8SplitProxyNative.V8Isolate_Create(string name, int maxNewSpaceSize, int maxOldSpaceSize, double heapExpansionMultiplier, ulong maxArrayBufferAllocation, V8RuntimeFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_Create(nameScope.Value, maxNewSpaceSize, maxOldSpaceSize, heapExpansionMultiplier, maxArrayBufferAllocation, flags, debugPort);
                }
            }

            V8Context.Handle IV8SplitProxyNative.V8Isolate_CreateContext(V8Isolate.Handle hIsolate, string name, V8ScriptEngineFlags flags, int debugPort)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_CreateContext(hIsolate, nameScope.Value, flags, debugPort);
                }
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxHeapSize(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxHeapSize(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxHeapSize(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxHeapSize(hIsolate, size);
            }

            double IV8SplitProxyNative.V8Isolate_GetHeapSizeSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetHeapSizeSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetHeapSizeSampleInterval(V8Isolate.Handle hIsolate, double milliseconds)
            {
                V8Isolate_SetHeapSizeSampleInterval(hIsolate, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Isolate_GetMaxStackUsage(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetMaxStackUsage(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetMaxStackUsage(V8Isolate.Handle hIsolate, UIntPtr size)
            {
                V8Isolate_SetMaxStackUsage(hIsolate, size);
            }

            void IV8SplitProxyNative.V8Isolate_AwaitDebuggerAndPause(V8Isolate.Handle hIsolate)
            {
                V8Isolate_AwaitDebuggerAndPause(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_CancelAwaitDebugger(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CancelAwaitDebugger(hIsolate);
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_Compile(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Isolate_Compile(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileProducingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Isolate_CompileProducingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileConsumingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Isolate_CompileConsumingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Isolate_CompileUpdatingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Isolate_CompileUpdatingCache(hIsolate, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }

                                return hScript;
                            }
                        }
                    }
                }
            }

            bool IV8SplitProxyNative.V8Isolate_GetEnableInterruptPropagation(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetEnableInterruptPropagation(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetEnableInterruptPropagation(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetEnableInterruptPropagation(hIsolate, value);
            }

            bool IV8SplitProxyNative.V8Isolate_GetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetDisableHeapSizeViolationInterrupt(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate, bool value)
            {
                V8Isolate_SetDisableHeapSizeViolationInterrupt(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_GetHeapStatistics(V8Isolate.Handle hIsolate, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Isolate_GetHeapStatistics(hIsolate, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Isolate_GetStatistics(V8Isolate.Handle hIsolate, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Isolate_GetStatistics(hIsolate, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectGarbage(V8Isolate.Handle hIsolate, bool exhaustive)
            {
                V8Isolate_CollectGarbage(hIsolate, exhaustive);
            }

            bool IV8SplitProxyNative.V8Isolate_BeginCpuProfile(V8Isolate.Handle hIsolate, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Isolate_BeginCpuProfile(hIsolate, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Isolate_EndCpuProfile(V8Isolate.Handle hIsolate, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Isolate_EndCpuProfile(hIsolate, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Isolate_CollectCpuProfileSample(V8Isolate.Handle hIsolate)
            {
                V8Isolate_CollectCpuProfileSample(hIsolate);
            }

            uint IV8SplitProxyNative.V8Isolate_GetCpuProfileSampleInterval(V8Isolate.Handle hIsolate)
            {
                return V8Isolate_GetCpuProfileSampleInterval(hIsolate);
            }

            void IV8SplitProxyNative.V8Isolate_SetCpuProfileSampleInterval(V8Isolate.Handle hIsolate, uint value)
            {
                V8Isolate_SetCpuProfileSampleInterval(hIsolate, value);
            }

            void IV8SplitProxyNative.V8Isolate_WriteHeapSnapshot(V8Isolate.Handle hIsolate, IntPtr pStream)
            {
                V8Isolate_WriteHeapSnapshot(hIsolate, pStream);
            }

            #endregion

            #region V8 context methods

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateHeapSize(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateHeapSize(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateHeapSize(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateHeapSize(hContext, size);
            }

            double IV8SplitProxyNative.V8Context_GetIsolateHeapSizeSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetIsolateHeapSizeSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetIsolateHeapSizeSampleInterval(V8Context.Handle hContext, double milliseconds)
            {
                V8Context_SetIsolateHeapSizeSampleInterval(hContext, milliseconds);
            }

            UIntPtr IV8SplitProxyNative.V8Context_GetMaxIsolateStackUsage(V8Context.Handle hContext)
            {
                return V8Context_GetMaxIsolateStackUsage(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetMaxIsolateStackUsage(V8Context.Handle hContext, UIntPtr size)
            {
                V8Context_SetMaxIsolateStackUsage(hContext, size);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLock(V8Context.Handle hContext, IntPtr pAction)
            {
                V8Context_InvokeWithLock(hContext, pAction);
            }

            void IV8SplitProxyNative.V8Context_InvokeWithLockWithArg(V8Context.Handle hContext, IntPtr pAction, IntPtr pArg)
            {
                V8Context_InvokeWithLockWithArg(hContext, pAction, pArg);
            }

            object IV8SplitProxyNative.V8Context_GetRootItem(V8Context.Handle hContext)
            {
                using (var itemScope = V8Value.CreateScope())
                {
                    V8Context_GetRootItem(hContext, itemScope.Value);
                    return V8Value.Get(itemScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_AddGlobalItem(V8Context.Handle hContext, string name, object value, bool globalMembers)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Context_AddGlobalItem(hContext, nameScope.Value, valueScope.Value, globalMembers);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_AwaitDebuggerAndPause(V8Context.Handle hContext)
            {
                V8Context_AwaitDebuggerAndPause(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelAwaitDebugger(V8Context.Handle hContext)
            {
                V8Context_CancelAwaitDebugger(hContext);
            }

            object IV8SplitProxyNative.V8Context_ExecuteCode(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var resultScope = V8Value.CreateScope())
                            {
                                V8Context_ExecuteCode(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, evaluate, resultScope.Value);
                                return V8Value.Get(resultScope.Value);
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest, bool evaluate)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Context_ExecuteScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest, evaluate, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_Compile(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            return V8Context_Compile(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value);
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileScriptFromUtf8(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, IntPtr pCode, int codeLength, UIntPtr codeDigest)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        return V8Context_CompileScriptFromUtf8(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, pCode, codeLength, codeDigest);
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileProducingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope())
                            {
                                var hScript = V8Context_CompileProducingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value);
                                cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                return hScript;
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileConsumingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                return V8Context_CompileConsumingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheAccepted);
                            }
                        }
                    }
                }
            }

            V8Script.Handle IV8SplitProxyNative.V8Context_CompileUpdatingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
            {
                using (var resourceNameScope = StdString.CreateScope(resourceName))
                {
                    using (var sourceMapUrlScope = StdString.CreateScope(sourceMapUrl))
                    {
                        using (var codeScope = StdString.CreateScope(code))
                        {
                            using (var cacheBytesScope = StdByteArray.CreateScope(cacheBytes))
                            {
                                var hScript = V8Context_CompileUpdatingCache(hContext, resourceNameScope.Value, sourceMapUrlScope.Value, uniqueId, documentKind, pDocumentInfo, codeScope.Value, cacheKind, cacheBytesScope.Value, out cacheResult);
                                if (cacheResult == V8CacheResult.Updated)
                                {
                                    cacheBytes = StdByteArray.ToArray(cacheBytesScope.Value);
                                }
                
                                return hScript;
                            }
                        }
                    }
                }
            }

            object IV8SplitProxyNative.V8Context_ExecuteScript(V8Context.Handle hContext, V8Script.Handle hScript, bool evaluate)
            {
                using (var resultScope = V8Value.CreateScope())
                {
                    V8Context_ExecuteScript(hContext, hScript, evaluate, resultScope.Value);
                    return V8Value.Get(resultScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Context_Interrupt(V8Context.Handle hContext)
            {
                V8Context_Interrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_CancelInterrupt(V8Context.Handle hContext)
            {
                V8Context_CancelInterrupt(hContext);
            }

            bool IV8SplitProxyNative.V8Context_GetEnableIsolateInterruptPropagation(V8Context.Handle hContext)
            {
                return V8Context_GetEnableIsolateInterruptPropagation(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetEnableIsolateInterruptPropagation(V8Context.Handle hContext, bool value)
            {
                V8Context_SetEnableIsolateInterruptPropagation(hContext, value);
            }

            bool IV8SplitProxyNative.V8Context_GetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext)
            {
                return V8Context_GetDisableIsolateHeapSizeViolationInterrupt(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext, bool value)
            {
                V8Context_SetDisableIsolateHeapSizeViolationInterrupt(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateHeapStatistics(V8Context.Handle hContext, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize)
            {
                V8Context_GetIsolateHeapStatistics(hContext, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            void IV8SplitProxyNative.V8Context_GetIsolateStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts)
            {
                using (var postedTaskCountsScope = StdUInt64Array.CreateScope())
                {
                    using (var invokedTaskCountsScope = StdUInt64Array.CreateScope())
                    {
                        V8Context_GetIsolateStatistics(hContext, out scriptCount, out scriptCacheSize, out moduleCount, postedTaskCountsScope.Value, invokedTaskCountsScope.Value);
                        postedTaskCounts = StdUInt64Array.ToArray(postedTaskCountsScope.Value);
                        invokedTaskCounts = StdUInt64Array.ToArray(invokedTaskCountsScope.Value);
                    }
                }
            }

            void IV8SplitProxyNative.V8Context_GetStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong moduleCount, out ulong moduleCacheSize)
            {
                V8Context_GetStatistics(hContext, out scriptCount, out moduleCount, out moduleCacheSize);
            }

            void IV8SplitProxyNative.V8Context_CollectGarbage(V8Context.Handle hContext, bool exhaustive)
            {
                V8Context_CollectGarbage(hContext, exhaustive);
            }

            void IV8SplitProxyNative.V8Context_OnAccessSettingsChanged(V8Context.Handle hContext)
            {
                V8Context_OnAccessSettingsChanged(hContext);
            }

            bool IV8SplitProxyNative.V8Context_BeginCpuProfile(V8Context.Handle hContext, string name, bool recordSamples)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Context_BeginCpuProfile(hContext, nameScope.Value, recordSamples);
                }
            }

            void IV8SplitProxyNative.V8Context_EndCpuProfile(V8Context.Handle hContext, string name, IntPtr pAction)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    V8Context_EndCpuProfile(hContext, nameScope.Value, pAction);
                }
            }

            void IV8SplitProxyNative.V8Context_CollectCpuProfileSample(V8Context.Handle hContext)
            {
                V8Context_CollectCpuProfileSample(hContext);
            }

            uint IV8SplitProxyNative.V8Context_GetCpuProfileSampleInterval(V8Context.Handle hContext)
            {
                return V8Context_GetCpuProfileSampleInterval(hContext);
            }

            void IV8SplitProxyNative.V8Context_SetCpuProfileSampleInterval(V8Context.Handle hContext, uint value)
            {
                V8Context_SetCpuProfileSampleInterval(hContext, value);
            }

            void IV8SplitProxyNative.V8Context_WriteIsolateHeapSnapshot(V8Context.Handle hContext, IntPtr pStream)
            {
                V8Context_WriteIsolateHeapSnapshot(hContext, pStream);
            }

            #endregion

            #region V8 object methods

            object IV8SplitProxyNative.V8Object_GetNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        V8Object_GetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                        return V8Value.Get(valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_TryGetNamedProperty(V8Object.Handle hObject, string name, out object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope())
                    {
                        if (V8Object_TryGetNamedProperty(hObject, nameScope.Value, valueScope.Value))
                        {
                            value = V8Value.Get(valueScope.Value);
                            return true;
                        }

                        value = null;
                        return false;
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_SetNamedProperty(V8Object.Handle hObject, string name, object value)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var valueScope = V8Value.CreateScope(value))
                    {
                        V8Object_SetNamedProperty(hObject, nameScope.Value, valueScope.Value);
                    }
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteNamedProperty(V8Object.Handle hObject, string name)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    return V8Object_DeleteNamedProperty(hObject, nameScope.Value);
                }
            }

            string[] IV8SplitProxyNative.V8Object_GetPropertyNames(V8Object.Handle hObject, bool includeIndices)
            {
                using (var namesScope = StdStringArray.CreateScope())
                {
                    V8Object_GetPropertyNames(hObject, includeIndices, namesScope.Value);
                    return StdStringArray.ToArray(namesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_GetIndexedProperty(V8Object.Handle hObject, int index)
            {
                using (var valueScope = V8Value.CreateScope())
                {
                    V8Object_GetIndexedProperty(hObject, index, valueScope.Value);
                    return V8Value.Get(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_SetIndexedProperty(V8Object.Handle hObject, int index, object value)
            {
                using (var valueScope = V8Value.CreateScope(value))
                {
                    V8Object_SetIndexedProperty(hObject, index, valueScope.Value);
                }
            }

            bool IV8SplitProxyNative.V8Object_DeleteIndexedProperty(V8Object.Handle hObject, int index)
            {
                return V8Object_DeleteIndexedProperty(hObject, index);
            }

            int[] IV8SplitProxyNative.V8Object_GetPropertyIndices(V8Object.Handle hObject)
            {
                using (var indicesScope = StdInt32Array.CreateScope())
                {
                    V8Object_GetPropertyIndices(hObject, indicesScope.Value);
                    return StdInt32Array.ToArray(indicesScope.Value);
                }
            }

            object IV8SplitProxyNative.V8Object_Invoke(V8Object.Handle hObject, bool asConstructor, object[] args)
            {
                using (var argsScope = StdV8ValueArray.CreateScope(args))
                {
                    using (var resultScope = V8Value.CreateScope())
                    {
                        V8Object_Invoke(hObject, asConstructor, argsScope.Value, resultScope.Value);
                        return V8Value.Get(resultScope.Value);
                    }
                }
            }

            object IV8SplitProxyNative.V8Object_InvokeMethod(V8Object.Handle hObject, string name, object[] args)
            {
                using (var nameScope = StdString.CreateScope(name))
                {
                    using (var argsScope = StdV8ValueArray.CreateScope(args))
                    {
                        using (var resultScope = V8Value.CreateScope())
                        {
                            V8Object_InvokeMethod(hObject, nameScope.Value, argsScope.Value, resultScope.Value);
                            return V8Value.Get(resultScope.Value);
                        }
                    }
                }
            }

            void IV8SplitProxyNative.V8Object_GetArrayBufferOrViewInfo(V8Object.Handle hObject, out IV8Object arrayBuffer, out ulong offset, out ulong size, out ulong length)
            {
                using (var arrayBufferScope = V8Value.CreateScope())
                {
                    V8Object_GetArrayBufferOrViewInfo(hObject, arrayBufferScope.Value, out offset, out size, out length);
                    arrayBuffer = (IV8Object)V8Value.Get(arrayBufferScope.Value);
                }
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewData(V8Object.Handle hObject, IntPtr pAction)
            {
                V8Object_InvokeWithArrayBufferOrViewData(hObject, pAction);
            }

            void IV8SplitProxyNative.V8Object_InvokeWithArrayBufferOrViewDataWithArg(V8Object.Handle hObject, IntPtr pAction, IntPtr pArg)
            {
                V8Object_InvokeWithArrayBufferOrViewDataWithArg(hObject, pAction, pArg);
            }

            #endregion

            #region V8 debug callback methods

            void IV8SplitProxyNative.V8DebugCallback_ConnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_ConnectClient(hCallback);
            }

            void IV8SplitProxyNative.V8DebugCallback_SendCommand(V8DebugCallback.Handle hCallback, string command)
            {
                using (var commandScope = StdString.CreateScope(command))
                {
                    V8DebugCallback_SendCommand(hCallback, commandScope.Value);
                }
            }

            void IV8SplitProxyNative.V8DebugCallback_DisconnectClient(V8DebugCallback.Handle hCallback)
            {
                V8DebugCallback_DisconnectClient(hCallback);
            }

            #endregion

            #region native callback methods

            void IV8SplitProxyNative.NativeCallback_Invoke(NativeCallback.Handle hCallback)
            {
                NativeCallback_Invoke(hCallback);
            }

            #endregion

            #region V8 entity methods

            void IV8SplitProxyNative.V8Entity_Release(V8Entity.Handle hEntity)
            {
                V8Entity_Release(hEntity);
            }

            V8Entity.Handle IV8SplitProxyNative.V8Entity_CloneHandle(V8Entity.Handle hEntity)
            {
                return V8Entity_CloneHandle(hEntity);
            }

            void IV8SplitProxyNative.V8Entity_DestroyHandle(V8Entity.Handle hEntity)
            {
                V8Entity_DestroyHandle(hEntity);
            }

            #endregion

            #region error handling

            void IV8SplitProxyNative.HostException_Schedule(string message, object exception)
            {
                using (var messageScope = StdString.CreateScope(message))
                {
                    using (var exceptionScope = V8Value.CreateScope(exception))
                    {
                        HostException_Schedule(messageScope.Value, exceptionScope.Value);
                    }
                }
            }

            #endregion

            #region unit test support

            UIntPtr IV8SplitProxyNative.V8UnitTestSupport_GetTextDigest(string value)
            {
                using (var valueScope = StdString.CreateScope(value))
                {
                    return V8UnitTestSupport_GetTextDigest(valueScope.Value);
                }
            }

            void IV8SplitProxyNative.V8UnitTestSupport_GetStatistics(out ulong isolateCount, out ulong contextCount)
            {
                V8UnitTestSupport_GetStatistics(out isolateCount, out contextCount);
            }

            #endregion

            #endregion

            #region native methods

            #region initialization

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyManaged_SetMethodTable(
                [In] IntPtr pMethodTable
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr V8SplitProxyNative_GetVersion();

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Environment_InitializeICU(
                [In] IntPtr pICUData,
                [In] uint size
            );

            #endregion

            #region memory methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_Allocate(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr Memory_AllocateZeroed(
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void Memory_Free(
                [In] IntPtr pMemory
            );

            #endregion

            #region StdString methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdString.Ptr StdString_New(
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdString_GetValue(
                [In] StdString.Ptr pString,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_SetValue(
                [In] StdString.Ptr pString,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdString_Delete(
                [In] StdString.Ptr pString
            );

            #endregion

            #region StdStringArray methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdStringArray.Ptr StdStringArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdStringArray_GetElementCount(
                [In] StdStringArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElementCount(
                [In] StdStringArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdStringArray_GetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [Out] out int length
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_SetElement(
                [In] StdStringArray.Ptr pArray,
                [In] int index,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdStringArray_Delete(
                [In] StdStringArray.Ptr pArray
            );

            #endregion

            #region StdByteArray methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdByteArray.Ptr StdByteArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdByteArray_GetElementCount(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_SetElementCount(
                [In] StdByteArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdByteArray_GetData(
                [In] StdByteArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdByteArray_Delete(
                [In] StdByteArray.Ptr pArray
            );

            #endregion

            #region StdInt32Array methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdInt32Array.Ptr StdInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdInt32Array_GetElementCount(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_SetElementCount(
                [In] StdInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdInt32Array_GetData(
                [In] StdInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdInt32Array_Delete(
                [In] StdInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt32Array methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt32Array.Ptr StdUInt32Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt32Array_GetElementCount(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_SetElementCount(
                [In] StdUInt32Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt32Array_GetData(
                [In] StdUInt32Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt32Array_Delete(
                [In] StdUInt32Array.Ptr pArray
            );

            #endregion

            #region StdUInt64Array methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdUInt64Array.Ptr StdUInt64Array_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdUInt64Array_GetElementCount(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_SetElementCount(
                [In] StdUInt64Array.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdUInt64Array_GetData(
                [In] StdUInt64Array.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdUInt64Array_Delete(
                [In] StdUInt64Array.Ptr pArray
            );

            #endregion

            #region StdPtrArray methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdPtrArray.Ptr StdPtrArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdPtrArray_GetElementCount(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_SetElementCount(
                [In] StdPtrArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern IntPtr StdPtrArray_GetData(
                [In] StdPtrArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdPtrArray_Delete(
                [In] StdPtrArray.Ptr pArray
            );

            #endregion

            #region StdV8ValueArray methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern StdV8ValueArray.Ptr StdV8ValueArray_New(
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern int StdV8ValueArray_GetElementCount(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_SetElementCount(
                [In] StdV8ValueArray.Ptr pArray,
                [In] int elementCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr StdV8ValueArray_GetData(
                [In] StdV8ValueArray.Ptr pArray
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void StdV8ValueArray_Delete(
                [In] StdV8ValueArray.Ptr pArray
            );

            #endregion

            #region V8Value methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Value.Ptr V8Value_New();

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNonexistent(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetUndefined(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNull(
                [In] V8Value.Ptr pV8Value
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBoolean(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetNumber(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetString(
                [In] V8Value.Ptr pV8Value,
                [In] [MarshalAs(UnmanagedType.LPWStr)] string value,
                [In] int length
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetDateTime(
                [In] V8Value.Ptr pV8Value,
                [In] double value
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetBigInt(
                [In] V8Value.Ptr pV8Value,
                [In] int signBit,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] bytes,
                [In] int length
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetV8Object(
                [In] V8Value.Ptr pV8Value,
                [In] V8Object.Handle hObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_SetHostObject(
                [In] V8Value.Ptr pV8Value,
                [In] IntPtr pObject,
                [In] V8Value.Subtype subtype,
                [In] V8Value.Flags flags
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Decode(
                [In] V8Value.Ptr pV8Value,
                [Out] out V8Value.Decoded decoded
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Value_Delete(
                [In] V8Value.Ptr pV8Value
            );

            #endregion

            #region V8CpuProfile methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfile_GetInfo(
                [In] V8CpuProfile.Ptr pProfile,
                [In] V8Entity.Handle hEntity,
                [In] StdString.Ptr pName,
                [Out] out ulong startTimestamp,
                [Out] out ulong endTimestamp,
                [Out] out int sampleCount,
                [Out] out V8CpuProfile.Node.Ptr pRootNode
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfile_GetSample(
                [In] V8CpuProfile.Ptr pProfile,
                [In] int index,
                [Out] out ulong nodeId,
                [Out] out ulong timestamp
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8CpuProfileNode_GetInfo(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] V8Entity.Handle hEntity,
                [Out] out ulong nodeId,
                [Out] out long scriptId,
                [In] StdString.Ptr pScriptName,
                [In] StdString.Ptr pFunctionName,
                [In] StdString.Ptr pBailoutReason,
                [Out] out long lineNumber,
                [Out] out long columnNumber,
                [Out] out ulong hitCount,
                [Out] out uint hitLineCount,
                [Out] out int childCount
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8CpuProfileNode_GetHitLines(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] StdInt32Array.Ptr pLineNumbers,
                [In] StdUInt32Array.Ptr pHitCounts
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8CpuProfile.Node.Ptr V8CpuProfileNode_GetChildNode(
                [In] V8CpuProfile.Node.Ptr pNode,
                [In] int index
            );

            #endregion

            #region V8 isolate methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Isolate.Handle V8Isolate_Create(
                [In] StdString.Ptr pName,
                [In] int maxNewSpaceSize,
                [In] int maxOldSpaceSize,
                [In] double heapExpansionMultiplier,
                [In] ulong maxArrayBufferAllocation,
                [In] V8RuntimeFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Context.Handle V8Isolate_CreateContext(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] V8ScriptEngineFlags flags,
                [In] int debugPort
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxHeapSize(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Isolate_GetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetHeapSizeSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Isolate_GetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetMaxStackUsage(
                [In] V8Isolate.Handle hIsolate,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_AwaitDebuggerAndPause(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CancelAwaitDebugger(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_Compile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileProducingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileConsumingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Isolate_CompileUpdatingCache(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetEnableInterruptPropagation(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_GetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetDisableHeapSizeViolationInterrupt(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetHeapStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_GetStatistics(
                [In] V8Isolate.Handle hIsolate,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectGarbage(
                [In] V8Isolate.Handle hIsolate,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Isolate_BeginCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_EndCpuProfile(
                [In] V8Isolate.Handle hIsolate,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_CollectCpuProfileSample(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Isolate_GetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_SetCpuProfileSampleInterval(
                [In] V8Isolate.Handle hIsolate,
                [In] uint value
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Isolate_WriteHeapSnapshot(
                [In] V8Isolate.Handle hIsolate,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 context methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateHeapSize(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern double V8Context_GetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetIsolateHeapSizeSampleInterval(
                [In] V8Context.Handle hContext,
                [In] double milliseconds
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8Context_GetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetMaxIsolateStackUsage(
                [In] V8Context.Handle hContext,
                [In] UIntPtr size
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLock(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_InvokeWithLockWithArg(
                [In] V8Context.Handle hContext,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetRootItem(
                [In] V8Context.Handle hContext,
                [In] V8Value.Ptr pItem
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AddGlobalItem(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue,
                [In] [MarshalAs(UnmanagedType.I1)] bool globalMembers
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_AwaitDebuggerAndPause(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelAwaitDebugger(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteCode(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_Compile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileScriptFromUtf8(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] IntPtr pCode,
                [In] int codeLength,
                [In] UIntPtr codeDigest
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileProducingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileConsumingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] [MarshalAs(UnmanagedType.I1)] out bool cacheAccepted
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Script.Handle V8Context_CompileUpdatingCache(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pResourceName,
                [In] StdString.Ptr pSourceMapUrl,
                [In] ulong uniqueId,
                [In] DocumentKind documentKind,
                [In] IntPtr pDocumentInfo,
                [In] StdString.Ptr pCode,
                [In] V8CacheKind cacheKind,
                [In] StdByteArray.Ptr pCacheBytes,
                [Out] out V8CacheResult cacheResult
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_ExecuteScript(
                [In] V8Context.Handle hContext,
                [In] V8Script.Handle hScript,
                [In] [MarshalAs(UnmanagedType.I1)] bool evaluate,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_Interrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CancelInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetEnableIsolateInterruptPropagation(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_GetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetDisableIsolateHeapSizeViolationInterrupt(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool value
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateHeapStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong totalHeapSize,
                [Out] out ulong totalHeapSizeExecutable,
                [Out] out ulong totalPhysicalSize,
                [Out] out ulong totalAvailableSize,
                [Out] out ulong usedHeapSize,
                [Out] out ulong heapSizeLimit,
                [Out] out ulong totalExternalSize
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetIsolateStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong scriptCacheSize,
                [Out] out ulong moduleCount,
                [In] StdUInt64Array.Ptr pPostedTaskCounts,
                [In] StdUInt64Array.Ptr pInvokedTaskCounts
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_GetStatistics(
                [In] V8Context.Handle hContext,
                [Out] out ulong scriptCount,
                [Out] out ulong moduleCount,
                [Out] out ulong moduleCacheSize
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectGarbage(
                [In] V8Context.Handle hContext,
                [In] [MarshalAs(UnmanagedType.I1)] bool exhaustive
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_OnAccessSettingsChanged(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Context_BeginCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] [MarshalAs(UnmanagedType.I1)] bool recordSamples
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_EndCpuProfile(
                [In] V8Context.Handle hContext,
                [In] StdString.Ptr pName,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_CollectCpuProfileSample(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern uint V8Context_GetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_SetCpuProfileSampleInterval(
                [In] V8Context.Handle hContext,
                [In] uint value
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Context_WriteIsolateHeapSnapshot(
                [In] V8Context.Handle hContext,
                [In] IntPtr pStream
            );

            #endregion

            #region V8 object methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_TryGetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteNamedProperty(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyNames(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool includeIndices,
                [In] StdStringArray.Ptr pNames
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_SetIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index,
                [In] V8Value.Ptr pValue
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.I1)]
            private static extern bool V8Object_DeleteIndexedProperty(
                [In] V8Object.Handle hObject,
                [In] int index
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetPropertyIndices(
                [In] V8Object.Handle hObject,
                [In] StdInt32Array.Ptr pIndices
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_Invoke(
                [In] V8Object.Handle hObject,
                [In] [MarshalAs(UnmanagedType.I1)] bool asConstructor,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeMethod(
                [In] V8Object.Handle hObject,
                [In] StdString.Ptr pName,
                [In] StdV8ValueArray.Ptr pArgs,
                [In] V8Value.Ptr pResult
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_GetArrayBufferOrViewInfo(
                [In] V8Object.Handle hObject,
                [In] V8Value.Ptr pArrayBuffer,
                [Out] out ulong offset,
                [Out] out ulong size,
                [Out] out ulong length
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewData(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Object_InvokeWithArrayBufferOrViewDataWithArg(
                [In] V8Object.Handle hObject,
                [In] IntPtr pAction,
                [In] IntPtr pArg
            );

            #endregion

            #region V8 debug callback methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_ConnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_SendCommand(
                [In] V8DebugCallback.Handle hCallback,
                [In] StdString.Ptr pCommand
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8DebugCallback_DisconnectClient(
                [In] V8DebugCallback.Handle hCallback
            );

            #endregion

            #region native callback methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void NativeCallback_Invoke(
                [In] NativeCallback.Handle hCallback
            );

            #endregion

            #region V8 entity methods

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_Release(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern V8Entity.Handle V8Entity_CloneHandle(
                [In] V8Entity.Handle hEntity
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8Entity_DestroyHandle(
                [In] V8Entity.Handle hEntity
            );

            #endregion

            #region error handling

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void HostException_Schedule(
                [In] StdString.Ptr pMessage,
                [In] V8Value.Ptr pException
            );

            #endregion

            #region unit test support

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern UIntPtr V8UnitTestSupport_GetTextDigest(
                [In] StdString.Ptr pString
            );

            [DllImport("ClearScriptV8.osx-arm64.dylib", CallingConvention = CallingConvention.StdCall)]
            private static extern void V8UnitTestSupport_GetStatistics(
                [Out] out ulong isolateCount,
                [Out] out ulong contextCount
            );

            #endregion

            #endregion
        }

        #endregion

        

    }
}

