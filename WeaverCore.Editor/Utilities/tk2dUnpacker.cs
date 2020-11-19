using Mono.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.DataTypes;
using WeaverCore.Editor.DataTypes;
using WeaverCore.Editor.Enums;
using WeaverCore.Editor.Structures;
using WeaverCore.Enums;
using WeaverCore.Utilities;

using WrapMode = WeaverCore.Editor.DataTypes.WrapMode;

namespace WeaverCore.Editor.DataTypes
{
	struct SpriteCollectionData
	{
		public FilterMode TextureFilterMode;
		public int Version;
		public string CollectionName;
		public string GUID;
		public Texture MainTexture;
		public SpriteDefinitionData[] Definitions;

	}

	struct SpriteDefinitionData
	{
		static Vector2 uvOffset = new Vector2(0.001f, 0.001f);

		public string Name;
		public Vector2 TextureSize;
		public Vector2[] Positions;
		public Vector2[] UVs;
		public bool Flipped;
	}

	struct tk2DSpriteData
	{
		public string Name;
		public Vector2 Pivot;
		public Texture2D Texture;
		public Vector2Int UVDimensions;
		public float PixelsPerUnit;
		public Rect SpriteCoords;
	}

	struct SpriteAnimationData
	{
		public ClipData[] Clips;
	}

	enum WrapMode
	{
		Loop = 0,
		LoopSection = 1,
		Once = 2,
		PingPong = 3,
		RandomFrame = 4,
		RandomLoop = 5,
		Single = 6,
	}

	struct ClipData
	{
		public string Name;
		public FrameData[] Frames;
		public float FPS;
		public WrapMode WrapMode;
		public int LoopStart;
	}

	struct FrameData
	{
		public UnityEngine.Object SpriteCollectionRaw;
		public int SpriteID;
	}
}


namespace WeaverCore.Editor.Utilities
{
	class OutClass<T>
	{
		public T Value;
	}


	class SpriteUnpacker : EditorWindow
	{
		UnityEngine.Object collection;


		[MenuItem("WeaverCore/Tools/Unpack TK2D")]
		public static void UnpackTK2D()
		{
			var packer = GetWindow<SpriteUnpacker>();
			packer.Init();
			packer.Show();
		}


		void Init()
		{
			collection = null;
		}


		void OnGUI()
		{
			if (TK2DUnpacker.CollectionDataType == null)
			{
				EditorGUILayout.LabelField("There is no tk2dSpriteCollectionData Type found");
				return;
			}
			else
			{
				collection = EditorGUILayout.ObjectField("Collection Data", collection, TK2DUnpacker.CollectionDataType, true);
				if (GUILayout.Button("Unpack"))
				{
					Close();
					TK2DUnpacker.UnpackToSprite(collection);
				}
			}
		}
	}

	class AnimationUnpacker : EditorWindow
	{
		UnityEngine.Object animation;


		[MenuItem("WeaverCore/Tools/Unpack TK2D Animation")]
		public static void UnpackTK2D()
		{
			var packer = GetWindow<AnimationUnpacker>();
			packer.Init();
			packer.Show();
		}


		void Init()
		{
			animation = null;
		}


		void OnGUI()
		{
			if (TK2DUnpacker.CollectionDataType == null)
			{
				EditorGUILayout.LabelField("There is no tk2dSpriteAnmation Type found");
				return;
			}
			else
			{
				animation = EditorGUILayout.ObjectField("Animation Data", animation, TK2DUnpacker.SpriteAnimationType, true);
				if (GUILayout.Button("Unpack"))
				{
					Close();
					UnboundCoroutine.Start(TK2DUnpacker.DecomposeTK2DAnimation(animation));
				}
			}
		}
	}




	public static class TK2DUnpacker
	{


		static Type _collectionType;
		public static Type CollectionDataType
		{
			get
			{
				if (_collectionType == null)
				{
					var mainAssembly = Assembly.Load("Assembly-CSharp");
					if (mainAssembly != null)
					{
						_collectionType = mainAssembly.GetType("tk2dSpriteCollectionData",true);
					}
				}
				return _collectionType;
			}
		}

