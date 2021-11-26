using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DynType : DynamicObject
{
	public Type ReflectedType { get; private set; }
	public bool ExposePrivateFields { get; private set; } = false;
	public DynType(Type type, bool exposePrivateFields = false)
	{
		ReflectedType = type;
		ExposePrivateFields = exposePrivateFields;
	}

	public override bool TryGetMember(GetMemberBinder binder, out object result)
	{
		var flags = GetBindingFlags();
		var prop = ReflectedType.GetProperty(binder.Name, flags);
		if (prop != null)
		{
			result = prop.GetValue(null);
			return true;
		}
		var field = ReflectedType.GetField(binder.Name, flags);
		if (field != null)
		{
			result = field;
			return true;
		}
		result = default;
		return false;
	}

	public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
	{
		var flags = GetBindingFlags();
		var method = ReflectedType.GetMethod(binder.Name, flags);
		if (method != null)
		{
			result = method.Invoke(null,args);
			return true;
		}
		result = default;
		return false;
	}

	public BindingFlags GetBindingFlags()
	{
		if (ExposePrivateFields)
		{
			return BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
		}
		else
		{
			return BindingFlags.Public | BindingFlags.Static;
		}
	}
}

public class DynAsm : DynamicObject
{
	public static dynamic WeaverCore_ASM = new DynAsm("WeaverCore");

	public Assembly ReflectedAssembly { get; private set; }
	public string Path { get; private set; }


	public bool ExposePrivateFields { get; private set; } = false;

	public DynAsm(Assembly assembly, bool exposePrivateFields = false)
	{
		ExposePrivateFields = exposePrivateFields;
		ReflectedAssembly = assembly;
	}

	public DynAsm(string assemblyName, bool exposePrivateFields = false)
	{
		ExposePrivateFields = exposePrivateFields;
		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (asm.FullName == assemblyName)
			{
				ReflectedAssembly = asm;
				return;
			}
		}

		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (asm.GetName().Name == assemblyName)
			{
				ReflectedAssembly = asm;
				return;
			}
		}
	}

	public override bool TryGetMember(GetMemberBinder binder, out object result)
	{
		var typePath = $"{Path}.{binder.Name}";
		var type = ReflectedAssembly.GetType(typePath);
		if (type != null)
		{
			result = new DynType(type, ExposePrivateFields);
			return true;
		}
		var newAsm = new DynAsm(ReflectedAssembly, ExposePrivateFields);
		newAsm.Path = typePath;
		if (newAsm.Path.StartsWith("."))
		{
			newAsm.Path = newAsm.Path.Remove(0, 1);
		}
		result = newAsm;
		return true;
	}
}


public class Test
{
	public static void Test2()
	{
		dynamic test = new DynAsm("WeaverCore");
	}
}

