// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;

namespace System.Runtime.InteropServices.Expando
{
    internal interface IExpando : IReflect
    {
        FieldInfo AddField(string name);
        PropertyInfo AddProperty(string name);
        MethodInfo AddMethod(string name, Delegate method);
        void RemoveMember(MemberInfo member);
    }
}
