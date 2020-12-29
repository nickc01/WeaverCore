using System;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Components.DeathEffects
{
	// Token: 0x0200001F RID: 31
	public class InfectedDeathEffects : BasicDeathEffects
	{
		// Token: 0x06000098 RID: 152 RVA: 0x00003FB4 File Offset: 0x000021B4
		public override void EmitEffects()
		{
			if (this.DeathType != InfectedDeathEffects.InfectedDeathType.SmallInfected)
			{
				if (this.DeathType != InfectedDeathEffects.InfectedDeathType.LargeInfected)
				{
					if (this.DeathType != InfectedDeathEffects.InfectedDeathType.Infected)
					{
						Debug.LogWarningFormat(this, "Enemy death type {0} not implemented!", new object[]
						{
							this.DeathType
						});
					}
					else
					{
						this.EmitInfectedEffects();
					}
				}
				else
				{
					this.EmitLargeInfectedEffects();
				}
			}
			else
			{
				this.EmitSmallInfectedEffects();
			}
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00004024 File Offset: 0x00002224
		private void EmitInfectedEffects()
		{
			if (this.SwordDeathSound != null)
			{
				WeaverAudioPlayer weaverAudioPlayer = WeaverAudio.PlayAtPoint(this.SwordDeathSound, base.transform.position, this.swordDeathSoundVolume, AudioChannel.Sound);
				weaverAudioPlayer.AudioSource.pitch = UnityEngine.Random.Range(this.swordDeathSoundMinPitch, this.swordDeathSoundMaxPitch);
			}
			if (this.DamageSound != null)
			{
				WeaverAudioPlayer weaverAudioPlayer2 = WeaverAudio.PlayAtPoint(this.DamageSound, base.transform.position, this.damageSoundVolume, AudioChannel.Sound);
				weaverAudioPlayer2.AudioSource.pitch = UnityEngine.Random.Range(this.damageSoundMinPitch, this.damageSoundMaxPitch);
			}
			if (this.InfectedDeathWavePrefab != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.InfectedDeathWavePrefab, base.transform.position + this.EffectsOffset, Quaternion.identity);
			}
			base.gameObject.transform.SetXLocalScale(1.25f);
			base.gameObject.transform.SetYLocalScale(1.25f);
			Blood.SpawnRandomBlood(base.transform.position + this.EffectsOffset);
			if (this.DeathPuffPrefab != null)
			{
				UnityEngine.Object.Instantiate<GameObject>(this.DeathPuffPrefab, base.transform.position + this.EffectsOffset, Quaternion.identity);
			}
			this.ShakeCamera(ShakeType.EnemyKillShake);
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00004188 File Offset: 0x00002388
		private void ShakeCamera(ShakeType shakeType = ShakeType.EnemyKillShake)
		{
			Renderer renderer = base.GetComponent<Renderer>();
			if (renderer == null)
			{
				renderer = base.GetComponentInChildren<Renderer>();
			}
			if (renderer != null && renderer.isVisible)
			{
				WeaverCam.Instance.Shaker.Shake(shakeType);
			}
		}

		// Token: 0x0600009B RID: 155 RVA: 0x000041D8 File Offset: 0x000023D8
		private void EmitSmallInfectedEffects()
		{
			if (this.SwordDeathSound != null)
			{
				WeaverAudioPlayer weaverAudioPlayer = WeaverAudio.PlayAtPoint(this.SwordDeathSound, base.transform.position, this.swordDeathSoundVolume, AudioChannel.Sound);
				weaverAudioPlayer.AudioSource.pitch = UnityEngine.Random.Range(this.swordDeathSoundMinPitch, this.swordDeathSoundMaxPitch);
			}
			if (this.DamageSound != null)
			{
				WeaverAudioPlayer weaverAudioPlayer2 = WeaverAudio.PlayAtPoint(this.DamageSound, base.transform.position, this.damageSoundVolume, AudioChannel.Sound);
				weaverAudioPlayer2.AudioSource.pitch = UnityEngine.Random.Range(this.damageSoundMinPitch, this.damageSoundMaxPitch);
			}
			if (this.InfectedDeathWavePrefab != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.InfectedDeathWavePrefab, base.transform.position + this.EffectsOffset, Quaternion.identity);
				Vector3 localScale = gameObject.transform.localScale;
				localScale.x = 0.5f;
				localScale.y = 0.5f;
				gameObject.transform.localScale = localScale;
			}
			Blood.SpawnRandomBlood(base.transform.position + this.EffectsOffset);
		}

		// Token: 0x0600009C RID: 156 RVA: 0x00004300 File Offset: 0x00002500
		private void EmitLargeInfectedEffects()
		{
			if (this.SwordDeathSound != null)
			{
				WeaverAudioPlayer weaverAudioPlayer = WeaverAudio.PlayAtPoint(this.SwordDeathSound, base.transform.position, this.swordDeathSoundVolume, AudioChannel.Sound);
				weaverAudioPlayer.AudioSource.pitch = UnityEngine.Random.Range(this.swordDeathSoundMinPitch, this.swordDeathSoundMaxPitch);
			}
			if (this.DamageSound != null)
			{
				WeaverAudioPlayer weaverAudioPlayer2 = WeaverAudio.PlayAtPoint(this.DamageSound, base.transform.position, this.damageSoundVolume, AudioChannel.Sound);
				weaverAudioPlayer2.AudioSource.pitch = UnityEngine.Random.Range(this.damageSoundMinPitch, this.damageSoundMaxPitch);
			}
			if (this.DeathPuffPrefab != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.DeathPuffPrefab, base.transform.position + this.EffectsOffset, Quaternion.identity);
				gameObject.transform.localScale = new Vector3(2f, 2f, gameObject.transform.GetZLocalScale());
			}
			this.ShakeCamera(ShakeType.AverageShake);
			if (this.InfectedDeathWavePrefab != null)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.InfectedDeathWavePrefab, base.transform.position + this.EffectsOffset, Quaternion.identity);
				gameObject2.transform.SetXLocalScale(2f);
				gameObject2.transform.SetYLocalScale(2f);
			}
			Blood.SpawnRandomBlood(base.transform.position + this.EffectsOffset);
		}

		// Token: 0x0400006F RID: 111
		public InfectedDeathEffects.InfectedDeathType DeathType;

		// Token: 0x04000070 RID: 112
		[SerializeField]
		protected GameObject InfectedDeathWavePrefab;

		// Token: 0x04000071 RID: 113
		[SerializeField]
		protected GameObject DeathPuffPrefab;

		// Token: 0x04000072 RID: 114
		[SerializeField]
		protected AudioClip DamageSound;

		// Token: 0x04000073 RID: 115
		[SerializeField]
		protected float damageSoundVolume;

		// Token: 0x04000074 RID: 116
		[SerializeField]
		protected float damageSoundMinPitch;

		// Token: 0x04000075 RID: 117
		[SerializeField]
		protected float damageSoundMaxPitch;

		// Token: 0x04000076 RID: 118
		[SerializeField]
		protected AudioClip SwordDeathSound;

		// Token: 0x04000077 RID: 119
		[SerializeField]
		protected float swordDeathSoundVolume;

		// Token: 0x04000078 RID: 120
		[SerializeField]
		protected float swordDeathSoundMinPitch;

		// Token: 0x04000079 RID: 121
		[SerializeField]
		protected float swordDeathSoundMaxPitch;

		// Token: 0x02000020 RID: 32
		public enum InfectedDeathType
		{
			// Token: 0x0400007B RID: 123
			Infected,
			// Token: 0x0400007C RID: 124
			SmallInfected,
			// Token: 0x0400007D RID: 125
			LargeInfected
		}
	}
}
