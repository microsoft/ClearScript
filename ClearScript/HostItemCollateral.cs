// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal sealed class HostItemCollateral
    {
        #region special targets

        public readonly CollateralObject<IDynamic> TargetDynamic = new();
        public readonly CollateralObject<IPropertyBag> TargetPropertyBag = new();
        public readonly CollateralObject<IHostList> TargetList = new();
        public readonly CollateralObject<DynamicHostMetaObject> TargetDynamicMetaObject = new();
        public readonly CollateralObject<IEnumerator> TargetEnumerator = new();

        #endregion

        #region dynamic collateral

        public readonly CollateralObject<HashSet<string>> ExpandoMemberNames = new();
        public readonly CollateralObject<ListDataFields> ListData = new();

        #endregion

        #region  tear-off member cache

        public readonly CollateralObject<Dictionary<string, HostMethod>> HostMethodMap = new();
        public readonly CollateralObject<Dictionary<string, HostIndexedProperty>> HostIndexedPropertyMap = new();

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
