using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains many utility functions for enums
	/// </summary>
	public static class EnumUtilities
	{
		/// <summary>
		/// Randomly selects a random enum value
		/// </summary>
		/// <typeparam name="EnumType">The type of enum to randomly select from</typeparam>
		/// <param name="excludedValues">Any values that are excluded from the randomizer</param>
		/// <returns></returns>
		/// <exception cref="Exception">Throws if <typeparamref name="EnumType"/> is not an Enum</exception>
		/// <exception cref="ArgumentException">Throws if all values are excluded from the randomizer, preventing a single value from being selected</exception>
		/// <exception cref="InvalidEnumArgumentException">Throws if the enum has no values to select from</exception>
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

		/// <summary>
		/// Forcefully converts from one enum to another
		/// </summary>
		/// <typeparam name="SourceEnumType">The source enum type</typeparam>
		/// <typeparam name="DestEnumType">The destination enum type</typeparam>
		/// <param name="source">The enum value to convert</param>
		/// <returns>Returns the forcefully converted enum value</returns>
		public static DestEnumType RawConvert<SourceEnumType,DestEnumType>(SourceEnumType source)
		{
			return (DestEnumType)RawConvert(source, typeof(DestEnumType));
		}

		/// <summary>
		/// Forcefully converts from one enum to another
		/// </summary>
		/// <param name="source">The enum value to convert</param>
		/// <param name="destEnumType">The destination enum type</param>
		/// <returns>Returns the forcefully converted enum value</returns>
		/// <exception cref="Exception">Throws if the source type or destination type isn't an enum type</exception>
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

		/// <summary>
		/// Returns an Enumerable for all the enum values in an enum type
		/// </summary>
		/// <typeparam name="T">The enum type to get the list from</typeparam>
		/// <returns>Returns an Enumerable for all the enum values in an enum type</returns>
		/// <exception cref="Exception">Throws if the enum type isn't an enum</exception>
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
