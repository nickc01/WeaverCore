using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using WeaverCore.Editor.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Internal
{
	public class FeatureIndex
	{
		public int Index;
		public FeatureSet Value;

		public FeatureIndex(int Index, FeatureSet Value)
		{
			this.Index = Index;
			this.Value = Value;
		}
	}

	public sealed class FeatureEnumerator : IEnumerator<FeatureIndex>
	{
		public int featureIndex = -1;

		public FeatureIndex Current
		{
			get
			{
				return new FeatureIndex(featureIndex, Checker.GetFeature(featureIndex));
			}
		}

		object IEnumerator.Current
		{
			get
			{
				return Current;
			}
		}

		RegistryChecker Checker;

		public FeatureEnumerator(RegistryChecker checker)
		{
			Checker = checker;
			Checker.FeatureEnumerators.Add(this);
		}

		bool disposed = false;

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				Checker.FeatureEnumerators.Remove(this);
				GC.SuppressFinalize(this);
			}
		}

		public void IndexRemoved(int index)
		{
			if (index <= featureIndex)
			{
				featureIndex--;
			}
		}

		public bool MoveNext()
		{
			featureIndex++;
			if (featureIndex >= Checker.GetFeatureCount())
			{
				return false;
			}
			return true;
		}

		public void Reset()
		{
			featureIndex = -1;
		}

		~FeatureEnumerator()
		{
			Dispose();
		}
	}

	public sealed class FeatureEnumerable : IEnumerable<FeatureIndex>
	{
		RegistryChecker Checker;
		public FeatureEnumerable(RegistryChecker checker)
		{
			Checker = checker;
		}

		public IEnumerator<FeatureIndex> GetEnumerator()
		{
			return new FeatureEnumerator(Checker);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}



	public class RegistryChecker : SerializedObjectChecker
	{
		public List<Type> ModList { get; private set; }
		public List<Type> FeatureList { get; private set; }
		public string[] ModNames { get; private set; }
		public string[] FeatureNames { get; private set; }

		public List<FeatureEnumerator> FeatureEnumerators = new List<FeatureEnumerator>();

		public RegistryChecker(SerializedObject obj) : base(obj)
		{
			Start();
		}

		public RegistryChecker(string assetPath) : base(assetPath)
		{
			Start();
		}

		void Start()
		{
			ModList = Mods.GetMods();
			ModNames = Mods.GetModNames(ModList);
			FeatureList = Utilities.Features.GetFeatures();
			FeatureNames = Utilities.Features.GetFeatureNames(FeatureList);
		}

		public static List<RegistryChecker> LoadAllRegistries()
		{
			List<RegistryChecker> registries = new List<RegistryChecker>();
			string[] guids = AssetDatabase.FindAssets("t:Registry");
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				registries.Add(new RegistryChecker(path));
			}
			return registries;
		}

		public override void Check()
		{
			var hashCode = GetListHashCode(ModList);
			if (hashCode != GetInt("modListHashCode") || GetAssetBundleName(serializedObject.targetObject) == "")
			{
				int newIndex = 0;
				Type mod = null;
				if (Mods.FindMod(GetString("modTypeName"), GetString("modAssemblyName"), ModList, out mod))
				{
					newIndex = ModList.IndexOf(mod);
					SetAssetBundle(ModNames[newIndex]);
				}
				else if (ModList.Count > 0)
				{
					SetString("modTypeName", ModList[0].FullName);
					SetString("modAssemblyName", ModList[0].Assembly.GetName().Name);
					SetString("modName", ModNames[0]);
					SetAssetBundle(ModNames[0]);
				}
				else
				{
					SetString("modTypeName", "");
					SetString("modAssemblyName", "");
					SetString("modName", "");
					SetAssetBundle("");
				}
				SetInt("modListHashCode", hashCode);
				SetInt("selectedModIndex", newIndex);
			}
			SetInt("selectedFeatureIndex", 0);
			ApplyChanges();
			//serializedObject.ApplyModifiedProperties();
		}

		public void ApplyChanges()
		{
			serializedObject.ApplyModifiedProperties();
		}

		public void ReplaceAssemblyName(string old, string replacement)
		{
			if (GetString("modAssemblyName") == old)
			{
				SetString("modAssemblyName", replacement);
			}
			foreach (var feature in GetAllFeatures())
			{
				if (feature.Value.AssemblyName == old)
				{
					feature.Value.AssemblyName = replacement;
					SetFeature(feature.Index, feature.Value);
				}
			}
		}

		public void SetAssetBundle(string name)
		{
			string bundleName = CreateBundleName(name);
			SetAssetBundleWithBundleName(bundleName, serializedObject.targetObject);
			var features = serializedObject.FindProperty("featuresRaw");
			for (int i = 0; i < features.arraySize; i++)
			{
				var obj = features.GetArrayElementAtIndex(i).objectReferenceValue;
				if (obj != null)
				{
					SetAssetBundleWithBundleName(bundleName, obj);
				}
			}
		}

		public string GetAssetBundleName(UnityEngine.Object obj)
		{
			var path = AssetDatabase.GetAssetPath(obj);
			if (path != null && path != "")
			{
				var import = AssetImporter.GetAtPath(path);
				return import.assetBundleName;
				//import.SetAssetBundleNameAndVariant(bundleName, import.assetBundleVariant);
			}
			return "";
		}

		void SetAssetBundleWithBundleName(string bundleName, UnityEngine.Object obj)
		{
			var path = AssetDatabase.GetAssetPath(obj);
			if (path != null && path != "")
			{
				var import = AssetImporter.GetAtPath(path);
				import.SetAssetBundleNameAndVariant(bundleName, import.assetBundleVariant);
			}
		}

		public void SetAssetBundle(string name, UnityEngine.Object obj)
		{
			if (obj != null)
			{
				string bundleName = CreateBundleName(name);
				SetAssetBundleWithBundleName(bundleName, obj);
			}
		}

		string CreateBundleName(string name)
		{
			if (name == "")
			{
				return "";
			}
			return (Regex.Match(name, @"([^.]+?)\.?$").Groups[0].Value + "_bundle").ToLower().Replace(" ", "");
		}

		public void SetMod(Type mod)
		{
			var index = ModList.IndexOf(mod);
			serializedObject.SetString("modAssemblyName", mod.Assembly.GetName().Name);
			serializedObject.SetString("modTypeName", mod.FullName);
			serializedObject.SetInt("selectedModIndex", index);
			serializedObject.SetString("modName", ModNames[index]);
			SetAssetBundle(ModNames[index]);
		}

		public void SetMod(int index)
		{
			SetMod(ModList[index]);
		}

		public int GetFeatureCount()
		{
			return serializedObject.FindProperty("features").arraySize;
		}

		public FeatureSet GetFeature(int index)
		{
			var featureInfo = serializedObject.FindProperty("features").GetArrayElementAtIndex(index);
			return new FeatureSet()
			{
				feature = featureInfo.FindPropertyRelative("feature").objectReferenceValue,
				AssemblyName = featureInfo.FindPropertyRelative("AssemblyName").stringValue,
				TypeName = featureInfo.FindPropertyRelative("TypeName").stringValue
			};
		}

		public void SetFeature(int index, FeatureSet feature)
		{
			var featureInfo = serializedObject.FindProperty("features").GetArrayElementAtIndex(index);
			featureInfo.FindPropertyRelative("feature").objectReferenceValue = feature.feature;
			featureInfo.FindPropertyRelative("AssemblyName").stringValue = feature.AssemblyName;
			featureInfo.FindPropertyRelative("TypeName").stringValue = feature.TypeName;

			serializedObject.FindProperty("featuresRaw").GetArrayElementAtIndex(index).objectReferenceValue = feature.feature;

			SetAssetBundle(serializedObject.GetString("modName"), feature.feature);
		}

		public void SetFeature(int index, IFeature feature, Type featureType)
		{
			SetFeature(index, new FeatureSet()
			{
				feature = feature as UnityEngine.Object,
				AssemblyName = featureType.Assembly.GetName().Name,
				TypeName = featureType.FullName
			});
		}

		public void SetFeature(int index, IFeature feature)
		{
			SetFeature(index, feature, feature.GetType());
		}

		public void DeleteFeature(int index)
		{
			serializedObject.FindProperty("features").DeleteArrayElementAtIndex(index);
			serializedObject.FindProperty("featuresRaw").DeleteArrayElementAtIndex(index);
			foreach (var enumerator in FeatureEnumerators)
			{
				enumerator.IndexRemoved(index);
			}
		}

		public void AddFeature(FeatureSet feature)
		{
			var currentSize = GetFeatureCount();

			serializedObject.FindProperty("features").InsertArrayElementAtIndex(currentSize);
			serializedObject.FindProperty("featuresRaw").InsertArrayElementAtIndex(currentSize);
			SetFeature(GetFeatureCount() - 1, feature);
		}

		public void AddFeature(IFeature value, Type featureType)
		{
			AddFeature(new FeatureSet()
			{
				feature = value as UnityEngine.Object,
				AssemblyName = featureType.Assembly.GetName().Name,
				TypeName = featureType.FullName
			});
		}

		public void AddFeature(IFeature value)
		{
			if (value == null)
			{
				throw new NullReferenceException("The feature cannot be null here. If you need the value to be null, you must specify the type as well");
			}
			AddFeature(value, value.GetType());
		}

		public IEnumerable<FeatureIndex> GetAllFeatures()
		{
			return new FeatureEnumerable(this);
		}

		public Type GetMod()
		{
			return Mods.FindMod(GetString("modTypeName"), GetString("modAssemblyName"), ModList);
		}

		public string GetModName(Type mod)
		{
			return ModNames[ModList.IndexOf(mod)];
		}

		public string GetModName(int index)
		{
			return ModNames[index];
		}

		public int GetModIndex(Type mod)
		{
			return ModList.IndexOf(mod);
		}

		public static int GetListHashCode(List<Type> list)
		{
			unchecked
			{
				int hash = 19;
				foreach (var type in list)
				{
					hash = hash * 31 + (type.Assembly.FullName + type.FullName).GetHashCode();
				}
				return hash;
			}
		}
	}
}