using UnityEngine;

namespace WeaverCore
{
	[CreateAssetMenu(fileName = "AtmosPack", menuName = "WeaverCore/Atmos Pack")]
	public class AtmosPack : ScriptableObject
	{
		[SerializeField]
		Atmos.SnapshotType snapshot;

		public Atmos.SnapshotType Snapshot => snapshot;
	}
}
