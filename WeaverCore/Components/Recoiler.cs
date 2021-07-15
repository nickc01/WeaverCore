using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
	public class Recoiler : Recoil
	{
		float? _origRecoilSpeed = null;
		public float OriginalRecoilSpeed
		{
			get
			{
				if (_origRecoilSpeed == null)
				{
					_origRecoilSpeed = GetRecoilSpeed();
				}
				return _origRecoilSpeed.Value;
			}
			set
			{
				_origRecoilSpeed = value;
			}
		}


		/*[OnHarmonyPatch]
		static void HarmonyPatch(HarmonyPatcher patcher)
		{
			patcher.Patch(typeof(Recoiler).BaseType.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance), null, typeof(Recoiler).GetMethod(nameof(AwakePostfix),BindingFlags.Static | BindingFlags.NonPublic));
		}

		static void AwakePostfix(object __instance)
		{
			if (__instance is Recoiler recoiler)
			{
				recoiler.OriginalRecoilSpeed = recoiler.GetRecoilSpeed();
			}
		}*/
		public virtual void Start()
		{
			OriginalRecoilSpeed = GetRecoilSpeed();
		}

		public void RecoilByDamage(HitInfo hit)
		{
			int attackDirection = 0;
			CardinalDirection cardinalDirection = DirectionUtilities.DegreesToDirection(hit.Direction);
			if (cardinalDirection != CardinalDirection.Up)
			{
				if (cardinalDirection != CardinalDirection.Down)
				{
					if (cardinalDirection == CardinalDirection.Left)
					{
						attackDirection = 2;
					}
				}
				else
				{
					attackDirection = 3;
				}
			}
			else
			{
				attackDirection = 1;
			}
			base.RecoilByDirection(attackDirection, hit.AttackStrength);
		}

		public void RecoilByDirection(CardinalDirection attackDirection, float attackStrength)
		{
			int attackDirection2 = 0;
			if (attackDirection != CardinalDirection.Up)
			{
				if (attackDirection != CardinalDirection.Down)
				{
					if (attackDirection == CardinalDirection.Left)
					{
						attackDirection2 = 2;
					}
				}
				else
				{
					attackDirection2 = 3;
				}
			}
			else
			{
				attackDirection2 = 1;
			}
			base.RecoilByDirection(attackDirection2, attackStrength);
		}


		public void ResetRecoilSpeed()
		{
			SetRecoilSpeed(OriginalRecoilSpeed);
		}


		static Func<Recoil, float> recoilSpeedDel;


		public float GetRecoilSpeed()
		{
			if (recoilSpeedDel == null)
			{
				recoilSpeedDel = ReflectionUtilities.CreateFieldGetter<Recoil, float>(typeof(Recoiler).BaseType.GetField("recoilSpeedBase", BindingFlags.NonPublic | BindingFlags.Instance));
			}

			return recoilSpeedDel.Invoke(this);
		}
	}

