using System.Collections;
using System.Collections.Generic;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains many utility functions for working with dictionaries
    /// </summary>
    public static class DictionaryUtilities
	{
        public static bool TryGetValueOfType<TCast>(this IDictionary dictionary, object key, out TCast value)
        {
            return TryGetValueOfType(dictionary, key, out value, default);
        }

        public static bool TryGetValueOfType<TCast>(this IDictionary dictionary, object key, out TCast value, TCast defaultVal)
        {
            if (dictionary.Contains(key))
            {
                var dictVal = dictionary[key];

                if (dictVal is TCast castedValue)
                {
                    value = castedValue;
                    return true;
                }
            }

            value = defaultVal;
            return false;
        }
    }
}
