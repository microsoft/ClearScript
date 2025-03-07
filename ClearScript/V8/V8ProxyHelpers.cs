// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8.FastProxy;
using Microsoft.ClearScript.V8.SplitProxy;

namespace Microsoft.ClearScript.V8
{
    internal static class V8ProxyHelpers
    {
        #region host object lifetime

        public static IntPtr AddRefHostObject(IntPtr pObject)
        {
            return AddRefHostObject(GetHostObject(pObject));
        }

        public static IntPtr AddRefHostObject(object obj)
        {
            return GCHandle.ToIntPtr(GCHandle.Alloc(obj));
        }

        public static void ReleaseHostObject(IntPtr pObject)
        {
            GCHandle.FromIntPtr(pObject).Free();
        }

        public static ValueScope<IntPtr> CreateAddRefHostObjectScope(object obj)
        {
            return ScopeFactory.Create(static obj => AddRefHostObject(obj), static pObject => ReleaseHostObject(pObject), obj);
        }

        #endregion

        #region host object access

        public static object GetHostObject(IntPtr pObject)
        {
            return GCHandle.FromIntPtr(pObject).Target;
        }

        public static T GetHostObject<T>(IntPtr pObject) where T : class
        {
            return (T)GetHostObject(pObject);
        }

        public static object GetHostObjectProperty(IntPtr pObject, string name, out bool isCacheable)
        {
            return GetHostObjectProperty(GetHostObject(pObject), name, out isCacheable);
        }

        public static object GetHostObjectProperty(object obj, string name, out bool isCacheable)
        {
            return ((IDynamic)obj).GetProperty(name, out isCacheable);
        }

        public static void SetHostObjectProperty(IntPtr pObject, string name, object value)
        {
            SetHostObjectProperty(GetHostObject(pObject), name, value);
        }

        public static void SetHostObjectProperty(object obj, string name, object value)
        {
            ((IDynamic)obj).SetProperty(name, value);
        }

        public static bool DeleteHostObjectProperty(IntPtr pObject, string name)
        {
            return DeleteHostObjectProperty(GetHostObject(pObject), name);
        }

        public static bool DeleteHostObjectProperty(object obj, string name)
        {
            return ((IDynamic)obj).DeleteProperty(name);
        }

        public static string[] GetHostObjectPropertyNames(IntPtr pObject)
        {
            return GetHostObjectPropertyNames(GetHostObject(pObject));
        }

        public static string[] GetHostObjectPropertyNames(object obj)
        {
            return ((IDynamic)obj).GetPropertyNames();
        }

        public static object GetHostObjectProperty(IntPtr pObject, int index)
        {
            return GetHostObjectProperty(GetHostObject(pObject), index);
        }

        public static object GetHostObjectProperty(object obj, int index)
        {
            return ((IDynamic)obj).GetProperty(index);
        }

        public static void SetHostObjectProperty(IntPtr pObject, int index, object value)
        {
            SetHostObjectProperty(GetHostObject(pObject), index, value);
        }

        public static void SetHostObjectProperty(object obj, int index, object value)
        {
            ((IDynamic)obj).SetProperty(index, value);
        }

        public static bool DeleteHostObjectProperty(IntPtr pObject, int index)
        {
            return DeleteHostObjectProperty(GetHostObject(pObject), index);
        }

        public static bool DeleteHostObjectProperty(object obj, int index)
        {
            return ((IDynamic)obj).DeleteProperty(index);
        }

        public static int[] GetHostObjectPropertyIndices(IntPtr pObject)
        {
            return GetHostObjectPropertyIndices(GetHostObject(pObject));
        }

        public static int[] GetHostObjectPropertyIndices(object obj)
        {
            return ((IDynamic)obj).GetPropertyIndices();
        }

        public static object InvokeHostObject(IntPtr pObject, bool asConstructor, object[] args)
        {
            return InvokeHostObject(GetHostObject(pObject), asConstructor, args);
        }

        public static object InvokeHostObject(object obj, bool asConstructor, object[] args)
        {
            return ((IDynamic)obj).Invoke(asConstructor, args);
        }

        public static object InvokeHostObjectMethod(IntPtr pObject, string name, object[] args)
        {
            return InvokeHostObjectMethod(GetHostObject(pObject), name, args);
        }

        public static object InvokeHostObjectMethod(object obj, string name, object[] args)
        {
            return ((IDynamic)obj).InvokeMethod(name, args);
        }

        public static Invocability GetHostObjectInvocability(IntPtr pObject)
        {
            return GetHostObjectInvocability(GetHostObject(pObject));
        }

