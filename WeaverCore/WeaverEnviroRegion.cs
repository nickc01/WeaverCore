using UnityEngine;
using WeaverCore.Enums;

namespace WeaverCore
{
    public class WeaverEnviroRegion : EnviroRegion
    {
        public int EnvironmentTypeName = (int)EnvironmentType.Dust;

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Hero Detector");
            environmentType = EnvironmentTypeName;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            environmentType = EnvironmentTypeName;
        }
#endif
    }
}
