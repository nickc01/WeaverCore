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
		DamageHero component = otherCollider.gameObject.GetComponent<DamageHero>();
		if (component != null && component.damageDealt > 0 && (!heroCtrl.cState.shadowDashing || !component.shadowDashHazard))
		{
			/*if (heroCtrl.cState.shadowDashing && component.shadowDashHazard)
			{
				return;
			}*/
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
