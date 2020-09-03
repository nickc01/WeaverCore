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
	}
}
