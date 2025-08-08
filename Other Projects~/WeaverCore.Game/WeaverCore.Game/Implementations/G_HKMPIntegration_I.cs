using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
    /// <summary>
    /// Game implementation of HKMP integration.
    /// Uses reflection to detect and integrate with HKMP if available.
    /// </summary>
    public class G_HKMPIntegration_I : HKMPIntegration_I
    {
        private bool _isInitialized = false;
        private bool _hkmpAvailable = false;
        private object _clientAddon = null;
        private object _networkSender = null;
        private object _networkReceiver = null;

        // Cached reflection types and methods
        private Assembly _hkmpAssembly = null;
        private Type _clientAddonType = null;
        private Type _clientApiType = null;
        private Type _networkSenderType = null;
        private Type _networkReceiverType = null;
        private Type _clientManagerType = null;
        private Type _packetDataType = null;
        private Type _packetType = null;
        
        // HKMP API instances
        private object _clientApi = null;
        private object _clientManager = null;
        private MethodInfo _sendPacketMethod = null;
        private MethodInfo _registerPacketMethod = null;

        // Events
        public override event Action<ushort> OnPlayerConnected;
        public override event Action<ushort> OnPlayerDisconnected;
        public override event Action OnBecameHost;
        public override event Action OnLostHost;

        // Packet and entity tracking
        private Dictionary<byte, Action<object>> _packetHandlers = new Dictionary<byte, Action<object>>();
        private Dictionary<byte, GameObject> _registeredEntities = new Dictionary<byte, GameObject>();
        private Dictionary<byte, Action<byte, byte[]>> _entityUpdateHandlers = new Dictionary<byte, Action<byte, byte[]>>();

        public override bool IsHKMPAvailable => _hkmpAvailable;

        public override bool IsConnectedToMultiplayer
        {
            get
            {
                if (!_hkmpAvailable || _clientApi == null) return false;
                try
                {
                    // Access NetClient.IsConnected via reflection
                    var netClientProp = _clientApiType.GetProperty("NetClient");
                    var netClient = netClientProp?.GetValue(_clientApi);
                    if (netClient == null) return false;
                    
                    var isConnectedProp = netClient.GetType().GetProperty("IsConnected");
                    return (bool)(isConnectedProp?.GetValue(netClient) ?? false);
                }
                catch
                {
                    return false;
                }
            }
        }

        public override bool IsHost
        {
            get
            {
                if (!_hkmpAvailable || _clientManager == null) return false;
                try
                {
                    // Check if local player ID is 1 (host is typically ID 1 in HKMP)
                    var localPlayerId = GetLocalPlayerId();
                    return localPlayerId == 1;
                }
                catch
                {
                    return false;
                }
            }
        }

        public override void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            // Try to detect HKMP using reflection
            DetectHKMP();

            if (_hkmpAvailable)
            {
                InitializeHKMPIntegration();
            }
        }

        private void DetectHKMP()
        {
            try
            {
                // Try to find HKMP assembly
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == "HKMP")
                    {
                        _hkmpAssembly = assembly;
                        break;
                    }
                }

                if (_hkmpAssembly == null)
                {
                    Debug.Log("[WeaverCore HKMP] HKMP assembly not found - multiplayer features disabled");
                    return;
                }

                // Try to find required types
                _clientAddonType = _hkmpAssembly.GetType("Hkmp.Api.Client.ClientAddon");
                _clientApiType = _hkmpAssembly.GetType("Hkmp.Api.Client.IClientApi");
                _packetDataType = _hkmpAssembly.GetType("Hkmp.Networking.Packet.IPacketData");
                _clientManagerType = _hkmpAssembly.GetType("Hkmp.Api.Client.IClientManager");
                _networkSenderType = _hkmpAssembly.GetType("Hkmp.Api.Client.Networking.IClientAddonNetworkSender`1");
                _networkReceiverType = _hkmpAssembly.GetType("Hkmp.Api.Client.Networking.IClientAddonNetworkReceiver`1");
                _packetType = _hkmpAssembly.GetType("Hkmp.Networking.Packet.IPacket");

                if (_clientAddonType == null || _clientApiType == null || _packetDataType == null ||
                    _clientManagerType == null || _networkSenderType == null || _networkReceiverType == null)
                {
                    Debug.LogWarning("[WeaverCore HKMP] Required HKMP types not found - multiplayer features disabled");
                    return;
                }

                _hkmpAvailable = true;
                Debug.Log("[WeaverCore HKMP] HKMP detected and available");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WeaverCore HKMP] Error detecting HKMP: {e.Message}");
                _hkmpAvailable = false;
            }
        }

        private void InitializeHKMPIntegration()
        {
            try
            {
                // Create our HKMP addon 
                _clientAddon = new WeaverCoreHKMPAddon(this);

                // Register the addon with HKMP
                var registerMethod = _clientAddonType.GetMethod("RegisterAddon", BindingFlags.Static | BindingFlags.Public);
                registerMethod?.Invoke(null, new[] { _clientAddon });

                Debug.Log("[WeaverCore HKMP] HKMP integration initialized successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaverCore HKMP] Failed to initialize HKMP integration: {e.Message}");
                _hkmpAvailable = false;
            }
        }

        internal void OnAddonInitialize(object clientApi)
        {
            try
            {
                _clientApi = clientApi;
                
                // Get ClientManager
                var clientManagerProp = _clientApiType.GetProperty("ClientManager");
                _clientManager = clientManagerProp?.GetValue(_clientApi);
                
                // Get NetClient and set up networking
                var netClientProp = _clientApiType.GetProperty("NetClient");
                var netClient = netClientProp?.GetValue(_clientApi);
                
                if (netClient != null)
                {
                    SetupNetworking(netClient);
                }
                
                // Subscribe to events
                SubscribeToHKMPEvents();
                
                Debug.Log("[WeaverCore HKMP] Addon initialized with HKMP API");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaverCore HKMP] Failed to initialize addon: {e.Message}");
            }
        }

        private void SetupNetworking(object netClient)
        {
            try
            {
                // For simplicity, we'll use a basic enum approach instead of dynamic types
                var packetIdEnumType = typeof(WeaverCorePacketId);
                
                // Get network sender
                var getSenderMethod = netClient.GetType().GetMethod("GetNetworkSender").MakeGenericMethod(packetIdEnumType);
                _networkSender = getSenderMethod.Invoke(netClient, new[] { _clientAddon });
                
                // Get network receiver with a packet instantiator
                var getReceiverMethod = netClient.GetType().GetMethod("GetNetworkReceiver").MakeGenericMethod(packetIdEnumType);
                Func<WeaverCorePacketId, object> packetInstantiator = InstantiatePacketData;
                _networkReceiver = getReceiverMethod.Invoke(netClient, new object[] { _clientAddon, packetInstantiator });
                
                // Cache send/register methods
                _sendPacketMethod = _networkSender?.GetType().GetMethod("SendSingleData");
                _registerPacketMethod = _networkReceiver?.GetType().GetMethod("RegisterPacketHandler");
                
                Debug.Log("[WeaverCore HKMP] Networking setup complete");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaverCore HKMP] Failed to setup networking: {e.Message}");
            }
        }

        private object InstantiatePacketData(WeaverCorePacketId packetId)
        {
            try
            {
                switch (packetId)
                {
                    case WeaverCorePacketId.AbilityPacket:
                        return new WeaverCoreAbilityPacket();
                    case WeaverCorePacketId.ProjectileSpawn:
                        return new WeaverCoreProjectileSpawn();
                    case WeaverCorePacketId.ProjectileUpdate:
                        return new WeaverCoreProjectileUpdate();
                    case WeaverCorePacketId.EntityUpdate:
                        return new WeaverCoreEntityUpdate();
                    case WeaverCorePacketId.PvPDamage:
                        return new WeaverCorePvPDamage();
                    case WeaverCorePacketId.CustomData:
                        return new WeaverCoreCustomData();
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private void SubscribeToHKMPEvents()
        {
            if (_clientManager == null) return;
            
            try
            {
                // Subscribe to player connect/disconnect events
                var playerConnectEvent = _clientManagerType.GetEvent("PlayerConnectEvent");
                var playerDisconnectEvent = _clientManagerType.GetEvent("PlayerDisconnectEvent");
                
                if (playerConnectEvent != null)
                {
                    var handler = Delegate.CreateDelegate(playerConnectEvent.EventHandlerType, this, "OnPlayerConnectedInternal");
                    playerConnectEvent.AddEventHandler(_clientManager, handler);
                }
                
                if (playerDisconnectEvent != null)
                {
                    var handler = Delegate.CreateDelegate(playerDisconnectEvent.EventHandlerType, this, "OnPlayerDisconnectedInternal");
                    playerDisconnectEvent.AddEventHandler(_clientManager, handler);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaverCore HKMP] Failed to subscribe to events: {e.Message}");
            }
        }

        private void OnPlayerConnectedInternal(object player)
        {
            try
            {
                var idProp = player.GetType().GetProperty("Id");
                var playerId = (ushort)(idProp?.GetValue(player) ?? 0);
                OnPlayerConnected?.Invoke(playerId);
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaverCore HKMP] Error in player connected handler: {e.Message}");
            }
        }

        private void OnPlayerDisconnectedInternal(object player)
        {
            try
            {
                if (player == null) return;
                
                var idProp = player.GetType().GetProperty("Id");
                var playerId = (ushort)(idProp?.GetValue(player) ?? 0);
                OnPlayerDisconnected?.Invoke(playerId);
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaverCore HKMP] Error in player disconnected handler: {e.Message}");
            }
        }

        public override void RegisterAbilityPacket<T>(byte packetId, Action<T> onReceive)
        {
            if (!_hkmpAvailable) return;

            try
            {
                _packetHandlers[packetId] = (data) => onReceive((T)data);
                Debug.Log($"[WeaverCore HKMP] Registered ability packet {packetId} for type {typeof(T).Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaverCore HKMP] Failed to register ability packet {packetId}: {e.Message}");
            }
        }

        public override void SendAbilityPacket<T>(byte packetId, T data)
        {
            if (!_hkmpAvailable || _networkSender == null || _sendPacketMethod == null) return;

            try
            {
                // Create wrapper packet data
                var packetData = new WeaverCoreAbilityPacket
                {
                    PacketId = packetId,
                    SerializedData = SerializeData(data)
                };
                
                // Send packet using HKMP's network sender
                _sendPacketMethod.Invoke(_networkSender, new object[] { WeaverCorePacketId.AbilityPacket, packetData });
                Debug.Log($"[WeaverCore HKMP] Sent ability packet {packetId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaverCore HKMP] Failed to send ability packet {packetId}: {e.Message}");
            }
        }

        public override void RegisterCustomEntity(byte entityId, GameObject entityObject, Action<byte, byte[]> onStateUpdate)
        {
            if (!_hkmpAvailable) return;

            _registeredEntities[entityId] = entityObject;
            _entityUpdateHandlers[entityId] = onStateUpdate;
            Debug.Log($"[WeaverCore HKMP] Registered custom entity {entityId}: {entityObject.name}");
        }

        public override void SendEntityUpdate(byte entityId, byte state, byte[] variables = null)
        {
            if (!_hkmpAvailable || _networkSender == null) return;

            try
            {
                // Create entity update packet
                var entityUpdateData = new WeaverCoreEntityUpdate
                {
                    EntityId = entityId,
                    State = state,
                    Variables = variables ?? new byte[0]
                };
                
                SendAbilityPacket(3, entityUpdateData); // Use packet ID 3 for entity updates
                Debug.Log($"[WeaverCore HKMP] Sent entity update for {entityId}, state {state}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaverCore HKMP] Failed to send entity update: {e.Message}");
            }
        }

        public override void SpawnNetworkedProjectile(byte projectileId, Vector3 spawnPosition, Vector3 targetPosition, byte[] customData = null)
        {
            if (!_hkmpAvailable) return;

            try
            {
                // Create projectile spawn packet and send it
                var packetData = new WeaverCoreProjectileSpawn
                {
                    ProjectileId = projectileId,
                    SpawnPosition = spawnPosition,
                    TargetPosition = targetPosition,
                    CustomData = customData ?? new byte[0]
                };

                SendAbilityPacket(1, packetData); // Use packet ID 1 for projectile spawns
                Debug.Log($"[WeaverCore HKMP] Spawned networked projectile {projectileId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaverCore HKMP] Failed to spawn networked projectile: {e.Message}");
            }
        }

        public override void RegisterProjectileType(byte projectileId, Action<Vector3, Vector3, byte[]> onSpawn, Action<byte, Vector3, Vector3> onUpdate)
        {
            if (!_hkmpAvailable) return;

            // Register packet handler for projectile spawns
            RegisterAbilityPacket<WeaverCoreProjectileSpawn>(1, (data) =>
            {
                if (data.ProjectileId == projectileId)
                {
                    onSpawn(data.SpawnPosition, data.TargetPosition, data.CustomData);
                }
            });

            Debug.Log($"[WeaverCore HKMP] Registered projectile type {projectileId}");
        }

        public override void DealPvPDamage(ushort targetPlayerId, int damage, string damageType = "spell")
        {
            if (!_hkmpAvailable || _networkSender == null) return;

            try
            {
                // Create PvP damage packet
                var pvpDamageData = new WeaverCorePvPDamage
                {
                    TargetPlayerId = targetPlayerId,
                    Damage = damage,
                    DamageType = damageType ?? "spell",
                    SourcePlayerId = GetLocalPlayerId()
                };
                
                SendAbilityPacket(4, pvpDamageData); // Use packet ID 4 for PvP damage
                Debug.Log($"[WeaverCore HKMP] Dealt {damage} {damageType} damage to player {targetPlayerId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaverCore HKMP] Failed to deal PvP damage: {e.Message}");
            }
        }

        public override ushort GetLocalPlayerId()
        {
            if (!_hkmpAvailable || _clientManager == null) return 0;

            try
            {
                // Get local player from HKMP ClientManager
                var playersProperty = _clientManagerType.GetProperty("Players");
                var players = playersProperty?.GetValue(_clientManager);
                
                if (players == null) return 0;
                
                // Find the local player (usually the first one in the collection)
                foreach (var player in (System.Collections.IEnumerable)players)
                {
                    var isLocalProperty = player.GetType().GetProperty("IsLocal");
                    var isLocal = (bool)(isLocalProperty?.GetValue(player) ?? false);
                    
                    if (isLocal)
                    {
                        var idProperty = player.GetType().GetProperty("Id");
                        return (ushort)(idProperty?.GetValue(player) ?? 0);
                    }
                }
                
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public override ushort[] GetConnectedPlayerIds()
        {
            if (!_hkmpAvailable || _clientManager == null) return new ushort[0];

            try
            {
                var playersProperty = _clientManagerType.GetProperty("Players");
                var players = playersProperty?.GetValue(_clientManager);
                
                if (players == null) return new ushort[0];
                
                var playerIds = new List<ushort>();
                
                foreach (var player in (System.Collections.IEnumerable)players)
                {
                    var idProperty = player.GetType().GetProperty("Id");
                    var playerId = (ushort)(idProperty?.GetValue(player) ?? 0);
                    playerIds.Add(playerId);
                }
                
                return playerIds.ToArray();
            }
            catch
            {
                return new ushort[0];
            }
        }

        public override bool HasSkinEquipped(string skinName)
        {
            // This would need to integrate with your skin system
            // For now, return false as placeholder
            return false;
        }

        private byte[] SerializeData<T>(T data)
        {
            try
            {
                // Simple JSON serialization for now
                var json = JsonUtility.ToJson(data);
                return System.Text.Encoding.UTF8.GetBytes(json);
            }
            catch
            {
                return new byte[0];
            }
        }

        private T DeserializeData<T>(byte[] data)
        {
            try
            {
                var json = System.Text.Encoding.UTF8.GetString(data);
                return JsonUtility.FromJson<T>(json);
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Packet ID enum for WeaverCore networking
    /// </summary>
    public enum WeaverCorePacketId : byte
    {
        AbilityPacket = 0,
        ProjectileSpawn = 1,
        ProjectileUpdate = 2,
        EntityUpdate = 3,
        PvPDamage = 4,
        CustomData = 5
    }

    /// <summary>
    /// Base class for WeaverCore packet data
    /// </summary>
    [Serializable]
    public abstract class WeaverCorePacketDataBase
    {
        public virtual bool IsReliable => true;
        public virtual bool DropReliableDataIfNewerExists => false;
        
        public virtual void WriteData(object packet)
        {
            // Implementation would serialize this object to the packet
        }
        
        public virtual void ReadData(object packet)
        {
            // Implementation would deserialize this object from the packet
        }
    }

    [Serializable]
    public class WeaverCoreAbilityPacket : WeaverCorePacketDataBase
    {
        public byte PacketId;
        public byte[] SerializedData;
    }

    [Serializable]
    public class WeaverCoreProjectileSpawn : WeaverCorePacketDataBase
    {
        public byte ProjectileId;
        public Vector3 SpawnPosition;
        public Vector3 TargetPosition;
        public byte[] CustomData;
    }

    [Serializable]
    public class WeaverCoreProjectileUpdate : WeaverCorePacketDataBase
    {
        public byte ProjectileId;
        public Vector3 Position;
        public Vector3 Velocity;
    }
    
    [Serializable]
    public class WeaverCoreEntityUpdate : WeaverCorePacketDataBase
    {
        public byte EntityId;
        public byte State;
        public byte[] Variables;
    }
    
    [Serializable]
    public class WeaverCorePvPDamage : WeaverCorePacketDataBase
    {
        public ushort TargetPlayerId;
        public ushort SourcePlayerId;
        public int Damage;
        public string DamageType;
    }

    [Serializable]
    public class WeaverCoreCustomData : WeaverCorePacketDataBase
    {
        public byte[] Data;
    }

    /// <summary>
    /// HKMP ClientAddon implementation for WeaverCore integration
    /// </summary>
    public class WeaverCoreHKMPAddon
    {
        private readonly G_HKMPIntegration_I _integration;
        
        public WeaverCoreHKMPAddon(G_HKMPIntegration_I integration)
        {
            _integration = integration;
        }
        
        protected virtual string Name => "WeaverCore";
        protected virtual string Version => "1.0.0";
        public virtual bool NeedsNetwork => true;

        public virtual void Initialize(object clientApi)
        {
            Debug.Log("[WeaverCore HKMP] WeaverCore HKMP Addon initialized");
            _integration?.OnAddonInitialize(clientApi);
        }
    }
}