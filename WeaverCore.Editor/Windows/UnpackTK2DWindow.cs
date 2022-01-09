using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Compilation;

namespace WeaverCore.Editor
{
	/// <summary>
	/// The window used for unpacking TK2D sprites and animations
	/// </summary>
	public class UnpackTK2DWindow : EditorWindow
	{
		bool animation = false;

		public static SettingData Settings { get; private set; }

		[Serializable]
		public class SettingData
		{
			public UnpackMode UnpackMode;
		}

		public enum UnpackMode
		{
			ToTextures,
			ToSprite
		}

		private void Awake()
		{
			Settings = new SettingData();
			if (PersistentData.TryGetData<SettingData>(out var data))
			{
				Settings = data;
			}
		}

		public static void OpenSprite()
		{
			UnpackTK2DWindow window = (UnpackTK2DWindow)GetWindow(typeof(UnpackTK2DWindow));
			window.titleContent = new UnityEngine.GUIContent("Unpack Settings");
			window.animation = false;
			window.Show();
		}

		public static void OpenAnim()
		{
			UnpackTK2DWindow window = (UnpackTK2DWindow)GetWindow(typeof(UnpackTK2DWindow));
			window.titleContent = new UnityEngine.GUIContent("Unpack Anim Settings");
			window.animation = true;
			window.Show();
		}

		void OnGUI()
		{
			string tooltip = @"Determines the output of the unpacking process

To Textures -> Unpacks the map into textures

To Sprite -> Unpacks the map into a single texture, composed of multiple sprites";
			Settings.UnpackMode = (UnpackMode)EditorGUILayout.EnumPopup(new GUIContent("Unpack Mode",tooltip), Settings.UnpackMode);
			if (GUILayout.Button("Begin"))
			{
				Close();
				PersistentData.StoreData(Settings);
				PersistentData.SaveData();
				if (animation)
				{
					TK2DUnpacker.UnpackTK2DAnimation();
				}
				else
				{
					TK2DUnpacker.UnpackTK2DSprites();
				}
			}
		}
	}
}
