using UnityEngine;

public class HazardRespawnTrigger : MonoBehaviour
{
	public HazardRespawnMarker respawnMarker;

	private PlayerData playerData;

	public bool fireOnce;

	private bool inactive;

	private void Awake()
	{
		playerData = PlayerData.instance;
	}

	private void Reset()
	{
		var collider = gameObject.GetComponent<Collider2D>();
		if (collider == null)
		{
			collider = gameObject.AddComponent<BoxCollider2D>();
		}
		collider.isTrigger = true;

		transform.SetPositionZ(0f);
	}

	private void Start()
	{
		if (respawnMarker == null)
		{
			Debug.LogWarning(base.name + " does not have a Hazard Respawn Marker Set");
		}
	}

	private void OnTriggerEnter2D(Collider2D otherCollider)
	{
		if (!inactive && otherCollider.gameObject.layer == 9 && PlayerData.instance != null)
		{
			playerData.SetHazardRespawn(respawnMarker);
			if (fireOnce)
			{
				inactive = true;
			}
		}
	}
}
