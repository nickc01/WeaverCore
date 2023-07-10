using UnityEngine;


public class DamageHero : MonoBehaviour
{
	private void OnEnable()
	{
		if (resetOnEnable)
		{
			if (initialValue == null)
			{
				initialValue = damageDealt;
				return;
			}
			damageDealt = initialValue.Value;
		}
	}

	public int damageDealt = 1;

	public int hazardType = 1;

	public bool shadowDashHazard;

	public bool resetOnEnable;

	private int? initialValue;
}

