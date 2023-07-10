using AssetsTools.NET;
using System;

namespace WeaverCore.Editor
{
    public static class AssetToolsExtensions
	{
		public static AssetTypeValueField Get(this AssetTypeValueField source, Func<string,bool> predicate)
		{
            AssetTypeValueField[] array = source.children;
            foreach (AssetTypeValueField assetTypeValueField in array)
            {
                if (predicate(assetTypeValueField.templateField.name))
                {
                    return assetTypeValueField;
                }
            }

            return AssetTypeInstance.GetDummyAssetTypeField();
        }
	}
}
