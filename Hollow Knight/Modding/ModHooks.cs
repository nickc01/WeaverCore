using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Modding
{
	public class ModHooks
	{
		/*static ModHooks _instance;
		public static ModHooks Instance
		{
			get
			{
				bool flag = ModHooks._instance != null;
				ModHooks instance;
				if (flag)
				{
					instance = ModHooks._instance;
				}
				else
				{
					ModHooks._instance = new ModHooks();
					instance = ModHooks._instance;
				}
				return instance;
			}
		}*/

		public static event GetVector3Proxy GetPlayerVector3Hook;
		public static event GetBoolProxy GetPlayerBoolHook;
		public static event LanguageGetHandler LanguageGetHook;
		public static event GetFloatProxy GetPlayerFloatHook;
		public static event GetIntProxy GetPlayerIntHook;
		public static event GetStringProxy GetPlayerStringHook;
		public static event GetVariableProxy GetPlayerVariableHook;
	}

	public delegate string LanguageGetHandler(string key, string sheetTitle);
	public delegate object GetVariableProxy(Type type, string varName, object orig);
	public delegate string GetStringProxy(string stringName);
	public delegate string GetSaveFileNameHandler(int slot);
	public delegate int GetIntProxy(string intName);
	public delegate float GetFloatProxy(string floatName);
	public delegate bool GetBoolProxy(string originalSet);
	public delegate Vector3 GetVector3Proxy(string vector3Name);
}
