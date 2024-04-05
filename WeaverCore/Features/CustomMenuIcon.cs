using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Features
{
    [ShowFeature]
    [CreateAssetMenu(fileName = "Custom Menu Icon", menuName = "WeaverCore/Custom Menu Icon")]
    public class CustomMenuIcon : ScriptableObject
    {
        public Sprite icon;
    }
}
