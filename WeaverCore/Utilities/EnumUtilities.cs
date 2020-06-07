using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq;
using System.ComponentModel;

namespace WeaverCore.Utilities
{
	public static class EnumUtilities
	{
		public static EnumType RandomEnumValue<EnumType>(params EnumType[] excludedValues) where EnumType : Enum
		{
			var comparer = EqualityComparer<EnumType>.Default;

			var values = Enum.GetValues(typeof(EnumType));

			List<EnumType> enumValues = new List<EnumType>();

			foreach (var val in values)
			{
				var eVal = (EnumType)val;
				if (!excludedValues.Any(exclusion => comparer.Equals(exclusion,eVal)))
				{
					enumValues.Add((EnumType)val);
				}
			}

			if (enumValues.Count == 0)
			{
				if (excludedValues.GetLength(0) > 0)
				{
					throw new ArgumentException("All enum values cannot be excluded from the randomizer");
				}
				else
				{
					throw new InvalidEnumArgumentException($"The enum value of {typeof(EnumType).Name} does not have any values to randomly select from");
				}
			}

			return enumValues[UnityEngine.Random.Range(0,enumValues.Count)];
		}
	}
}
