using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;


namespace WeaverCore.Assets
{

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

		public static bool TimeFrozen = false;
		public static bool IsOpen => Instance != null;
		public static WeaverCoreDebugTools Instance { get; private set; }

		public static void FreezeTime()
		{
			if (TimeFrozen)
			{
				TimeFrozen = !TimeFrozen;
			}

			Time.timeScale = TimeFrozen ? 0f : 1f;
		}

		public static void Open()
		{
			GameObject.Instantiate(WeaverAssets.LoadWeaverAsset<GameObject>("WeaverCore Debug Tools"), WeaverDebugCanvas.Content);
		}

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
