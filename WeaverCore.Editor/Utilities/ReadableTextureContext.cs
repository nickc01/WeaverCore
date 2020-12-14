using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace WeaverCore.Editor.Utilities
{
	public sealed class ReadableTextureContext : IDisposable
	{
		public readonly List<bool> PreviousStates;
		public readonly List<Texture2D> Textures;
		public bool TexturesReadable
		{
			get
			{
				return Textures != null;
			}
		}

		public ReadableTextureContext(List<Texture2D> textures)
		{
			PreviousStates = MakeTexturesReadable(textures);
			Textures = textures;
		}

		public ReadableTextureContext(params Texture2D[] textures)
		{
			Textures = textures.ToList();
			PreviousStates = MakeTexturesReadable(Textures);
		}


		public void Dispose()
		{
			if (TexturesReadable)
			{
				RevertTextureReadability(Textures, PreviousStates);
			}
		}

		/// <summary>
		/// Makes all the input textures readable
		/// </summary>
		/// <param name="textures">The textures to make readable</param>
		/// <returns>A list the size of the textures list. This list stores whether the textures where readable or not previously. This is useful to revert the textures back to their previous state</returns>
		public static List<bool> MakeTexturesReadable(List<Texture2D> textures)
		{
			try
			{
				List<bool> previousStates = new List<bool>();
				AssetDatabase.StartAssetEditing();
				foreach (var texture in textures)
				{
					var importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
					previousStates.Add(importer.isReadable);
					if (!importer.isReadable)
					{
						importer.isReadable = true;
						importer.SaveAndReimport();
					}
				}
				return previousStates;
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}

		public static void RevertTextureReadability(List<Texture2D> textures, List<bool> previousReadabilityStates)
		{
			try
			{
				AssetDatabase.StartAssetEditing();
				for (int i = 0; i < textures.Count; i++)
				{
					var importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(textures[i]));
					if (importer.isReadable != previousReadabilityStates[i])
					{
						importer.isReadable = previousReadabilityStates[i];
						importer.SaveAndReimport();
					}
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}
	}
}