		static Type _spriteDefinitionType;
		public static Type SpriteDefinitionType
		{
			get
			{
				if (_spriteDefinitionType == null)
				{
					var mainAssembly = Assembly.Load("Assembly-CSharp");
					if (mainAssembly != null)
					{
						_spriteDefinitionType = mainAssembly.GetType("tk2dSpriteDefinition", true);
					}
				}
				return _spriteDefinitionType;
			}
		}

		static Type _spriteAnimationType;
		public static Type SpriteAnimationType
		{
			get
			{
				if (_spriteAnimationType == null)
				{
					var mainAssembly = Assembly.Load("Assembly-CSharp");
					if (mainAssembly != null)
					{
						_spriteAnimationType = mainAssembly.GetType("tk2dSpriteAnimation", true);
					}
				}
				return _spriteAnimationType;
			}
		}

		static void GetWorldCorners(Vector2[] positions, out Vector2 BottomLeft, out Vector2 TopRight)
		{
			if (positions.GetLength(0) == 0)
			{
				TopRight = Vector2.zero;
				BottomLeft = Vector2.zero;
			}
			else
			{

				Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

				foreach (var pos in positions)
				{
					if (pos.x < min.x)
					{
						min.x = pos.x;
					}
					if (pos.y < min.y)
					{
						min.y = pos.y;
					}

					if (pos.x > max.x)
					{
						max.x = pos.x;
					}
					if (pos.y > max.y)
					{
						max.y = pos.y;
					}
				}

				TopRight = max;
				BottomLeft = min;
			}
		}

		static Vector2 GetWorldSize(Vector2[] positions)
		{
			Vector2 BL, TR;
			GetWorldCorners(positions, out BL, out TR);

			return TR - BL;
		}

		static Vector2 GetPivot(Vector2[] positions)
		{
			Vector2 BL, TR;
			GetWorldCorners(positions, out BL, out TR);

			return new Vector2(1f - Mathf.InverseLerp(BL.x,TR.x,0f),1f - Mathf.InverseLerp(BL.y, TR.y, 0f));
		}

		static Array GetSpriteDefinitionsRaw(UnityEngine.Object spriteCollectionData)
		{
			if (spriteCollectionData == null)
			{
				throw new NullReferenceException("Sprite Collection Data is null");
			}
			var type = spriteCollectionData.GetType();
			if (type != CollectionDataType)
			{
				throw new Exception("The data is not of the type tk2dSpriteCollectionData");
			}
			return (Array)type.GetField("spriteDefinitions").GetValue(spriteCollectionData);
		}

		static SpriteCollectionData GetCollectionData(UnityEngine.Object spriteCollectionData)
		{
			var rawSprites = GetSpriteDefinitionsRaw(spriteCollectionData);
			SpriteCollectionData data = new SpriteCollectionData();
			data.CollectionName = (string)CollectionDataType.GetField("spriteCollectionName").GetValue(spriteCollectionData);
			data.MainTexture = (Texture)((Array)CollectionDataType.GetField("textures").GetValue(spriteCollectionData)).GetValue(0);
			data.GUID = (string)CollectionDataType.GetField("spriteCollectionName").GetValue(spriteCollectionData);
			data.TextureFilterMode = (FilterMode)CollectionDataType.GetField("textureFilterMode").GetValue(spriteCollectionData);
			data.Version = (int)CollectionDataType.GetField("version").GetValue(spriteCollectionData);

			data.Definitions = new SpriteDefinitionData[rawSprites.Length];

			for (int i = 0; i < rawSprites.GetLength(0); i++)
			{
				var rawSprite = rawSprites.GetValue(i);

				var definition = new SpriteDefinitionData();
				definition.Name = (string)SpriteDefinitionType.GetField("name").GetValue(rawSprite) ?? ("unknown_" + i.ToString());
				var oldPositions = (Vector3[])SpriteDefinitionType.GetField("positions").GetValue(rawSprite);
				var newPositions = new Vector2[oldPositions.GetLength(0)];
				for (int j = 0; j < oldPositions.GetLength(0); j++)
				{
					newPositions[j] = oldPositions[j];
				}
				definition.Positions = newPositions;
				definition.TextureSize = (Vector2)SpriteDefinitionType.GetField("texelSize").GetValue(rawSprite);
				definition.UVs = (Vector2[])SpriteDefinitionType.GetField("uvs").GetValue(rawSprite);
				var flippedEnum = SpriteDefinitionType.GetField("flipped").GetValue(rawSprite);
				var enumType = flippedEnum.GetType();
				definition.Flipped = Enum.GetName(enumType, flippedEnum) == "Tk2d";
				data.Definitions[i] = definition;
			}

			return data;
		}

