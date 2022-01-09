using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;


namespace WeaverCore.Assets
{
	/// <summary>
	/// A debug tools menu that can be activated/deactivated in game using CTRL + NUMPAD7.
	/// 
	/// This menu allows the player to view all objects and components in the game, and do certain actions on them, such as extracting sprite information
	/// 
	/// The game can also be frozen and unfrozen using CTRL + NUMPAD9
	/// </summary>
	public class WeaverCoreDebugTools : MonoBehaviour
	{
		class KeyListener : MonoBehaviour
		{
			private void Update()
			{
				if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Keypad7))
				{
					if (!IsOpen)
					{
						Open();
					}
					else
					{
						Close();
					}
				}

				if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Keypad9))
				{
					FreezeTime();
				}
			}
		}

		[OnRuntimeInit]
		static void OnGameStart()
		{
			GameObject inputListener = new GameObject();
			inputListener.hideFlags = HideFlags.HideInHierarchy;
			GameObject.DontDestroyOnLoad(inputListener);
			inputListener.AddComponent<KeyListener>();
		}

		/// <summary>
		/// Is game time currently frozen?
		/// </summary>
		public static bool TimeFrozen = false;

		/// <summary>
		/// Is the Debug Tools Menu currently open?
		/// </summary>
		public static bool IsOpen => Instance != null;

		/// <summary>
		/// The currently running instance of the Debug Tools Menu
		/// </summary>
		public static WeaverCoreDebugTools Instance { get; private set; }

		/// <summary>
		/// Causes time to freezes
		/// </summary>
		public static void FreezeTime()
		{
			if (TimeFrozen)
			{
				TimeFrozen = !TimeFrozen;
			}

			Time.timeScale = TimeFrozen ? 0f : 1f;
		}

		/// <summary>
		/// Opens up the Debug Tools Menu
		/// </summary>
		public static void Open()
		{
			GameObject.Instantiate(WeaverAssets.LoadWeaverAsset<GameObject>("WeaverCore Debug Tools"), WeaverDebugCanvas.Content);
		}

		/// <summary>
		/// Closes the Debug Tools Menu
		/// </summary>
		public static void Close()
		{
			if (Instance != null)
			{
				Instance.CloseInterface();
			}
		}


		private void Awake()
		{
			Instance = this;
		}

		public void CloseInterface()
		{
			Destroy(gameObject);
		}
	}

}
