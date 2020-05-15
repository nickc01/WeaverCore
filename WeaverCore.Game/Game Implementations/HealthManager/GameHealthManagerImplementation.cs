using IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Game.Implementations
{
    public class GameHealthManagerImplementation : WeaverCore.Implementations.HealthManagerImplementation, IHitResponder
    {
		[Serializable]
		class GamePrefabs
		{
			public AudioSource audioPlayerPrefab;
			public AudioEvent regularInvincibleAudio;
			public GameObject blockHitPrefab;
			public GameObject strikeNailPrefab;
			public GameObject slashImpactPrefab;
			public GameObject fireballHitPrefab;
			public GameObject sharpShadowImpactPrefab;
			public GameObject corpseSplatPrefab;
			public AudioEvent enemyDeathSwordAudio;
			public AudioEvent enemyDamageAudio;
			public GameObject smallGeoPrefab;
			public GameObject mediumGeoPrefab;
			public GameObject largeGeoPrefab;
		}

		static GamePrefabs Prefabs;

		Collider2D boxCollider;

		void Awake()
		{
			if (Prefabs == null)
			{
				Prefabs = JsonUtility.FromJson<GamePrefabs>(@"{""audioPlayerPrefab"":{""m_FileID"":7478,""m_PathID"":0},""regularInvincibleAudio"":{""Clip"":{""m_FileID"":14884,""m_PathID"":0},""PitchMin"":0.75,""PitchMax"":1.25,""Volume"":1.0},""blockHitPrefab"":{""m_FileID"":14022,""m_PathID"":0},""strikeNailPrefab"":{""m_FileID"":9966,""m_PathID"":0},""slashImpactPrefab"":{""m_FileID"":9746,""m_PathID"":0},""fireballHitPrefab"":{""m_FileID"":14886,""m_PathID"":0},""sharpShadowImpactPrefab"":{""m_FileID"":18624,""m_PathID"":0},""corpseSplatPrefab"":{""m_FileID"":11292,""m_PathID"":0},""enemyDeathSwordAudio"":{""Clip"":{""m_FileID"":24710,""m_PathID"":0},""PitchMin"":0.75,""PitchMax"":1.25,""Volume"":1.0},""enemyDamageAudio"":{""Clip"":{""m_FileID"":24712,""m_PathID"":0},""PitchMin"":0.75,""PitchMax"":1.25,""Volume"":1.0},""smallGeoPrefab"":{""m_FileID"":23278,""m_PathID"":0},""mediumGeoPrefab"":{""m_FileID"":22882,""m_PathID"":0},""largeGeoPrefab"":{""m_FileID"":21948,""m_PathID"":0}}");
			}
			boxCollider = GetComponent<Collider2D>();
		}

        public override void ReceiveHit(HitInfo info)
        {
            if (Manager.Health <= 0 || Manager.EvasionTimeLeft > 0.0f || info.Damage <= 0 || gameObject.activeSelf == false)
            {
                return;
            }

            var hitInstance = Misc.ConvertHitInfo(info);

            FSMUtility.SendEventToGameObject(hitInstance.Source, "DEALT DAMAGE", false);

            int cardinalDirection = DirectionUtils.GetCardinalDirection(hitInstance.Direction);
			if (!Manager.Invincible || ((hitInstance.AttackType == AttackTypes.Spell || hitInstance.AttackType == AttackTypes.SharpShadow) && gameObject.CompareTag("Spell Vulnerable")))
			{
				TakeDamage(hitInstance);
			}
			else
			{
				Invincible(hitInstance);
			}
		}


        void TakeDamage(HitInstance hitInstance)
        {
			if (CheatManager.IsInstaKillEnabled)
			{
				hitInstance.DamageDealt = 9999;
			}
			int cardinalDirection = DirectionUtils.GetCardinalDirection(hitInstance.Direction);
			FSMUtility.SendEventToGameObject(hitInstance.Source, "HIT LANDED", false);

			/*if (this.recoil != null)
			{
				this.recoil.RecoilByDirection(cardinalDirection, hitInstance.MagnitudeMultiplier);
			}*/
			switch (hitInstance.AttackType)
			{
				case AttackTypes.Nail:
				case AttackTypes.NailBeam:
					{
						if (hitInstance.AttackType == AttackTypes.Nail && Manager.GainSoul)
						{
							HeroController.instance.SoulGain();
						}
						Vector3 position = (hitInstance.Source.transform.position + transform.position) * 0.5f + Manager.EffectsOffset;
						Prefabs.strikeNailPrefab.Spawn(position, Quaternion.identity);
						GameObject slashImpact = Prefabs.slashImpactPrefab.Spawn(position, Quaternion.identity);
						switch (cardinalDirection)
						{
							case 0:
								slashImpact.transform.SetRotation2D((float)UnityEngine.Random.Range(340, 380));
								slashImpact.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
								break;
							case 1:
								slashImpact.transform.SetRotation2D((float)UnityEngine.Random.Range(70, 110));
								slashImpact.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
								break;
							case 2:
								slashImpact.transform.SetRotation2D((float)UnityEngine.Random.Range(340, 380));
								slashImpact.transform.localScale = new Vector3(-1.5f, 1.5f, 1f);
								break;
							case 3:
								slashImpact.transform.SetRotation2D((float)UnityEngine.Random.Range(250, 290));
								slashImpact.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
								break;
						}
						break;
					}
				case AttackTypes.Generic:
					Prefabs.strikeNailPrefab.Spawn(base.transform.position + Manager.EffectsOffset, Quaternion.identity).transform.SetPositionZ(0.0031f);
					break;
				case AttackTypes.Spell:
					Prefabs.fireballHitPrefab.Spawn(base.transform.position + Manager.EffectsOffset, Quaternion.identity).transform.SetPositionZ(0.0031f);
					break;
				case AttackTypes.SharpShadow:
					Prefabs.sharpShadowImpactPrefab.Spawn(base.transform.position + Manager.EffectsOffset, Quaternion.identity);
					break;
			}
			/*if (this.hitEffectReceiver != null && hitInstance.AttackType != AttackTypes.RuinsWater)
			{
				this.hitEffectReceiver.RecieveHitEffect(hitInstance.GetActualDirection(base.transform));
			}*/
			//int num = Mathf.RoundToInt((float)hitInstance.DamageDealt * hitInstance.Multiplier);
			//this.hp = Mathf.Max(this.hp - num, -50);
			Manager.Health -= Mathf.Min(hitInstance.DamageDealt, Manager.Health);
			if (Manager.Health > 0)
			{
				Manager.EvasionTimeLeft = Manager.EvasionTime;
				/*this.NonFatalHit(hitInstance.IgnoreInvulnerable);
				if (this.stunControlFSM)
				{
					this.stunControlFSM.SendEvent("STUN DAMAGE");
				}*/
			}
			else
			{
				//this.Die(new float?(hitInstance.GetActualDirection(base.transform)), hitInstance.AttackType, hitInstance.IgnoreInvulnerable);
			}
		}

        void Invincible(HitInstance hitInstance)
        {
			int cardinalDirection = DirectionUtils.GetCardinalDirection(hitInstance.Direction);
			FSMUtility.SendEventToGameObject(hitInstance.Source, "HIT LANDED", false);
			if (Manager.DeflectBlows)
			{
				if (hitInstance.AttackType == AttackTypes.Nail)
				{
					if (cardinalDirection == 0)
					{
						HeroController.instance.RecoilLeft();
					}
					else if (cardinalDirection == 2)
					{
						HeroController.instance.RecoilRight();
					}
				}
				GameManager.instance.FreezeMoment(1);
				GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");
				Vector2 v;
				Vector3 eulerAngles;
				if (boxCollider != null)
				{
					switch (cardinalDirection)
					{
						case 0:
							v = new Vector2(boxCollider.bounds.min.x, hitInstance.Source.transform.GetPositionY());
							eulerAngles = new Vector3(0f, 0f, 0f);
							FSMUtility.SendEventToGameObject(gameObject, "BLOCKED HIT R", false);
							break;
						case 1:
							v = new Vector2(hitInstance.Source.transform.GetPositionX(), Mathf.Max(hitInstance.Source.transform.GetPositionY(), boxCollider.bounds.min.y));
							eulerAngles = new Vector3(0f, 0f, 90f);
							FSMUtility.SendEventToGameObject(gameObject, "BLOCKED HIT U", false);
							break;
						case 2:
							v = new Vector2(boxCollider.bounds.max.x, hitInstance.Source.transform.GetPositionY());
							eulerAngles = new Vector3(0f, 0f, 180f);
							FSMUtility.SendEventToGameObject(gameObject, "BLOCKED HIT L", false);
							break;
						case 3:
							v = new Vector2(hitInstance.Source.transform.GetPositionX(), Mathf.Min(hitInstance.Source.transform.GetPositionY(), boxCollider.bounds.max.y));
							eulerAngles = new Vector3(0f, 0f, 270f);
							FSMUtility.SendEventToGameObject(gameObject, "BLOCKED DOWN", false);
							break;
						default:
							v = transform.position;
							eulerAngles = new Vector3(0f, 0f, 0f);
							break;
					}
				}
				else
				{
					v = transform.position;
					eulerAngles = new Vector3(0f, 0f, 0f);
				}
				GameObject blockHitInstance = Prefabs.blockHitPrefab.Spawn();
				blockHitInstance.transform.position = v;
				blockHitInstance.transform.eulerAngles = eulerAngles;

				Prefabs.regularInvincibleAudio.SpawnAndPlayOneShot(Prefabs.audioPlayerPrefab, transform.position);
				/*if (hasAlternateInvincibleSound)
				{
					AudioSource component = GetComponent<AudioSource>();
					if (alternateInvincibleSound != null && component != null)
					{
						component.PlayOneShot(alternateInvincibleSound);
					}
				}
				else
				{
					regularInvincibleAudio.SpawnAndPlayOneShot(audioPlayerPrefab, transform.position);
				}*/
			}
			Manager.EvasionTimeLeft = Manager.EvasionTime;
		}

		public override void OnDeath()
		{
			
		}

		void IHitResponder.Hit(HitInstance damageInstance)
		{
			Manager.ReceiveHit(Misc.ConvertHitInstance(damageInstance, transform));
		}

		/*bool IsBlockingByDirection(int cardinalDirection, AttackTypes attackType)
		{
			if ((attackType == AttackTypes.Spell || attackType == AttackTypes.SharpShadow) && base.gameObject.CompareTag("Spell Vulnerable"))
			{
				return false;
			}
			if (!invincible)
			{
				return false;
			}
			if (invincibleFromDirection == 0)
			{
				return true;
			}
			switch (cardinalDirection)
			{
				case 0:
					{
						int num = invincibleFromDirection;
						switch (num)
						{
							case 5:
							case 8:
							case 10:
								break;
							default:
								if (num != 1)
								{
									return false;
								}
								break;
						}
						return true;
					}
				case 1:
					switch (invincibleFromDirection)
					{
						case 2:
						case 5:
						case 6:
						case 7:
						case 8:
						case 9:
							return true;
					}
					return false;
				case 2:
					{
						int num2 = invincibleFromDirection;
						switch (num2)
						{
							case 3:
							case 6:
								break;
							default:
								switch (num2)
								{
									case 9:
									case 11:
										return true;
								}
								return false;
						}
						return true;
					}
				case 3:
					switch (invincibleFromDirection)
					{
						case 4:
						case 7:
						case 8:
						case 9:
						case 10:
						case 11:
							return true;
					}
					return false;
				default:
					return false;
			}
		}*/
	}
}
