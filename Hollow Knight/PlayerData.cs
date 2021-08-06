using GlobalEnums;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public class PlayerData
{
	static PlayerData dummyInstance;
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
		set
		{
			PlayerData.dummyInstance = value;
		}
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
	public MapZone mapZone;
	public List<string> scenesVisited;
	public bool hasDoubleJump;

	protected PlayerData()
	{
		SetupNewPlayerData();
	}

	void SetupNewPlayerData()
	{
		hazardRespawnLocation = Vector3.zero;
		hazardRespawnFacingRight = false;
		respawnMarkerName = "Death Respawn Marker";
		respawnFacingRight = false;
		respawnScene = "Tutorial_01";
		respawnType = 0;
		health = 5;
		maxHealth = 5;
		maxHealthBase = 5;
		healthBlue = 0;
		joniHealthBlue = 0;
		damagedBlue = false;
		prevHealth = health;
		isInvincible = false;
		mapZone = MapZone.GODS_GLORY;
		scenesVisited = new List<string>();
		hasDoubleJump = true;
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

	void SetField(string fieldName,object value)
	{
		var field = GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if (field != null)
		{
			field.SetValue(this, value);
		}
	}

	T GetField<T>(string fieldName)
	{
		var field = GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if (field != null)
		{
			var value = field.GetValue(this);
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
		if (GetType().GetField(intName,BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null)
		{
			ModHooks.SetPlayerInt(intName, this.GetIntInternal(intName) + 1);
		}
	}

	public void IntAdd(string intName, int amount)
	{
		if (GetType().GetField(intName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null)
		{
			ModHooks.SetPlayerInt(intName, this.GetIntInternal(intName) + amount);
		}

	}

	public void DecrementInt(string intName)
	{
		if (GetType().GetField(intName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null)
		{
			ModHooks.SetPlayerInt(intName, this.GetIntInternal(intName) - 1);
		}
	}

	// Token: 0x0600310B RID: 12555 RVA: 0x0010BB70 File Offset: 0x00109D70
	public bool GetBoolInternal(string boolName)
	{
		return GetField<bool>(boolName);
	}

	// Token: 0x0600310C RID: 12556 RVA: 0x0010BB9C File Offset: 0x00109D9C
	public void SetIntInternal(string intName, int value)
	{
		SetField(intName, value);
	}

	// Token: 0x0600310D RID: 12557 RVA: 0x0010BBA8 File Offset: 0x00109DA8
	public int GetIntInternal(string intName)
	{
		return GetField<int>(intName);
	}

	// Token: 0x0600310E RID: 12558 RVA: 0x0010BBE4 File Offset: 0x00109DE4
	public void SetFloatInternal(string floatName, float value)
	{
		SetField(floatName, value);
	}

	// Token: 0x0600310F RID: 12559 RVA: 0x0010BBF0 File Offset: 0x00109DF0
	public float GetFloatInternal(string floatName)
	{
		return GetField<float>(floatName);
	}

	// Token: 0x06003110 RID: 12560 RVA: 0x0010BC2C File Offset: 0x00109E2C
	public void SetStringInternal(string stringName, string value)
	{
		SetField(stringName, value);
	}

	// Token: 0x06003111 RID: 12561 RVA: 0x0010BC38 File Offset: 0x00109E38
	public string GetStringInternal(string stringName)
	{
		return GetField<string>(stringName);
	}

	// Token: 0x06003112 RID: 12562 RVA: 0x0010BC5C File Offset: 0x00109E5C
	public void SetVector3Internal(string vector3Name, Vector3 value)
	{
		SetField(vector3Name, value);
	}

	// Token: 0x06003113 RID: 12563 RVA: 0x0010BC68 File Offset: 0x00109E68
	public Vector3 GetVector3Internal(string vector3Name)
	{
		return GetField<Vector3>(vector3Name);
	}

	// Token: 0x06003114 RID: 12564 RVA: 0x0010BCA4 File Offset: 0x00109EA4
	public void SetVariableInternal<T>(string variableName, T value)
	{
		SetField(variableName, value);
	}

	// Token: 0x06003115 RID: 12565 RVA: 0x0010BCB0 File Offset: 0x00109EB0
	public T GetVariableInternal<T>(string variableName)
	{
		return GetField<T>(variableName);
	}

	internal void SetBoolSwappedArgs(bool value, string name)
	{
		this.SetBool(name, value);
	}

	// Token: 0x060014F7 RID: 5367 RVA: 0x00063BF9 File Offset: 0x00061DF9
	internal void SetFloatSwappedArgs(float value, string name)
	{
		this.SetFloat(name, value);
	}

	// Token: 0x060014F8 RID: 5368 RVA: 0x00063C03 File Offset: 0x00061E03
	internal void SetIntSwappedArgs(int value, string name)
	{
		this.SetInt(name, value);
	}

	// Token: 0x060014F9 RID: 5369 RVA: 0x00063C0D File Offset: 0x00061E0D
	internal void SetStringSwappedArgs(string value, string name)
	{
		this.SetString(name, value);
	}

	// Token: 0x060014FA RID: 5370 RVA: 0x00063C17 File Offset: 0x00061E17
	internal void SetVector3SwappedArgs(Vector3 value, string name)
	{
		this.SetVector3(name, value);
	}

	// Token: 0x060014FB RID: 5371 RVA: 0x00063C21 File Offset: 0x00061E21
	internal void SetVariableSwappedArgs<T0>(T0 value, string name)
	{
		this.SetVariable<T0>(name, value);
	}

	public void AddHealth(int amount)
	{
		if (this.GetInt("health") + amount >= this.GetInt("maxHealth"))
		{
			this.SetIntSwappedArgs(this.GetInt("maxHealth"), "health");
		}
		else
		{
			this.SetIntSwappedArgs(this.GetInt("health") + amount, "health");
		}
		if (this.GetInt("health") >= this.maxHealth)
		{
			this.SetIntSwappedArgs(this.GetInt("maxHealth"), "health");
		}
	}

	public void MaxHealth()
	{
		this.SetIntSwappedArgs(this.GetInt("health"), "prevHealth");
		this.SetIntSwappedArgs(this.maxHealth, "health");
		this.SetIntSwappedArgs(4, "blockerHits");
	}

	public void TakeHealth(int amount)
	{
		if (amount > 0 && this.GetInt("health") == this.GetInt("maxHealth") && this.GetInt("health") != this.maxHealth)
		{
			//this.SetIntSwappedArgs(this.CurrentMaxHealth, "health");
			this.SetIntSwappedArgs(GetInt("maxHealth"), "health");
		}
		if (this.GetInt("healthBlue") > 0)
		{
			int num = amount - this.GetInt("healthBlue");
			this.SetBoolSwappedArgs(true, "damagedBlue");
			this.SetIntSwappedArgs(this.GetInt("healthBlue") - amount, "healthBlue");
			if (this.GetInt("healthBlue") < 0)
			{
				this.SetIntSwappedArgs(0, "healthBlue");
			}
			if (num > 0)
			{
				this.TakeHealth(num);
				return;
			}
		}
		else
		{
			this.SetBoolSwappedArgs(false, "damagedBlue");
			if (this.GetInt("health") - amount <= 0)
			{
				this.SetIntSwappedArgs(0, "health");
				return;
			}
			this.SetIntSwappedArgs(this.GetInt("health") - amount, "health");
		}
	}

	public void SetBenchRespawn(RespawnMarker spawnMarker, string sceneName, int spawnType)
	{
		this.SetStringSwappedArgs(spawnMarker.name, "respawnMarkerName");
		this.SetStringSwappedArgs(sceneName, "respawnScene");
		this.SetIntSwappedArgs(spawnType, "respawnType");
		this.SetBoolSwappedArgs(spawnMarker.respawnFacingRight, "respawnFacingRight");
		GameManager.instance.SetCurrentMapZoneAsRespawn();
	}

	// Token: 0x060014ED RID: 5357 RVA: 0x0005C7BD File Offset: 0x0005A9BD
	public void SetBenchRespawn(string spawnMarker, string sceneName, bool facingRight)
	{
		this.SetStringSwappedArgs(spawnMarker, "respawnMarkerName");
		this.SetStringSwappedArgs(sceneName, "respawnScene");
		this.SetBoolSwappedArgs(facingRight, "respawnFacingRight");
		GameManager.instance.SetCurrentMapZoneAsRespawn();
	}

	// Token: 0x060014EE RID: 5358 RVA: 0x0005C7ED File Offset: 0x0005A9ED
	public void SetBenchRespawn(string spawnMarker, string sceneName, int spawnType, bool facingRight)
	{
		this.SetStringSwappedArgs(spawnMarker, "respawnMarkerName");
		this.SetStringSwappedArgs(sceneName, "respawnScene");
		this.SetIntSwappedArgs(spawnType, "respawnType");
		this.SetBoolSwappedArgs(facingRight, "respawnFacingRight");
		GameManager.instance.SetCurrentMapZoneAsRespawn();
	}

	// Token: 0x060014EF RID: 5359 RVA: 0x0005C82A File Offset: 0x0005AA2A
	public void SetHazardRespawn(HazardRespawnMarker location)
	{
		this.SetVector3SwappedArgs(location.transform.position, "hazardRespawnLocation");
		this.SetBoolSwappedArgs(location.respawnFacingRight, "hazardRespawnFacingRight");
	}

	// Token: 0x060014F0 RID: 5360 RVA: 0x0005C853 File Offset: 0x0005AA53
	public void SetHazardRespawn(Vector3 position, bool facingRight)
	{
		this.SetVector3SwappedArgs(position, "hazardRespawnLocation");
		this.SetBoolSwappedArgs(facingRight, "hazardRespawnFacingRight");
	}
}


