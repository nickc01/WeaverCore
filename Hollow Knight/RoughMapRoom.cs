using System.Collections.Generic;
using UnityEngine;

public class RoughMapRoom : MonoBehaviour
{
	public Sprite fullSprite;

	public PlayerData pd;

	private GameManager gm;

	private SpriteRenderer sr;

	public bool fullSpriteDisplayed;

	private void Start()
	{
		gm = GameManager.instance;
		pd = PlayerData.instance;
		sr = GetComponent<SpriteRenderer>();
	}

	private void OnEnable()
	{
		if (gm == null)
		{
			gm = GameManager.instance;
		}
		if (sr == null)
		{
			sr = GetComponent<SpriteRenderer>();
		}
		if (!fullSpriteDisplayed && (gm.playerData.GetVariable<List<string>>("scenesMapped").Contains(transform.name) || gm.playerData.GetBool("mapAllRooms")))
		{
			sr.sprite = fullSprite;
			fullSpriteDisplayed = true;
		}
	}
}
