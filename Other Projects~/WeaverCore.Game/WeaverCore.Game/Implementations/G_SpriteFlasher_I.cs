using WeaverCore.Components;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
    public class G_SpriteFlasher_I : SpriteFlasher_I
    {
        public override void OnFlasherInit(SpriteFlasher flasher)
        {
			flasher.gameObject.AddComponent<SpriteFlashProxy>();
        }
    }
}
