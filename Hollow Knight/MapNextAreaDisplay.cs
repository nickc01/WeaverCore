using UnityEngine;

public class MapNextAreaDisplay : MonoBehaviour
{
    [HideInInspector]
	public GameMap gameMap;

	public string visitedString;

	private PlayerData pd;

	private bool activated = true;

	private bool areaVisited;

	private void OnEnable()
	{
		if (pd == null)
		{
			pd = GameManager.instance.playerData;
		}
		if (visitedString == "")
		{
			areaVisited = true;
		}
		if (!areaVisited)
		{
			areaVisited = pd.GetBool(visitedString);
		}
		if (activated)
		{
			if (!areaVisited || !gameMap.displayNextArea)
			{
				DeactivateChildren();
			}
		}
		else if (areaVisited && gameMap.displayNextArea)
		{
			ActivateChildren();
		}
	}

	private void ActivateChildren()
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: true);
			activated = true;
		}
	}

	private void DeactivateChildren()
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: false);
			activated = false;
		}
	}
}
