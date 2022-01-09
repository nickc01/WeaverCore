using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Utilities;

namespace WeaverCore.Settings.Elements
{
	/// <summary>
	/// A UI Element where it's value can be set to any number. Adding the <see cref="SettingRangeAttribute"/> limits this number to a certain range
	/// </summary>
	public sealed class NumberElement : UIElement
	{

		TMP_InputField inputField;

		string originalText;

		bool rangeLimited = false;
		double lowerBound = 0f;
		double upperBound = 0f;

		void Awake()
		{
			inputField = GetComponentInChildren<TMP_InputField>();
			inputField.onEndEdit.AddListener(OnInputChange);
		}

		/// <inheritdoc/>
		public override bool CanWorkWithAccessor(IAccessor accessor)
		{
			Type type = accessor.MemberType;
			if (type != null && type == typeof(float) || type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(double))
			{
				return true;
			}
			return false;
		}

		/// <inheritdoc/>
		protected override void OnAccessorChanged(IAccessor accessor)
		{
			Title = accessor.FieldName;
			inputField.text = accessor.FieldValue.ToString();
			originalText = inputField.text;
			Title = accessor.FieldName;

            if (accessor.MemberInfo != null)
            {
				rangeLimited = SettingsScreen.GetRangeOfNumberMember(accessor.MemberInfo, out lowerBound, out upperBound);
			}
			else
            {
				rangeLimited = false;
            }
			base.OnAccessorChanged(accessor);
		}

		void OnInputChange(string text)
		{
			object value;
			if (TryConvertToNumber(text, FieldAccessor.MemberType, out value))
			{
				if (rangeLimited)
				{
					value = LimitToRange(value, lowerBound, upperBound);
					inputField.text = value.ToString();
				}
				FieldAccessor.FieldValue = value;
				originalText = text;
			}
			else
			{
				inputField.text = originalText;
			}
		}

		static bool TryConvertToNumber(string text,Type type, out object value)
		{
			if (type == typeof(float))
			{
				float float_out;
				var canConvert = float.TryParse(text, out float_out);
				value = float_out;
				return canConvert;
			}
			else if (type == typeof(int))
			{
				int int_out;
				var canConvert = int.TryParse(text, out int_out);
				value = int_out;
				return canConvert;
			}
			else if (type == typeof(long))
			{
				long long_out;
				var canConvert = long.TryParse(text, out long_out);
				value = long_out;
				return canConvert;
			}
			else if (type == typeof(short))
			{
				short short_out;
				var canConvert = short.TryParse(text, out short_out);
				value = short_out;
				return canConvert;
			}
			else if (type == typeof(double))
			{
				double double_out;
				var canConvert = double.TryParse(text, out double_out);
				value = double_out;
				return canConvert;
			}
			value = null;
			return false;
		}

		static object LimitToRange(object input, double min, double max)
		{
			if (input is float)
			{
				return Clamp((float)input, (float)min, (float)max);
			}
			else if (input is int)
			{
				return Clamp((int)input, (int)min, (int)max);
			}
			else if (input is long)
			{
				return Clamp((long)input, (long)min, (long)max);
			}
			else if (input is short)
			{
				return Clamp((short)input, (short)min, (short)max);
			}
			else if (input is double)
			{
				return Clamp((double)input, min, max);
			}
			return input;
		}

		static float Clamp(float value, float min, float max)
		{
			if (value > max)
			{
				return max;
			}
			else if (value < min)
			{
				return min;
			}
			return value;
		}

		static int Clamp(int value, int min, int max)
		{
			if (value > max)
			{
				return max;
			}
			else if (value < min)
			{
				return min;
			}
			return value;
		}

		static double Clamp(double value, double min, double max)
		{
			if (value > max)
			{
				return max;
			}
			else if (value < min)
			{
				return min;
			}
			return value;
		}

		static long Clamp(long value, long min, long max)
		{
			if (value > max)
			{
				return max;
			}
			else if (value < min)
			{
				return min;
			}
			return value;
		}

		static short Clamp(short value, short min, short max)
		{
			if (value > max)
			{
				return max;
			}
			else if (value < min)
			{
				return min;
			}
			return value;
		}
	}
}
