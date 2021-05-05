using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class WeaverAssets_I : IImplementation
	{
		public abstract void Initialize();
		public abstract T LoadAsset<T>(string assetName) where T : UnityEngine.Object;
		public abstract IEnumerable<string> AllAssetBundles { get; }

		public abstract T LoadAssetFromBundle<T>(string bundleName, string name) where T : UnityEngine.Object;
	}
}
