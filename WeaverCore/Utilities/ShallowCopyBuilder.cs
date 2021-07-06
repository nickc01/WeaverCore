using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if NET_4_6
using System.Reflection.Emit;
#endif

namespace WeaverCore.Utilities
{
	public sealed class ShallowCopyBuilder<ParameterType>
	{
		public delegate void ShallowCopyDelegate(ParameterType objectToOverwrite, ParameterType objectToCopyFrom);
#if NET_4_6
		private readonly DynamicMethod finalCopyMethod;
		private readonly ILGenerator gen;
#else
		ShallowCopyDelegate delegateVersion;
		Action<ParameterType, ParameterType> funcVersion;

		private readonly List<FieldInfo> fieldsToCopy = new List<FieldInfo>();
#endif
		private bool returnAdded = false;
		private readonly Type copyType;
		private readonly Type paramType = typeof(ParameterType);

		public ShallowCopyBuilder() : this(typeof(ParameterType)) { }

		public ShallowCopyBuilder(Type typeToCopy)
		{
			if (!paramType.IsAssignableFrom(typeToCopy))
			{
				throw new Exception("The Type [" + typeToCopy.FullName + "] does not inherit from the parameter type [" + paramType.FullName + "]");
			}
			copyType = typeToCopy;
#if NET_4_6
			finalCopyMethod = new DynamicMethod(copyType.FullName + "_fieldCopier", null, new Type[2] { paramType, paramType }, true);
			gen = finalCopyMethod.GetILGenerator();

			if (copyType != paramType)
			{
				gen.DeclareLocal(copyType);
				gen.DeclareLocal(copyType);

				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Castclass, copyType);
				gen.Emit(OpCodes.Stloc_0);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Castclass, copyType);
				gen.Emit(OpCodes.Stloc_1);
			}
#endif
		}

		public void AddField(FieldInfo field)
		{
			if (returnAdded)
			{
				throw new Exception("No new fields can be added once the method has been created");
			}
			if (field.ReflectedType != copyType)
			{
				throw new Exception("The field " + field.Name + " is not declared in the type " + copyType.FullName);
			}
			if (field.IsStatic)
			{
				throw new Exception("The field " + field.Name + " is static. Static fields are not allowed in the copier");
			}
#if NET_4_6
			if (copyType != paramType)
			{
				gen.Emit(OpCodes.Ldloc_0);
				gen.Emit(OpCodes.Ldloc_1);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldarg_1);
			}
			gen.Emit(OpCodes.Ldfld, field);
			gen.Emit(OpCodes.Stfld, field);
#else
			fieldsToCopy.Add(field);
#endif
		}

		public void AddField(string fieldName, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		{
			FieldInfo field = copyType.GetField(fieldName, flags);
			if (field == null)
			{
				throw new Exception("The type " + copyType.FullName + " does not have a field called " + fieldName);
			}
			AddField(field);
		}

		public void AddRange(IEnumerable<FieldInfo> fields)
		{
			foreach (FieldInfo field in fields)
			{
				AddField(field);
			}
		}

		/// <summary>
		/// Creates the finished method that copies the fields from one type to another. The first parameter is the object to copy the data to, and the second parameter is the object to copy the data from
		/// </summary>
		/// <returns></returns>
		public ShallowCopyDelegate Finish()
		{
			if (!returnAdded)
			{
#if NET_4_6
				gen.Emit(OpCodes.Ret);
#endif
				returnAdded = true;
			}
#if NET_4_6
			return (ShallowCopyDelegate)finalCopyMethod.CreateDelegate(typeof(ShallowCopyDelegate));
#else
			if (delegateVersion == null)
			{
				delegateVersion = (dest, source) =>
				{
					//Debug.Log("Copying Data for types = " + source.GetType().FullName);
					for (int i = 0; i < fieldsToCopy.Count; i++)
					{
						//Debug.Log("Source " + fieldsToCopy[i].Name + " = " + fieldsToCopy[i].GetValue(source));
						//Debug.Log("Destination " + fieldsToCopy[i].Name + " = " + fieldsToCopy[i].GetValue(dest));
						fieldsToCopy[i].SetValue(dest, fieldsToCopy[i].GetValue(source));

						//Debug.Log("Post Destination " + fieldsToCopy[i].Name + " = " + fieldsToCopy[i].GetValue(dest));
					}
				};
			}
			return delegateVersion;
#endif
		}

		public Action<ParameterType, ParameterType> FinishFunc()
		{
			if (!returnAdded)
			{
#if NET_4_6
				gen.Emit(OpCodes.Ret);
#endif
				returnAdded = true;
			}
#if NET_4_6
			return (Action<ParameterType, ParameterType>)finalCopyMethod.CreateDelegate(typeof(Action<ParameterType, ParameterType>));
#else
			if (funcVersion == null)
			{
				funcVersion = (dest, source) =>
				{
					for (int i = 0; i < fieldsToCopy.Count; i++)
					{
						fieldsToCopy[i].SetValue(dest, fieldsToCopy[i].GetValue(source));
					}
				};
			}
			return funcVersion;
#endif
		}
	}
}
