﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains some utility functions related to strings
	/// </summary>
	public static class StringUtilities
	{
		/// <summary>
		/// Makes a string look nicer (example: converts "thisIsATest123" to "This is a test 123"
		/// </summary>
		/// <param name="input">The input string</param>
		/// <returns>The prettified string</returns>
		public static string Prettify(string input)
		{
			StringBuilder builder = new StringBuilder(input);
			if (builder.Length > 0)
			{
				if (char.IsLower(builder[0]))
				{
					builder[0] = char.ToUpper(builder[0]);
				}
				builder.Replace("_","");

				var output = builder.ToString();

				output = Regex.Replace(output, @"([a-z])([A-Z])", "$1 $2");
				output = Regex.Replace(output, @"([a-zA-Z])([A-Z])([a-z])", "$1 $2$3");

				return output;
			}

			return input;
		}

		/// <summary>
		/// Adds page markers between strings so it can be used in a dialogue conversation
		/// </summary>
		/// <param name="strings">The strings to pagify</param>
		/// <returns>Returns the final string with page separators</returns>
        public static string Pagify(params string[] strings)
        {
			return Pagify((IEnumerable<string>)strings);
        }

        /// <summary>
        /// Adds page markers between strings so it can be used in a dialogue conversation
        /// </summary>
        /// <param name="strings">The strings to pagify</param>
        /// <returns>Returns the final string with page separators</returns>
        public static string Pagify(IEnumerable<string> strings)
		{
			const string page = "<page>";

            StringBuilder builder = new StringBuilder();

			foreach (var str in strings)
			{
				builder.Append(str);
				builder.Append(page);
			}

			builder.Length -= page.Length;
			return builder.ToString();
        }

		/// <summary>
		/// All &lt;/br&gt; statements get replaced with new-line characters. You can also use &lt;/br:10&gt; for multiple lines (this creates 10 new-lines for example)
		/// </summary>
		public static string AddNewLines(string input)
		{
			StringBuilder result = new StringBuilder(input);
			var breakMatches = Regex.Matches(input, @"(<\/br\:?(\d+?)?>)");
			for (int i = breakMatches.Count - 1; i >= 0; i--)
			{
				var match = breakMatches[i];
				if (match.Success)
				{
					int breakCount = 1;
					if (match.Groups.Count >= 3 && match.Groups[2].Success)
					{
						if (!int.TryParse(match.Groups[2].Value,out breakCount))
						{
							continue;
						}
					}
					result.Remove(match.Index, match.Length);
					result.Insert(match.Index, new string('\n', breakCount));
				}
			}
			return result.ToString();
		}

		/// <summary>
		/// All &lt;/sp&gt; statements get replaced with space characters. You can also use &lt;/sp:10&gt; for multiple spaces (this creates 10 new spaces for example)
		/// </summary>
		public static string AddSpaces(string input)
		{
			StringBuilder result = new StringBuilder(input);
			var breakMatches = Regex.Matches(input, @"(<\/sp\:?(\d+?)?>)");
			for (int i = breakMatches.Count - 1; i >= 0; i--)
			{
				var match = breakMatches[i];
				if (match.Success)
				{
					int breakCount = 1;
					if (match.Groups.Count >= 3 && match.Groups[2].Success)
					{
						if (!int.TryParse(match.Groups[2].Value, out breakCount))
						{
							continue;
						}
					}
					result.Remove(match.Index, match.Length);
					result.Insert(match.Index, new string(' ', breakCount));
				}
			}
			return result.ToString();
		}

        /// <summary>
        /// Tries to find the specified substring in the input string and returns the index of the first occurrence.
        /// </summary>
        /// <param name="input">The input string to search within.</param>
        /// <param name="strToFind">The substring to find.</param>
        /// <param name="index">When this method returns, contains the zero-based index of the first occurrence of the substring,
        /// or -1 if the substring is not found.</param>
        /// <returns>True if the substring is found; otherwise, false.</returns>
        public static bool TryFind(this string input, string strToFind, out int index)
        {
            index = input.IndexOf(strToFind);
            return index >= 0;
        }

        /// <summary>
        /// Tries to find the specified substring in the input string and returns the start and end indices of the substring.
        /// </summary>
        /// <param name="input">The input string to search within.</param>
        /// <param name="strToFind">The substring to find.</param>
        /// <param name="startIndex">When this method returns, contains the zero-based index of the first occurrence of the substring,
        /// or -1 if the substring is not found.</param>
        /// <param name="endIndex">When this method returns, contains the zero-based index of the end of the substring,
        /// or -1 if the substring is not found.</param>
        /// <returns>True if the substring is found; otherwise, false.</returns>
        public static bool TryFind(this string input, string strToFind, out int startIndex, out int endIndex)
        {
            startIndex = input.IndexOf(strToFind);
            endIndex = startIndex + strToFind.Length;
            return startIndex >= 0;
        }

    }
}
