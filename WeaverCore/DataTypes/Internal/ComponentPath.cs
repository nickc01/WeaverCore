using UnityEngine;

namespace WeaverCore.DataTypes
{
	internal struct ComponentPath
	{
		public int SiblingHash;
		public Component Component;
		public bool Enabled;

		public ComponentPath(int siblingHash,Component component)
		{
			SiblingHash = siblingHash;
			Component = component;
			if (component is Behaviour)
			{
				Enabled = ((Behaviour)component).enabled;
			}
			else
			{
				Enabled = true;
			}
		}
	}
}
