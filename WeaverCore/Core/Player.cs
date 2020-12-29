using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Assets;
using WeaverCore.DataTypes;
using WeaverCore.Enums;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore
{
	// Token: 0x0200002A RID: 42
	public class Player : MonoBehaviour
	{
		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060000C3 RID: 195 RVA: 0x00004D60 File Offset: 0x00002F60
		public static IEnumerable<Player> AllPlayers
		{
			get
			{
				return Player.Players;
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060000C4 RID: 196 RVA: 0x00004D67 File Offset: 0x00002F67
		public static Player Player1
		{
			get
			{
				return (Player.Players.Count <= 0) ? null : Player.Players[0];
			}
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00004D8C File Offset: 0x00002F8C
		public static Player NearestPlayer(Vector3 position)
		{
			float num = float.PositiveInfinity;
			Player result = null;
			foreach (Player player in Player.Players)
			{
				float num2 = Vector3.Distance(player.transform.position, position);
				if (num2 < num)
				{
					num = num2;
					result = player;
				}
			}
			return result;
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00004E0C File Offset: 0x0000300C
		public static Player NearestPlayer(Component component)
		{
			return Player.NearestPlayer(component.transform.position);
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x00004E1E File Offset: 0x0000301E
		public static Player NearestPlayer(Transform transform)
		{
			return Player.NearestPlayer(transform.position);
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x00004E2B File Offset: 0x0000302B
		public static Player NearestPlayer(GameObject gameObject)
		{
			return Player.NearestPlayer(gameObject.transform.position);
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00004E3D File Offset: 0x0000303D
		public virtual void PlayAttackSlash(GameObject target, HitInfo hit, Vector3 effectsOffset = default(Vector3))
		{
			this.PlayAttackSlash((base.transform.position + target.transform.position) * 0.5f + effectsOffset, hit);
		}

		// Token: 0x060000CA RID: 202 RVA: 0x00004E74 File Offset: 0x00003074
		public virtual void PlayAttackSlash(Vector3 target, HitInfo hit)
		{
			Player.NailStrikePool.Instantiate(target, Quaternion.identity);
			GameObject gameObject = Player.SlashImpactPool.Instantiate(target, Quaternion.identity);
			switch (DirectionUtilities.DegreesToDirection(hit.Direction))
			{
				case CardinalDirection.Up:
					this.SetRotation2D(gameObject.transform, (float)UnityEngine.Random.Range(70, 110));
					gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
					break;
				case CardinalDirection.Down:
					this.SetRotation2D(gameObject.transform, (float)UnityEngine.Random.Range(70, 110));
					gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
					break;
				case CardinalDirection.Left:
					this.SetRotation2D(gameObject.transform, (float)UnityEngine.Random.Range(340, 380));
					gameObject.transform.localScale = new Vector3(-1.5f, 1.5f, 1f);
					break;
				case CardinalDirection.Right:
					this.SetRotation2D(gameObject.transform, (float)UnityEngine.Random.Range(340, 380));
					gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
					break;
			}
		}

		// Token: 0x060000CB RID: 203 RVA: 0x00004FC4 File Offset: 0x000031C4
		private void SetRotation2D(Transform t, float rotation)
		{
			Vector3 eulerAngles = t.eulerAngles;
			eulerAngles.z = rotation;
			t.eulerAngles = eulerAngles;
		}

		// Token: 0x060000CC RID: 204 RVA: 0x00004FE8 File Offset: 0x000031E8
		private void Awake()
		{
			if (Player.NailStrikePool == null)
			{
				Player.NailStrikePool = new WeaverCore.Utilities.ObjectPool(EffectAssets.NailStrikePrefab, PoolLoadType.Local);
				Player.NailStrikePool.FillPool(1);
				Player.SlashImpactPool = new WeaverCore.Utilities.ObjectPool(EffectAssets.SlashImpactPrefab, PoolLoadType.Local);
				Player.SlashImpactPool.FillPool(1);
			}
			Type implementationType = ImplFinder.GetImplementationType<Player_I>();
			this.impl = (Player_I)base.gameObject.AddComponent(implementationType);
			this.impl.Initialize();
		}

		// Token: 0x060000CD RID: 205 RVA: 0x0000505D File Offset: 0x0000325D
		private void Start()
		{
			Player.Players.AddIfNotContained(this);
		}

		// Token: 0x060000CE RID: 206 RVA: 0x0000505D File Offset: 0x0000325D
		private void OnEnable()
		{
			Player.Players.AddIfNotContained(this);
		}

		// Token: 0x060000CF RID: 207 RVA: 0x0000506B File Offset: 0x0000326B
		private void OnDisable()
		{
			Player.Players.Remove(this);
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x0000506B File Offset: 0x0000326B
		private void OnDestroy()
		{
			Player.Players.Remove(this);
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x00005079 File Offset: 0x00003279
		public void SoulGain()
		{
			this.impl.SoulGain();
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x00005086 File Offset: 0x00003286
		public void RefreshSoulUI()
		{
			this.impl.RefreshSoulUI();
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x00005093 File Offset: 0x00003293
		public void EnterParryState()
		{
			this.impl.EnterParryState();
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x000050A0 File Offset: 0x000032A0
		public void RecoverFromParry()
		{
			this.impl.RecoverFromParry();
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x000050AD File Offset: 0x000032AD
		public void Recoil(CardinalDirection recoilDirection)
		{
			this.impl.Recoil(recoilDirection);
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000D6 RID: 214 RVA: 0x000050BB File Offset: 0x000032BB
		public bool HasDreamNail
		{
			get
			{
				return this.impl.HasDreamNail;
			}
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x000050C8 File Offset: 0x000032C8
		public bool HasCharmEquipped(int charmNumber)
		{
			return this.impl.HasCharmEquipped(charmNumber);
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000D8 RID: 216 RVA: 0x000050D6 File Offset: 0x000032D6
		// (set) Token: 0x060000D9 RID: 217 RVA: 0x000050E3 File Offset: 0x000032E3
		public int EssenceCollected
		{
			get
			{
				return this.impl.EssenceCollected;
			}
			set
			{
				this.impl.EssenceCollected = value;
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000DA RID: 218 RVA: 0x000050F1 File Offset: 0x000032F1
		// (set) Token: 0x060000DB RID: 219 RVA: 0x000050FE File Offset: 0x000032FE
		public int EssenceSpent
		{
			get
			{
				return this.impl.EssenceSpent;
			}
			set
			{
				this.impl.EssenceSpent = value;
			}
		}

		// Token: 0x040000AE RID: 174
		private static WeaverCore.Utilities.ObjectPool NailStrikePool;

		// Token: 0x040000AF RID: 175
		private static WeaverCore.Utilities.ObjectPool SlashImpactPool;

		// Token: 0x040000B0 RID: 176
		private static List<Player> Players = new List<Player>();

		// Token: 0x040000B1 RID: 177
		private Player_I impl;
	}
}
