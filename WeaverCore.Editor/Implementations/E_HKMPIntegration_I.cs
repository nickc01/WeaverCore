using System;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
    /// <summary>
    /// Editor implementation of HKMP integration.
    /// All methods are stubbed out since HKMP doesn't run in the editor.
    /// </summary>
    public class E_HKMPIntegration_I : HKMPIntegration_I
    {
        public override bool IsHKMPAvailable => false;
        public override bool IsConnectedToMultiplayer => false;
        public override bool IsHost => false;

        // Stub events that never fire
        public override event Action<ushort> OnPlayerConnected { add { } remove { } }
        public override event Action<ushort> OnPlayerDisconnected { add { } remove { } }
        public override event Action OnBecameHost { add { } remove { } }
        public override event Action OnLostHost { add { } remove { } }

        public override void Initialize()
        {
            // Do nothing in editor
        }

        public override void RegisterAbilityPacket<T>(byte packetId, Action<T> onReceive)
        {
            // Do nothing in editor
        }

        public override void SendAbilityPacket<T>(byte packetId, T data)
        {
            // Do nothing in editor
        }

        public override void RegisterCustomEntity(byte entityId, GameObject entityObject, Action<byte, byte[]> onStateUpdate)
        {
            // Do nothing in editor
        }

        public override void SendEntityUpdate(byte entityId, byte state, byte[] variables = null)
        {
            // Do nothing in editor
        }

        public override void SpawnNetworkedProjectile(byte projectileId, Vector3 spawnPosition, Vector3 targetPosition, byte[] customData = null)
        {
            // Do nothing in editor
        }

        public override void RegisterProjectileType(byte projectileId, Action<Vector3, Vector3, byte[]> onSpawn, Action<byte, Vector3, Vector3> onUpdate)
        {
            // Do nothing in editor
        }

        public override void DealPvPDamage(ushort targetPlayerId, int damage, string damageType = "spell")
        {
            // Do nothing in editor
        }

        public override ushort GetLocalPlayerId()
        {
            return 0;
        }

        public override ushort[] GetConnectedPlayerIds()
        {
            return new ushort[0];
        }

        public override bool HasSkinEquipped(string skinName)
        {
            return false;
        }
    }
}