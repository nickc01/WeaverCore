using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components.DeathEffects
{
    /// <summary>
    /// Contains the most basic effects that are played on death. These are effects that virtually all enemies share
    /// </summary>
    public class BasicDeathEffects : MonoBehaviour, IDeathEffects
    {
        [Tooltip("The journal entry to unlock when the death effects are played")]
        public string JournalEntryName;

        [Tooltip("If set to true, the game will briefly slow down when playing the death effects")]
        public bool FreezeGameOnDeath;

        [Space]
        [SerializeField]
        [Tooltip("An positional offset applied to the effects when played")]
        protected Vector3 EffectsOffset;

        [HideInInspector]
        [SerializeField]
        [Tooltip("The prefab for the dream particles played on death. This has a slim chance of occuring upon death")]
        private GameObject EssenceCollectPrefab;

        [SerializeField]
        [Tooltip("When set to true, will cause the enemy to rarely give the player 1 essence upon death")]
        private bool doEssenceChance = true;

        /// <summary>
        /// Have the death effects been played already?
        /// </summary>
        public bool HasBeenPlayed { get; protected set; }

        protected virtual void Awake()
        {
            OnValidate();
        }

        protected virtual void OnValidate()
        {
            if (EssenceCollectPrefab == null)
            {
                EssenceCollectPrefab = WeaverAssets.LoadWeaverAsset<GameObject>("Corpse Dream Essence");
            }
        }

        /// <summary>
        /// Used for playing the death effects
        /// </summary>
        public virtual void EmitEffects() { }

        /// <summary>
        /// Used for playing the death sound effects
        /// </summary>
        public virtual void EmitSounds() { }

        /// <summary>
        /// Plays the death effects
        /// </summary>
        /// <param name="finalBlow">The final blow to the enemy</param>
        public virtual void PlayDeathEffects(HitInfo finalBlow)
        {
            if (HasBeenPlayed)
            {
                return;
            }
            HasBeenPlayed = true;
            if (!string.IsNullOrEmpty(JournalEntryName) && HunterJournal.HasEntryFor(JournalEntryName))
            {
                HunterJournal.RecordKillFor(JournalEntryName);
            }
            if (finalBlow.AttackType != AttackType.Acid && finalBlow.AttackType != AttackType.RuinsWater)
            {
                EmitEffects();
            }
            if (FreezeGameOnDeath)
            {
                WeaverGameManager.FreezeGameTime(WeaverGameManager.TimeFreezePreset.Preset1);
            }
            if (!Boss.InGodHomeArena && doEssenceChance)
            {
                DoEssenceChance();
            }
        }

        public bool DoEssenceChance()
        {
            Player player = Player.Player1;
            if (!Player.Player1.HasDreamNail)
            {
                return false;
            }
            int max;
            if (player.HasCharmEquipped(30) && player.EssenceSpent > 0)
            {
                max = 40;
            }
            else if (player.HasCharmEquipped(30) && player.EssenceSpent <= 0)
            {
                max = 200;
            }
            else if (player.EssenceSpent <= 0)
            {
                max = 300;
            }
            else
            {
                max = 60;
            }
            if (UnityEngine.Random.Range(0, max) == 0)
            {
                EmitEssenceParticles();
                player.EssenceCollected++;
                player.EssenceSpent--;
                return true;
            }
            return false;
        }

        protected void EmitEssenceParticles()
        {
            UnityEngine.Object.Instantiate<GameObject>(EssenceCollectPrefab, base.transform.position + EffectsOffset, Quaternion.identity);
        }
    }
}
