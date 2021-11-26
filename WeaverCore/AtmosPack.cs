using UnityEngine;

namespace WeaverCore
{
	[CreateAssetMenu(fileName = "AtmosPack", menuName = "Atmos Pack")]
	public class AtmosPack : ScriptableObject
	{
		[SerializeField]
		Atmos.SnapshotType snapshot;

		public Atmos.SnapshotType Snapshot => snapshot;
	}
}
