using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Hashing;
using System.Text;

namespace System
{
	internal interface ITupleInternal
	{
		int GetHashCode(IEqualityComparer comparer);

		int Size { get; }

		string ToStringEnd();
	}

	public struct ValueTuple<T1, T2> : IEquatable<ValueTuple<T1, T2>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1, T2>>, ITupleInternal
	{
		public ValueTuple(T1 item1, T2 item2)
		{
			Item1 = item1;
			Item2 = item2;
		}

		public override bool Equals(object obj)
		{
			return obj is ValueTuple<T1, T2> && Equals((ValueTuple<T1, T2>)obj);
		}

		public bool Equals(ValueTuple<T1, T2> other)
		{
			return EqualityComparer<T1>.Default.Equals(Item1, other.Item1) && EqualityComparer<T2>.Default.Equals(Item2, other.Item2);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			bool flag = other == null || !(other is ValueTuple<T1, T2>);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				ValueTuple<T1, T2> valueTuple = (ValueTuple<T1, T2>)other;
				result = (comparer.Equals(Item1, valueTuple.Item1) && comparer.Equals(Item2, valueTuple.Item2));
			}
			return result;
		}

		int IComparable.CompareTo(object other)
		{
			bool flag = other == null;
			int result;
			if (flag)
			{
				result = 1;
			}
			else
			{
				bool flag2 = !(other is ValueTuple<T1, T2>);
				if (flag2)
				{
					throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", "other");
				}
				result = CompareTo((ValueTuple<T1, T2>)other);
			}
			return result;
		}

		public int CompareTo(ValueTuple<T1, T2> other)
		{
			int num = Comparer<T1>.Default.Compare(Item1, other.Item1);
			bool flag = num != 0;
			int result;
			if (flag)
			{
				result = num;
			}
			else
			{
				result = Comparer<T2>.Default.Compare(Item2, other.Item2);
			}
			return result;
		}

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			bool flag = other == null;
			int result;
			if (flag)
			{
				result = 1;
			}
			else
			{
				bool flag2 = !(other is ValueTuple<T1, T2>);
				if (flag2)
				{
					throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", "other");
				}
				ValueTuple<T1, T2> valueTuple = (ValueTuple<T1, T2>)other;
				int num = comparer.Compare(Item1, valueTuple.Item1);
				bool flag3 = num != 0;
				if (flag3)
				{
					result = num;
				}
				else
				{
					result = comparer.Compare(Item2, valueTuple.Item2);
				}
			}
			return result;
		}

		public override int GetHashCode()
		{
			return CombineHashCodes(EqualityComparer<T1>.Default.GetHashCode(Item1), EqualityComparer<T2>.Default.GetHashCode(Item2));
		}

		internal static int CombineHashCodes(int h1, int h2)
		{
			return HashHelpers.Combine(HashHelpers.Combine(HashHelpers.RandomSeed, h1), h2);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		private int GetHashCodeCore(IEqualityComparer comparer)
		{
			return CombineHashCodes(comparer.GetHashCode(Item1), comparer.GetHashCode(Item2));
		}

		int ITupleInternal.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}


		string ITupleInternal.ToStringEnd()
		{
			return "";
		}

		int ITupleInternal.Size
		{
			get
			{
				return 2;
			}
		}

		public T1 Item1;

		public T2 Item2;
	}
}

namespace System.Collections
{
	public interface IStructuralComparable
	{
		int CompareTo(object other, IComparer comparer);
	}

	public interface IStructuralEquatable
	{
		bool Equals(object other, IEqualityComparer comparer);

		int GetHashCode(IEqualityComparer comparer);
	}
}

namespace System.Numerics.Hashing
{
	internal static class HashHelpers
	{
		public static int Combine(int h1, int h2)
		{
			uint num = (uint)(h1 << 5 | (int)((uint)h1 >> 27));
			return (int)(num + (uint)h1 ^ (uint)h2);
		}

		public static readonly int RandomSeed = Guid.NewGuid().GetHashCode();
	}
}

