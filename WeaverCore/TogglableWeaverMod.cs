using Modding;

namespace WeaverCore
{
	public abstract class TogglableWeaverMod : WeaverMod, ITogglableMod
    {
        public virtual void Unload()
        {
            DisableRegistries();
        }
    }
}
