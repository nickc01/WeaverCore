using System;
using System.Linq;
using UnityEngine;

public class EnemyDreamnailReaction : MonoBehaviour
{
	// Token: 0x06002576 RID: 9590 RVA: 0x000D4FFC File Offset: 0x000D31FC
	protected void Reset()
	{
		convoAmount = 8;
		convoTitle = "GENERIC";
		startSuppressed = false;
	}

	// Token: 0x06002577 RID: 9591 RVA: 0x000D5018 File Offset: 0x000D3218
	protected void Start()
	{
		state = ((!startSuppressed) ? EnemyDreamnailReaction.States.Ready : EnemyDreamnailReaction.States.Suppressed);
	}

	// Token: 0x06002578 RID: 9592 RVA: 0x000D502C File Offset: 0x000D322C
	public void RecieveDreamImpact()
	{
		if (state != EnemyDreamnailReaction.States.Ready)
		{
			return;
		}
		if (!noSoul)
		{
			int amount = (!PlayerData.instance.GetBool("equippedCharm_30")) ? 33 : 66;
			//HeroController.instance.AddMPCharge(amount);
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
		dynamic flasher = GetComponent("WeaverCore.Components.SpriteFlasher");
		if (flasher != null)
		{
			flasher.flashDreamImpact();
		}
		//SpriteFlash component2 = base.gameObject.GetComponent<SpriteFlash>();
		//if (component2 != null)
		//{
		//	component2.flashDreamImpact();
		//}
		//var weaverCoreFlashT = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "WeaverCore").GetType("WeaverCore.Components.SpriteFlasher");

		state = EnemyDreamnailReaction.States.CoolingDown;
		cooldownTimeRemaining = 0.2f;
	}

	// Token: 0x06002579 RID: 9593 RVA: 0x000D5120 File Offset: 0x000D3320
	public void MakeReady()
	{
		if (state != EnemyDreamnailReaction.States.Suppressed)
		{
			return;
		}
		state = EnemyDreamnailReaction.States.Ready;
	}

	// Token: 0x0600257A RID: 9594 RVA: 0x000D5134 File Offset: 0x000D3334
	public void SetConvoTitle(string title)
	{
		convoTitle = title;
	}

	// Token: 0x0600257B RID: 9595 RVA: 0x000D5140 File Offset: 0x000D3340
	private void ShowConvo()
	{
	}

	// Token: 0x0600257C RID: 9596 RVA: 0x000D51B0 File Offset: 0x000D33B0
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

	// Token: 0x040028EC RID: 10476
	private const int RegularMPGain = 33;

	// Token: 0x040028ED RID: 10477
	private const int BoostedMPGain = 66;

	// Token: 0x040028EE RID: 10478
	private const float AttackMagnitude = 2f;

	// Token: 0x040028EF RID: 10479
	private const float CooldownDuration = 0.2f;

	// Token: 0x040028F0 RID: 10480
	[SerializeField]
	private int convoAmount;

	// Token: 0x040028F1 RID: 10481
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

	// Token: 0x040028F5 RID: 10485
	public bool allowUseChildColliders;

	// Token: 0x040028F6 RID: 10486
	private EnemyDreamnailReaction.States state;

	// Token: 0x040028F7 RID: 10487
	private float cooldownTimeRemaining;

	// Token: 0x02000801 RID: 2049
	private enum States
	{
		// Token: 0x040028F9 RID: 10489
		Suppressed,
		// Token: 0x040028FA RID: 10490
		Ready,
		// Token: 0x040028FB RID: 10491
		CoolingDown
	}
}