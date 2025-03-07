// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.ClearScript.Util
{
    internal static class ArrayHelpers
    {
        public static void Iterate(this Array array, Action<int[]> action)
        {
            if (array.Rank > 0)
            {
                var dimensions = Enumerable.Range(0, array.Rank);
                if (dimensions.Aggregate(1, (count, dimension) => count * array.GetLength(dimension)) > 0)
                {
                    Iterate(array, new int[array.Rank], 0, action);
                }
            }
        }

        private static void Iterate(Array array, int[] indices, int dimension, Action<int[]> action)
        {
            if (dimension >= indices.Length)
            {
                action(indices);
            }
            else
            {
                var lowerBound = array.GetLowerBound(dimension);
                var upperBound = array.GetUpperBound(dimension);
                for (var index = lowerBound; index <= upperBound; index++)
                {
                    indices[dimension] = index;
                    Iterate(array, indices, dimension + 1, action);
                }
            }
        }

        public static T[] GetEmptyArray<T>()
        {
            return EmptyArray<T>.Value;
        }

        public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this T[] array)
        {
            foreach (var item in array)
            {
                await Task.CompletedTask;
                yield return item;
            }
        }

        #region Nested type: EmptyArray<T>

        private static class EmptyArray<T>
        {
            public static readonly T[] Value = new T[0];
        }

        #endregion
    }
}
