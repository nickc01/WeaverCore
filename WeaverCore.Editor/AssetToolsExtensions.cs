using AssetsTools.NET;
using System;

namespace WeaverCore.Editor
{
    public static class AssetToolsExtensions
	{
		public static AssetTypeValueField Get(this AssetTypeValueField source, Func<string,bool> predicate)
		{
            var array = source.Children;
            foreach (AssetTypeValueField assetTypeValueField in array)
            {
                if (predicate(assetTypeValueField.TemplateField.Name))
                {
                    return assetTypeValueField;
                }
            }

            return AssetTypeValueField.DUMMY_FIELD;
        }
	}
}
