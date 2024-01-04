using System.Collections;
using System.Collections.Generic;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains many utility functions for working with dictionaries
    /// </summary>
    public static class DictionaryUtilities
	{
        /// <summary>
        /// Tries to get a value of a certain type
        /// </summary>
        /// <typeparam name="TCast">The type of value to get</typeparam>
        /// <param name="dictionary">The dictionary to try on</param>
        /// <param name="key">The key of the value</param>
        /// <param name="value">The output value if successful</param>
        /// <returns>Returns true if a value of the specified type exists at the key</returns>
        public static bool TryGetValueOfType<TCast>(this IDictionary dictionary, object key, out TCast value)
        {
            return TryGetValueOfType(dictionary, key, out value, default);
        }

        /// <summary>
        /// Tries to get a value of a certain type
        /// </summary>
        /// <typeparam name="TCast">The type of value to get</typeparam>
        /// <param name="dictionary">The dictionary to try on</param>
        /// <param name="key">The key of the value</param>
        /// <param name="value">The output value if successful</param>
        /// <param name="defaultVal">The default value to use if unsuccessful</param>
        /// <returns>Returns true if a value of the specified type exists at the key. Uses the <paramref name="defaultVal"/> if unsuccessful</returns>
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