		static IEnumerable<tk2DSpriteData> ReadSpritesFromData(SpriteCollectionData data)
		{
			var mainTexture = data.MainTexture as Texture2D;
			using (var progress = new ProgressBar(data.Definitions.GetLength(0) - 1, "Unpacking", "Unpacking TK2DSpriteDefinition", true))
			{
				using (var context = new ReadableTextureContext(mainTexture))
				{
					for (int i = 0; i < data.Definitions.GetLength(0); i++)
					{
						progress.GoToNextStep();
						var sprite = data.Definitions[i];

						Vector2 uvOffset = new Vector2(0.001f, 0.001f);

						Vector2 PostProcessedBL = Vector2.zero;
						Vector2 PostProcessedTR = Vector2.zero;

						bool rotated = false;

						if (sprite.UVs[0].x == sprite.UVs[1].x && sprite.UVs[2].x == sprite.UVs[3].x)
						{
							rotated = true;
						}


						if (rotated)
						{
							PostProcessedBL = sprite.UVs[1];
							PostProcessedTR = sprite.UVs[2];
						}
						else
						{
							PostProcessedBL = sprite.UVs[0];
							PostProcessedTR = sprite.UVs[3];
						}

						int PreBLx = Mathf.RoundToInt((PostProcessedBL.x * mainTexture.width) - uvOffset.x);
						int PreTRy = Mathf.RoundToInt(((PostProcessedBL.y) * mainTexture.height) - uvOffset.y);
						int PreTRx = Mathf.RoundToInt((PostProcessedTR.x * mainTexture.width) + uvOffset.x);
						int PreBLy = Mathf.RoundToInt(((PostProcessedTR.y) * mainTexture.height) + uvOffset.y);

						int PreWidth = Mathf.Abs(PreBLx - PreTRx);
						int PreHeight = Mathf.Abs(PreBLy - PreTRy);

						Orientation orientation = Orientation.Up;

						if (PostProcessedBL.x < PostProcessedTR.x && PostProcessedBL.y < PostProcessedTR.y)
						{
							orientation = Orientation.Up;
						}
						else if (PostProcessedBL.x < PostProcessedTR.x && PostProcessedBL.y > PostProcessedTR.y)
						{
							orientation = Orientation.Right;
						}
						else if (PostProcessedBL.x > PostProcessedTR.x && PostProcessedBL.y > PostProcessedTR.y)
						{
							orientation = Orientation.Down;
						}
						else if (PostProcessedBL.x > PostProcessedTR.x && PostProcessedBL.y < PostProcessedTR.y)
						{
							orientation = Orientation.Left;
						}

						Vector2Int Min = new Vector2Int(Mathf.RoundToInt(Mathf.Min(PreBLx, PreTRx)), Mathf.RoundToInt(Mathf.Min(PreBLy, PreTRy)));

						Vector2Int Max = new Vector2Int(Mathf.RoundToInt(Mathf.Max(PreBLx, PreTRx)), Mathf.RoundToInt(Mathf.Max(PreBLy, PreTRy)));

						Vector2Int SpriteDimensions = new Vector2Int(Mathf.Abs(Max.x - Min.x) + 1, Mathf.Abs(Max.y - Min.y) + 1);

						Texture2D texture = new Texture2D(SpriteDimensions.x, SpriteDimensions.y);

						if (Min.x < 0 || Min.y < 0 || Min.x + SpriteDimensions.x - 1 >= mainTexture.width || Min.y + SpriteDimensions.y - 1 >= mainTexture.height)
						{
							continue;
						}

						var test = mainTexture.GetPixels(Min.x, Min.y, SpriteDimensions.x, SpriteDimensions.y);

						texture.SetPixels(test);


						switch (orientation)
						{
							case Orientation.Up:
								texture.Rotate(RotationType.None);
								break;
							case Orientation.Right:
								texture.Rotate(RotationType.Right);
								break;
							case Orientation.Down:
								texture.Rotate(RotationType.HalfFullRotation);
								break;
							case Orientation.Left:
								texture.Rotate(RotationType.Left);
								break;
							default:
								break;
						}


						if (sprite.Flipped)
						{
							TextureUtilities.FlipHorizontally(texture);
						}
						texture.name = sprite.Name;

						var worldSize = GetWorldSize(sprite.Positions);

						//TODO
						yield return new tk2DSpriteData()
						{
							Name = sprite.Name,
							Pivot = GetPivot(sprite.Positions),
							Texture = texture,
							UVDimensions = new Vector2Int(PreWidth, PreHeight),
							PixelsPerUnit = texture.width / worldSize.x,
							SpriteCoords = new Rect(Min.x, Min.y, Max.x - Min.x + 1, Max.y - Min.y + 1)
						};
					}
				}
			}
		}

