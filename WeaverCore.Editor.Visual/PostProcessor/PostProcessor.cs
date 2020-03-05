using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Helpers;

namespace WeaverCore.Editor.Visual
{
    /*public class PostProcessor : AssetPostprocessor
    {
		static List<AssetBundle> bundles;

        static void LoadAssetBundles()
        {
			Debugger.Log("A");
			bundles = new List<AssetBundle>();
			string extension = null;
			if (SystemInfo.operatingSystem.Contains("Windows"))
			{
				extension = ".bundle.win";
			}
			else if (SystemInfo.operatingSystem.Contains("Mac"))
			{
				extension = ".bundle.mac";
			}
			else if (SystemInfo.operatingSystem.Contains("Linux"))
			{
				extension = ".bundle.unix";
			}

			var assembly = typeof(WeaverCore.Internal.WeaverCore).Assembly;

			foreach (var name in assembly.GetManifestResourceNames())
			{
				if (name.EndsWith(extension))
				{
					var bundle = AssetBundle.LoadFromStream(assembly.GetManifestResourceStream(name));
					bundles.Add(bundle);
					foreach (var assetName in bundle.GetAllAssetNames())
					{
						Debugger.Log("Asset Name = " + assetName);
					}
					bundle.LoadAllAssets();
				}
			}
		}

		void OnPostprocessMaterial(Material material)
		{
			Debugger.Log("THIS IS A TEST");
			Debugger.Log("MAterial = " + material);
		}


		Material OnAssignMaterialModel(Material material, Renderer renderer)
        {
			if (bundles == null)
			{
				LoadAssetBundles();
			}
			Debugger.Log("In Material Post Processor");
			Debugger.Log("Material = " + material);
			Debugger.Log("Renderer = " + renderer);

			return material;
        }
    }*/
}
