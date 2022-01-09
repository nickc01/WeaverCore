using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Implementations;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Used for getting assets/resources from the WeaverCore Bundle
    /// </summary>
    public static class WeaverAssets
	{
		public static readonly string WeaverAssetBundleName = "weavercore_modclass_bundle";

		static WeaverAssets_I Impl;

		/// <summary>
		/// Loads a Asset from a WeaverCore Asset Bundle
		/// </summary>
		/// <typeparam name="T">The type of asset to load</typeparam>
		/// <param name="name">The name of the asset to load</param>
		/// <returns>Returns the loaded asset, or null of the asset wasn't found</returns>
		public static T LoadWeaverAsset<T>(string name) where T : UnityEngine.Object
		{
			if (Impl == null)
			{
				Impl = ImplFinder.GetImplementation<WeaverAssets_I>();
				Impl.Initialize();
			}

			return Impl.LoadAsset<T>(name);
		}

		/// <summary>
		/// Loads weaver assets with the same name in the WeaverCore Bundle
		/// </summary>
		/// <typeparam name="T">The type of asset to load</typeparam>
		/// <param name="name">The name of the asset to load</param>
		/// <returns>Returns a list of loaded assets</returns>
		public static IEnumerable<T> LoadWeaverAssets<T>(string name) where T : UnityEngine.Object
        {
			if (Impl == null)
			{
				Impl = ImplFinder.GetImplementation<WeaverAssets_I>();
				Impl.Initialize();
			}

			return Impl.LoadAssets<T>(name);
		}

		/// <summary>
		/// Gets the names of all the loaded WeaverCore Asset Bundles
		/// </summary>
		public static IEnumerable<string> AllBundles()
		{
			return Impl.AllAssetBundles;
		}

		/// <summary>
		/// Loads a Asset from a WeaverCore Asset Bundle
		/// </summary>
		/// <typeparam name="T">The type of asset to load</typeparam>
		/// <param name="bundleName">The bundle to load from</param>
		/// <param name="name">The name of the asset to load</param>
		/// <returns>Returns the loaded asset, or null of the asset wasn't found</returns>
		public static T LoadAssetFromBundle<T>(string bundleName, string name) where T : UnityEngine.Object
		{
			return Impl.LoadAssetFromBundle<T>(bundleName, name);
		}

		/// <summary>
		/// Loads an asset from a mod bundle
		/// </summary>
		/// <typeparam name="T">The type of asset to load</typeparam>
		/// <param name="modType">The mod to load the bundle from</param>
		/// <param name="name">The name of the asset to load</param>
		/// <returns>Returns the loaded asset, or null of the asset wasn't found</returns>
		public static T LoadAssetFromBundle<T>(Type modType, string name) where T : UnityEngine.Object
		{
			var bundleName = $"{modType.Name.ToLower()}_bundle";
			return Impl.LoadAssetFromBundle<T>(bundleName, name);
        }

		/// <summary>
		/// Loads an asset from a mod bundle
		/// </summary>
		/// <typeparam name="T">The type of asset to load</typeparam>
		/// <typeparam name="ModType">The mod to load the bundle from</typeparam>
		/// <param name="name">The name of the asset to load</param>
		/// <returns>Returns the loaded asset, or null of the asset wasn't found</returns>
		public static T LoadAssetFromBundle<T, ModType>(string name) where T : UnityEngine.Object
        {
			return LoadAssetFromBundle<T>(typeof(ModType), name);
        }



		/// <summary>
		/// Loads weaver assets with the same name from a bundle
		/// </summary>
		/// <typeparam name="T">The type of assets to load</typeparam>
		/// <param name="bundleName">The bundle to load from</param>
		/// <param name="name">The name of the assets to load</param>
		/// <returns>Returns the loaded assets</returns>
		public static IEnumerable<T> LoadAssetsFromBundle<T>(string bundleName, string name) where T : UnityEngine.Object
		{
			return Impl.LoadAssetsFromBundle<T>(bundleName, name);
		}

		/// <summary>
		/// Loads weaver assets with the same name from a bundle
		/// </summary>
		/// <typeparam name="T">The type of asset to load</typeparam>
		/// <param name="modType">The mod to load the bundle from</param>
		/// <param name="name">ThThe name of the assets to load</param>
		/// <returns>Returns the loaded assets</returns>
		public static IEnumerable<T> LoadAssetsFromBundle<T>(Type modType, string name) where T : UnityEngine.Object
		{
			var bundleName = $"{modType.Name.ToLower()}_bundle";
			return Impl.LoadAssetsFromBundle<T>(bundleName, name);
		}

		/// <summary>
		/// Loads weaver assets with the same name from a bundle
		/// </summary>
		/// <typeparam name="T">The type of asset to load</typeparam>
		/// <typeparam name="ModType">The mod to load the bundle from</typeparam>
		/// <param name="name">The name of the assets to load</param>
		/// <returns>Returns the loaded assets</returns>
		public static IEnumerable<T> LoadAssetsFromBundle<T, ModType>(string name) where T : UnityEngine.Object
		{
			return LoadAssetsFromBundle<T>(typeof(ModType), name);
		}
	}
}
