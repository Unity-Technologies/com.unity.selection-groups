using System;
using System.Collections.Generic;

namespace Unity.FilmInternalUtilities {

//[TODO-sin: 2022-1-6] Move to FilmInternalUtilities
internal static class ListExtensions {

    internal static void Move<T>(this List<T> list, int oldIndex, int newIndex) {
        if (oldIndex == newIndex)
            return;
                
        T item = list[oldIndex];
        list.RemoveAt(oldIndex);
        list.Insert(newIndex, item);
    }

    internal static bool AreElementsEqual<T>(this List<T> list, IList<T> otherList) where T: IComparable{
        if (null == otherList || list.Count != otherList.Count)
            return false;

        int numElements = list.Count;
        for (int i = 0; i < numElements; ++i) {            
            if (!list[i].Equals(otherList[i]))
                return false;
        }        

        return true;

    }
    
}

} //

