using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WeaverCore.Utilities
{
	public static class StringUtilities
	{
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
		/// All </br> statements get replaced with new-line characters. You can also use </br:10> for multiple lines (this creates 10 new-lines for example)
		/// </summary>
		/// <returns></returns>
		public static string AddNewLines(string input)
		{
			StringBuilder result = new StringBuilder(input);
			var breakMatches = Regex.Matches(input, @"(<\/br(\:\d*?)?>)");
			for (int i = breakMatches.Count - 1; i >= 0; i--)
			{
				var match = breakMatches[i];
				if (match.Success)
				{
					int breakCount = 1;
					if (match.Groups.Count == 3)
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
	}
}
