using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/*
 * This file contains all the classes needed to import TK2D sprites and animations and convert them into something that can be used with WeaverCore
 */

namespace WeaverCore.Editor.TK2D
{
	[Serializable]
	public class AnimationMapImport
	{
		public class CollectionTextures
		{
			public tk2dSpriteCollectionData collection;
			public List<string> TextureNames;
		}

		public tk2dSpriteAnimation animation;
		public List<CollectionTextures> collectionTextures;
	}

	[Serializable]
	public class SpriteMapImport
	{
		public tk2dSpriteCollectionData collection;
		public List<string> TextureNames;
	}

	[Serializable]
	public class tk2dSpriteAnimationFrame
	{
		public tk2dSpriteCollectionData spriteCollection;
		public int spriteId;
		public bool triggerEvent;
		public string eventInfo;
		public int eventInt;
		public float eventFloat;
	}

	[Serializable]
	public class tk2dSpriteAnimationClip
	{
		public string name;
		public tk2dSpriteAnimationFrame[] frames;
		public float fps;
		public int loopStart;
		public WrapMode wrapMode;

		public enum WrapMode
		{
			Loop = 0,
			LoopSection = 1,
			Once = 2,
			PingPong = 3,
			RandomFrame = 4,
			RandomLoop = 5,
			Single = 6
		}
	}

	[Serializable]
	public class tk2dSpriteAnimation
	{
		public tk2dSpriteAnimationClip[] clips;
	}

	public class tk2dSpriteCollectionData
	{
		public string[] spriteCollectionPlatformGUIDs;
		public string[] spriteCollectionPlatforms;
		public bool hasPlatformData;
		public bool managedSpriteCollection;
		public string dataGuid;
		public float halfTargetHeight;
		public float invOrthoSize;
		public bool loadable;
		public string assetName;
		public string spriteCollectionName;
		public string spriteCollectionGUID;
		public bool allowMultipleAtlases;
		public int buildKey;
		public FilterMode textureFilterMode;
		public bool textureMipMaps;
		public int version;
		public bool materialIdsValid;
		public bool needMaterialInstance;
		public tk2dSpriteDefinition[] spriteDefinitions;
		public bool premultipliedAlpha;
		public int[] materialPngTextureId;
	}

	public class tk2dSpriteDefinition
	{
		public string name;
		public bool colliderSmoothSphereCollisions;
		public bool colliderConvex;
		public int[] colliderIndicesBack;
		public int[] colliderIndicesFwd;
		public Vector3[] colliderVertices;
		public bool complexGeometry;
		public int regionH;
		public int regionW;
		public int regionY;
		public FlipMode flipped;
		public bool extractRegion;
		public Vector3[] boundsData;
		public Vector3[] untrimmedBoundsData;
		public Vector2 texelSize;
		public int regionX;
		public Vector3[] normals;
		public Vector4[] tangents;
		public Vector2[] uvs;
		public Vector3[] positions;
		public int[] indices;
		public int materialId;
		public string sourceTextureGUID;
		public Vector2[] normalizedUvs;
	}

	public enum FlipMode
	{
		None = 0,
		Tk2d = 1,
		TPackerCW = 2
	}
}
