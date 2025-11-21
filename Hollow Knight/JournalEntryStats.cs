using UnityEngine;

public class JournalEntryStats : MonoBehaviour
{
	public GameObject frameObject;

	public GameObject newDotObject;

	public string playerDataName;

	public string convoName;

	public Sprite sprite;

	public bool warriorGhost;

	public bool grimmEntry;

	[Space(10f)]
	[Header("Below variables don't need to be filled out")]
	[Space(5f)]
	public string nameConvo;

	public string descConvo;

	public string notesConvo;

	public string playerDataKillsName;

	public string playerDataBoolName;

	public string playerDataNewDataName;

	public int itemNumber;

	private PlayerData pd;

	private float timer;

	private float topY = 5.78f;

	private float botY = -6.49f;

	private bool hidden = true;

	private float posOriginalY;

	private float posUpY;

	private float posDownY;

	private Vector3 scaleNormal = new Vector3(0.634f, 0.634f, 0.634f);

	private Vector3 scaleSmall = new Vector3(0.5f, 0.5f, 0.5f);

	private float centreTopY = 1.2f;

	private float centreBotY = -0.38f;

	private GameObject portrait;

	private GameObject nameObject;

	private GameObject frame;

	private GameObject newDot;

	private bool frameVisible;

	private bool dotVisible;

	private bool shrinkingDot;

	private float dotScale = 0.48f;

	private Transform dotTransform;

	private void Awake()
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		hidden = true;
		pd = PlayerData.instance;
		posOriginalY = base.transform.localPosition.y;
		posUpY = posOriginalY + 0.8f;
		posDownY = posOriginalY - 0.8f;
		portrait = base.transform.Find("Portrait").gameObject;
		nameObject = base.transform.Find("Name").gameObject;
		frame = Object.Instantiate(frameObject);
		frame.transform.parent = portrait.transform;
		frame.transform.localPosition = new Vector3(0f, 0f, -0.0001f);
		frame.transform.localScale = new Vector3(1f, 1f, 1f);
		portrait.SetActive(value: false);
		newDot = Object.Instantiate(newDotObject);
		newDot.transform.parent = base.transform;
		newDot.transform.localPosition = new Vector3(-0.65f, 0f, -0.0001f);
		newDot.SetActive(value: false);
		dotTransform = newDot.transform;
		nameConvo = "NAME_" + convoName;
		descConvo = "DESC_" + convoName;
		notesConvo = "NOTE_" + convoName;
		playerDataKillsName = "kills" + playerDataName;
		playerDataBoolName = "killed" + playerDataName;
		playerDataNewDataName = "newData" + playerDataName;
	}

	private void OnEnable()
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		hidden = true;
		if (!frameVisible && pd.GetInt(playerDataKillsName) <= 0)
		{
			frame.GetComponent<SpriteRenderer>().enabled = true;
			frameVisible = true;
		}
		if (!dotVisible && pd.GetBool(playerDataNewDataName))
		{
			newDot.GetComponent<SpriteRenderer>().enabled = true;
			dotVisible = true;
		}
	}

	private void OnDisable()
	{
		if (!dotVisible)
		{
			newDot.GetComponent<SpriteRenderer>().enabled = false;
		}
		shrinkingDot = false;
	}

	private void Update()
	{
		float y = base.transform.position.y;
		if (y > topY || y < botY)
		{
			if (!hidden)
			{
				foreach (Transform item in base.transform)
				{
					item.gameObject.SetActive(value: false);
				}
				hidden = true;
			}
		}
		else if (hidden)
		{
			foreach (Transform item2 in base.transform)
			{
				item2.gameObject.SetActive(value: true);
			}
			hidden = false;
		}
		if (!hidden)
		{
			if (y < centreBotY)
			{
				portrait.transform.localPosition = new Vector3(0f, -0.778f, 0f);
				portrait.transform.localScale = scaleSmall;
				nameObject.transform.localPosition = new Vector3(2.16f, -0.769f, 0f);
				newDot.transform.localPosition = new Vector3(-0.65f, -0.8f, -0.0001f);
			}
			else if (y > centreTopY)
			{
				portrait.transform.localPosition = new Vector3(0f, 0.822f, 0f);
				portrait.transform.localScale = scaleSmall;
				nameObject.transform.localPosition = new Vector3(2.16f, 0.831f, 0f);
				newDot.transform.localPosition = new Vector3(-0.65f, 0.8f, -0.0001f);
			}
			else
			{
				portrait.transform.localPosition = new Vector3(0f, 0.022f, 0f);
				portrait.transform.localScale = scaleNormal;
				nameObject.transform.localPosition = new Vector3(2.16f, 0.031f, 0f);
				newDot.transform.localPosition = new Vector3(-0.65f, 0f, -0.0001f);
				if (dotVisible)
				{
					shrinkingDot = true;
					dotVisible = false;
					pd.SetBool(playerDataNewDataName, value: false);
				}
			}
		}
		if (shrinkingDot)
		{
			dotScale -= Time.deltaTime * 3f;
			dotTransform.localScale = new Vector3(dotScale, dotScale, dotScale);
			if (dotScale <= 0f)
			{
				newDot.GetComponent<SpriteRenderer>().enabled = false;
				shrinkingDot = false;
			}
		}
	}

	public string GetNameConvo()
	{
		return nameConvo;
	}

	public string GetDescConvo()
	{
		return descConvo;
	}

	public string GetNotesConvo()
	{
		return notesConvo;
	}

	public string GetPlayerDataBoolName()
	{
		return playerDataBoolName;
	}

	public string GetPlayerDataKillsName()
	{
		return playerDataKillsName;
	}

	public string GetPlayerDataNewDataName()
	{
		return playerDataNewDataName;
	}

	public int GetItemNumber()
	{
		return itemNumber;
	}

	public Sprite GetSprite()
	{
		return sprite;
	}

	public bool GetWarriorGhost()
	{
		return warriorGhost;
	}

	public bool GetGrimm()
	{
		return grimmEntry;
	}
}
