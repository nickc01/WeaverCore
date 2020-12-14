using System;
using System.Collections.Generic;
using System.IO;
using TMPro.EditorUtilities;
using TMPro.SpriteAssetUtilities;
using UnityEditor;
using UnityEngine;

namespace TMPro
{
	public class TMP_SpriteAssetImporter : EditorWindow
	{
		private Texture2D m_SpriteAtlas;

		private SpriteAssetImportFormats m_SpriteDataFormat = SpriteAssetImportFormats.TexturePacker;

		private TextAsset m_JsonFile;

		private string m_CreationFeedback;

		private TMP_SpriteAsset m_SpriteAsset;

		private List<TMP_Sprite> m_SpriteInfoList = new List<TMP_Sprite>();

		[MenuItem("Window/TextMeshPro/Sprite Importer")]
		public static void ShowFontAtlasCreatorWindow()
		{
			TMP_SpriteAssetImporter window = EditorWindow.GetWindow<TMP_SpriteAssetImporter>();
			window.titleContent = new GUIContent("Sprite Importer");
			window.Focus();
		}

		private void OnEnable()
		{
			SetEditorWindowSize();
			TMP_UIStyleManager.GetUIStyles();
		}

		public void OnGUI()
		{
			DrawEditorPanel();
		}

		private void DrawEditorPanel()
		{
			GUILayout.BeginVertical();
			GUILayout.Label("<b>TMP Sprite Importer</b>", TMP_UIStyleManager.Section_Label);
			GUILayout.Label("Import Settings", TMP_UIStyleManager.Section_Label, GUILayout.Width(150f));
			GUILayout.BeginVertical(TMP_UIStyleManager.TextureAreaBox);
			EditorGUI.BeginChangeCheck();
			m_JsonFile = (EditorGUILayout.ObjectField("Sprite Data Source", m_JsonFile, typeof(TextAsset), false) as TextAsset);
			m_SpriteDataFormat = (SpriteAssetImportFormats)(object)EditorGUILayout.EnumPopup("Import Format", m_SpriteDataFormat);
			m_SpriteAtlas = (EditorGUILayout.ObjectField("Sprite Texture Atlas", m_SpriteAtlas, typeof(Texture2D), false) as Texture2D);
			if (EditorGUI.EndChangeCheck())
			{
				m_CreationFeedback = string.Empty;
			}
			GUILayout.Space(10f);
			if (GUILayout.Button("Create Sprite Asset"))
			{
				m_CreationFeedback = string.Empty;
				if (m_SpriteDataFormat == SpriteAssetImportFormats.TexturePacker)
				{
					TexturePacker.SpriteDataObject spriteDataObject = JsonUtility.FromJson<TexturePacker.SpriteDataObject>(m_JsonFile.text);
					if (spriteDataObject != null && spriteDataObject.frames != null && spriteDataObject.frames.Count > 0)
					{
						int count = spriteDataObject.frames.Count;
						m_CreationFeedback = "<b>Import Results</b>\n--------------------\n";
						m_CreationFeedback = m_CreationFeedback + "<color=#C0ffff><b>" + count + "</b></color> Sprites were imported from file.";
						m_SpriteInfoList = CreateSpriteInfoList(spriteDataObject);
					}
				}
			}
			GUILayout.Space(5f);
			GUILayout.BeginVertical(TMP_UIStyleManager.TextAreaBoxWindow, GUILayout.Height(60f));
			EditorGUILayout.LabelField(m_CreationFeedback, TMP_UIStyleManager.Label);
			GUILayout.EndVertical();
			GUILayout.Space(5f);
			GUI.enabled = ((m_SpriteInfoList != null) ? true : false);
			if (GUILayout.Button("Save Sprite Asset"))
			{
				string empty = string.Empty;
				empty = EditorUtility.SaveFilePanel("Save Sprite Asset File", new FileInfo(AssetDatabase.GetAssetPath(m_JsonFile)).DirectoryName, m_JsonFile.name, "asset");
				if (empty.Length == 0)
				{
					return;
				}
				SaveSpriteAsset(empty);
			}
			GUILayout.EndVertical();
			GUILayout.EndVertical();
		}

		private List<TMP_Sprite> CreateSpriteInfoList(TexturePacker.SpriteDataObject spriteDataObject)
		{
			List<TexturePacker.SpriteData> frames = spriteDataObject.frames;
			List<TMP_Sprite> list = new List<TMP_Sprite>();
			for (int i = 0; i < frames.Count; i++)
			{
				TMP_Sprite tMP_Sprite = new TMP_Sprite();
				tMP_Sprite.id = i;
				tMP_Sprite.name = Path.GetFileNameWithoutExtension(frames[i].filename);
				tMP_Sprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(tMP_Sprite.name);
				int num = 0;
				int num2 = tMP_Sprite.name.IndexOf('-');
				num = (tMP_Sprite.unicode = ((num2 == -1) ? TMP_TextUtilities.StringToInt(tMP_Sprite.name) : TMP_TextUtilities.StringToInt(tMP_Sprite.name.Substring(num2 + 1))));
				tMP_Sprite.x = frames[i].frame.x;
				tMP_Sprite.y = (float)m_SpriteAtlas.height - (frames[i].frame.y + frames[i].frame.h);
				tMP_Sprite.width = frames[i].frame.w;
				tMP_Sprite.height = frames[i].frame.h;
				tMP_Sprite.pivot = frames[i].pivot;
				tMP_Sprite.xAdvance = tMP_Sprite.width;
				tMP_Sprite.scale = 1f;
				tMP_Sprite.xOffset = 0f - tMP_Sprite.width * tMP_Sprite.pivot.x;
				tMP_Sprite.yOffset = tMP_Sprite.height - tMP_Sprite.height * tMP_Sprite.pivot.y;
				list.Add(tMP_Sprite);
			}
			return list;
		}

		private void SaveSpriteAsset(string filePath)
		{
			filePath = filePath.Substring(0, filePath.Length - 6);
			string dataPath = Application.dataPath;
			if (filePath.IndexOf(dataPath, StringComparison.InvariantCultureIgnoreCase) == -1)
			{
				Debug.LogError("You're saving the font asset in a directory outside of this project folder. This is not supported. Please select a directory under \"" + dataPath + "\"");
				return;
			}
			string path = filePath.Substring(dataPath.Length - 6);
			string directoryName = Path.GetDirectoryName(path);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			string str = directoryName + "/" + fileNameWithoutExtension;
			m_SpriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
			AssetDatabase.CreateAsset(m_SpriteAsset, str + ".asset");
			m_SpriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(m_SpriteAsset.name);
			m_SpriteAsset.spriteSheet = m_SpriteAtlas;
			m_SpriteAsset.spriteInfoList = m_SpriteInfoList;
			AddDefaultMaterial(m_SpriteAsset);
		}

		private static void AddDefaultMaterial(TMP_SpriteAsset spriteAsset)
		{
			Shader shader = Shader.Find("TextMeshPro/Sprite");
			Material material = new Material(shader);
			material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);
			spriteAsset.material = material;
			material.hideFlags = HideFlags.HideInHierarchy;
			AssetDatabase.AddObjectToAsset(material, spriteAsset);
		}

		private void SetEditorWindowSize()
		{
			Vector2 minSize = this.minSize;
			this.minSize = new Vector2(Mathf.Max(230f, minSize.x), Mathf.Max(300f, minSize.y));
		}
	}
}
