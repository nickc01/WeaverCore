using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WeaverCore.Editor.Compilation
{
	[Serializable]
	public struct SerializedMethod
	{
		[SerializeField]
		string _assemblyName;
		public string AssemblyName { get { return _assemblyName; } set { _assemblyName = value; } }
		[SerializeField]
		string _typeName;
		public string TypeName { get { return _typeName; } set { _typeName = value; } }
		[SerializeField]
		string _methodName;
		public string MethodName { get { return _methodName; } set { _methodName = value; } }
		[SerializeField]
		bool _isPublic;
		public bool IsPublic { get { return _isPublic; } set { _isPublic = value; } }
		[SerializeField]
		bool _isStatic;
		public bool IsStatic { get { return _isStatic; } set { _isStatic = value; } }

		public MethodInfo Method
		{
			get
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
							return type.GetMethod(MethodName, flags);
						}
					}
				}
				return null;
			}
			set
			{
				AssemblyName = value.DeclaringType.Assembly.GetName().Name;
				TypeName = value.DeclaringType.FullName;
				MethodName = value.Name;
				IsPublic = value.IsPublic;
				IsStatic = value.IsStatic;
			}
		}

		public SerializedMethod(MethodInfo method)
		{
			_assemblyName = method.DeclaringType.Assembly.GetName().Name;
			_typeName = method.DeclaringType.FullName;
			_methodName = method.Name;
			_isPublic = method.IsPublic;
			_isStatic = method.IsStatic;
		}

		public SerializedMethod(Delegate method) : this(method.Method) { }
	}
}
