using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore
{
	public class WeightedRandomizer<T>
	{
		public float MissMultiplier = 1.2f;

		HashSet<WeightedValue> options = new HashSet<WeightedValue>();

		public IEnumerable<WeightedValue> AllWeightedValues
		{
			get
			{
				return options;
			}
		}

		public class WeightedValue
		{
			public T value;
			public float weight;
			public int TimesSelected;
			public int TimesNotSelected;

			/*public override bool Equals(object obj)
			{
				if (obj is WeightedValues)
				{
					return ((WeightedValues)obj).value.Equals(value);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return value.GetHashCode();
			}

			public static bool operator==(WeightedValues lhs, WeightedValues rhs)
			{
				return lhs.value.Equals(rhs.value);
			}

			public static bool operator !=(WeightedValues lhs, WeightedValues rhs)
			{
				return !lhs.value.Equals(rhs.value);
			}*/
		}


		public WeightedValue AddWeightedOption(T value, float weight = 1f)
		{
			var newVal = new WeightedValue
			{
				value = value,
				weight = weight
			};

			options.Add(newVal);

			return newVal;
		}

		public WeightedValue FindOption(T value)
		{
			foreach (var option in options)
			{
				if (option.value.Equals(value))
				{
					return option;
				}
			}
			return null;
		}

		public void RemoveOption(WeightedValue value)
		{
			options.Remove(value);
		}

		public void Clear()
		{
			options.Clear();
		}

		/*public void RemoveWeightedOption(T value)
		{
			options.RemoveWhere(w => w.value.Equals(value));
		}

		public bool HashOption(T value)
		{
			foreach (var option in options)
			{
				if (option.value.Equals(value))
				{
					return true;
				}
			}
			return false;
		}*/

		public WeightedValue GenerateRandomWeightedValue()
		{
			return GenerateRandomWeightedValue(options, options.Count);
			/*if (options.Count == 0)
			{
				return null;
			}
			var totalWeight = options.Sum(v => v.weight + (v.TimesNotSelected * MissMultiplier));

			var randomValue = UnityEngine.Random.Range(0f, totalWeight);

			WeightedValue selectedValue = null;

			foreach (var option in options)
			{
				var optionWeight = option.weight + (option.TimesNotSelected * MissMultiplier);
				if (randomValue <= optionWeight)
				{
					selectedValue = option;
					option.TimesSelected++;
				}
				else
				{
					option.TimesNotSelected++;
				}
				randomValue -= optionWeight;
			}
			return selectedValue;*/
		}

		WeightedValue GenerateRandomWeightedValue(IEnumerable<WeightedValue> values, int count)
		{
			if (count == 0)
			{
				return null;
			}
			var totalWeight = values.Sum(v => v.weight + (v.TimesNotSelected * MissMultiplier));

			var randomValue = UnityEngine.Random.Range(0f, totalWeight);

			WeightedValue selectedValue = null;

			foreach (var option in values)
			{
				var optionWeight = option.weight + (option.TimesNotSelected * MissMultiplier);
				if (randomValue <= optionWeight)
				{
					selectedValue = option;
					option.TimesSelected++;
				}
				else
				{
					option.TimesNotSelected++;
				}
				randomValue -= optionWeight;
			}
			return selectedValue;
		}

		static IEnumerable<WeightedValue> ExcludeValue(IEnumerable<WeightedValue> set, WeightedValue value)
		{
			foreach (var setValue in set)
			{
				if (setValue == value)
				{
					continue;
				}
				else
				{
					yield return setValue;
				}
			}
		}

		public T GenerateRandomValue()
		{
			var value = GenerateRandomWeightedValue();
			if (value == null)
			{
				return default(T);
			}
			return value.value;
		}
	}
}