#if UNITY_EDITOR

	/*public class RECOIL_BASE : MonoBehaviour
	{
		private Rigidbody2D body;
		private Collider2D bodyCollider;

		[SerializeField]
		public bool freezeInPlace;
		[SerializeField]
		private bool stopVelocityXWhenRecoilingUp;
		[SerializeField]
		private bool preventRecoilUp;
		[SerializeField]
		private float recoilSpeedBase = 15f;
		[SerializeField]
		private float recoilDuration = 0.15f;
		private bool skipFreezingByController;
		private Recoiler.States state;
		private float recoilTimeRemaining;
		private float recoilSpeed;

		public event Recoiler.FreezeEvent OnHandleFreeze;
		public event Recoiler.CancelRecoilEvent OnCancelRecoil;

		private const int SweepLayerMask = 256;

		Vector2 recoilDirection;

		public bool SkipFreezingByController
		{
			get
			{
				return this.skipFreezingByController;
			}
			set
			{
				this.skipFreezingByController = value;
			}
		}

		// Token: 0x17000280 RID: 640
		// (get) Token: 0x060026B1 RID: 9905 RVA: 0x000DC524 File Offset: 0x000DA724
		public bool IsRecoiling
		{
			get
			{
				return this.state == Recoiler.States.Recoiling || this.state == Recoiler.States.Frozen;
			}
		}

		static Vector2 DirectionToVector(int direction)
		{
			switch (direction)
			{
				case 1:
					return new Vector2(0f, 1f);
				case 2:
					return new Vector2(-1f, 0f);
				case 3:
					return new Vector2(0f, -1f);
				default:
					return new Vector2(1f,0f);
			}
		}

		// Token: 0x060026B2 RID: 9906 RVA: 0x000DC53C File Offset: 0x000DA73C
		protected void Reset()
		{
			this.freezeInPlace = false;
			this.stopVelocityXWhenRecoilingUp = false;
			this.recoilDuration = 0.15f;
			this.recoilSpeedBase = 15f;
			this.preventRecoilUp = false;
		}

		// Token: 0x060026B3 RID: 9907 RVA: 0x000DC56C File Offset: 0x000DA76C
		protected void Awake()
		{
			this.body = base.GetComponent<Rigidbody2D>();
			this.bodyCollider = base.GetComponent<Collider2D>();
		}

		// Token: 0x060026B4 RID: 9908 RVA: 0x000DC588 File Offset: 0x000DA788
		private void OnEnable()
		{
			this.CancelRecoil();
		}

		// Token: 0x060026B7 RID: 9911 RVA: 0x000DC628 File Offset: 0x000DA828
		public void RecoilByDirection(int attackDirection, float attackMagnitude)
		{
			if (this.state != Recoiler.States.Ready)
			{
				return;
			}
			if (this.freezeInPlace)
			{
				this.Freeze();
				return;
			}
			if (attackDirection == 1 && this.preventRecoilUp)
			{
				return;
			}
			if (this.bodyCollider == null)
			{
				this.bodyCollider = base.GetComponent<Collider2D>();
			}
			this.state = Recoiler.States.Recoiling;
			this.recoilSpeed = this.recoilSpeedBase * attackMagnitude;
			this.recoilTimeRemaining = this.recoilDuration;
			recoilDirection = DirectionToVector(attackDirection);
			this.UpdatePhysics(0f);
		}

		// Token: 0x060026B8 RID: 9912 RVA: 0x000DC748 File Offset: 0x000DA948
		public void CancelRecoil()
		{
			if (this.state != Recoiler.States.Ready)
			{
				this.state = Recoiler.States.Ready;
				if (this.OnCancelRecoil != null)
				{
					this.OnCancelRecoil();
				}
			}
		}

		// Token: 0x060026B9 RID: 9913 RVA: 0x000DC76C File Offset: 0x000DA96C
		private void Freeze()
		{
			if (this.skipFreezingByController)
			{
				if (this.OnHandleFreeze != null)
				{
					this.OnHandleFreeze();
				}
				this.state = Recoiler.States.Ready;
				return;
			}
			this.state = Recoiler.States.Frozen;
			if (this.body != null)
			{
				this.body.velocity = Vector2.zero;
			}
			this.recoilTimeRemaining = this.recoilDuration;
			this.UpdatePhysics(0f);
		}

		// Token: 0x060026BA RID: 9914 RVA: 0x000DC800 File Offset: 0x000DAA00
		protected void FixedUpdate()
		{
			this.UpdatePhysics(Time.fixedDeltaTime);
		}

		// Token: 0x060026BB RID: 9915 RVA: 0x000DC810 File Offset: 0x000DAA10
		private void UpdatePhysics(float deltaTime)
		{
			if (this.state == Recoiler.States.Frozen)
			{
				if (this.body != null)
				{
					this.body.velocity = Vector2.zero;
				}
				this.recoilTimeRemaining -= deltaTime;
				if (this.recoilTimeRemaining <= 0f)
				{
					this.CancelRecoil();
				}
			}
			else if (this.state == Recoiler.States.Recoiling)
			{
				//WeaverLog.Log("Recoil Direction = " + recoilDirection);
				//WeaverLog.Log("Recoil Speed = " + recoilSpeed);
				//WeaverLog.Log("Position Before = " + transform.position);
				transform.Translate(recoilDirection * recoilSpeed * Time.deltaTime,Space.World);
				//WeaverLog.Log("Position After = " + transform.position);
				this.recoilTimeRemaining -= deltaTime;
				if (this.recoilTimeRemaining <= 0f)
				{
					this.CancelRecoil();
				}
			}
		}

		// Token: 0x060026BC RID: 9916 RVA: 0x000DC8FC File Offset: 0x000DAAFC
		public void SetRecoilSpeed(float newSpeed)
		{
			this.recoilSpeedBase = newSpeed;
		}

		// Token: 0x02000841 RID: 2113
		// (Invoke) Token: 0x060026BE RID: 9918
		public delegate void FreezeEvent();

		// Token: 0x02000842 RID: 2114
		// (Invoke) Token: 0x060026C2 RID: 9922
		public delegate void CancelRecoilEvent();

		// Token: 0x02000843 RID: 2115
		private enum States
		{
			// Token: 0x04002A76 RID: 10870
			Ready,
			// Token: 0x04002A77 RID: 10871
			Frozen,
			// Token: 0x04002A78 RID: 10872
			Recoiling
		}
	}*/
#endif
}
