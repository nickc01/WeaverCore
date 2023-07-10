using System;
using System.Linq;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore
{
    /*[ShowFeature]
    [CreateAssetMenu(fileName = "Custom Charm", menuName = "WeaverCore/Custom Charm")]
    public class CustomCharm : ScriptableObject
	{
        [SerializeField]
        string charmClassName;

        [SerializeField]
        string __charmClassAssemblyName;

        [SerializeField]
        Sprite _charmSprite;

        public Sprite CharmSprite => _charmSprite;

        Type charmTypeCache = typeof(object);
        public Type CharmType
        {
            get
            {
                if (charmTypeCache == typeof(object))
                {
                    charmTypeCache = null;
                    var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == __charmClassAssemblyName);

                    if (assembly != null)
                    {
                        charmTypeCache = assembly.GetType(charmClassName);
                    }
                }

                return charmTypeCache;
            }
        }
	}*/
}
