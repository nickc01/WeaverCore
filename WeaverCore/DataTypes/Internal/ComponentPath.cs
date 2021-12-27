using UnityEngine;

namespace WeaverCore.DataTypes
{
	/// <summary>
	/// Used for identifying a component in an object hiearchy. Used in <seealso cref="ObjectPool"/>
	/// </summary>
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