		public static void UnpackToSprite(UnityEngine.Object spriteCollectionData, string fileLocation = null)
		{
			UnboundCoroutine.Start(UnpackToSpriteAsyncInternal(spriteCollectionData, fileLocation: fileLocation));
		}

		public static void UnpackToSprite(UnityEngine.Object spriteCollectionData, out SpritePackage sprites, string fileLocation = null)
		{
			OutClass<SpritePackage> resultSprites = new OutClass<SpritePackage>();

			var iterator = UnpackToSpriteAsyncInternal(spriteCollectionData, resultSprites,fileLocation);

			while (iterator.MoveNext()) { }

			sprites = resultSprites.Value;
		}

		static IEnumerator UnpackToSpriteAsyncInternal(UnityEngine.Object tk2dSpriteCollection, OutClass<SpritePackage> resultSprites = null, string fileLocation = null)
		{
			var data = GetCollectionData(tk2dSpriteCollection);

			List<Texture2D> spriteTextures = new List<Texture2D>();
			List<tk2DSpriteData> sprites = new List<tk2DSpriteData>();
			//yield return null;

			foreach (var spriteData in ReadSpritesFromData(data))
			{
				sprites.Add(spriteData);
				spriteTextures.Add(spriteData.Texture);
				yield return null;
			}

			Texture2D atlas = new Texture2D(data.MainTexture.width, data.MainTexture.height);
			string atlasName = data.MainTexture.name + "_unpacked";
			atlas.name = atlasName;

			var uvCoords = atlas.PackTextures(spriteTextures.ToArray(), 1, Mathf.Max(data.MainTexture.width, data.MainTexture.height), false);
			//WeaverLog.Log("Atlas Size = " + atlas.width + " , " + atlas.height);
			var pngData = atlas.EncodeToPNG();

			if (fileLocation == null)
			{
				fileLocation = "Assets\\" + atlasName + ".png";
			}
			else if (!fileLocation.EndsWith(".png"))
			{
				fileLocation += "\\" + atlasName + ".png";
			}

			var fileInfo = new FileInfo(fileLocation);

			if (!fileInfo.Directory.Exists)
			{
				fileInfo.Directory.Create();
			}

			using (var fileTest = File.Create(fileLocation))
			{
				fileTest.Write(pngData, 0, pngData.GetLength(0));
			}
			AssetDatabase.ImportAsset(fileLocation);

			yield return null;

			//DefaultTexturePlatform

			var importer = (TextureImporter)AssetImporter.GetAtPath(fileLocation);
			var platformSettings = importer.GetPlatformTextureSettings("DefaultTexturePlatform");
			platformSettings.maxTextureSize = Mathf.Max(data.MainTexture.width, data.MainTexture.height);
			importer.SetPlatformTextureSettings(platformSettings);

			float averagePPU = 0f;

			foreach (var sprite in sprites)
			{
				averagePPU += sprite.PixelsPerUnit;
				//yield return null;
			}
			averagePPU /= sprites.Count;


			importer.spriteImportMode = SpriteImportMode.Multiple;
			importer.spritePixelsPerUnit = averagePPU;

			List<SpriteMetaData> metas = new List<SpriteMetaData>();

			Dictionary<Rect, int> rectToId = new Dictionary<Rect, int>();

			//foreach (var sprite in sprites)
			for (int i = 0; i < sprites.Count; i++)
			{
				var sprite = sprites[i];
				var uv = uvCoords[i];
				//var textureSize = new Vector2(sprite.Texture.width, sprite.Texture.height);
				//WeaverLog.Log("Sprite Coords for " + sprite.Texture.name + " = " + uvCoords[i]);
				metas.Add(new SpriteMetaData()
				{
					name = sprite.Name,
					border = Vector4.zero,
					pivot = new Vector2(sprite.Pivot.x, 1 - sprite.Pivot.y),
					alignment = (int)SpriteAlignment.Custom,
					rect = new Rect(uv.x * atlas.width, uv.y * atlas.height, uv.width * atlas.width, uv.height * atlas.height)
				});

				rectToId.Add(metas[metas.Count - 1].rect,i);
			}
			importer.spritesheet = metas.ToArray();

			Debug.Log("Unpacked " + metas.Count + " different sprites");

			importer.SaveAndReimport();

			yield return null;

			EditorUtility.ClearProgressBar();

			var finalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fileLocation);

