using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using System.Collections.Generic;
using WeaverCore.Internal;
using WeaverCore.Editor.Structures;
using WeaverCore.Attributes;

namespace WeaverCore.Editor.Implementations
{
    class EditorInitializer
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

        [OnInit]
        static void Init()
        {
            AddInitializer(() =>
            {
                LayerData data = LayerData.GetData();
                for (int i = 8; i < 32; i++)
                {
                    LayerChanger.SetLayerName(i, data.NameData[i]);
                }
                for (int i = 0; i < 32; i++)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        int index = i + (j * 32);
                        Physics2D.IgnoreLayerCollision(i, j, data.CollisionData[index]);
                    }
                }

                foreach (string tag in Tags)
                {
                    LayerChanger.AddTagIfUnique(tag);
                }
                Tags = null;

                foreach (var sortingLayer in SortingLayers)
                {
                    LayerChanger.AddSortingLayer(sortingLayer.Name, sortingLayer.UniqueID);
                }

                SortingLayers = null;

                Physics2D.gravity = new Vector2(0f, -60f);

                SerializedObject graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);

                var AIS = graphicsSettings.FindProperty("m_AlwaysIncludedShaders");
                for (int i = 0; i < AIS.arraySize; i++)
                {
                    var element = AIS.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (element != null && element.name == "Sprites/Default")
                    {
                        AIS.DeleteArrayElementAtIndex(i);
                        graphicsSettings.ApplyModifiedProperties();
                        break;
                    }
                }
            });
        }

        [ExecuteInEditMode]
        class EditorInitializer_Internal : MonoBehaviour
        {
            void Awake()
            {
                Update();
            }

            void Update()
            {
                var next = InitializeEvents[0];
                try
                {
                    next();
                }
                finally
                {
                    InitializeEvents.Remove(next);
                    if (InitializeEvents.Count == 0)
                    {
                        EditorApplication.update -= OnUpdate;
                        DestroyImmediate(gameObject);
                        initializerObject = null;
                    }
                }
            }
        }

        static List<Action> InitializeEvents = new List<Action>();
        static GameObject initializerObject;

        static void OnUpdate()
        {
            if (initializerObject == null)
            {
                initializerObject = new GameObject("__INITIALIZER__", typeof(EditorInitializer_Internal));
            }
        }

        public static void AddInitializer(Action func)
        {
            if (InitializeEvents.Count == 0)
            {
                EditorApplication.update += OnUpdate;
            }
            InitializeEvents.Add(func);
        }
    }
}

