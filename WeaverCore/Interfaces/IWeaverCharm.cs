using UnityEngine;

namespace WeaverCore.Interfaces
{
    public interface IWeaverCharm
	{
		string Name { get; }
		string Description { get; }
		int NotchCost { get; }

		bool Acquired { get; set; }
		bool Equipped { get; set; }
		bool NewlyCollected { get; set; }

		Sprite CharmSprite { get; }
    }
}
