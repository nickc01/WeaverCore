using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Compilation;
using WeaverCore.Editor.Structures;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{

    [Serializable]
    class LayerData
    {
        public string[] NameData;

        public bool[] CollisionData;

        public LayerData(string[] nameData, bool[] collisionData)
        {
            NameData = nameData;
            CollisionData = collisionData;
        }

        public static LayerData GetData()
        {
            var assetDir = new DirectoryInfo(BuildTools.WeaverCoreFolder.AddSlash() + $"WeaverCore.Editor{Path.DirectorySeparatorChar}Data");
            if (assetDir.Exists)
            {
                var dataFile = new FileInfo(assetDir.FullName + "\\layerData.json");
                if (dataFile.Exists)
                {
                    return JsonUtility.FromJson<LayerData>(File.ReadAllText(dataFile.FullName));
                }
            }
            return default(LayerData);
        }
    }

    /// <summary>
    /// Verifies the Tags and Layers are properly configured
    /// </summary>
    class VerifyLayerData : DependencyCheck
    {
        private static string[] Tags = new string[]
{
             "TileMap",
             "GameManager",
             "BlackOverlay",
             "HeroBox",
             "Nail Attack",
             "RespawnPoint",
             "HeroWalkable",
             "SceneManager",
             "HeroLight",
             "Battle Gate",
             "Battle Scene",
             "CameraParent",
             "Terrain",
             "Canvas",
             "UIManager",
             "Hero Spell",
             "Enemy Message",
             "Orb Target",
             "Vignette",
             "RespawnTrigger",
             "Boss Corpse",
             "Heart Piece",
             "TransitionGate",
             "UI Soul Orb",
             "Shade Marker",
             "Hud Camera",
             "Cinematic",
             "Roar",
             "Stag Grate",
             "Platform",
             "Boss",
             "GeoCounter",
             "CameraTarget",
             "StagMapMarker",
             "HeroFootsteps",
             "Save Icon",
             "HeroLightMain",
             "Beta End",
             "Shop Window",
             "Journal Up Msg",
             "Charms Pane",
             "Inventory Top",
             "Charm Get Msg",
             "Acid",
             "Soul Vessels",
             "Teleplane",
             "Water Surface",
             "Relic Get Msg",
             "Fireball Safe",
             "Baby Centipede",
             "Knight Hatchling",
             "Dream Attack",
             "Infected Flag",
             "Ghost Warrior NPC",
             "Extra Tag",
             "Dream Plant",
             "Dream Orb",
             "Geo",
             "Sharp Shadow",
             "Boss Attack",
             "Nail Beam",
             "Grub Bottle",
             "Colosseum Manager",
             "Wall Breaker",
             "Ignore Hatchling",
             "Hatchling Magnet",
             "Spell Vulnerable",
             "Hopper",
             "Set Extrapolate",
             "Orbit Shield",
             "Grimmchild",
             "WindyGrass",
             "Weaverling"
};

        private static BasicSortingLayer[] SortingLayers = new BasicSortingLayer[]
        {
            new BasicSortingLayer("Default", 0),
            new BasicSortingLayer("Far BG 2", 3315419377),
            new BasicSortingLayer("Far BG 1", 1459018367),
            new BasicSortingLayer("Mid BG", 4015848369),
            new BasicSortingLayer("Immediate BG", 2917268371),
            new BasicSortingLayer("Actors", 1270309357),
            new BasicSortingLayer("Player", 3557629463),
            new BasicSortingLayer("Tiles", 3868594333),
            new BasicSortingLayer("MID Dressing", 3784110789),
            new BasicSortingLayer("Immediate FG", 31172181),
            new BasicSortingLayer("Far FG", 2577183099),
            new BasicSortingLayer("Vignette", 1038907033),
            new BasicSortingLayer("Over", 3945752401),
            new BasicSortingLayer("HUD", 629535577)
        };

        public override void StartCheck(Action<DependencyCheckResult> finishCheck)
        {
			LayerData data = LayerData.GetData();
			for (int i = 8; i < 32; i++)
			{
				SetLayerName(i, data.NameData[i]);
			}
			for (int i = 0; i < 32; i++)
			{
				for (int j = 0; j < 32; j++)
				{
					int index = i + (j * 32);
					Physics2D.IgnoreLayerCollision(i, j, data.CollisionData[index]);
				}
			}

			//foreach (string tag in Tags)
			for (int i = Tags.GetLength(0) - 1; i >= 0; i--)
			{
				AddTagIfUnique(Tags[i]);
			}
			Tags = null;

			foreach (var sortingLayer in SortingLayers)
			{
				AddSortingLayer(sortingLayer.Name, sortingLayer.UniqueID);
			}

			SortingLayers = null;

			finishCheck(DependencyCheckResult.Complete);
        }


		public static string GetLayerName(int layerIndex)
		{
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty layers = tagManager.FindProperty("layers");
			return layers.GetArrayElementAtIndex(layerIndex).stringValue;
		}

		public static void SetLayerName(int layerIndex, string name)
		{
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty layers = tagManager.FindProperty("layers");
			layers.GetArrayElementAtIndex(layerIndex).stringValue = name;
			tagManager.ApplyModifiedProperties();
		}

		public static IEnumerable<LayerIndex> GetAllLayers()
		{
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty layers = tagManager.FindProperty("layers");
			for (int i = 0; i < layers.arraySize; i++)
			{
				yield return new LayerIndex(i, layers.GetArrayElementAtIndex(i).stringValue);
			}
		}

		public static bool ContainsTag(string tag)
		{
			//Debug.Log("Compare Tag = " + tag);
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty tagsProp = tagManager.FindProperty("tags");

			for (int i = 0; i < tagsProp.arraySize; i++)
			{
				SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
				//Debug.Log("Other Tag = " + t.stringValue);
				if (t.stringValue.Equals(tag))
				{
					return true;
				}
			}
			return false;
		}

		public static void AddTag(string tag)
		{
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty tagsProp = tagManager.FindProperty("tags");

			tagsProp.InsertArrayElementAtIndex(0);

			SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
			n.stringValue = tag;

			tagManager.ApplyModifiedProperties();
		}

		public static void AddTagIfUnique(string tag)
		{
			if (!ContainsTag(tag))
			{
				AddTag(tag);
			}
		}

		//Adds a new sorting layer of it's unique in the list
		public static void AddSortingLayer(string sortingLayerName, long uniqueID)
		{
			var serializedObject = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
			var sortingLayers = serializedObject.FindProperty("m_SortingLayers");
			for (int i = 0; i < sortingLayers.arraySize; i++)
			{
				if (sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue.Equals(sortingLayerName))
				{
					return;
				}
			}
			sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
			var newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
			newLayer.FindPropertyRelative("name").stringValue = sortingLayerName;
			newLayer.FindPropertyRelative("uniqueID").longValue = uniqueID;
			serializedObject.ApplyModifiedProperties();
		}
	}
}
