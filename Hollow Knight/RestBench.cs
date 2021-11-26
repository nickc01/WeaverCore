using UnityEngine;

public class RestBench : MonoBehaviour
{
	private HeroController heroCtrl;

	private void Start()
	{
		heroCtrl = HeroController.instance;
	}

	private void OnTriggerEnter2D(Collider2D otherObject)
	{
		if (otherObject.gameObject.layer == 9)
		{
			heroCtrl.NearBench(isNearBench: true);
		}
	}

	private void OnTriggerExit2D(Collider2D otherObject)
	{
		if (otherObject.gameObject.layer == 9)
		{
			heroCtrl.NearBench(isNearBench: false);
		}
	}
}