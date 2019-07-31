// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    internal static class V8ProxyHelpers
    {
        #region strings

        public static unsafe char* AllocString(string value)
        {
            return (char*)Marshal.StringToHGlobalUni(value).ToPointer();
        }

        public static unsafe void FreeString(char* pValue)
        {
            Marshal.FreeHGlobal((IntPtr)pValue);
        }

        #endregion

        #region host object lifetime

        public static unsafe void* AddRefHostObject(void* pObject)
        {
            return AddRefHostObject(GetHostObject(pObject));
        }

        public static unsafe void* AddRefHostObject(object obj)
        {
            return GCHandle.ToIntPtr(GCHandle.Alloc(obj)).ToPointer();
        }

        public static unsafe void ReleaseHostObject(void* pObject)
        {
            GCHandle.FromIntPtr((IntPtr)pObject).Free();
        }

        #endregion

        #region host object access

        public static unsafe object GetHostObject(void* pObject)
        {
            return GCHandle.FromIntPtr((IntPtr)pObject).Target;
        }

        public static unsafe object GetHostObjectProperty(void* pObject, string name)
        {
            return GetHostObjectProperty(GetHostObject(pObject), name);
        }

        public static object GetHostObjectProperty(object obj, string name)
        {
            return ((IDynamic)obj).GetProperty(name, ArrayHelpers.GetEmptyArray<object>());
        }

        public static unsafe object GetHostObjectProperty(void* pObject, string name, out bool isCacheable)
        {
            return GetHostObjectProperty(GetHostObject(pObject), name, out isCacheable);
        }

        public static object GetHostObjectProperty(object obj, string name, out bool isCacheable)
        {
            return ((IDynamic)obj).GetProperty(name, out isCacheable, ArrayHelpers.GetEmptyArray<object>());
        }

        public static unsafe void SetHostObjectProperty(void* pObject, string name, object value)
        {
            SetHostObjectProperty(GetHostObject(pObject), name, value);
        }

        public static void SetHostObjectProperty(object obj, string name, object value)
        {
            ((IDynamic)obj).SetProperty(name, value);
        }

        public static unsafe bool DeleteHostObjectProperty(void* pObject, string name)
        {
            return DeleteHostObjectProperty(GetHostObject(pObject), name);
        }

        public static bool DeleteHostObjectProperty(object obj, string name)
        {
            return ((IDynamic)obj).DeleteProperty(name);
        }

        public static unsafe string[] GetHostObjectPropertyNames(void* pObject)
        {
            return GetHostObjectPropertyNames(GetHostObject(pObject));
        }

        public static string[] GetHostObjectPropertyNames(object obj)
        {
            return ((IDynamic)obj).GetPropertyNames();
        }

        public static unsafe object GetHostObjectProperty(void* pObject, int index)
        {
            return GetHostObjectProperty(GetHostObject(pObject), index);
        }

        public static object GetHostObjectProperty(object obj, int index)
        {
            return ((IDynamic)obj).GetProperty(index);
        }

        public static unsafe void SetHostObjectProperty(void* pObject, int index, object value)
        {
            SetHostObjectProperty(GetHostObject(pObject), index, value);
        }

        public static void SetHostObjectProperty(object obj, int index, object value)
        {
            ((IDynamic)obj).SetProperty(index, value);
        }

        public static unsafe bool DeleteHostObjectProperty(void* pObject, int index)
        {
            return DeleteHostObjectProperty(GetHostObject(pObject), index);
        }

        public static bool DeleteHostObjectProperty(object obj, int index)
        {
            return ((IDynamic)obj).DeleteProperty(index);
        }

        public static unsafe int[] GetHostObjectPropertyIndices(void* pObject)
        {
            return GetHostObjectPropertyIndices(GetHostObject(pObject));
        }

        public static int[] GetHostObjectPropertyIndices(object obj)
        {
            return ((IDynamic)obj).GetPropertyIndices();
        }

        public static unsafe object InvokeHostObject(void* pObject, bool asConstructor, object[] args)
        {
            return InvokeHostObject(GetHostObject(pObject), asConstructor, args);
        }

        public static object InvokeHostObject(object obj, bool asConstructor, object[] args)
        {
            return ((IDynamic)obj).Invoke(asConstructor, args);
        }

        public static unsafe object InvokeHostObjectMethod(void* pObject, string name, object[] args)
        {
            return InvokeHostObjectMethod(GetHostObject(pObject), name, args);
        }

        public static object InvokeHostObjectMethod(object obj, string name, object[] args)
        {
            return ((IDynamic)obj).InvokeMethod(name, args);
        }

        public static unsafe Invocability GetHostObjectInvocability(void* pObject)
        {
            return GetHostObjectInvocability(GetHostObject(pObject));
        }

        public static Invocability GetHostObjectInvocability(object obj)
        {
            var hostItem = obj as HostItem;
            if (hostItem == null)
            {
                return Invocability.None;
            }

            return hostItem.Invocability;
        }

        public static unsafe object GetEnumeratorForHostObject(void* pObject)
        {
            return GetEnumeratorForHostObject(GetHostObject(pObject));
        }

        public static object GetEnumeratorForHostObject(object obj)
        {
            return ((IDynamic)obj).InvokeMethod(SpecialMemberNames.NewEnum, ArrayHelpers.GetEmptyArray<object>());
        }

        public static unsafe bool AdvanceEnumerator(void* pEnumerator, out object value)
        {
            return AdvanceEnumerator(GetHostObject(pEnumerator), out value);
        }

        public static bool AdvanceEnumerator(object enumerator, out object value)
        {
            var wrapper = (IScriptMarshalWrapper)enumerator;
            if (((IEnumerator)wrapper.Unwrap()).MoveNext())
            {
                value = ((IDynamic)enumerator).GetProperty("Current", ArrayHelpers.GetEmptyArray<object>());
                return true;
            }

            value = null;
            return false;
        }

        #endregion

        #region exception marshaling

        public static unsafe object MarshalExceptionToScript(void* pSource, Exception exception)
        {
            return MarshalExceptionToScript(GetHostObject(pSource), exception);
        }

        public static object MarshalExceptionToScript(object source, Exception exception)
        {
            return ((IScriptMarshalWrapper)source).Engine.MarshalToScript(exception);
        }

        public static Exception MarshalExceptionToHost(object exception)
        {
            return (exception != null) ? (Exception)((IScriptMarshalWrapper)exception).Engine.MarshalToHost(exception, false) : null;
        }

        #endregion

        #region V8 object cache

        public static unsafe void* CreateV8ObjectCache()
        {
            return AddRefHostObject(new Dictionary<object, IntPtr>());
        }

        public static unsafe void CacheV8Object(void* pCache, void* pObject, void* pV8Object)
        {
            ((Dictionary<object, IntPtr>)GetHostObject(pCache)).Add(GetHostObject(pObject), (IntPtr)pV8Object);
        }

        public static unsafe void* GetCachedV8Object(void* pCache, void* pObject)
        {
            IntPtr pV8Object;
            return ((Dictionary<object, IntPtr>)GetHostObject(pCache)).TryGetValue(GetHostObject(pObject), out pV8Object) ? pV8Object.ToPointer() : null;
        }

        public static unsafe IntPtr[] GetAllCachedV8Objects(void* pCache)
        {
            return ((Dictionary<object, IntPtr>)GetHostObject(pCache)).Values.ToArray();
        }

        public static unsafe bool RemoveV8ObjectCacheEntry(void* pCache, void* pObject)
        {
            return ((Dictionary<object, IntPtr>)GetHostObject(pCache)).Remove(GetHostObject(pObject));
        }

        #endregion

        #region V8 debug agent

        public static unsafe void* CreateDebugAgent(string name, string version, int port, bool remote, IV8DebugListener listener)
        {
            return AddRefHostObject(new V8DebugAgent(name, version, port, remote, listener));
        }

        public static unsafe void SendDebugMessage(void* pAgent, string content)
        {
            ((V8DebugAgent)GetHostObject(pAgent)).SendMessage(content);
        }

        public static unsafe void DestroyDebugAgent(void* pAgent)
        {
            ((V8DebugAgent)GetHostObject(pAgent)).Dispose();
            ReleaseHostObject(pAgent);
        }

        #endregion

        #region native callback timer

        public static unsafe void* CreateNativeCallbackTimer(int dueTime, int period, INativeCallback callback)
        {
            return AddRefHostObject(new NativeCallbackTimer(dueTime, period, callback));
        }

        public static unsafe bool ChangeNativeCallbackTimer(void* pTimer, int dueTime, int period)
        {
            return ((NativeCallbackTimer)GetHostObject(pTimer)).Change(dueTime, period);
        }

        public static unsafe void DestroyNativeCallbackTimer(void* pTimer)
        {
            ((NativeCallbackTimer)GetHostObject(pTimer)).Dispose();
            ReleaseHostObject(pTimer);
        }

        #endregion

        #region module support

        public static unsafe string LoadModule(void* pSourceDocumentInfo, string specifier, DocumentCategory category, out UniqueDocumentInfo documentInfo)
        {
            var engine = ScriptEngine.Current;
            if (engine == null)
            {
                throw new InvalidOperationException("Module loading requires a script engine");
            }

            var settings = engine.DocumentSettings;
            var document = settings.Loader.LoadDocument(settings, ((UniqueDocumentInfo)GetHostObject(pSourceDocumentInfo)).Info, specifier, category, null);
            var code = document.GetTextContents();

            documentInfo = document.Info.MakeUnique(engine);
            return code;
        }

        public static unsafe IDictionary<string, object> CreateModuleContext(void* pDocumentInfo)
        {
            var engine = ScriptEngine.Current;
            if (engine == null)
            {
                throw new InvalidOperationException("Module context construction requires a script engine");
            }

            var documentInfo = (UniqueDocumentInfo)GetHostObject(pDocumentInfo);

            var callback = documentInfo.ContextCallback ?? engine.DocumentSettings.ContextCallback;
            if (callback != null)
            {
                var sharedContext = callback(documentInfo.Info);
                if (sharedContext != null)
                {
                    var context = new Dictionary<string, object>(sharedContext.Count);
                    foreach (var pair in sharedContext)
                    {
                        context.Add(pair.Key, engine.MarshalToScript(pair.Value));
                    }

                    return context;
                }
            }

            return null;
        }

        #endregion
    }
}
