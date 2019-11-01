// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using Tensorflow;

namespace AML.Client
{
    public static class TensorExtensions
    {
        public static T Convert<T>(this TensorProto tensor) where T : class
        {
            var shape = tensor.TensorShape;
            var dimCount = shape.Dim.Count;

            var resultType = typeof(T);

            if (!resultType.IsArray)
            {
                throw new Exception("Unable to convert tensor into scalar type");
            }

            var arrayRank = typeof(T).GetArrayRank();

            if (arrayRank != dimCount)
            {
                throw new Exception($"result tensor was not the expected rank {arrayRank} - was rank {dimCount}");
            }

            var elementType = resultType.GetElementType();

            Func<TensorProto, int, object> getItemFunc = null;

            if (elementType == typeof(float))
            {
                getItemFunc = (t, i) => t.FloatVal[i];
            }

            if (getItemFunc == null)
            {
                throw new Exception($"Don't know how to handle type {elementType}");
            }

            var dimSizes = shape.Dim.Select(d => (int)d.Size).ToArray();
            var sysArray = Array.CreateInstance(elementType, dimSizes);
            var tensorIndex = 0;

            foreach (var dimArray in GetPermutations(dimSizes))
            {
                sysArray.SetValue(getItemFunc(tensor, tensorIndex), dimArray);
                tensorIndex++;
            }

            return sysArray as T;
        }

        public static IEnumerable<int[]> GetPermutations(this int[] maxValues)
        {
            return GetPermutations(new int[maxValues.Length], 0, maxValues);
        }

        private static IEnumerable<int[]> GetPermutations(int[] values, int index, int[] maxValues)
        {
            if (index >= values.Length)
            {
                return new[] { values };
            }

            var result = new List<int[]>();

            for (var i = 0; i < maxValues[index]; i++)
            {
                var currentValues = values.ToArray();
                currentValues[index] = i;
                result.AddRange(GetPermutations(currentValues, index + 1, maxValues));
            }

            return result;
        }
    }
}
