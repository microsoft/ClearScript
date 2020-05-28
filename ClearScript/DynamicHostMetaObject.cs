// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Dynamic;

namespace Microsoft.ClearScript
{
    internal sealed class DynamicHostMetaObject : DynamicMetaObject
    {
        private readonly IDynamicMetaObjectProvider metaObjectProvider;
        private readonly DynamicMetaObject metaObject;

        public DynamicHostMetaObject(IDynamicMetaObjectProvider metaObjectProvider, DynamicMetaObject metaObject)
            : base(metaObject.Expression, metaObject.Restrictions, metaObject.Value)
        {
            this.metaObjectProvider = metaObjectProvider;
            this.metaObject = metaObject;
        }

        public bool HasMember(string name, bool ignoreCase)
        {
            return DynamicHostObject.HasMember(metaObjectProvider, metaObject, name, ignoreCase);
        }

        #region DynamicMetaObject overrides

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return metaObject.GetDynamicMemberNames();
        }

        public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
        {
            return metaObject.BindBinaryOperation(binder, arg);
        }

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            return metaObject.BindConvert(binder);
        }

        public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
        {
            return metaObject.BindCreateInstance(binder, args);
        }

        public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return metaObject.BindDeleteIndex(binder, indexes);
        }

        public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
        {
            return metaObject.BindDeleteMember(binder);
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return metaObject.BindGetIndex(binder, indexes);
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            return metaObject.BindGetMember(binder);
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        {
            return metaObject.BindInvoke(binder, args);
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            return metaObject.BindInvokeMember(binder, args);
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            return metaObject.BindSetIndex(binder, indexes, value);
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            return metaObject.BindSetMember(binder, value);
        }

        public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
        {
            return metaObject.BindUnaryOperation(binder);
        }

        #endregion
    }
}
