using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ViridianLink.Core
{
    [Serializable]
    public struct FeatureSet
    {
        public Feature feature;
        public string FullTypeName;
    }


    [CreateAssetMenu(fileName = "ModRegistry", menuName = "ViridianLink/Registry", order = 1)]
    public class Registry : ScriptableObject
    {
        [SerializeField]
        string mod;

        [SerializeField]
        List<FeatureSet> features;
        public int testIntField = 5;
    }
}
