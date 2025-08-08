using System;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Implementations;

namespace WeaverCore
{
    public class HKMPIntegration : MonoBehaviour
    {
        private static HKMPIntegration _instance;
        private HKMPIntegration_I _impl;

        /// <summary>
        /// Singleton instance of the HKMP integration
        /// </summary>
        public static HKMPIntegration Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("WeaverCore HKMP Integration");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<HKMPIntegration>();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Whether HKMP is available and loaded
        /// </summary>
        public static bool IsAvailable => Instance._impl?.IsHKMPAvailable ?? false;

        /// <summary>
        /// Whether the current player is connected to a multiplayer session
        /// </summary>
        public static bool IsConnectedToMultiplayer => Instance._impl?.IsConnectedToMultiplayer ?? false;

        /// <summary>
        /// Whether the current player is the host of the multiplayer session
        /// </summary>
        public static bool IsHost => Instance._impl?.IsHost ?? false;

        /// <summary>
        /// Event fired when a player connects to the session
        /// </summary>
        public static event Action<ushort> OnPlayerConnected
        {
            add { if (Instance._impl != null) Instance._impl.OnPlayerConnected += value; }
            remove { if (Instance._impl != null) Instance._impl.OnPlayerConnected -= value; }
        }

        /// <summary>
        /// Event fired when a player disconnects from the session
        /// </summary>
        public static event Action<ushort> OnPlayerDisconnected
        {
            add { if (Instance._impl != null) Instance._impl.OnPlayerDisconnected += value; }
            remove { if (Instance._impl != null) Instance._impl.OnPlayerDisconnected -= value; }
        }

        /// <summary>
        /// Event fired when the local player becomes the host
        /// </summary>
        public static event Action OnBecameHost
        {
            add { if (Instance._impl != null) Instance._impl.OnBecameHost += value; }
            remove { if (Instance._impl != null) Instance._impl.OnBecameHost -= value; }
        }

        /// <summary>
        /// Event fired when the local player is no longer the host
        /// </summary>
        public static event Action OnLostHost
        {
            add { if (Instance._impl != null) Instance._impl.OnLostHost += value; }
            remove { if (Instance._impl != null) Instance._impl.OnLostHost -= value; }
        }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Get the appropriate implementation (game or editor)
            if (!gameObject.TryGetComponent(out _impl))
            {
                _impl = (HKMPIntegration_I)gameObject.AddComponent(ImplFinder.GetImplementationType<HKMPIntegration_I>());
            }
        }

        /// <summary>
        /// Initialize the HKMP integration. Call this in your mod's initialization.
        /// </summary>
        public static void Initialize()
        {
            Instance._impl?.Initialize();
        }

        /// <summary>
        /// Register a custom ability packet handler.
        /// Use this to synchronize custom abilities across players.
        /// </summary>
        /// <typeparam name="T">The packet data type</typeparam>
        /// <param name="packetId">Unique packet ID (0-255)</param>
        /// <param name="onReceive">Callback when packet is received from another player</param>
        /// <example>
        /// // Register handler for fire bat ability
        /// HKMPIntegration.RegisterAbilityPacket&lt;FireBatData&gt;(1, (data) => {
        ///     SpawnFireBat(data.SpawnPosition, data.TargetPosition);
        /// });
        /// </example>
        public static void RegisterAbilityPacket<T>(byte packetId, Action<T> onReceive) where T : class, new()
        {
            Instance._impl?.RegisterAbilityPacket(packetId, onReceive);
        }

        /// <summary>
        /// Send a custom ability packet to all other players.
        /// Use this when triggering custom abilities that should be synchronized.
        /// </summary>
        /// <typeparam name="T">The packet data type</typeparam>
        /// <param name="packetId">The packet ID (must match registered handler)</param>
        /// <param name="data">The data to send</param>
        /// <example>
        /// // Send fire bat spawn to other players
        /// var data = new FireBatData { SpawnPosition = player.position, TargetPosition = target.position };
        /// HKMPIntegration.SendAbilityPacket(1, data);
        /// </example>
        public static void SendAbilityPacket<T>(byte packetId, T data) where T : class
        {
            Instance._impl?.SendAbilityPacket(packetId, data);
        }

        /// <summary>
        /// Register a custom entity (like a boss) for synchronization across players.
        /// </summary>
        /// <param name="entityId">Unique entity ID (0-255)</param>
        /// <param name="entityObject">The GameObject to synchronize</param>
        /// <param name="onStateUpdate">Callback when entity state is updated from network</param>
        /// <example>
        /// // Register custom boss for synchronization
        /// HKMPIntegration.RegisterCustomEntity(1, bossGameObject, (state, variables) => {
        ///     ApplyBossState(state, variables);
        /// });
        /// </example>
        public static void RegisterCustomEntity(byte entityId, GameObject entityObject, Action<byte, byte[]> onStateUpdate)
        {
            Instance._impl?.RegisterCustomEntity(entityId, entityObject, onStateUpdate);
        }

