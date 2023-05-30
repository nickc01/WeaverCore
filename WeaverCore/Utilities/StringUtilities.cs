using System;
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

		public static bool TryFind(this string input, string strToFind, out int index)
		{
			index = input.IndexOf(strToFind);
			return index >= 0;
		}

        public static bool TryFind(this string input, string strToFind, out int startIndex, out int endIndex)
        {
            startIndex = input.IndexOf(strToFind);
			endIndex = startIndex + strToFind.Length;
            return startIndex >= 0;
        }
    }
}
