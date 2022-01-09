using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;

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


		/// <summary>
		/// The file path where persistent data is stored
		/// </summary>
		public static string PersistentDataPath = BuildTools.WeaverCoreFolder.AddSlash() + "Hidden~\\PersistentData.dat";

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

		/// <summary>
		/// Attempts to get some persistent data
		/// </summary>
		/// <typeparam name="T">The type of data to retrieve</typeparam>
		/// <param name="data">The output data</param>
		/// <returns>Returns whether data of the specific type has been stored away</returns>
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

		/// <summary>
		/// Attempts to get some persistent data
		/// </summary>
		/// <typeparam name="T">The type of data to retrieve</typeparam>
		/// <param name="key">The key the data is stored under</param>
		/// <param name="data">The output data</param>
		/// <returns>Returns whether data of the specific type has been stored away</returns>
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

		/// <summary>
		/// Stores some data so it will survive even after the unity editor has been reloaded
		/// </summary>
		/// <typeparam name="T">The type of data to store</typeparam>
		/// <param name="data">The data to store</param>
		/// <param name="persistenceType">How the data should be stored</param>
		public static void StoreData<T>(T data, PersistenceType persistenceType = PersistenceType.Forever)
		{
			StoreData(data, typeof(T).FullName + "_typeKey_", persistenceType);
		}

		/// <summary>
		/// Stores some data so it will survive even after the unity editor has been reloaded
		/// </summary>
		/// <typeparam name="T">The type of data to store</typeparam>
		/// <param name="data">The data to store</param>
		/// <param name="key">The key the data will be stored under</param>
		/// <param name="persistenceType">How the data should be stored</param>
		public static void StoreData<T>(T data, string key, PersistenceType persistenceType = PersistenceType.Forever)
		{
			RemoveData(key);
			Storage.Objects.Add(new DataKey(key,data,persistenceType == PersistenceType.SingleUse));
			if (AutoSave)
			{
				SaveData();
			}
		}

		/// <summary>
		/// Loads some stored data
		/// </summary>
		/// <typeparam name="T">The type of data to load</typeparam>
		/// <returns>Returns the loaded data, or null/default if not</returns>
		public static T LoadData<T>()
		{
			return LoadData<T>(typeof(T).FullName + "_typeKey_");
		}

		/// <summary>
		/// Loads some stored data
		/// </summary>
		/// <typeparam name="T">The type of data to load</typeparam>
		/// <param name="key">The key the data is be stored under</param>
		/// <returns>Returns the loaded data, or null/default if not</returns>
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

		/// <summary>
		/// Is data of the specified type currently being stored?
		/// </summary>
		/// <typeparam name="T">The type of data to load</typeparam>
		/// <returns>Returns true if data of the specified type is currently being stored</returns>
		public static bool ContainsData<T>()
		{
			return ContainsData(typeof(T).FullName + "_typeKey_");
		}

		/// <summary>
		/// Checks if data is being stored under the specified key
		/// </summary>
		/// <param name="key">The key the data is be stored under</param>
		/// <returns>Returns true if data of the specified type is currently being stored</returns>
		public static bool ContainsData(string key)
		{
			return Storage.Objects.FindIndex(d => d.Key == key) != -1;
		}

		/// <summary>
		/// If data of the specified type is currently being stored, this function removes it
		/// </summary>
		/// <typeparam name="T">The type of data to remove</typeparam>
		/// <returns>Returns true if data has been removed</returns>
		public static bool RemoveData<T>()
		{
			return RemoveData(typeof(T).FullName + "_typeKey_");
		}

		/// <summary>
		/// If data of the specified key is currently being stored, this function removes it
		/// </summary>
		/// <param name="key">The key to remove</param>
		/// <returns>Returns true if data has been removed</returns>
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

		/// <summary>
		/// Saves the stored data to the <see cref="PersistentDataPath"/>
		/// </summary>
		public static void SaveData()
		{
			var directory = new FileInfo(PersistentDataPath).Directory;
			if (!directory.Exists)
			{
				directory.Create();
			}
			File.WriteAllText(PersistentDataPath, JsonUtility.ToJson(Storage,true));
		}
	}
}
