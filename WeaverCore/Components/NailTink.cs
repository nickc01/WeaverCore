using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.DataTypes;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class NailTink : MonoBehaviour, IHittable
{
	public AudioClip TinkSound;
	public GameObject TinkEffectPrefab;

	string collisionLayerName = "Tinker";
	int collisionLayerID = 16;

	Enemy enemy;
	EntityHealth healthManager;

	public bool Hit(HitInfo hit)
	{
		if (healthManager == null)
		{
			healthManager = GetComponentInParent<EntityHealth>();
		}

		if (enemy == null)
		{
			enemy = GetComponentInParent<Enemy>();
		}

		if (healthManager != null)
		{
			var validity = healthManager.IsValidHit(hit);
			if (validity == HitResult.Valid)
			{
				StartCoroutine(HitRoutine(hit));
			}
			return validity == HitResult.Valid;
		}
		else
		{
			StartCoroutine(HitRoutine(hit));
			return true;
		}
	}

	IEnumerator HitRoutine(HitInfo hit)
	{
		WeaverGameManager.FreezeGameTime(WeaverGameManager.TimeFreezePreset.Preset3);

		Player.Player1.EnterParryState();

		WeaverCam.Instance.Shaker.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);

		//PLAY AUDIO

		var attackDirection = hit.Direction;

		CardinalDirection direction = CardinalDirection.Right;

		if (attackDirection < 360f && attackDirection > 225f)
		{
			direction = CardinalDirection.Down;
		}
		else if (attackDirection <= 225f && attackDirection > 135f)
		{
			direction = CardinalDirection.Left;
		}
		else if (attackDirection <= 135 && attackDirection > 45f)
		{
			direction = CardinalDirection.Up;
		}
		else
		{
			direction = CardinalDirection.Right;
		}

		switch (direction)
		{
			case CardinalDirection.Up:
				Player.Player1.Recoil(CardinalDirection.Down);
				GameObject.Instantiate(TinkEffectPrefab, transform.position + new Vector3(0f, -1.5f, 0f), Quaternion.identity);
				break;
			case CardinalDirection.Down:
				Player.Player1.Recoil(CardinalDirection.Up);
				GameObject.Instantiate(TinkEffectPrefab, transform.position + new Vector3(0f, 1.5f, 0f), Quaternion.identity);
				break;
			case CardinalDirection.Left:
				Player.Player1.Recoil(CardinalDirection.Right);
				GameObject.Instantiate(TinkEffectPrefab, transform.position + new Vector3(1.5f, 0f, 0f), Quaternion.identity);
				break;
			case CardinalDirection.Right:
				Player.Player1.Recoil(CardinalDirection.Left);
				GameObject.Instantiate(TinkEffectPrefab, transform.position + new Vector3(-1.5f, 0f, 0f), Quaternion.identity);
				break;
		}

		yield return null;

		Player.Player1.RecoverFromParry();

		if (enemy != null)
		{
			enemy.OnParry(this, hit);
		}

		yield return null;

		yield return new WaitForSeconds(0.15f);
	}

	void Awake()
	{
		//layerTest = LayerMask.LayerToName(16);
	}


	/*void OnTriggerEnter2D(Collider2D collider)
	{
		
	}*/
}
