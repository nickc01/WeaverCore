using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;
using VoidCore.Hooks;

namespace VoidCore.Debug
{
    class PlayerDebug : PlayerHook<VoidCore>
    {
        public override void LoadHook(IMod mod)
        {
            
        }

        public override void UnloadHook(IMod mod)
        {
            
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (Settings.DebugMode)
                ModLog.Log($"Player Collided With {collision.gameObject.name}");
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (Settings.DebugMode)
                ModLog.Log($"Player Stopped Colliding With {collision.gameObject.name}");
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (Settings.DebugMode)
                ModLog.Log($"Player Triggered With {collider.gameObject.name}");
        }

        void OnTriggerExit2D(Collider2D collider)
        {
            if (Settings.DebugMode)
                ModLog.Log($"Player Stopped Triggering With {collider.gameObject.name}");
        }
    }
}
