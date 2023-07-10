using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore.Assets;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore
{
	/// <summary>
	/// A class for accessing and doing things related to the player
	/// </summary>
    public class Player : MonoBehaviour
	{
		static object[] paramCache = new object[1];

		HeroController heroCtrl;
		static ObjectPool NailStrikePool;
		static ObjectPool SlashImpactPool;

		static List<Player> _players;

		static List<Player> Players
        {
            get
            {
                if (_players == null)
                {
					_players = new List<Player>();
					_players.AddRange(GameObject.FindObjectsOfType<Player>());

					foreach (var player in _players)
					{
                        paramCache[0] = player;
                        //WeaverLog.Log("P_INIT_G");
                        foreach (var (method, _) in ReflectionUtilities.GetMethodsWithAttribute<OnPlayerInit>())
                        {
                            method.Invoke(null, paramCache);
                        }
                    }
                }
				return _players;
            }
        }
		Player_I _impl;

		Player_I Impl
        {
			get
            {
                if (_impl == null)
                {
					Type implementationType = ImplFinder.GetImplementationType<Player_I>();
					_impl = (Player_I)gameObject.AddComponent(implementationType);
					_impl.Initialize();
				}
				return _impl;
            }
        }

		/// <summary>
		/// A list of all players in the game.
		/// </summary>
		public static IEnumerable<Player> AllPlayers
		{
			get
			{
				return Player.Players;
			}
		}

		/// <summary>
		/// The main player in the game
		/// </summary>
		public static Player Player1
		{
			get
			{
#if UNITY_EDITOR
                if (Players.Count == 0)
                {
					throw new Exception("There is no test player currently in the game. You can add one by going to \"WeaverCore\" -> \"Insert\" -> \"Demo Player\" at the top menu bar");
                }
#endif
				return (Player.Players.Count <= 0) ? null : Player.Players[0];
			}
		}

		/// <summary>
		/// Gets the player that is nearest to a certain position
		/// </summary>
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

		/// <summary>
		/// Gets the player that is nearest to a certain object
		/// </summary>
		public static Player NearestPlayer(Component component)
		{
			return Player.NearestPlayer(component.transform.position);
		}

		/// <summary>
		/// Gets the player that is nearest to a certain object
		/// </summary>
		public static Player NearestPlayer(Transform transform)
		{
			return Player.NearestPlayer(transform.position);
		}

		/// <summary>
		/// Gets the player that is nearest to a certain object
		/// </summary>
		public static Player NearestPlayer(GameObject gameObject)
		{
			return Player.NearestPlayer(gameObject.transform.position);
		}


		/// <summary>
		/// Plays a nail slash effect. Used when the player hits an enemy
		/// </summary>
		/// <param name="target">The target object receiving the hit</param>
		/// <param name="hit">The hit on the enemy</param>
		/// <param name="effectsOffset">An offset applied to the effects</param>
		public virtual void PlayAttackSlash(GameObject target, HitInfo hit, Vector3 effectsOffset = default(Vector3))
		{
			this.PlayAttackSlash((base.transform.position + target.transform.position) * 0.5f + effectsOffset, hit);
		}

		/// <summary>
		/// Plays a nail slash effect. Used when the player hits an enemy
		/// </summary>
		/// <param name="target">The target object receiving the hit</param>
		/// <param name="hit">The hit on the enemy</param>
		public virtual void PlayAttackSlash(Vector3 target, HitInfo hit)
		{
			Player.NailStrikePool.Instantiate(target, Quaternion.identity);

			GameObject gameObject = Player.SlashImpactPool.Instantiate(target, Quaternion.identity);
			switch (DirectionUtilities.DegreesToDirection(hit.Direction))
			{
				case CardinalDirection.Up:
					gameObject.transform.SetRotation2D(UnityEngine.Random.Range(70, 110));
					gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
					break;
				case CardinalDirection.Down:
					gameObject.transform.SetRotation2D(UnityEngine.Random.Range(70, 110));
					gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
					break;
				case CardinalDirection.Left:
					gameObject.transform.SetRotation2D(UnityEngine.Random.Range(340, 380));
					gameObject.transform.localScale = new Vector3(-1.5f, 1.5f, 1f);
					break;
				case CardinalDirection.Right:
					gameObject.transform.SetRotation2D(UnityEngine.Random.Range(340, 380));
					gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
					break;
			}
		}

		private void Awake()
		{
			gameObject.tag = "Player";
			if (Player.NailStrikePool == null)
			{
				Player.NailStrikePool = ObjectPool.Create(EffectAssets.NailStrikePrefab);
				Player.NailStrikePool.FillPool(1);
				Player.SlashImpactPool = ObjectPool.Create(EffectAssets.SlashImpactPrefab);
				Player.SlashImpactPool.FillPool(1);
			}
			heroCtrl = GetComponent<HeroController>();
		}

		private void Start()
		{
			if (Player.Players.AddIfNotContained(this))
			{
				paramCache[0] = this;
				//WeaverLog.Log("P_INIT_A");
                foreach (var (method, _) in ReflectionUtilities.GetMethodsWithAttribute<OnPlayerInit>())
				{
					method.Invoke(null, paramCache);
				}
			}
		}

		private void OnEnable()
		{
			if (Player.Players.AddIfNotContained(this))
			{
                paramCache[0] = this;
                //WeaverLog.Log("P_INIT_B");
                foreach (var (method, _) in ReflectionUtilities.GetMethodsWithAttribute<OnPlayerInit>())
                {
                    method.Invoke(null, paramCache);
                }
            }
		}

		private void OnDisable()
		{
			if (Player.Players.Remove(this))
			{
                paramCache[0] = this;
                //WeaverLog.Log("P_INIT_C");
                foreach (var (method, _) in ReflectionUtilities.GetMethodsWithAttribute<OnPlayerUninit>())
                {
                    method.Invoke(null, paramCache);
                }
            }
		}

		private void OnDestroy()
		{
			if (Player.Players.Remove(this))
			{
                paramCache[0] = this;
                //WeaverLog.Log("P_INIT_D");
                foreach (var (method, _) in ReflectionUtilities.GetMethodsWithAttribute<OnPlayerUninit>())
                {
                    method.Invoke(null, paramCache);
                }
            }
		}

		/// <summary>
		/// Causes the player to gain soul
		/// </summary>
		public void SoulGain()
		{
			this.Impl.SoulGain();
		}

		/// <summary>
		/// Refreshes the soul UI
		/// </summary>
		public void RefreshSoulUI()
		{
			this.Impl.RefreshSoulUI();
		}

		/// <summary>
		/// The player enters a parry state, where they are very shortly invincible
		/// </summary>
		public void EnterParryState()
		{
			this.Impl.EnterParryState();
		}

		/// <summary>
		/// The player exits a parry state
		/// </summary>
		public void RecoverFromParry()
		{
			this.Impl.RecoverFromParry();
		}

		/// <summary>
		/// Makes the player recoil from an attack
		/// </summary>
		/// <param name="recoilDirection">The direction the player should recoil in</param>
		public void Recoil(CardinalDirection recoilDirection)
		{
			this.Impl.Recoil(recoilDirection);
		}

		/// <summary>
		/// Does the player have a dream nail?
		/// </summary>
		public bool HasDreamNail
		{
			get
			{
				return this.Impl.HasDreamNail;
			}
		}

		/// <summary>
		/// Does the player have a certain charm equipped?
		/// </summary>
		/// <param name="charmNumber">The charm ID to check for</param>
		public bool HasCharmEquipped(int charmNumber)
		{
			return this.Impl.HasCharmEquipped(charmNumber);
		}

		/// <summary>
		/// The amount of essence the player has collected
		/// </summary>
		public int EssenceCollected
		{
			get
			{
				return this.Impl.EssenceCollected;
			}
			set
			{
				this.Impl.EssenceCollected = value;
			}
		}

		/// <summary>
		/// The amount of essence the player has spent
		/// </summary>
		public int EssenceSpent
		{
			get
			{
				return this.Impl.EssenceSpent;
			}
			set
			{
				this.Impl.EssenceSpent = value;
			}
		}

		/// <summary>
		/// Causes the player to enter a locked state during an enemy roar
		/// </summary>
		public void EnterRoarLock()
		{
			Impl.EnterRoarLock();
		}

		/// <summary>
		/// Causes the player to exit from a locked roar state
		/// </summary>
		public void ExitRoarLock()
		{
			Impl.ExitRoarLock();
		}

		public void EnterCutsceneLock(bool playSound, int darknessLevel = -1)
		{
			Impl.EnterCutsceneLock(playSound);
		}

		public void ExitCutsceneLock()
		{
			Impl.ExitCutsceneLock();
		}
	}
}
