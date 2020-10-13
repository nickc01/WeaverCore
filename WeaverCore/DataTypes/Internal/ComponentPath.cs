using UnityEngine;

namespace WeaverCore.DataTypes
{
	internal struct ComponentPath
	{
		public int SiblingHash;
		public Component Component;

		public ComponentPath(int siblingHash,Component component)
		{
			SiblingHash = siblingHash;
			Component = component;
		}
	}
}
