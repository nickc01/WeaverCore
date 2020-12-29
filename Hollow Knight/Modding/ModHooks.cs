using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Modding
{
	public class ModHooks
	{
		static ModHooks _instance;
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
		}

		public event GetVector3Proxy GetPlayerVector3Hook;
		public event GetBoolProxy GetPlayerBoolHook;
		public event GetFloatProxy GetPlayerFloatHook;
		public event GetIntProxy GetPlayerIntHook;
		public event GetStringProxy GetPlayerStringHook;
		public event GetVariableProxy GetPlayerVariableHook;
	}

	public delegate object GetVariableProxy(Type type, string varName, object orig);
	public delegate string GetStringProxy(string stringName);
	public delegate string GetSaveFileNameHandler(int slot);
	public delegate int GetIntProxy(string intName);
	public delegate float GetFloatProxy(string floatName);
	public delegate bool GetBoolProxy(string originalSet);
	public delegate Vector3 GetVector3Proxy(string vector3Name);
}