        /// <summary>
        /// Send an entity state update to all other players.
        /// Use this when your custom entity changes state and needs to be synchronized.
        /// </summary>
        /// <param name="entityId">The entity ID</param>
        /// <param name="state">The new state</param>
        /// <param name="variables">Additional state variables</param>
        /// <example>
        /// // Send boss state change
        /// HKMPIntegration.SendEntityUpdate(1, (byte)BossState.Attacking, attackData);
        /// </example>
        public static void SendEntityUpdate(byte entityId, byte state, byte[] variables = null)
        {
            Instance._impl?.SendEntityUpdate(entityId, state, variables);
        }

        /// <summary>
        /// Spawn a networked projectile that will appear for all players.
        /// Use this for custom abilities that create projectiles (like fire bats).
        /// </summary>
        /// <param name="projectileId">Unique projectile type ID</param>
        /// <param name="spawnPosition">Where to spawn the projectile</param>
        /// <param name="targetPosition">Target position for homing projectiles</param>
        /// <param name="customData">Additional custom data for the projectile</param>
        /// <example>
        /// // Spawn fire bats that home in on enemies
        /// HKMPIntegration.SpawnNetworkedProjectile(1, player.position, enemy.position);
        /// </example>
        public static void SpawnNetworkedProjectile(byte projectileId, Vector3 spawnPosition, Vector3 targetPosition, byte[] customData = null)
        {
            Instance._impl?.SpawnNetworkedProjectile(projectileId, spawnPosition, targetPosition, customData);
        }

        /// <summary>
        /// Register a projectile type for network synchronization.
        /// Call this during initialization to set up projectile handlers.
        /// </summary>
        /// <param name="projectileId">Unique projectile type ID</param>
        /// <param name="onSpawn">Callback when projectile should be spawned locally</param>
        /// <param name="onUpdate">Callback when projectile is updated from network</param>
        /// <example>
        /// // Register fire bat projectile type
        /// HKMPIntegration.RegisterProjectileType(1, 
        ///     (spawnPos, targetPos, data) => CreateFireBat(spawnPos, targetPos),
        ///     (id, pos, vel) => UpdateFireBat(id, pos, vel));
        /// </example>
        public static void RegisterProjectileType(byte projectileId, Action<Vector3, Vector3, byte[]> onSpawn, Action<byte, Vector3, Vector3> onUpdate)
        {
            Instance._impl?.RegisterProjectileType(projectileId, onSpawn, onUpdate);
        }

        /// <summary>
        /// Deal damage to another player in PvP.
        /// Use this when your custom abilities should damage other players.
        /// </summary>
        /// <param name="targetPlayerId">The target player ID</param>
        /// <param name="damage">Amount of damage to deal</param>
        /// <param name="damageType">Type of damage (spell, nail, etc.)</param>
        /// <example>
        /// // Deal fire damage to target player
        /// HKMPIntegration.DealPvPDamage(targetPlayerId, 15, "fire");
        /// </example>
        public static void DealPvPDamage(ushort targetPlayerId, int damage, string damageType = "spell")
        {
            Instance._impl?.DealPvPDamage(targetPlayerId, damage, damageType);
        }

        /// <summary>
        /// Get the local player's ID in the multiplayer session.
        /// </summary>
        /// <returns>Local player ID, or 0 if not connected</returns>
        public static ushort GetLocalPlayerId()
        {
            return Instance._impl?.GetLocalPlayerId() ?? 0;
        }

        /// <summary>
        /// Get all connected player IDs.
        /// </summary>
        /// <returns>Array of connected player IDs</returns>
        public static ushort[] GetConnectedPlayerIds()
        {
            return Instance._impl?.GetConnectedPlayerIds() ?? new ushort[0];
        }

        /// <summary>
        /// Check if a specific skin is equipped by the local player.
        /// Use this to trigger skin-specific abilities.
        /// </summary>
        /// <param name="skinName">Name of the skin to check</param>
        /// <returns>True if the skin is equipped</returns>
        /// <example>
        /// // Check if Little Grimm skin is equipped
        /// if (HKMPIntegration.HasSkinEquipped("LittleGrimm")) {
        ///     CastFireBats();
        /// }
        /// </example>
        public static bool HasSkinEquipped(string skinName)
        {
            return Instance._impl?.HasSkinEquipped(skinName) ?? false;
        }
    }
}