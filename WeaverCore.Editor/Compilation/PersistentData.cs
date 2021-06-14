using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Editor.Compilation
{
	public enum PersistenceType
	{
		/// <summary>
		/// The data will only be stored after a single reload
		/// </summary>
		SingleUse,
		/// <summary>
		/// The data will be stored permanently
		/// </summary>
		Forever
	}

	/// <summary>
	/// Used for storing data even after the editor has been reloaded
	/// </summary>
	public static class PersistentData
	{
		[Serializable]
		class DataStorage
		{
			public List<DataKey> Objects = new List<DataKey>();
		}

		[Serializable]
		class DataKey
		{
			public DataKey(string key, object data, bool onlyOnce)
			{
				Key = key;
				Data = JsonUtility.ToJson(data);
				UsageCounter = onlyOnce ? 1 : -1;
			}

			public string Key;
			public string Data;
			public int UsageCounter = -1;
		}

		/// <summary>
		/// Whether to autosave after making a call to <see cref="StoreData{T}(T, PersistenceType)"/>
		/// </summary>
		public static bool AutoSave { get; set; }
		public const string PersistentDataPath = "Assets\\WeaverCore\\Hidden~\\PersistentData.dat";

		static DataStorage Storage;

		static PersistentData()
		{
			AutoSave = true;
			if (File.Exists(PersistentDataPath))
			{
				Storage = JsonUtility.FromJson<DataStorage>(File.ReadAllText(PersistentDataPath));
				if (Storage == null)
				{
					Storage = new DataStorage();
					return;
				}
				for (int i = Storage.Objects.Count - 1; i >= 0; i--)
				{
					var data = Storage.Objects[i];
					if (data.UsageCounter >= 1)
					{
						data.UsageCounter--;
					}
					else if (data.UsageCounter == 0)
					{
						Storage.Objects.RemoveAt(i);
					}
				}
			}
			else
			{
				Storage = new DataStorage();
			}
		}

		public static bool TryGetData<T>(out T data)
		{
			if (ContainsData<T>())
			{
				data = LoadData<T>();
				return true;
			}
			data = default;
			return false;
		}

		public static bool TryGetData<T>(string key, out T data)
		{
			if (ContainsData(key))
			{
				data = LoadData<T>(key);
				return true;
			}
			data = default;
			return false;
		}

		public static void StoreData<T>(T data, PersistenceType persistenceType = PersistenceType.Forever)
		{
			StoreData(data, typeof(T).FullName + "_typeKey_", persistenceType);
		}

		public static void StoreData<T>(T data, string key, PersistenceType persistenceType = PersistenceType.Forever)
		{
			RemoveData(key);
			Storage.Objects.Add(new DataKey(key,data,persistenceType == PersistenceType.SingleUse));
			if (AutoSave)
			{
				SaveData();
			}
		}

		public static T LoadData<T>()
		{
			return LoadData<T>(typeof(T).FullName + "_typeKey_");
		}

		public static T LoadData<T>(string key)
		{
			var index = Storage.Objects.FindIndex(d => d.Key == key);
			if (index < 0)
			{
				return default(T);
			}
			else
			{
				return JsonUtility.FromJson<T>(Storage.Objects[index].Data);
			}
		}

		public static bool ContainsData<T>()
		{
			return ContainsData(typeof(T).FullName + "_typeKey_");
		}

		public static bool ContainsData(string key)
		{
			return Storage.Objects.FindIndex(d => d.Key == key) != -1;
		}

		public static bool RemoveData<T>()
		{
			return RemoveData(typeof(T).FullName + "_typeKey_");
		}

		public static bool RemoveData(string key)
		{
			var index = Storage.Objects.FindIndex(d => d.Key == key);
			if (index < 0)
			{
				return false;
			}
			else
			{
				Storage.Objects.RemoveAt(index);
				if (AutoSave)
				{
					SaveData();
				}
				return true;
			}
		}

		public static void SaveData()
		{
			File.WriteAllText(PersistentDataPath, JsonUtility.ToJson(Storage,true));
		}
	}
}
