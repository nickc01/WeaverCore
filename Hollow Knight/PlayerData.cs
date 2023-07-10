using GlobalEnums;
using Modding;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PlayerData
{
    private static PlayerData dummyInstance;
    public static PlayerData instance
    {
        get
        {
            if (PlayerData.dummyInstance == null)
            {
                PlayerData.dummyInstance = new PlayerData();
            }
            return PlayerData.dummyInstance;
        }
        set => PlayerData.dummyInstance = value;
    }

    [NonSerialized]
    public Vector3 hazardRespawnLocation;
    public bool hazardRespawnFacingRight;
    public string respawnMarkerName;
    public bool respawnFacingRight;
    public string respawnScene;
    public int respawnType;
    public int prevHealth;
    public int health;
    public int maxHealth;
    public int maxHealthBase;
    public int healthBlue;
    public int joniHealthBlue;
    public bool damagedBlue;
    public bool isInvincible;
    public bool atBench;
    public MapZone mapZone;
    public List<string> scenesVisited;
    public bool hasDoubleJump;
    public List<string> scenesEncounteredBench;
    public string currentBossStatueCompletionKey;
    public int bossStatueTargetLevel;
    public bool equippedCharm_23;
    public bool brokenCharm_23;
    public int nailDamage;
    public BossSequenceController.BossSequenceData currentBossSequence;
    public int MPCharge;
    public int MPReserve;
    public string bossReturnEntryGate;

    protected PlayerData()
    {
        SetupNewPlayerData();
    }

    private void SetupNewPlayerData()
    {
        atBench = false;
        hazardRespawnLocation = Vector3.zero;
        hazardRespawnFacingRight = false;
        respawnMarkerName = "Death Respawn Marker";
        respawnFacingRight = false;
        respawnScene = "Tutorial_01";
        respawnType = 0;
        health = int.MaxValue / 2;
        maxHealth = int.MaxValue / 2;
        maxHealthBase = int.MaxValue / 2;
        healthBlue = 0;
        joniHealthBlue = 0;
        damagedBlue = false;
        prevHealth = health;
        isInvincible = false;
        mapZone = MapZone.GODS_GLORY;
        scenesVisited = new List<string>();
        hasDoubleJump = true;
        scenesEncounteredBench = new List<string>();
        nailDamage = 5;
        currentBossStatueCompletionKey = "";
        bossStatueTargetLevel = -1;
        equippedCharm_23 = false;
        brokenCharm_23 = false;
        currentBossSequence = null;
        bossReturnEntryGate = "";
        MPCharge = 0;
        MPReserve = 0;
    }

    public void ClearMP()
    {
        MPCharge = 0;
        MPReserve = 0;
    }

    public bool GetBool(string boolName)
    {
        return ModHooks.GetPlayerBool(boolName);
    }

    public float GetFloat(string floatName)
    {
        return ModHooks.GetPlayerFloat(floatName);
    }

    public int GetInt(string intName)
    {
        return ModHooks.GetPlayerInt(intName);
    }

    public string GetString(string stringName)
    {
        return ModHooks.GetPlayerString(stringName);
    }

    public T GetVariable<T>(string fieldName)
    {
        return ModHooks.GetPlayerVariable<T>(fieldName);
    }

    public Vector3 GetVector3(string vectorName)
    {
        return ModHooks.GetPlayerVector3(vectorName);
    }

    public void SetBool(string boolName, bool value)
    {
        ModHooks.SetPlayerBool(boolName, value);
    }

    public void SetFloat(string floatName, float value)
    {
        ModHooks.SetPlayerFloat(floatName, value);
    }

    public void SetInt(string intName, int value)
    {
        ModHooks.SetPlayerInt(intName, value);
    }

    public void SetString(string stringName, string value)
    {
        ModHooks.SetPlayerString(stringName, value);
    }

    public void SetVariable<T>(string fieldName, T value)
    {
        ModHooks.SetPlayerVariable(fieldName, value);
    }

    public void SetVector3(string vectorName, Vector3 value)
    {
        ModHooks.SetPlayerVector3(vectorName, value);
    }

    private void SetField(string fieldName, object value)
    {
        FieldInfo field = GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(this, value);
        }
    }

    private T GetField<T>(string fieldName)
    {
        FieldInfo field = GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            object value = field.GetValue(this);
            if (value is T castedVal)
            {
                return castedVal;
            }
        }
        return default;
    }

    public void SetBoolInternal(string boolName, bool value)
    {
        SetField(boolName, value);
    }

    public void IncrementInt(string intName)
    {
        if (GetType().GetField(intName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null)
        {
            ModHooks.SetPlayerInt(intName, GetIntInternal(intName) + 1);
        }
    }

    public void IntAdd(string intName, int amount)
    {
        if (GetType().GetField(intName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null)
        {
            ModHooks.SetPlayerInt(intName, GetIntInternal(intName) + amount);
        }

    }

    public void DecrementInt(string intName)
    {
        if (GetType().GetField(intName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null)
        {
            ModHooks.SetPlayerInt(intName, GetIntInternal(intName) - 1);
        }
    }

    public bool GetBoolInternal(string boolName)
    {
        return GetField<bool>(boolName);
    }

    public void SetIntInternal(string intName, int value)
    {
        SetField(intName, value);
    }

    public int GetIntInternal(string intName)
    {
        return GetField<int>(intName);
    }

    public void SetFloatInternal(string floatName, float value)
    {
        SetField(floatName, value);
    }

    public float GetFloatInternal(string floatName)
    {
        return GetField<float>(floatName);
    }

    public void SetStringInternal(string stringName, string value)
    {
        SetField(stringName, value);
    }

    public string GetStringInternal(string stringName)
    {
        return GetField<string>(stringName);
    }

    public void SetVector3Internal(string vector3Name, Vector3 value)
    {
        SetField(vector3Name, value);
    }

    public Vector3 GetVector3Internal(string vector3Name)
    {
        return GetField<Vector3>(vector3Name);
    }

    public void SetVariableInternal<T>(string variableName, T value)
    {
        SetField(variableName, value);
    }

    public T GetVariableInternal<T>(string variableName)
    {
        return GetField<T>(variableName);
    }

    internal void SetBoolSwappedArgs(bool value, string name)
    {
        SetBool(name, value);
    }

    internal void SetFloatSwappedArgs(float value, string name)
    {
        SetFloat(name, value);
    }

    internal void SetIntSwappedArgs(int value, string name)
    {
        SetInt(name, value);
    }

    internal void SetStringSwappedArgs(string value, string name)
    {
        SetString(name, value);
    }

    internal void SetVector3SwappedArgs(Vector3 value, string name)
    {
        SetVector3(name, value);
    }

    internal void SetVariableSwappedArgs<T0>(T0 value, string name)
    {
        SetVariable<T0>(name, value);
    }

    public void AddHealth(int amount)
    {
        if (GetInt("health") + amount >= GetInt("maxHealth"))
        {
            SetIntSwappedArgs(GetInt("maxHealth"), "health");
        }
        else
        {
            SetIntSwappedArgs(GetInt("health") + amount, "health");
        }
        if (GetInt("health") >= maxHealth)
        {
            SetIntSwappedArgs(GetInt("maxHealth"), "health");
        }
    }

    public void MaxHealth()
    {
        SetIntSwappedArgs(GetInt("health"), "prevHealth");
        SetIntSwappedArgs(maxHealth, "health");
        SetIntSwappedArgs(4, "blockerHits");
    }

    public void TakeHealth(int amount)
    {
        if (amount > 0 && GetInt("health") == GetInt("maxHealth") && GetInt("health") != maxHealth)
        {
            SetIntSwappedArgs(GetInt("maxHealth"), "health");
        }
        if (GetInt("healthBlue") > 0)
        {
            int num = amount - GetInt("healthBlue");
            SetBoolSwappedArgs(true, "damagedBlue");
            SetIntSwappedArgs(GetInt("healthBlue") - amount, "healthBlue");
            if (GetInt("healthBlue") < 0)
            {
                SetIntSwappedArgs(0, "healthBlue");
            }
            if (num > 0)
            {
                TakeHealth(num);
                return;
            }
        }
        else
        {
            SetBoolSwappedArgs(false, "damagedBlue");
            if (GetInt("health") - amount <= 0)
            {
                SetIntSwappedArgs(0, "health");
                return;
            }
            SetIntSwappedArgs(GetInt("health") - amount, "health");
        }
    }

    public void SetBenchRespawn(RespawnMarker spawnMarker, string sceneName, int spawnType)
    {
        SetStringSwappedArgs(spawnMarker.name, "respawnMarkerName");
        SetStringSwappedArgs(sceneName, "respawnScene");
        SetIntSwappedArgs(spawnType, "respawnType");
        SetBoolSwappedArgs(spawnMarker.respawnFacingRight, "respawnFacingRight");
        GameManager.instance.SetCurrentMapZoneAsRespawn();
    }

    public void SetBenchRespawn(string spawnMarker, string sceneName, bool facingRight)
    {
        SetStringSwappedArgs(spawnMarker, "respawnMarkerName");
        SetStringSwappedArgs(sceneName, "respawnScene");
        SetBoolSwappedArgs(facingRight, "respawnFacingRight");
        GameManager.instance.SetCurrentMapZoneAsRespawn();
    }

    public void SetBenchRespawn(string spawnMarker, string sceneName, int spawnType, bool facingRight)
    {
        SetStringSwappedArgs(spawnMarker, "respawnMarkerName");
        SetStringSwappedArgs(sceneName, "respawnScene");
        SetIntSwappedArgs(spawnType, "respawnType");
        SetBoolSwappedArgs(facingRight, "respawnFacingRight");
        GameManager.instance.SetCurrentMapZoneAsRespawn();
    }

    public void SetHazardRespawn(HazardRespawnMarker location)
    {
        SetVector3SwappedArgs(location.transform.position, "hazardRespawnLocation");
        SetBoolSwappedArgs(location.respawnFacingRight, "hazardRespawnFacingRight");
    }

    public void SetHazardRespawn(Vector3 position, bool facingRight)
    {
        SetVector3SwappedArgs(position, "hazardRespawnLocation");
        SetBoolSwappedArgs(facingRight, "hazardRespawnFacingRight");
    }
}


