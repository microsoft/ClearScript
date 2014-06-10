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

using System;
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
            return ((IDynamic)obj).GetProperty(name);
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

        public static unsafe object InvokeHostObject(void* pObject, object[] args, bool asConstructor)
        {
            return InvokeHostObject(GetHostObject(pObject), args, asConstructor);
        }

        public static object InvokeHostObject(object obj, object[] args, bool asConstructor)
        {
            return ((IDynamic)obj).Invoke(args, asConstructor);
        }

        public static unsafe object InvokeHostObjectMethod(void* pObject, string name, object[] args)
        {
            return InvokeHostObjectMethod(GetHostObject(pObject), name, args);
        }

        public static object InvokeHostObjectMethod(object obj, string name, object[] args)
        {
            return ((IDynamic)obj).InvokeMethod(name, args);
        }

        #endregion

        #region V8 object cache

        public static unsafe void* CreateV8ObjectCache()
        {
            var cache = new Dictionary<object, IntPtr>();
            return AddRefHostObject(cache);
        }

        public static unsafe void CacheV8Object(void* pV8ObjectCache, void* pObject, void* pV8Object)
        {
            var cache = (Dictionary<object, IntPtr>)GetHostObject(pV8ObjectCache);
            cache.Add(GetHostObject(pObject), (IntPtr)pV8Object);
        }

        public static unsafe void* GetCachedV8Object(void* pV8ObjectCache, void* pObject)
        {
            IntPtr pV8Object;
            var cache = (Dictionary<object, IntPtr>)GetHostObject(pV8ObjectCache);
            return cache.TryGetValue(GetHostObject(pObject), out pV8Object) ? pV8Object.ToPointer() : null;
        }

        public static unsafe IntPtr[] GetAllCachedV8Objects(void* pV8ObjectCache)
        {
            var cache = (Dictionary<object, IntPtr>)GetHostObject(pV8ObjectCache);
            return cache.Values.ToArray();
        }

        public static unsafe bool RemoveV8ObjectCacheEntry(void* pV8ObjectCache, void* pObject)
        {
            var cache = (Dictionary<object, IntPtr>)GetHostObject(pV8ObjectCache);
            return cache.Remove(GetHostObject(pObject));
        }

        #endregion
    }
}
