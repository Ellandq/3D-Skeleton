using System.Collections.Generic;
using System.Linq;

namespace Utils.Collections
{
    public static class CollectionUtils
    {
        public static void CompareListAndDictionary<TKey, TValue>(
            Dictionary<TKey, TValue> dict,
            List<TKey> list,
            out List<TKey> onlyInList,
            out List<TKey> onlyInDict
        )
        {
            var dictKeys = dict.Keys.ToHashSet();

            onlyInList = list.Where(item => !dictKeys.Contains(item)).ToList();

            var listSet = list.ToHashSet();
            onlyInDict = dictKeys.Where(key => !listSet.Contains(key)).ToList();
        }
        
        public static void CompareCollections<T>(
            IEnumerable<T> first,
            IEnumerable<T> second,
            out List<T> onlyInFirst,
            out List<T> onlyInSecond
        )
        {
            var firstSet = first.ToHashSet();
            var secondSet = second.ToHashSet();

            onlyInFirst = firstSet
                .Where(item => !secondSet.Contains(item))
                .ToList();

            onlyInSecond = secondSet
                .Where(item => !firstSet.Contains(item))
                .ToList();
        }
    }
}