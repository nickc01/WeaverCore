using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace WeaverCore.Editor.Utilities
{
	/// <summary>
	/// Used to temporarily make a texture readable
	/// </summary>
	public sealed class ReadableTextureContext : IDisposable
	{
		public class PreviousState
		{
			public bool Readable;
			public bool Compressed;
		}

		public readonly List<PreviousState> PreviousStates;
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
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
		}

		public ReadableTextureContext(params Texture2D[] textures)
		{
			Textures = textures.ToList();
			PreviousStates = MakeTexturesReadable(Textures);
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
		}


		public void Dispose()
		{
			if (TexturesReadable)
			{
				RevertTextureReadability(Textures, PreviousStates);
				AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			}
		}

		/// <summary>
		/// Makes all the input textures readable
		/// </summary>
		/// <param name="textures">The textures to make readable</param>
		/// <returns>Returns a list the size of the textures list. This list stores whether the textures where readable or not previously. This is useful to revert the textures back to their previous state</returns>
		public static List<PreviousState> MakeTexturesReadable(List<Texture2D> textures)
		{
			try
			{
				List<PreviousState> previousStates = new List<PreviousState>();
				AssetDatabase.StartAssetEditing();
				foreach (var texture in textures)
				{
					if (texture == null)
					{
						previousStates.Add(null);
						continue;
					}
					var importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
					var settings = new TextureImporterSettings();
					importer.ReadTextureSettings(settings);
					previousStates.Add(new PreviousState
					{
						Readable = importer.isReadable,
						Compressed = importer.textureCompression != TextureImporterCompression.Uncompressed
					});
					if (!importer.isReadable)
					{
						importer.isReadable = true;
						settings.readable = true;
					}

					if (importer.textureCompression != TextureImporterCompression.Uncompressed)
					{
						importer.textureCompression = TextureImporterCompression.Uncompressed;
						var platSettings = importer.GetDefaultPlatformTextureSettings();
						platSettings.textureCompression = TextureImporterCompression.Uncompressed;
						importer.SetPlatformTextureSettings(platSettings);
					}

					importer.SetTextureSettings(settings);
					importer.SaveAndReimport();
				}
				return previousStates;
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}

		/// <summary>
		/// Reverts all textures back to their original states
		/// </summary>
		/// <param name="textures">The textures to revert</param>
		/// <param name="previousStates">A list of the previosu states of the textures</param>
		public static void RevertTextureReadability(List<Texture2D> textures, List<PreviousState> previousStates)
		{
			try
			{
				AssetDatabase.StartAssetEditing();
				for (int i = 0; i < textures.Count; i++)
				{
					if (textures[i] == null)
					{
						continue;
					}
					var importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(textures[i]));
					var settings = new TextureImporterSettings();
					importer.ReadTextureSettings(settings);

					var prevState = previousStates[i];

					if (importer.isReadable != prevState.Readable)
					{
						importer.isReadable = prevState.Readable;
						settings.readable = prevState.Readable;
					}

					if (prevState.Compressed)
					{
						importer.textureCompression = TextureImporterCompression.Compressed;
						var platSettings = importer.GetDefaultPlatformTextureSettings();
						platSettings.textureCompression = TextureImporterCompression.Compressed;
						importer.SetPlatformTextureSettings(platSettings);
					}

					importer.SetTextureSettings(settings);
					importer.SaveAndReimport();
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}
	}
}
