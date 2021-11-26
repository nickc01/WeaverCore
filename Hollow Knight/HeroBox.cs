using GlobalEnums;
using UnityEngine;

public class HeroBox : MonoBehaviour
{
	private void Start()
	{
		heroCtrl = HeroController.instance;
	}

	private void OnTriggerEnter2D(Collider2D otherCollider)
	{
		if (!HeroBox.inactive)
		{
			CheckForDamage(otherCollider);
		}
	}

	private void OnTriggerStay2D(Collider2D otherCollider)
	{
		if (!HeroBox.inactive)
		{
			CheckForDamage(otherCollider);
		}
	}

	private void CheckForDamage(Collider2D otherCollider)
	{
		//if (!FSMUtility.ContainsFSM(otherCollider.gameObject, "damages_hero"))
		//{
			DamageHero component = otherCollider.gameObject.GetComponent<DamageHero>();
		Debug.Log("HIT DAMAGABLE OBJECT = " + component?.name);
			if (component != null)
			{
				if (heroCtrl.cState.shadowDashing && component.shadowDashHazard)
				{
					return;
				}
				damageDealt = component.damageDealt;
				hazardType = component.hazardType;
				damagingObject = otherCollider.gameObject;
				collisionSide = ((damagingObject.transform.position.x > base.transform.position.x) ? CollisionSide.right : CollisionSide.left);
				if (!HeroBox.IsHitTypeBuffered(hazardType))
				{
					ApplyBufferedHit();
					return;
				}
				isHitBuffered = true;
			}
			return;
		//}
		/*PlayMakerFSM fsm = FSMUtility.LocateFSM(otherCollider.gameObject, "damages_hero");
		int @int = FSMUtility.GetInt(fsm, "damageDealt");
		int int2 = FSMUtility.GetInt(fsm, "hazardType");
		if (otherCollider.transform.position.x > base.transform.position.x)
		{
			this.heroCtrl.TakeDamage(otherCollider.gameObject, CollisionSide.right, @int, int2);
			return;
		}
		this.heroCtrl.TakeDamage(otherCollider.gameObject, CollisionSide.left, @int, int2);*/
	}

	private static bool IsHitTypeBuffered(int hazardType)
	{
		return hazardType == 0;
	}

	private void LateUpdate()
	{
		if (isHitBuffered)
		{
			ApplyBufferedHit();
		}
	}

	private void ApplyBufferedHit()
	{
		heroCtrl.TakeDamage(damagingObject, collisionSide, damageDealt, hazardType);
		isHitBuffered = false;
	}

	public static bool inactive;

	private HeroController heroCtrl;

	private GameObject damagingObject;

	private bool isHitBuffered;

	private int damageDealt;

	private int hazardType;

	private CollisionSide collisionSide;
}
