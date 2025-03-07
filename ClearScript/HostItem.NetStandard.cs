// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Expando;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.COM;

namespace Microsoft.ClearScript
{
    internal partial class HostItem
    {
        #region initialization

        private static HostItem Create(ScriptEngine engine, HostTarget target, HostItemFlags flags)
        {
            if (!MiscHelpers.PlatformIsWindows())
            {
                return new HostItem(engine, target, flags);
            }

            return (TargetSupportsExpandoMembers(target, flags) && (engine is IWindowsScriptEngineTag)) ? new DispatchExHostItem(engine, target, flags) : new HostItem(engine, target, flags);
        }

        #endregion

        #region Nested type: DispatchExHostItem

        private sealed class DispatchExHostItem : ExpandoHostItem
        {
            #region data

            private static readonly Dictionary<IntPtr, PatchEntry> patchMap = new();
            private readonly List<Member> expandoMembers = new();

            #endregion

            #region IDispatch[Ex] delegates

            private delegate int RawGetIDsOfNames(
                [In] IntPtr pThis,
                [In] ref Guid iid,
                [In] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)] string[] names,
                [In] uint count,
                [In] int lcid,
                [Out] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] dispids
            );

            private delegate int RawGetDispID(
                [In] IntPtr pThis,
                [In] [MarshalAs(UnmanagedType.BStr)] string name,
                [In] DispatchNameFlags flags,
                [Out] out int dispid
            );

            private delegate int RawDeleteMemberByName(
                [In] IntPtr pThis,
                [In] [MarshalAs(UnmanagedType.BStr)] string name,
                [In] DispatchNameFlags flags
            );

            private delegate int RawDeleteMemberByDispID(
                [In] IntPtr pThis,
                [In] int dispid
            );

            private delegate int RawGetMemberProperties(
                [In] IntPtr pThis,
                [In] int dispid,
                [In] DispatchPropFlags fetchFlags,
                [Out] out DispatchPropFlags flags
            );

            private delegate int RawGetMemberName(
                [In] IntPtr pThis,
                [In] int dispid,
                [Out] [MarshalAs(UnmanagedType.BStr)] out string name
            );

            private delegate int RawGetNextDispID(
                [In] IntPtr pThis,
                [In] DispatchEnumFlags flags,
                [In] int dispidCurrent,
                [Out] out int dispidNext
            );

            #endregion

            #region constructors

            public DispatchExHostItem(ScriptEngine engine, HostTarget target, HostItemFlags flags)
                : base(engine, target, flags)
            {
                EnsurePatched();
            }

            #endregion

            #region internal members

            private IEnumerable<Member> ReflectMembers
            {
                get
                {
                    var members = ThisReflect.GetMembers(GetCommonBindFlags());
                    if (members.Length > 0)
                    {
                        using (var unknownScope = Scope.Create(() => Marshal.GetIUnknownForObject(this), pUnknown => Marshal.Release(pUnknown)))
                        {
                            var pUnknown = unknownScope.Value;
                            using (var dispatchScope = Scope.Create(() => UnknownHelpers.QueryInterface<IDispatch>(pUnknown), pDispatch => Marshal.Release(pDispatch)))
                            {
                                var pDispatch = dispatchScope.Value;
                                var getIDsOfNames = VTableHelpers.GetMethodDelegate<RawGetIDsOfNames>(pDispatch, 5);

                                var iid = Guid.Empty;
                                var names = new string[1];
                                var dispids = new int[1];

                                for (var index = 0; index < members.Length; index++)
                                {
                                    names[0] = members[index].Name;
                                    if (HResult.Succeeded(getIDsOfNames(pDispatch, ref iid, names, 1, 0, dispids)))
                                    {
                                        yield return new Member { Name = names[0], DispID = dispids[0], DispIDName = MiscHelpers.GetDispIDName(dispids[0]) };
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private IExpando ThisExpando => this;

            private void EnsurePatched()
            {
                using (var unknownScope = ScopeFactory.Create(static item => Marshal.GetIUnknownForObject(item), static pUnknown => Marshal.Release(pUnknown), this))
                {
                    var pUnknown = unknownScope.Value;
                    using (var dispatchExScope = ScopeFactory.Create(static pUnknown => UnknownHelpers.QueryInterface<IDispatchEx>(pUnknown), static pDispatchEx => Marshal.Release(pDispatchEx), pUnknown))
                    {
                        lock (VTablePatcher.PatchLock)
                        {
                            var pDispatchEx = dispatchExScope.Value;
                            var pVTable = Marshal.ReadIntPtr(pDispatchEx);

                            if (!patchMap.ContainsKey(pVTable))
                            {
                                var entry = new PatchEntry();

                                var origGetDispID = VTableHelpers.GetMethodDelegate<RawGetDispID>(pDispatchEx, 7);
                                entry.AddDelegate(VTableHelpers.SetMethodDelegate(pDispatchEx, 7, new RawGetDispID((IntPtr pThis, string name, DispatchNameFlags nameFlags, out int dispid) =>
                                {
                                    try
                                    {
                                        return (Marshal.GetObjectForIUnknown(pThis) is DispatchExHostItem item) ? item.GetDispID(name, nameFlags, out dispid) : origGetDispID(pThis, name, nameFlags, out dispid);
                                    }
                                    catch (Exception exception)
                                    {
                                        dispid = SpecialDispIDs.Unknown;
                                        return exception.HResult;
                                    }
                                })));

                                var origDeleteMemberByName = VTableHelpers.GetMethodDelegate<RawDeleteMemberByName>(pDispatchEx, 9);
                                entry.AddDelegate(VTableHelpers.SetMethodDelegate(pDispatchEx, 9, new RawDeleteMemberByName((pThis, name, nameFlags) =>
                                {
                                    try
                                    {
                                        return (Marshal.GetObjectForIUnknown(pThis) is DispatchExHostItem item) ? item.DeleteMemberByName(name, nameFlags) : origDeleteMemberByName(pThis, name, nameFlags);
                                    }
                                    catch (Exception exception)
                                    {
                                        return exception.HResult;
                                    }
                                })));

                                var origDeleteMemberByDispID = VTableHelpers.GetMethodDelegate<RawDeleteMemberByDispID>(pDispatchEx, 10);
                                entry.AddDelegate(VTableHelpers.SetMethodDelegate(pDispatchEx, 10, new RawDeleteMemberByDispID((pThis, dispid) =>
                                {
                                    try
                                    {
                                        return (Marshal.GetObjectForIUnknown(pThis) is DispatchExHostItem item) ? item.DeleteMemberByDispID(dispid) : origDeleteMemberByDispID(pThis, dispid);
                                    }
                                    catch (Exception exception)
                                    {
                                        return exception.HResult;
                                    }
                                })));

                                var origGetMemberProperties = VTableHelpers.GetMethodDelegate<RawGetMemberProperties>(pDispatchEx, 11);
                                entry.AddDelegate(VTableHelpers.SetMethodDelegate(pDispatchEx, 11, new RawGetMemberProperties((IntPtr pThis, int dispid, DispatchPropFlags fetchFlags, out DispatchPropFlags propFlags) =>
                                {
                                    try
                                    {
                                        var item = Marshal.GetObjectForIUnknown(pThis) as DispatchExHostItem;
                                        if (item is null)
                                        {
                                            return origGetMemberProperties(pThis, dispid, fetchFlags, out propFlags);
                                        }

                                        var result = item.GetMemberProperties(dispid, fetchFlags, out propFlags);
                                        if (result == HResult.DISP_E_MEMBERNOTFOUND)
                                        {
                                            return origGetMemberProperties(pThis, dispid, fetchFlags, out propFlags);
                                        }

                                        return result;
                                    }
                                    catch (Exception exception)
                                    {
                                        propFlags = 0;
                                        return exception.HResult;
                                    }
                                })));

                                var origGetMemberName = VTableHelpers.GetMethodDelegate<RawGetMemberName>(pDispatchEx, 12);
                                entry.AddDelegate(VTableHelpers.SetMethodDelegate(pDispatchEx, 12, new RawGetMemberName((IntPtr pThis, int dispid, out string name) =>
                                {
                                    try
                                    {
                                        return (Marshal.GetObjectForIUnknown(pThis) is DispatchExHostItem item) ? item.GetMemberName(dispid, out name) : origGetMemberName(pThis, dispid, out name);
                                    }
                                    catch (Exception exception)
                                    {
                                        name = null;
                                        return exception.HResult;
                                    }
                                })));

                                var origGetNextDispID = VTableHelpers.GetMethodDelegate<RawGetNextDispID>(pDispatchEx, 13);
                                entry.AddDelegate(VTableHelpers.SetMethodDelegate(pDispatchEx, 13, new RawGetNextDispID((IntPtr pThis, DispatchEnumFlags enumFlags, int dispidCurrent, out int dispidNext) =>
                                {
                                    try
                                    {
                                        return (Marshal.GetObjectForIUnknown(pThis) is DispatchExHostItem item) ? item.GetNextDispID(dispidCurrent, out dispidNext) : origGetNextDispID(pThis, enumFlags, dispidCurrent, out dispidNext);
                                    }
                                    catch (Exception exception)
                                    {
                                        dispidNext = SpecialDispIDs.Unknown;
                                        return exception.HResult;
                                    }
                                })));

                                patchMap.Add(pVTable, entry);
                                Debug.Assert(patchMap.Count < 16);
                            }
                        }
                    }
                }
            }

            #endregion

            #region HostItem overrides

            protected override string AdjustInvokeName(string name)
            {
                var member = expandoMembers.FirstOrDefault(expandoMember => name == expandoMember.DispIDName);
                if (!member.IsDefault)
                {
                    return member.Name;
                }

                return name;
            }

            protected override void AddExpandoMemberName(string name)
            {
                var dispid = 1;

                var dispids = ReflectMembers.Concat(expandoMembers).Select(member => member.DispID).ToArray();
                for (; dispids.Contains(dispid); dispid++)
                {
                    if (dispid >= int.MaxValue)
                    {
                        throw new NotSupportedException("The object cannot support additional dynamic members");
                    }
                }

                base.AddExpandoMemberName(name);
                expandoMembers.Add(new Member { Name = name, DispID = dispid, DispIDName = MiscHelpers.GetDispIDName(dispid) });
            }

            protected override void RemoveExpandoMemberName(string name)
            {
                base.RemoveExpandoMemberName(name);

                var index = expandoMembers.FindIndex(member => member.Name == name);
                if (index >= 0)
                {
                    expandoMembers.RemoveAt(index);
                }
            }

            #endregion

            #region IDispatchEx patches

            private int GetDispID(string name, DispatchNameFlags nameFlags, out int dispid)
            {
                var nameComparison = nameFlags.HasAllFlags(DispatchNameFlags.CaseInsensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                var member = ReflectMembers.Concat(expandoMembers).FirstOrDefault(testMember => string.Equals(testMember.Name, name, nameComparison));
                if (!member.IsDefault)
                {
                    dispid = member.DispID;
                    return HResult.S_OK;
                }

                if (nameFlags.HasAllFlags(DispatchNameFlags.Ensure))
                {
                    ThisExpando.AddProperty(name);
                    return GetDispID(name, nameFlags & ~DispatchNameFlags.Ensure, out dispid);
                }

                dispid = SpecialDispIDs.Unknown;
                return HResult.DISP_E_UNKNOWNNAME;
            }

            private int DeleteMemberByName(string name, DispatchNameFlags nameFlags)
            {
                var nameComparison = nameFlags.HasAllFlags(DispatchNameFlags.CaseInsensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                var member = ReflectMembers.Concat(expandoMembers).FirstOrDefault(testMember => string.Equals(testMember.Name, name, nameComparison));
                if (!member.IsDefault && RemoveMember(member.Name))
                {
                    return HResult.S_OK;
                }

                // IDispatchEx specifies S_FALSE, but DISP_E_UNKNOWNNAME provides parity with .NET Framework CCWs
                return HResult.DISP_E_UNKNOWNNAME;
            }

            private int DeleteMemberByDispID(int dispid)
            {
                var member = ReflectMembers.Concat(expandoMembers).FirstOrDefault(testMember => testMember.DispID == dispid);
                if (!member.IsDefault && RemoveMember(member.Name))
                {
                    return HResult.S_OK;
                }

                // IDispatchEx specifies S_FALSE, but DISP_E_UNKNOWNNAME provides parity with .NET Framework CCWs
                return HResult.DISP_E_UNKNOWNNAME;
            }

            private int GetMemberProperties(int dispid, DispatchPropFlags fetchFlags, out DispatchPropFlags propFlags)
            {
                var member = expandoMembers.FirstOrDefault(testMember => testMember.DispID == dispid);
                if (!member.IsDefault)
                {
                    propFlags =
                        DispatchPropFlags.CanGet |
                        DispatchPropFlags.CanPut |
                        DispatchPropFlags.CannotPutRef |
                        DispatchPropFlags.CannotCall |
                        DispatchPropFlags.CannotConstruct |
                        DispatchPropFlags.CannotSourceEvents;

                    propFlags = propFlags & fetchFlags;
                    return HResult.S_OK;
                }

                propFlags = 0;
                return HResult.DISP_E_MEMBERNOTFOUND;
            }

            private int GetMemberName(int dispid, out string name)
            {
                var member = ReflectMembers.Concat(expandoMembers).OrderBy(testMember => testMember.DispID).FirstOrDefault(testMember => testMember.DispID == dispid);
                if (!member.IsDefault)
                {
                    name = member.Name;
                    return HResult.S_OK;
                }

                name = null;
                return HResult.S_FALSE;
            }

            private int GetNextDispID(int dispidCurrent, out int dispidNext)
            {
                var member = ReflectMembers.Concat(expandoMembers).OrderBy(testMember => testMember.DispID).FirstOrDefault(testMember => testMember.DispID > dispidCurrent);
                if (!member.IsDefault)
                {
                    dispidNext = member.DispID;
                    return HResult.S_OK;
                }

                dispidNext = SpecialDispIDs.Default;
                return HResult.S_FALSE;
            }

            #endregion

            #region Nested type: PatchEntry

            private sealed class PatchEntry
            {
                // ReSharper disable once CollectionNeverQueried.Local
                private readonly List<Delegate> delegates = new();

                public void AddDelegate(Delegate del)
                {
                    delegates.Add(del);
                }
            }

            #endregion

            #region Nested type: Member

            private struct Member
            {
                public string Name;
                public int DispID;
                public string DispIDName;

                public bool IsDefault => DispID == SpecialDispIDs.Default;
            }

            #endregion
        }

        #endregion
    }
}
