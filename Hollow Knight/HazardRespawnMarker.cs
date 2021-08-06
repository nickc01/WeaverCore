using UnityEngine;

// Token: 0x0200009D RID: 157
[ExecuteInEditMode]
public class HazardRespawnMarker : MonoBehaviour
{
	// Token: 0x06000367 RID: 871 RVA: 0x000123A0 File Offset: 0x000105A0
	private void Awake()
	{
		if (base.transform.parent != null && base.transform.parent.name.Contains("top"))
		{
			groundSenseDistance = 50f;
		}
		heroSpawnLocation = Physics2D.Raycast(base.transform.position, groundSenseRay, groundSenseDistance, 256).point;
	}

	// Token: 0x06000368 RID: 872 RVA: 0x0001241C File Offset: 0x0001061C
	private void Update()
	{
		if (drawDebugRays)
		{
			Debug.DrawRay(base.transform.position, groundSenseRay * groundSenseDistance, Color.green);
			Debug.DrawRay(heroSpawnLocation - Vector2.right / 2f, Vector2.right, Color.green);
		}
	}

	// Token: 0x040002C8 RID: 712
	public bool respawnFacingRight;

	// Token: 0x040002C9 RID: 713
	private float groundSenseDistance = 3f;

	// Token: 0x040002CA RID: 714
	private Vector2 groundSenseRay = -Vector2.up;

	// Token: 0x040002CB RID: 715
	private Vector2 heroSpawnLocation;

	// Token: 0x040002CC RID: 716
	public bool drawDebugRays;
}
