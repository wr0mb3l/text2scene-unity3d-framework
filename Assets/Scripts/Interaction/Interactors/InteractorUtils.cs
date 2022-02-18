using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InteractorUtils
{
    // Sorts a List in order based on a comparer.
    public static void Sort<T>(IList<T> hits, IComparer<T> comparer) where T : struct
    {
        bool flag;
        do
        {
            flag = true;
            for (int i = 1; i < hits.Count; i++)
            {
                if (comparer.Compare(hits[i - 1], hits[i]) > 0)
                {
                    T value = hits[i - 1];
                    hits[i - 1] = hits[i];
                    hits[i] = value;
                    flag = false;
                }
            }
        }
        while (!flag);
    }

    // Returns next entry in an enum. If last element is given, return first.
    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }

    // Returns previous entry in an enum. If first element is given, return last.
    public static T Previous<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) - 1;
        return (-1 == j) ? Arr[Arr.Length - 1] : Arr[j];
    }
}
