// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.ClearScript.Util.COM
{
    internal sealed class DynamicDispatchWrapper : IDynamic
    {
        private readonly HostItem hostItem;
        private readonly IDispatch dispatch;
        private IReadOnlyList<DispatchMember> members;

        public DynamicDispatchWrapper(HostItem hostItem, IDispatch dispatch)
        {
            this.hostItem = hostItem;
            this.dispatch = dispatch;
        }

        private IReadOnlyList<DispatchMember> GetMembers()
        {
            return members ?? (members = dispatch.GetMembers().ToArray());
        }

        public object GetProperty(string name, params object[] args)
        {
            return GetProperty(name, out _, args);
        }

        public object GetProperty(string name, out bool isCacheable, params object[] args)
        {
            isCacheable = false;

            DispatchMember member = null;
            if (args.Length < 1)
            {
                // some objects crash on attempt to retrieve a method as a property

                member = GetMembers().FirstOrDefault(testMember => testMember.Name == name);
                if ((member is not null) && member.DispatchFlags == DispatchFlags.Method)
                {
                    return new HostMethod(hostItem, name);
                }
            }

            try
            {
                var result = dispatch.GetProperty(name, args);
                return result;
            }
            catch
            {
                if (args.Length < 1)
                {
                    if (member is null)
                    {
                        member = GetMembers().FirstOrDefault(testMember => testMember.Name == name);
                    }

                    if ((member is not null) && !member.DispatchFlags.HasAllFlags(DispatchFlags.Method))
                    {
                        return new HostIndexedProperty(hostItem, name);
                    }

                    return new HostMethod(hostItem, name);
                }

                throw;
            }
        }

        public void SetProperty(string name, params object[] args)
        {
            dispatch.SetProperty(name, args);
        }

        public bool DeleteProperty(string name)
        {
            throw new NotSupportedException("The object does not support dynamic properties");
        }

        public string[] GetPropertyNames()
        {
            return GetMembers().Select(member => member.Name).ExcludeIndices().ToArray();
        }

        public object GetProperty(int index)
        {
            return dispatch.GetProperty(index.ToString(CultureInfo.InvariantCulture));
        }

        public void SetProperty(int index, object value)
        {
            dispatch.SetProperty(index.ToString(CultureInfo.InvariantCulture), value);
        }

        public bool DeleteProperty(int index)
        {
            throw new NotSupportedException("The object does not support dynamic properties");
        }

        public int[] GetPropertyIndices()
        {
            return GetMembers().Select(member => member.Name).GetIndices().ToArray();
        }

        public object Invoke(bool asConstructor, params object[] args)
        {
            if (asConstructor)
            {
                throw new NotSupportedException("The object does not support constructor invocation");
            }

            return dispatch.Invoke(args);
        }

        public object InvokeMethod(string name, params object[] args)
        {
            return dispatch.InvokeMethod(name, args);
        }
    }

    internal sealed class DynamicDispatchExWrapper : IDynamic
    {
        private readonly HostItem hostItem;
        private readonly IDispatchEx dispatchEx;

        public DynamicDispatchExWrapper(HostItem hostItem, IDispatchEx dispatchEx)
        {
            this.hostItem = hostItem;
            this.dispatchEx = dispatchEx;
        }

        public object GetProperty(string name, params object[] args)
        {
            return GetProperty(name, out _, args);
        }

        public object GetProperty(string name, out bool isCacheable, params object[] args)
        {
            isCacheable = false;

            DispatchMember member = null;
            if (args.Length < 1)
            {
                // some objects crash on attempt to retrieve a method as a property

                member = dispatchEx.GetMembers().FirstOrDefault(testMember => testMember.Name == name);
                if ((member is not null) && member.DispatchFlags == DispatchFlags.Method)
                {
                    return new HostMethod(hostItem, name);
                }
            }

            try
            {
                var result = dispatchEx.GetProperty(name, false, args);
                return result;
            }
            catch
            {
                if (args.Length < 1)
                {
                    if (member is null)
                    {
                        member = dispatchEx.GetMembers().FirstOrDefault(testMember => testMember.Name == name);
                    }

                    if ((member is not null) && !member.DispatchFlags.HasAllFlags(DispatchFlags.Method))
                    {
                        return new HostIndexedProperty(hostItem, name);
                    }

                    return new HostMethod(hostItem, name);
                }

                throw;
            }
        }

        public void SetProperty(string name, params object[] args)
        {
            dispatchEx.SetProperty(name, false, args);
        }

        public bool DeleteProperty(string name)
        {
            return dispatchEx.DeleteProperty(name, false);
        }

        public string[] GetPropertyNames()
        {
            return dispatchEx.GetPropertyNames().ExcludeIndices().ToArray();
        }

        public object GetProperty(int index)
        {
            return dispatchEx.GetProperty(index.ToString(CultureInfo.InvariantCulture), false);
        }

        public void SetProperty(int index, object value)
        {
            dispatchEx.SetProperty(index.ToString(CultureInfo.InvariantCulture), false, value);
        }

        public bool DeleteProperty(int index)
        {
            return dispatchEx.DeleteProperty(index.ToString(CultureInfo.InvariantCulture), false);
        }

        public int[] GetPropertyIndices()
        {
            return dispatchEx.GetPropertyNames().GetIndices().ToArray();
        }

        public object Invoke(bool asConstructor, params object[] args)
        {
            return dispatchEx.Invoke(asConstructor, args);
        }

        public object InvokeMethod(string name, params object[] args)
        {
            return dispatchEx.InvokeMethod(name, false, args);
        }
    }
}
