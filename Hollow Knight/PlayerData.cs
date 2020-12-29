using System;
using System.Collections.Generic;
using System.Linq;
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

	public bool GetBool(string boolName)
	{
		return false;
	}

	public float GetFloat(string floatName)
	{
		return 0f;
	}

	public int GetInt(string intName)
	{
		return 0;
	}

	public string GetString(string stringName)
	{
		return "";
	}

	public T GetVariable<T>(string fieldName)
	{
		return default(T);
	}

	public Vector3 GetVector3(string vectorName)
	{
		return default(Vector3);
	}

	public void SetBool(string boolName, bool value)
	{

	}

	public void SetFloat(string floatName, float value)
	{

	}

	public void SetInt(string intName, int value)
	{

	}

	public void SetString(string stringName, string value)
	{

	}

	public void SetVariable<T>(string fieldName, T value)
	{
		
	}

	public void SetVector3(string vectorName, Vector3 value)
	{

	}

	public void SetBoolInternal(string boolName, bool value)
	{

	}

	// Token: 0x0600310B RID: 12555 RVA: 0x0010BB70 File Offset: 0x00109D70
	public bool GetBoolInternal(string boolName)
	{
		return false;
	}

	// Token: 0x0600310C RID: 12556 RVA: 0x0010BB9C File Offset: 0x00109D9C
	public void SetIntInternal(string intName, int value)
	{

	}

	// Token: 0x0600310D RID: 12557 RVA: 0x0010BBA8 File Offset: 0x00109DA8
	public int GetIntInternal(string intName)
	{
		return 0;
	}

	// Token: 0x0600310E RID: 12558 RVA: 0x0010BBE4 File Offset: 0x00109DE4
	public void SetFloatInternal(string floatName, float value)
	{

	}

	// Token: 0x0600310F RID: 12559 RVA: 0x0010BBF0 File Offset: 0x00109DF0
	public float GetFloatInternal(string floatName)
	{
		return 0f;
	}

	// Token: 0x06003110 RID: 12560 RVA: 0x0010BC2C File Offset: 0x00109E2C
	public void SetStringInternal(string stringName, string value)
	{

	}

	// Token: 0x06003111 RID: 12561 RVA: 0x0010BC38 File Offset: 0x00109E38
	public string GetStringInternal(string stringName)
	{
		return "";
	}

	// Token: 0x06003112 RID: 12562 RVA: 0x0010BC5C File Offset: 0x00109E5C
	public void SetVector3Internal(string vector3Name, Vector3 value)
	{

	}

	// Token: 0x06003113 RID: 12563 RVA: 0x0010BC68 File Offset: 0x00109E68
	public Vector3 GetVector3Internal(string vector3Name)
	{
		return default(Vector3);
	}

	// Token: 0x06003114 RID: 12564 RVA: 0x0010BCA4 File Offset: 0x00109EA4
	public void SetVariableInternal<T>(string variableName, T value)
	{
		
	}

	// Token: 0x06003115 RID: 12565 RVA: 0x0010BCB0 File Offset: 0x00109EB0
	public T GetVariableInternal<T>(string variableName)
	{
		return default(T);
	}
}