        public static Invocability GetHostObjectInvocability(object obj)
        {
            var hostItem = obj as HostItem;
            if (hostItem is null)
            {
                return Invocability.None;
            }

            return hostItem.Invocability;
        }

        public static object GetHostObjectEnumerator(IntPtr pObject)
        {
            return GetHostObjectEnumerator(GetHostObject(pObject));
        }

        public static object GetHostObjectEnumerator(object obj)
        {
            return ((IDynamic)obj).GetProperty(SpecialMemberNames.NewEnum);
        }

        public static object GetHostObjectAsyncEnumerator(IntPtr pObject)
        {
            return GetHostObjectAsyncEnumerator(GetHostObject(pObject));
        }

        public static object GetHostObjectAsyncEnumerator(object obj)
        {
            return ((IDynamic)obj).GetProperty(SpecialMemberNames.NewAsyncEnum);
        }

        #endregion

        #region fast host object access

        public static void GetFastHostObjectProperty(IntPtr pObject, StdString.Ptr pName, V8Value.FastResult.Ptr pValue, out bool isCacheable)
        {
            GetFastHostObjectProperty(GetHostObject(pObject), pName, pValue, out isCacheable);
        }

        public static void GetFastHostObjectProperty(object obj, StdString.Ptr pName, V8Value.FastResult.Ptr pValue, out bool isCacheable)
        {
            ((V8FastHostItem)obj).GetProperty(pName, pValue, out isCacheable);
        }

        public static void SetFastHostObjectProperty(IntPtr pObject, StdString.Ptr pName, V8Value.FastArg.Ptr pValue)
        {
            SetFastHostObjectProperty(GetHostObject(pObject), pName, pValue);
        }

        public static void SetFastHostObjectProperty(object obj, StdString.Ptr pName, V8Value.FastArg.Ptr pValue)
        {
            ((V8FastHostItem)obj).SetProperty(pName, pValue);
        }

        public static V8FastHostPropertyFlags QueryFastHostObjectProperty(IntPtr pObject, StdString.Ptr pName)
        {
            return QueryFastHostObjectProperty(GetHostObject(pObject), pName);
        }

        public static V8FastHostPropertyFlags QueryFastHostObjectProperty(object obj, StdString.Ptr pName)
        {
            return ((V8FastHostItem)obj).QueryProperty(pName);
        }

        public static bool DeleteFastHostObjectProperty(IntPtr pObject, StdString.Ptr pName)
        {
            return DeleteFastHostObjectProperty(GetHostObject(pObject), pName);
        }

        public static bool DeleteFastHostObjectProperty(object obj, StdString.Ptr pName)
        {
            return ((V8FastHostItem)obj).DeleteProperty(pName);
        }

        public static IEnumerable<string> GetFastHostObjectPropertyNames(IntPtr pObject)
        {
            return GetFastHostObjectPropertyNames(GetHostObject(pObject));
        }

        public static IEnumerable<string> GetFastHostObjectPropertyNames(object obj)
        {
            return ((V8FastHostItem)obj).GetPropertyNames();
        }

        public static void GetFastHostObjectProperty(IntPtr pObject, int index, V8Value.FastResult.Ptr pValue)
        {
            GetFastHostObjectProperty(GetHostObject(pObject), index, pValue);
        }

        public static void GetFastHostObjectProperty(object obj, int index, V8Value.FastResult.Ptr pValue)
        {
            ((V8FastHostItem)obj).GetProperty(index, pValue);
        }

        public static void SetFastHostObjectProperty(IntPtr pObject, int index, V8Value.FastArg.Ptr pValue)
        {
            SetFastHostObjectProperty(GetHostObject(pObject), index, pValue);
        }

        public static void SetFastHostObjectProperty(object obj, int index, V8Value.FastArg.Ptr pValue)
        {
            ((V8FastHostItem)obj).SetProperty(index, pValue);
        }

        public static V8FastHostPropertyFlags QueryFastHostObjectProperty(IntPtr pObject, int index)
        {
            return QueryFastHostObjectProperty(GetHostObject(pObject), index);
        }

        public static V8FastHostPropertyFlags QueryFastHostObjectProperty(object obj, int index)
        {
            return ((V8FastHostItem)obj).QueryProperty(index);
        }

        public static bool DeleteFastHostObjectProperty(IntPtr pObject, int index)
        {
            return DeleteFastHostObjectProperty(GetHostObject(pObject), index);
        }

        public static bool DeleteFastHostObjectProperty(object obj, int index)
        {
            return ((V8FastHostItem)obj).DeleteProperty(index);
        }

        public static IEnumerable<int> GetFastHostObjectPropertyIndices(IntPtr pObject)
        {
            return GetFastHostObjectPropertyIndices(GetHostObject(pObject));
        }

