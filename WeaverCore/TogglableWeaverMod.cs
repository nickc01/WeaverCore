using Modding;

namespace WeaverCore
{
    /// <summary>
    /// A WeaverCore mod that can be toggled on and off
    /// </summary>
	public abstract class TogglableWeaverMod : WeaverMod, ITogglableMod
    {
        public virtual void Unload()
        {
            DisableRegistries();
        }
    }
}
