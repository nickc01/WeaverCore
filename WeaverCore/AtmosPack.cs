using UnityEngine;

namespace WeaverCore
{
	/// <summary>
	/// A pack used for changing the atmosphere sounds
	/// </summary>
	[CreateAssetMenu(fileName = "AtmosPack", menuName = "WeaverCore/Atmos Pack")]
	public class AtmosPack : ScriptableObject
	{
		[SerializeField]
		Atmos.SnapshotType snapshot;

		public Atmos.SnapshotType Snapshot => snapshot;
	}
}
