using System.Collections.Generic;
using UnityEngine;

namespace OnlineFPS
{
    public static class ListExtensions
    {
        public static T GetLastElement<T>(this IList<T> list) =>
            list[list.Count - 1];

        public static void RemoveLast<T>(this IList<T> list) =>
            list.RemoveAt(list.Count - 1);
    }

    public static class ArrayExtensions
    {
        public static T GetLastElement<T>(this T[] arr) =>
            arr[arr.Length - 1];
    }

    public static class StringExtensions
    {
        public static (int result, bool canParse) TryParseToInt(this string st)
        {
            int res;
            var valid = int.TryParse(st, out res);
            return (result: res, canParse: valid);
        }
    }

    public static class MathExtensions
    {
        public static bool Approximately(this Quaternion q1, Quaternion q2, float acceptableRange)
        {
            return 1 - Mathf.Abs(Quaternion.Dot(q1, q2)) < acceptableRange;
        }

        public static bool Approximately(this Vector3 v1, Vector3 v2, float acceptableRange)
        {
            Vector3 difference = v1 - v2;

            return (difference.x * difference.x + difference.y * difference.y + difference.z * difference.z) < acceptableRange;
        }
    }
}