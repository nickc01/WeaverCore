using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WeaverCore.Editor.Compilation
{
	/// <summary>
	/// Used for serializing a method so it can be stored to a file
	/// </summary>
	[Serializable]
	public struct SerializedMethod
	{
		[SerializeField]
		string _assemblyName;
		public string AssemblyName { get { return _assemblyName; } set { _assemblyName = value; methodCache = null; } }
		[SerializeField]
		string _typeName;
		public string TypeName { get { return _typeName; } set { _typeName = value; methodCache = null; } }
		[SerializeField]
		string _methodName;
		public string MethodName { get { return _methodName; } set { _methodName = value; methodCache = null; } }
		[SerializeField]
		bool _isPublic;
		public bool IsPublic { get { return _isPublic; } set { _isPublic = value; methodCache = null; } }
		[SerializeField]
		bool _isStatic;
		public bool IsStatic { get { return _isStatic; } set { _isStatic = value; methodCache = null; } }

		[NonSerialized]
		MethodInfo methodCache;

		public MethodInfo Method
		{
			get
			{
				if (methodCache == null)
				{
					foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
					{
						if (assembly.GetName().Name == AssemblyName)
						{
							var type = assembly.GetType(TypeName);
							if (type != null)
							{
								BindingFlags flags = BindingFlags.Default;
								flags |= IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
								flags |= IsStatic ? BindingFlags.Static : BindingFlags.Instance;
								methodCache = type.GetMethod(MethodName, flags);
							}
						}
					}
				}
				return methodCache;
			}
			set
			{
				if (value == null)
				{
					AssemblyName = "";
					TypeName = "";
					MethodName = "";
					IsPublic = false;
					IsStatic = false;
					methodCache = null;
				}
				else
				{
					AssemblyName = value.DeclaringType.Assembly.GetName().Name;
					TypeName = value.DeclaringType.FullName;
					MethodName = value.Name;
					IsPublic = value.IsPublic;
					IsStatic = value.IsStatic;
					methodCache = value;
				}
			}
		}

		public SerializedMethod(MethodInfo method)
		{
			if (method == null)
			{
				_assemblyName = "";
				_typeName = "";
				_methodName = "";
				_isPublic = false;
				_isStatic = false;
				methodCache = null;
			}
			else
			{
				_assemblyName = method.DeclaringType.Assembly.GetName().Name;
				_typeName = method.DeclaringType.FullName;
				_methodName = method.Name;
				_isPublic = method.IsPublic;
				_isStatic = method.IsStatic;
				methodCache = method;
			}
		}

		public SerializedMethod(Delegate method) : this(method.Method) { }
	}
}
