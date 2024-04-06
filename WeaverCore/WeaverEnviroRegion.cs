using WeaverCore.Enums;

namespace WeaverCore
{
    public class WeaverEnviroRegion : EnviroRegion
    {
        public EnvironmentType EnvironmentTypeName;

        private void Awake()
        {
            this.environmentType = (int)EnvironmentTypeName;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            environmentType = (int)EnvironmentTypeName;
        }
#endif
    }
}
