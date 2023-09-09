using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Attributes;
using WeaverCore.Implementations;
using WeaverCore.Inventory;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{
	[ShowFeature]
    [RequireComponent(typeof(InventoryInputManager))]
	public abstract class InventoryPanel : MonoBehaviour
	{
        static Dictionary<Type, InventoryPanel> instances = new Dictionary<Type, InventoryPanel>();

		static HashSet<InventoryPanel> loadedPanelPrefabs = new HashSet<InventoryPanel>();
		static Assembly sfCoreAssembly;
        static MethodInfo addInventoryPageMethod;
        static MethodInfo doCopyPaneMethod;
        static object emptyEnum;
        static IDictionary _customPaneData = null;
        static bool gameCamerasStarted = false;

        [SerializeField]
        [Tooltip("The save settings used to store whether this Inventory Panel is enabled or not")]
        SaveSpecificSettings settings;

        [SerializeField]
        [Tooltip("The boolean field in the save settings used to store whether this panel is enabled")]
        string settingsEnabledBool;

        [field: NonSerialized]
        public Guid PanelGUID { get; private set; }

        [field: SerializeField]
        [field: Tooltip("The element to start on when first opening up the UI")]
        public InventoryElement StartElement { get; private set; }

        public abstract string PanelTitle { get; }

        public InventoryNavigator_I Navigator { get; private set; }

        public InventoryElement LeftArrow { get; private set; }
        public InventoryElement RightArrow { get; private set; }

        /// <summary>
        /// The currently highlighted element
        /// </summary>
        public InventoryElement HighlightedElement => Navigator.HighlightedElement;

        public virtual bool IsAvailable
        {
            get
            {
                if (settings.TryGetFieldValue<bool>(settingsEnabledBool,out var result))
                {
                    return result;
                }
                return true;
            }
            set
            {
                if (settings.HasField(settingsEnabledBool))
                {
                    settings.SetFieldValue(settingsEnabledBool, value);
                }
            }
        }

        private string LanguageGet(string key, string sheetTitle, string orig)
        {
            if (key == $"{GetType().FullName}_{PanelGUID}_LANG_KEY")
            {
                if (instances.TryGetValue(GetType(),out var instance))
                {
                    return instance.PanelTitle;
                }
                return PanelTitle;
            }
            return orig;
        }

        private bool GetPlayerBool(string name, bool orig)
        {
            if (name == $"{GetType().FullName}_{PanelGUID}_IS_AVAILABLE")
            {
                if (instances.TryGetValue(GetType(), out var instance))
                {
                    return instance.IsAvailable;
                }
                return IsAvailable;
            }
            return orig;
        }

        public void NavigateToElement(InventoryElement element)
        {
            Navigator.HighlightElement(element);
        }

        protected virtual void Awake()
        {
#if UNITY_EDITOR
            Setup();
#endif
        }

        protected virtual void OnSetup()
        {

        }

        void OnCreate(GameObject inventoryPage)
        {
            var instance = GameObject.Instantiate(this, inventoryPage.transform);
            instance.transform.SetLocalPosition(0f, 0f, 0f);
            instance.PanelGUID = PanelGUID;
            instances.Add(GetType(), instance);

            instance.Setup();
        }

        void Setup()
        {
            LeftArrow = transform.Find("Arrow Left").GetComponent<InventoryElement>();
            RightArrow = transform.Find("Arrow Right").GetComponent<InventoryElement>();

            if (Initialization.Environment == Enums.RunningState.Game)
            {
                var contents = transform.Find("PUT CONTENTS IN HERE");
                var background = transform.Find("Background");

                background.GetComponent<SpriteRenderer>().enabled = false;

                contents.SetParent(background);
                contents.localPosition = Vector3.zero;

                background.SetZLocalPosition(0f);

                GameObject.Destroy(transform.Find("Editor Borders").gameObject);

                var uiLayer = LayerMask.NameToLayer("UI");
                var defaultLayer = LayerMask.NameToLayer("Default");

                foreach (var renderer in contents.GetComponentsInChildren<Renderer>())
                {
                    renderer.sortingLayerName = "HUD";
                    if (renderer.gameObject.layer == defaultLayer)
                    {
                        renderer.gameObject.layer = uiLayer;
                    }
                }

                GameObject.Destroy(LeftArrow.GetComponent<SpriteRenderer>());
                GameObject.Destroy(RightArrow.GetComponent<SpriteRenderer>());
            }

            var navigatorType = ImplFinder.GetImplementationType<InventoryNavigator_I>();
            Navigator = (InventoryNavigator_I)gameObject.AddComponent(navigatorType);

            Navigator.InitPanel(this);

            InventoryNavigator_I.InventoryOpenEvent.AddListener(OnInventoryOpen);
            InventoryNavigator_I.InventoryCloseEvent.AddListener(OnInventoryClose);
            InventoryNavigator_I.PaneOpenBeginEvent.AddListener(name =>
            {
                if (name == $"{GetType().FullName}_{PanelGUID}")
                {
                    OnPaneOpenBegin();
                }
            });

            InventoryNavigator_I.PaneOpenEndEvent.AddListener(name =>
            {
                if (name == $"{GetType().FullName}_{PanelGUID}")
                {
                    OnPaneOpenEnd();
                }
            });

            InventoryNavigator_I.PaneCloseBeginEvent.AddListener(name =>
            {
                if (name == $"{GetType().FullName}_{PanelGUID}")
                {
                    OnPaneCloseBegin();
                }
            });

            InventoryNavigator_I.PaneCloseEndEvent.AddListener(name =>
            {
                if (name == $"{GetType().FullName}_{PanelGUID}")
                {
                    OnPaneCloseEnd();
                }
            });


            Navigator.SetStartupElement(StartElement);

            OnSetup();
        }

        protected virtual void OnInventoryOpen()
        {
            WeaverLog.Log("INV PANEL - INV OPEN");
        }

        protected virtual void OnInventoryClose()
        {
            WeaverLog.Log("INV PANEL - INV CLOSE");
        }

        protected virtual void OnPaneOpenBegin()
        {
            WeaverLog.Log("INV PANEL - PANE OPEN BEGIN");
        }

        protected virtual void OnPaneOpenEnd()
        {
            WeaverLog.Log("INV PANEL - PANE OPEN END");
        }

        protected virtual void OnPaneCloseBegin()
        {
            WeaverLog.Log("INV PANEL - PANE CLOSE BEGIN");
        }

        protected virtual void OnPaneCloseEnd()
        {
            WeaverLog.Log("INV PANEL - PANE CLOSE END");
        }


        static bool SFCoreAddPanel(InventoryPanel panel)
		{
            if (sfCoreAssembly == null)
            {
                sfCoreAssembly = ReflectionUtilities.FindLoadedAssembly("SFCore");
                if (sfCoreAssembly == null)
                {
#if UNITY_EDITOR
                    return false;
#else
                    throw new System.Exception("Attempting to add a charm without SFCore installed. Install SFCore to fix this issue");
#endif
                }

                var inventoryHelper = sfCoreAssembly.GetType("SFCore.InventoryHelper");
                var inventoryPageType = sfCoreAssembly.GetType("SFCore.InventoryPageType");

                emptyEnum = Enum.Parse(inventoryPageType, "Empty");

                addInventoryPageMethod = inventoryHelper.GetMethod("AddInventoryPage");
                doCopyPaneMethod = inventoryHelper.GetMethod("DoCopyPane", BindingFlags.NonPublic | BindingFlags.Static);

                _customPaneData = (IDictionary)inventoryHelper.GetField("_customPaneData", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            }
            WeaverLog.Log("ADDING PANEL = " + panel.GetType().FullName);
            panel.PanelGUID = Guid.NewGuid();

            var eventName = panel.PanelGUID.ToString();

            addInventoryPageMethod.Invoke(null, new object[]
            {
                emptyEnum,
                $"{panel.GetType().FullName}_{panel.PanelGUID}",
                $"{panel.GetType().FullName}_{panel.PanelGUID}_LANG_KEY",
                eventName,
                $"{panel.GetType().FullName}_{panel.PanelGUID}_IS_AVAILABLE",
                new Action<GameObject>(panel.OnCreate)
            });

            ModHooks.LanguageGetHook += panel.LanguageGet;
            ModHooks.GetPlayerBoolHook += panel.GetPlayerBool;

            if (gameCamerasStarted)
            {
                var paneData = _customPaneData[eventName];
                var ret = (GameObject)doCopyPaneMethod.Invoke(null, new object[] {GameCameras.instance, paneData});
                panel.OnCreate(ret);
            }

            return true;
        }

        [AfterGameCameraStartLoad]
        static void CamLoad()
        {
            gameCamerasStarted = true;
            //WeaverLog.Log("Game Camera Loaded!!!");
        }

        [OnFeatureLoad]
        static void OnPanelLoad(InventoryPanel panel)
		{
            //WeaverLog.Log("NEW PANEL LOADED = " + panel.GetType().FullName);
			if (loadedPanelPrefabs.Add(panel)) {
				SFCoreAddPanel(panel);
            }
        }

        [OnFeatureUnload]
		static void OnPanelUnload(InventoryPanel panel)
		{
           // WeaverLog.Log("NEW PANEL UNLOADED = " + panel.GetType().FullName);
            //Not sure what do to here yet
        }
	}
}
