// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.Util
{
    internal interface IHolder
    {
        object Value { get; set; }
    }

    internal class Holder<T> : IHolder
    {
        public T Value { get; set; }

        #region IHolder implementation

        object IHolder.Value
        {
            get => Value;
            set => Value = (T)value;
        }

        #endregion
    }
}