			if (resultSprites != null)
			{
				var package = new SpritePackage(finalTexture);

				var finalSprites = GetSpritesFromTexture(finalTexture);

				foreach (var finalSprite in finalSprites)
				{
					if (rectToId.ContainsKey(finalSprite.rect))
					{
						//spriteList[rectToId[finalSprite.rect]] = finalSprite;
						package.AddSprite(rectToId[finalSprite.rect], finalSprite);
					}
				}

				resultSprites.Value = package;
			}

			/*if (spriteList != null)
			{
				spriteList.Clear();

				for (int i = 0; i < metas.Count; i++)
				{
					spriteList.Add(null);
				}

				var finalSprites = GetSpritesFromTexture(finalTexture);

				foreach (var finalSprite in finalSprites)
				{
					if (rectToId.ContainsKey(finalSprite.rect))
					{
						spriteList[rectToId[finalSprite.rect]] = finalSprite;
					}
				}
			}*/
		}

		static T GetField<T>(Type type, object obj, string field)
		{
			return (T)type.GetField(field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic).GetValue(obj);
		}

		static SpriteAnimationData GetAnimationData(UnityEngine.Object tk2dSpriteAnimation)
		{
			if (tk2dSpriteAnimation == null)
			{
				throw new NullReferenceException("Sprite Animation Data is null");
			}
			var type = tk2dSpriteAnimation.GetType();
			if (type != SpriteAnimationType)
			{
				throw new Exception("The data is not of the type tk2dSpriteAnimation");
			}

			SpriteAnimationData animationData = new SpriteAnimationData();

			var clipsArray = GetField<Array>(SpriteAnimationType,tk2dSpriteAnimation,"clips");

			animationData.Clips = new ClipData[clipsArray.GetLength(0)];

			for (int i = 0; i < clipsArray.GetLength(0); i++)
			{
				var clipRaw = clipsArray.GetValue(i);
				var clipType = clipRaw.GetType();

				var newClip = new ClipData
				{
					Name = GetField<string>(clipType, clipRaw, "name"),
					FPS = GetField<float>(clipType, clipRaw, "fps"),
					LoopStart = GetField<int>(clipType, clipRaw, "loopStart")
				};

				var wrapEnumRaw = clipType.GetField("wrapMode").GetValue(clipRaw);
				var rawEnumType = wrapEnumRaw.GetType();

				newClip.WrapMode = (WrapMode)Enum.Parse(typeof(WrapMode), Enum.GetName(rawEnumType,wrapEnumRaw));


				var framesArray = GetField<Array>(clipType,clipRaw,"frames");

				newClip.Frames = new FrameData[framesArray.GetLength(0)];

				for (int j = 0; j < framesArray.GetLength(0); j++)
				{
					var frameRaw = framesArray.GetValue(j);
					var frameType = frameRaw.GetType();

					var frame = new FrameData
					{
						SpriteID = GetField<int>(frameType, frameRaw, "spriteId"),
						SpriteCollectionRaw = GetField<UnityEngine.Object>(frameType, frameRaw, "spriteCollection")
					};

					newClip.Frames[j] = frame;
				}


				animationData.Clips[i] = newClip;
			}

			return animationData;
		}

