// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.Expando;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.COM;

namespace Microsoft.ClearScript
{
    internal partial class HostItem
    {
        #region data

        internal static bool EnableVTablePatching;
        [ThreadStatic] private static bool bypassVTablePatching;

        #endregion

        #region initialization

        private static bool TargetSupportsExpandoMembers(HostTarget target, HostItemFlags flags)
        {
            if (!TargetSupportsSpecialTargets(target))
            {
                return false;
            }

            if (typeof(IDynamic).IsAssignableFrom(target.Type))
            {
                return true;
            }

            if (target is IHostVariable)
            {
                if (target.Type.IsImport)
                {
                    return true;
                }
            }
            else
            {
                if ((target.InvokeTarget is IDispatchEx dispatchEx) && dispatchEx.GetType().IsCOMObject)
                {
                    return true;
                }
            }

            if (typeof(IPropertyBag).IsAssignableFrom(target.Type))
            {
                return true;
            }

            if (!flags.HasFlag(HostItemFlags.HideDynamicMembers) && typeof(IDynamicMetaObjectProvider).IsAssignableFrom(target.Type))
            {
                return true;
            }

            return false;
        }

        private bool CanAddExpandoMembers()
        {
            return (TargetDynamic != null) || ((TargetPropertyBag != null) && !TargetPropertyBag.IsReadOnly) || (TargetDynamicMetaObject != null);
        }

        #endregion

        #region ICustomQueryInterface implementation

        public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr pInterface)
        {
            if (iid == typeof(IEnumVARIANT).GUID)
            {
                if ((Target is HostObject) || (Target is IHostVariable) || (Target is IByRefArg))
                {
                    pInterface = IntPtr.Zero;
                    return BindSpecialTarget(Collateral.TargetEnumerator) ? CustomQueryInterfaceResult.NotHandled : CustomQueryInterfaceResult.Failed;
                }
            }
            else if (iid == typeof(IDispatchEx).GUID)
            {
                if (EnableVTablePatching && !bypassVTablePatching)
                {
                    var pUnknown = Marshal.GetIUnknownForObject(this);

                    bypassVTablePatching = true;
                    pInterface = UnknownHelpers.QueryInterfaceNoThrow<IDispatchEx>(pUnknown);
                    bypassVTablePatching = false;

                    Marshal.Release(pUnknown);

                    if (pInterface != IntPtr.Zero)
                    {
                        VTablePatcher.GetInstance().PatchDispatchEx(pInterface);
                        return CustomQueryInterfaceResult.Handled;
                    }
                }
            }

            pInterface = IntPtr.Zero;
            return CustomQueryInterfaceResult.NotHandled;
        }

        #endregion

        #region Nested type: ExpandoHostItem

        private class ExpandoHostItem : HostItem, IExpando
        {
            #region constructors

            // ReSharper disable MemberCanBeProtected.Local

            public ExpandoHostItem(ScriptEngine engine, HostTarget target, HostItemFlags flags)
                : base(engine, target, flags)
            {
            }

            // ReSharper restore MemberCanBeProtected.Local

            #endregion

            #region IExpando implementation

            FieldInfo IExpando.AddField(string name)
            {
                return HostInvoke(() =>
                {
                    if (CanAddExpandoMembers())
                    {
                        AddExpandoMemberName(name);
                        return MemberMap.GetField(name);
                    }

                    throw new NotSupportedException("The object does not support dynamic fields");
                });
            }

            PropertyInfo IExpando.AddProperty(string name)
            {
                return HostInvoke(() =>
                {
                    if (CanAddExpandoMembers())
                    {
                        AddExpandoMemberName(name);
                        return MemberMap.GetProperty(name);
                    }

                    throw new NotSupportedException("The object does not support dynamic properties");
                });
            }

            MethodInfo IExpando.AddMethod(string name, Delegate method)
            {
                throw new NotImplementedException();
            }

            void IExpando.RemoveMember(MemberInfo member)
            {
                RemoveMember(member.Name);
            }

            protected virtual bool RemoveMember(string name)
            {
                return HostInvoke(() =>
                {
                    if (TargetDynamic != null)
                    {
                        if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                        {
                            if (TargetDynamic.DeleteProperty(index))
                            {
                                RemoveExpandoMemberName(index.ToString(CultureInfo.InvariantCulture));
                                return true;
                            }
                        }
                        else if (TargetDynamic.DeleteProperty(name))
                        {
                            RemoveExpandoMemberName(name);
                            return true;
                        }
                    }
                    else if (TargetPropertyBag != null)
                    {
                        if (TargetPropertyBag.Remove(name))
                        {
                            RemoveExpandoMemberName(name);
                            return true;
                        }
                    }
                    else if (TargetDynamicMetaObject != null)
                    {
                        if (TargetDynamicMetaObject.TryDeleteMember(name, out var result) && result)
                        {
                            RemoveExpandoMemberName(name);
                            return true;
                        }

                        if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) && TargetDynamicMetaObject.TryDeleteIndex(new object[] { index }, out result))
                        {
                            RemoveExpandoMemberName(index.ToString(CultureInfo.InvariantCulture));
                            return true;
                        }

                        if (TargetDynamicMetaObject.TryDeleteIndex(new object[] { name }, out result))
                        {
                            RemoveExpandoMemberName(name);
                            return true;
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("The object does not support dynamic members");
                    }

                    return false;
                });
            }

            #endregion
        }

        #endregion
    }
}
