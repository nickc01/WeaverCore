using System;
using System.Linq;
using UnityEngine;

public class EnemyDreamnailReaction : MonoBehaviour
{
	static Type spriteFlasherType;

	static Type GetFlasherType()
    {
        if (spriteFlasherType == null)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name == "WeaverCore")
                {
					spriteFlasherType = asm.GetType("WeaverCore.Components.SpriteFlasher");
					break;
                }
            }
        }
		return spriteFlasherType;
    }

	protected void Reset()
	{
		convoAmount = 8;
		convoTitle = "GENERIC";
		startSuppressed = false;
	}

	protected void Start()
	{
		state = ((!startSuppressed) ? EnemyDreamnailReaction.States.Ready : EnemyDreamnailReaction.States.Suppressed);
	}

	public void RecieveDreamImpact()
	{
		if (state != EnemyDreamnailReaction.States.Ready)
		{
			return;
		}
		if (!noSoul)
		{
			int amount = (!PlayerData.instance.GetBool("equippedCharm_30")) ? 33 : 66;
		}
		ShowConvo();
		if (dreamImpactPrefab != null)
		{
			GameObject.Instantiate(dreamImpactPrefab, transform.position, Quaternion.identity);
		}
		Recoil component = base.GetComponent<Recoil>();
		if (component != null)
		{
			bool flag = HeroController.instance.transform.localScale.x <= 0f;
			component.RecoilByDirection((!flag) ? 2 : 0, 2f);
		}

		//var flasherType = typeof(WeaverCore)

		Component[] flashers = GetComponentsInChildren(GetFlasherType());
		//Component flasher = GetComponent("WeaverCore.Components.SpriteFlasher");
		if (flashers != null)
		{
            foreach (var flasher in flashers)
            {
				flasher.SendMessage("flashDreamImpact");
			}
		}
		state = EnemyDreamnailReaction.States.CoolingDown;
		cooldownTimeRemaining = 0.2f;
	}

	public void MakeReady()
	{
		if (state != EnemyDreamnailReaction.States.Suppressed)
		{
			return;
		}
		state = EnemyDreamnailReaction.States.Ready;
	}

	public void SetConvoTitle(string title)
	{
		convoTitle = title;
	}

	private void ShowConvo()
	{
	}

	protected void Update()
	{
		if (state == EnemyDreamnailReaction.States.CoolingDown)
		{
			cooldownTimeRemaining -= Time.deltaTime;
			if (cooldownTimeRemaining <= 0f)
			{
				state = EnemyDreamnailReaction.States.Ready;
			}
		}
	}

	private const int RegularMPGain = 33;

	private const int BoostedMPGain = 66;

	private const float AttackMagnitude = 2f;

	private const float CooldownDuration = 0.2f;

	[SerializeField]
	private int convoAmount;

	[SerializeField]
	private string convoTitle;

	[SerializeField]
	[Tooltip("If enabled, this component will not react to dream nail hits until MakeReady() is called")]
	private bool startSuppressed;

	[SerializeField]
	[Tooltip("If set to true, dreamnailing this object will not give the player any soul")]
	private bool noSoul;

	[SerializeField]
	[Tooltip("If this field is not null, then it will spawn the prefab when this object is dreamnailed")]
	private GameObject dreamImpactPrefab;

	public bool allowUseChildColliders;

	private EnemyDreamnailReaction.States state;

	private float cooldownTimeRemaining;

	private enum States
	{
		Suppressed,
		Ready,
		CoolingDown
	}
}