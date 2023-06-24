using UnityEngine;

[ExecuteAlways]
public class HazardRespawnMarker : MonoBehaviour
{
	public bool respawnFacingRight;

	private float groundSenseDistance = 3f;

	private Vector2 groundSenseRay = -Vector2.up;

	private Vector2 heroSpawnLocation;

	public bool drawDebugRays;

	private void Awake()
	{

		if (transform.parent != null && transform.parent.name.Contains("top"))
		{
			groundSenseDistance = 50f;
		}
		heroSpawnLocation = Physics2D.Raycast(transform.position, groundSenseRay, groundSenseDistance, LayerMask.GetMask("Terrain")).point;
	}

	private void Update()
	{
		if (drawDebugRays)
		{
			Debug.DrawRay(transform.position, groundSenseRay * groundSenseDistance, Color.green);
			Debug.DrawRay(heroSpawnLocation - Vector2.right / 2f, Vector2.right, Color.green);
		}
	}
}
