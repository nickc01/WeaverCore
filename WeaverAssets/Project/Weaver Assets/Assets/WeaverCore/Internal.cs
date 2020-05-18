/* A filed with snippets from the hollow knight api to allow for basic backwards compatibility with compiled Weaver Mods
 * 
 * Site = https://github.com/seanpr96/HollowKnight.Modding
 * 
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics.Hashing;
using UnityEngine;


namespace Modding
{
	namespace Patches
	{
		public class SaveGameData
		{

		}
	}

	[Serializable]
	public class ModSettings
	{

	}

	public interface ILogger
	{
		void Log(string message);
		void Log(object message);
		void LogDebug(string message);
		void LogDebug(object message);
		void LogError(string message);
		void LogError(object message);
		void LogFine(string message);
		void LogFine(object message);
		void LogWarn(string message);
		void LogWarn(object message);
	}

	public abstract class Loggable : ILogger
	{
		internal string ClassName;

		protected Loggable()
		{
			ClassName = GetType().Name;
		}

		public void LogFine(string message)
		{
			Debug.Log(FormatLogMessage(message));
		}

		public void LogFine(object message)
		{
			Debug.Log(FormatLogMessage(message));
		}

		public void LogDebug(string message)
		{
			Debug.Log(FormatLogMessage(message));
		}

		public void LogDebug(object message)
		{
			Debug.Log(FormatLogMessage(message));
		}

		public void Log(string message)
		{
			Debug.Log(FormatLogMessage(message));
		}

		public void Log(object message)
		{
			Debug.Log(FormatLogMessage(message));
		}

		public void LogWarn(string message)
		{
			Debug.LogWarning(FormatLogMessage(message));
		}

		public void LogWarn(object message)
		{
			Debug.LogWarning(FormatLogMessage(message));
		}

		public void LogError(string message)
		{
			Debug.LogError(FormatLogMessage(message));
		}

		public void LogError(object message)
		{
			Debug.LogError(FormatLogMessage(message));
		}

		private string FormatLogMessage(string message)
		{
			return "[" + ClassName + "] - " + message;
		}

		private string FormatLogMessage(object message)
		{
			return "[" + ClassName + "] - " + message;
		}
	}

	public interface IMod : ILogger
	{
		string GetName();

		List<ValueTuple<string, string>> GetPreloadNames();

		void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects);

		string GetVersion();

		bool IsCurrent();

		int LoadPriority();
	}

	public interface ITogglableMod : IMod
	{
		void Unload();
	}

	public abstract class Mod : Loggable, IMod
	{
		private readonly string _globalSettingsPath;

		public virtual ModSettings SaveSettings
		{
			get
			{
				return null;
			}
			set { }
		}

		public virtual ModSettings GlobalSettings
		{
			get
			{
				return null;
			}
			set { }
		}

		public readonly string Name;

		protected Mod() : this(null)
		{
		}

		protected Mod(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				name = GetType().Name;
			}

			Name = name;
		}

		public string GetName()
		{
			return Name;
		}

		public virtual List<ValueTuple<string, string>> GetPreloadNames()
		{
			return null;
		}

		public virtual void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
		{
			Initialize();
		}

		public virtual string GetVersion()
		{
			return "UNKNOWN";
		}

		public virtual bool IsCurrent()
		{
			return true;
		}

		public virtual int LoadPriority()
		{
			return 1;
		}

		public virtual void Initialize()
		{
		}

		private void HookSaveMethods()
		{

		}

		private void LoadGlobalSettings()
		{

		}

		private void SaveGlobalSettings()
		{

		}

		private void LoadSaveSettings(Patches.SaveGameData data)
		{

		}

		private void SaveSaveSettings(Patches.SaveGameData data)
		{

		}
	}
}

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

