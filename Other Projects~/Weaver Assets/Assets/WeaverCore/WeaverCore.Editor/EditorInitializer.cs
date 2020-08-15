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

namespace WeaverCore.Editor.Implementations
{
    [InitializeOnLoad]
    class EditorInitializer
    {
        static EditorInitializer()
        {
            //Init();
            InitRunner.RunInitFunctions();
            Init();
        }


        struct SortingLayer
        {
            public SortingLayer(string name, long id)
            {
                Name = name;
                UniqueID = id;
            }

            public string Name;
            public long UniqueID;
        }


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

        private static SortingLayer[] SortingLayers = new SortingLayer[]
        {
            new SortingLayer("Default", 0),
            new SortingLayer("Far BG 2", 3315419377),
            new SortingLayer("Far BG 1", 1459018367),
            new SortingLayer("Mid BG", 4015848369),
            new SortingLayer("Immediate BG", 2917268371),
            new SortingLayer("Actors", 1270309357),
            new SortingLayer("Player", 3557629463),
            new SortingLayer("Tiles", 3868594333),
            new SortingLayer("MID Dressing", 3784110789),
            new SortingLayer("Immediate FG", 31172181),
            new SortingLayer("Far FG", 2577183099),
            new SortingLayer("Vignette", 1038907033),
            new SortingLayer("Over", 3945752401),
            new SortingLayer("HUD", 629535577)
        };

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

