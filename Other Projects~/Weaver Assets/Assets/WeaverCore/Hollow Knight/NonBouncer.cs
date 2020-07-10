using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonBouncer : MonoBehaviour
{
	public void SetActive(bool set_active)
	{
		active = set_active;
	}

	public bool active = true;
}
