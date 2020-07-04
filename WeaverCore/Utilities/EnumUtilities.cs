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

		public static DestType RawConvert<SourceType,DestType>(SourceType source) where SourceType : Enum where DestType : Enum
		{
			return (DestType)RawConvert(source, typeof(DestType));
		}

		public static object RawConvert(object source, Type destType)
		{
			var sourceType = source.GetType();
			if (!sourceType.IsEnum)
			{
				throw new Exception("The source is of type " + source.GetType() + ", which is not an enum");
			}
			if (!destType.IsEnum)
			{
				throw new Exception("The destType is " + source.GetType() + ", which is not an enum");
			}

			object underlyingValue = Convert.ChangeType(source, Enum.GetUnderlyingType(sourceType));
			return Enum.ToObject(destType, underlyingValue);
		}
	}
}
