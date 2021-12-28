using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Enums;

/// <summary>
/// An effect that is instantiated when the player uses sharp shadow on an enemy
/// </summary>
public class SharpShadowImpact : MonoBehaviour 
{
	[SerializeField]
	OnDoneBehaviour doneBehaviour = OnDoneBehaviour.DestroyOrPool;

	[SerializeField]
	float lifetime = 1f;

	void OnEnable()
	{
		StopAllCoroutines();
		StartCoroutine(Destroyer());
	}

	IEnumerator Destroyer()
	{
		yield return new WaitForSeconds(lifetime);
		doneBehaviour.DoneWithObject(this);
	}
}
