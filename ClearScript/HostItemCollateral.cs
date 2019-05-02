// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal sealed class HostItemCollateral
    {
        #region special targets

        public readonly CollateralObject<IDynamic> TargetDynamic = new CollateralObject<IDynamic>();
        public readonly CollateralObject<IPropertyBag> TargetPropertyBag = new CollateralObject<IPropertyBag>();
        public readonly CollateralObject<IHostList> TargetList = new CollateralObject<IHostList>();
        public readonly CollateralObject<DynamicMetaObject> TargetDynamicMetaObject = new CollateralObject<DynamicMetaObject>();
        public readonly CollateralObject<IEnumerator> TargetEnumerator = new CollateralObject<IEnumerator>();

        #endregion

        #region dynamic collateral

        public readonly CollateralObject<HashSet<string>> ExpandoMemberNames = new CollateralObject<HashSet<string>>();
        public readonly CollateralObject<ListDataFields> ListData = new CollateralObject<ListDataFields>();

        #endregion

        #region  tear-off member cache

        public readonly CollateralObject<Dictionary<string, HostMethod>> HostMethodMap = new CollateralObject<Dictionary<string, HostMethod>>();
        public readonly CollateralObject<Dictionary<string, HostIndexedProperty>> HostIndexedPropertyMap = new CollateralObject<Dictionary<string, HostIndexedProperty>>();

        #endregion

        #region Nested type: CollateralObject<T>

        public class CollateralObject<T> : CollateralObject<HostItem, T> where T : class
        {
        }

        #endregion

        #region Nested type: ListDataFields

        public class ListDataFields
        {
            public int[] PropertyIndices;
            public int CachedCount;
        }

        #endregion 
    }
}
