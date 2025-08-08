using System;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
    /// <summary>
    /// Abstract implementation for HKMP (Hollow Knight Multiplayer) integration.
    /// Provides multiplayer functionality when HKMP is available, otherwise does nothing.
    /// </summary>
    public abstract class HKMPIntegration_I : MonoBehaviour, IImplementation
    {
        /// <summary>
        /// Whether HKMP is available and loaded
        /// </summary>
        public abstract bool IsHKMPAvailable { get; }

        /// <summary>
        /// Whether the current player is connected to a multiplayer session
        /// </summary>
        public abstract bool IsConnectedToMultiplayer { get; }

        /// <summary>
        /// Whether the current player is the host of the multiplayer session
        /// </summary>
        public abstract bool IsHost { get; }

        /// <summary>
        /// Initialize the HKMP integration
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Register a custom ability packet handler
        /// </summary>
        /// <param name="packetId">The packet ID for this ability</param>
        /// <param name="onReceive">Callback when packet is received</param>
        public abstract void RegisterAbilityPacket<T>(byte packetId, Action<T> onReceive) where T : class, new();

        /// <summary>
        /// Send a custom ability packet to all other players
        /// </summary>
        /// <param name="packetId">The packet ID</param>
        /// <param name="data">The data to send</param>
        public abstract void SendAbilityPacket<T>(byte packetId, T data) where T : class;

        /// <summary>
        /// Register a custom entity for synchronization
        /// </summary>
        /// <param name="entityId">Unique ID for this entity type</param>
        /// <param name="entityObject">The GameObject to synchronize</param>
        /// <param name="onStateUpdate">Callback when entity state is updated from network</param>
        public abstract void RegisterCustomEntity(byte entityId, GameObject entityObject, Action<byte, byte[]> onStateUpdate);

        /// <summary>
        /// Send an entity state update to all other players
        /// </summary>
        /// <param name="entityId">The entity ID</param>
        /// <param name="state">The state data</param>
        /// <param name="variables">Additional variables</param>
        public abstract void SendEntityUpdate(byte entityId, byte state, byte[] variables = null);

        /// <summary>
        /// Spawn a networked projectile (like fire bats)
        /// </summary>
        /// <param name="projectileId">Unique ID for this projectile type</param>
        /// <param name="spawnPosition">Where to spawn the projectile</param>
        /// <param name="targetPosition">Target position for homing projectiles</param>
        /// <param name="customData">Additional custom data</param>
        public abstract void SpawnNetworkedProjectile(byte projectileId, Vector3 spawnPosition, Vector3 targetPosition, byte[] customData = null);

        /// <summary>
        /// Register a projectile type for network synchronization
        /// </summary>
        /// <param name="projectileId">Unique ID for this projectile type</param>
        /// <param name="onSpawn">Callback when projectile should be spawned</param>
        /// <param name="onUpdate">Callback when projectile is updated</param>
        public abstract void RegisterProjectileType(byte projectileId, Action<Vector3, Vector3, byte[]> onSpawn, Action<byte, Vector3, Vector3> onUpdate);

        /// <summary>
        /// Deal damage to another player in PvP
        /// </summary>
        /// <param name="targetPlayerId">The target player ID</param>
        /// <param name="damage">Amount of damage to deal</param>
        /// <param name="damageType">Type of damage (spell, nail, etc.)</param>
        public abstract void DealPvPDamage(ushort targetPlayerId, int damage, string damageType = "spell");

        /// <summary>
        /// Get the local player's ID in the multiplayer session
        /// </summary>
        public abstract ushort GetLocalPlayerId();

        /// <summary>
        /// Get all connected player IDs
        /// </summary>
        public abstract ushort[] GetConnectedPlayerIds();

        /// <summary>
        /// Check if a specific skin is equipped by the local player
        /// </summary>
        /// <param name="skinName">Name of the skin to check</param>
        public abstract bool HasSkinEquipped(string skinName);

        /// <summary>
        /// Event fired when a player connects to the session
        /// </summary>
        public abstract event Action<ushort> OnPlayerConnected;

        /// <summary>
        /// Event fired when a player disconnects from the session
        /// </summary>
        public abstract event Action<ushort> OnPlayerDisconnected;

        /// <summary>
        /// Event fired when the local player becomes the host
        /// </summary>
        public abstract event Action OnBecameHost;

        /// <summary>
        /// Event fired when the local player is no longer the host
        /// </summary>
        public abstract event Action OnLostHost;
    }
}