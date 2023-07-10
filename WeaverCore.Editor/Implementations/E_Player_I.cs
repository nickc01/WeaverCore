using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Enums;

namespace WeaverCore.Editor.Implementations
{
	public class E_Player_I : Player_I
    {
        public override bool HasDreamNail
        {
            get
            {
                return false;
            }
        }

        public override int EssenceCollected
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }
        public override int EssenceSpent
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

		public override void Initialize()
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        public override void SoulGain()
        {
            
        }

        public override void RefreshSoulUI()
        {
            
        }

        public override void EnterParryState()
        {

        }

        public override void RecoverFromParry()
        {

        }

        public override void Recoil(CardinalDirection direction)
        {

        }

        public override bool HasCharmEquipped(int charmNumber)
        {
            return false;
        }

        public override void EnterRoarLock()
        {
            
        }

        public override void ExitRoarLock()
        {
            
        }

        public override void EnterCutsceneLock(bool playSound, int darknessLevel = -1)
        {
            
        }

        public override void ExitCutsceneLock()
        {
            
        }
    }
}
