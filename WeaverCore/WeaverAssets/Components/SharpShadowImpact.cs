using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Enums;

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
