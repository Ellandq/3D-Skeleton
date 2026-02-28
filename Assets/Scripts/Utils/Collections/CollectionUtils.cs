using System.Collections.Generic;
using System.Linq;

namespace Utils.Collections
{
    public class CollectionUtils
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
    }
}