        public static IEnumerable<int> GetFastHostObjectPropertyIndices(object obj)
        {
            return ((V8FastHostItem)obj).GetPropertyIndices();
        }

        public static void InvokeFastHostObject(IntPtr pObject, bool asConstructor, int argCount, V8Value.FastArg.Ptr pArgs, V8Value.FastResult.Ptr pResult)
        {
            InvokeFastHostObject(GetHostObject(pObject), asConstructor, argCount, pArgs, pResult);
        }

        public static void InvokeFastHostObject(object obj, bool asConstructor, int argCount, V8Value.FastArg.Ptr pArgs, V8Value.FastResult.Ptr pResult)
        {
            ((V8FastHostItem)obj).Invoke(asConstructor, argCount, pArgs, pResult);
        }

        public static void GetFastHostObjectEnumerator(IntPtr pObject, V8Value.FastResult.Ptr pResult)
        {
            GetFastHostObjectEnumerator(GetHostObject(pObject), pResult);
        }

        public static void GetFastHostObjectEnumerator(object obj, V8Value.FastResult.Ptr pResult)
        {
            ((V8FastHostItem)obj).CreateEnumerator(pResult);
        }

        public static void GetFastHostObjectAsyncEnumerator(IntPtr pObject, V8Value.FastResult.Ptr pResult)
        {
            GetFastHostObjectAsyncEnumerator(GetHostObject(pObject), pResult);
        }

        public static void GetFastHostObjectAsyncEnumerator(object obj, V8Value.FastResult.Ptr pResult)
        {
            ((V8FastHostItem)obj).CreateAsyncEnumerator(pResult);
        }

        #endregion

        #region exception marshaling

        public static object MarshalExceptionToScript(IntPtr pSource, Exception exception)
        {
            return MarshalExceptionToScript(GetHostObject(pSource), exception);
        }

        public static object MarshalExceptionToScript(object source, Exception exception)
        {
            return ((source as IScriptMarshalWrapper)?.Engine ?? ScriptEngine.Current)?.MarshalToScript(exception);
        }

        public static Exception MarshalExceptionToHost(object exception)
        {
            return (Exception)((IScriptMarshalWrapper)exception)?.Engine.MarshalToHost(exception, false);
        }

        #endregion

        #region module support

        public static string LoadModule(IntPtr pSourceDocumentInfo, string specifier, out UniqueDocumentInfo documentInfo, out object exports)
        {
            var engine = ScriptEngine.Current;
            if (engine is not IJavaScriptEngine javaScriptEngine)
            {
                throw new InvalidOperationException("Module loading requires a JavaScript engine");
            }

            var document = engine.DocumentSettings.LoadDocument(((UniqueDocumentInfo)GetHostObject(pSourceDocumentInfo)).Info, specifier, ModuleCategory.Standard, null);
            var code = document.GetTextContents();

            documentInfo = document.Info.MakeUnique(engine);

            var category = document.Info.Category;
            if (category == ModuleCategory.CommonJS)
            {
                javaScriptEngine.CommonJSManager.GetOrCreateModule(documentInfo, code).Process(out exports);
            }
            else if (category == ModuleCategory.Standard)
            {
                exports = null;
            }
            else if (category == DocumentCategory.Json)
            {
                exports = engine.MarshalToScript(javaScriptEngine.JsonModuleManager.GetOrCreateModule(documentInfo, code).Result);
            }
            else
            {
                var uri = document.Info.Uri;
                var name = (uri is not null) ? (uri.IsFile ? uri.LocalPath : uri.AbsoluteUri) : document.Info.Name;
                throw new FileLoadException($"Unsupported document category '{category}'", name);
            }

            return code;
        }

        public static IDictionary<string, object> CreateModuleContext(IntPtr pDocumentInfo)
        {
            var engine = ScriptEngine.Current;
            if (engine is not IJavaScriptEngine)
            {
                throw new InvalidOperationException("Module context construction requires a JavaScript engine");
            }

            var documentInfo = (UniqueDocumentInfo)GetHostObject(pDocumentInfo);

            var callback = documentInfo.ContextCallback ?? engine.DocumentSettings.ContextCallback;
            if ((callback is not null) && MiscHelpers.Try(out var sharedContext, static ctx => ctx.callback(ctx.documentInfo.Info), (callback, documentInfo)) && (sharedContext is not null))
            {
                var context = new Dictionary<string, object>(sharedContext.Count);
                foreach (var pair in sharedContext)
                {
                    context.Add(pair.Key, engine.MarshalToScript(pair.Value));
                }

                return context;
            }

            return null;
        }

        #endregion
    }
}
