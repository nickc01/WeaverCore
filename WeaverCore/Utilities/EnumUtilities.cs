using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace WeaverCore.Utilities
{
	public static class EnumUtilities
	{
		public static EnumType RandomEnumValue<EnumType>(params EnumType[] excludedValues)
		{
			if (!typeof(EnumType).IsEnum)
			{
				throw new Exception(typeof(EnumType).FullName + " is not an enum");
			}
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
					throw new InvalidEnumArgumentException("The enum value of " + typeof(EnumType).Name + " does not have any values to randomly select from");
				}
			}

			return enumValues[UnityEngine.Random.Range(0,enumValues.Count)];
		}

		public static DestEnumType RawConvert<SourceEnumType,DestEnumType>(SourceEnumType source)
		{
			/*if (!typeof(SourceType).IsEnum)
			{
				throw new Exception("The provided SourceType of "+ typeof(SourceType).FullName + " is not an enum");
			}
			if (!typeof(DestType).IsEnum)
			{
				throw new Exception("The provided DestType of " + typeof(DestType).FullName + " is not an enum");
			}*/
			return (DestEnumType)RawConvert(source, typeof(DestEnumType));
		}

		public static object RawConvert(object source, Type destEnumType)
		{
			var sourceType = source.GetType();
			if (!sourceType.IsEnum)
			{
				throw new Exception("The source is of type " + source.GetType() + ", which is not an enum");
			}
			if (!destEnumType.IsEnum)
			{
				throw new Exception("The destType is " + source.GetType() + ", which is not an enum");
			}

			object underlyingValue = Convert.ChangeType(source, Enum.GetUnderlyingType(sourceType));
			return Enum.ToObject(destEnumType, underlyingValue);
		}

		public static IEnumerable<T> GetAllEnumValues<T>()
		{
			if (!typeof(Enum).IsAssignableFrom(typeof(T)))
			{
				throw new Exception("The type " + typeof(T).FullName + " is not an enum");
			}

			foreach (var val in Enum.GetValues(typeof(T)))
			{
				yield return (T)val;
			}
		}
	}
}