		static Sprite[] GetSpritesFromTexture(Texture2D texture)
		{
			return AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>().ToArray();
		}

		static Dictionary<string,Sprite> GetSpritesByName(Texture2D texture)
		{
			return GetSpritesFromTexture(texture).ToDictionary(s => s.name);
		}

		public static IEnumerator DecomposeTK2DAnimation(UnityEngine.Object tk2dSpriteAnimation, string animationName = null)
		{
			//var data = GetCollectionData(tk2dSpriteCollection);

			if (animationName == null)
			{
				animationName = "DecomposedAnimation";
			}

			var animationData = GetAnimationData(tk2dSpriteAnimation);

			Dictionary<object, SpriteCollectionData> spriteCollections = new Dictionary<object, SpriteCollectionData>();


			Dictionary<object, SpritePackage> sprites = new Dictionary<object, SpritePackage>();

			foreach (var clip in animationData.Clips)
			{
				foreach (var frame in clip.Frames)
				{
					if (!spriteCollections.ContainsKey(frame.SpriteCollectionRaw))
					{
						spriteCollections.Add(frame.SpriteCollectionRaw, GetCollectionData(frame.SpriteCollectionRaw));

						OutClass<SpritePackage> outputSprites = new OutClass<SpritePackage>();
						//List<Sprite> sprites = new List<Sprite>();

						yield return UnpackToSpriteAsyncInternal(frame.SpriteCollectionRaw, outputSprites,"Assets\\" + animationName);

						sprites.Add(frame.SpriteCollectionRaw, outputSprites.Value);
					}
				}
			}

			WeaverAnimationData data = ScriptableObject.CreateInstance<WeaverAnimationData>();

			foreach (var clipRaw in animationData.Clips)
			{
				if (clipRaw.Name == null || clipRaw.Name == "")
				{
					continue;
				}
				var clip = new WeaverAnimationData.Clip
				{
					FPS = clipRaw.FPS,
					LoopStart = clipRaw.LoopStart,
					Name = clipRaw.Name,
				};
				switch (clipRaw.WrapMode)
				{
					case WrapMode.Loop:
						clip.WrapMode = WeaverAnimationData.WrapMode.Loop;
						break;
					case WrapMode.LoopSection:
						clip.WrapMode = WeaverAnimationData.WrapMode.LoopSection;
						break;
					case WrapMode.Once:
						clip.WrapMode = WeaverAnimationData.WrapMode.Once;
						break;
					case WrapMode.PingPong:
						clip.WrapMode = WeaverAnimationData.WrapMode.PingPong;
						break;
					case WrapMode.RandomFrame:
						clip.WrapMode = WeaverAnimationData.WrapMode.RandomFrame;
						break;
					case WrapMode.RandomLoop:
						clip.WrapMode = WeaverAnimationData.WrapMode.RandomLoop;
						break;
					case WrapMode.Single:
						clip.WrapMode = WeaverAnimationData.WrapMode.SingleFrame;
						break;
					default:
						break;
				}

				foreach (var frameRaw in clipRaw.Frames)
				{
					clip.AddFrame(sprites[frameRaw.SpriteCollectionRaw].GetSprite(frameRaw.SpriteID));
				}

				data.AddClip(clip);
			}



			AssetDatabase.CreateAsset(data, "Assets/" + animationName + "/" + animationName + ".asset");
		}
	}
}
