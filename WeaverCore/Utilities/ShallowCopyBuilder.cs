﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace WeaverCore.Utilities
{
	public sealed class ShallowCopyBuilder<ParameterType>
	{
		public delegate void ShallowCopyDelegate(ParameterType objectToOverwrite, ParameterType objectToCopyFrom);

		private readonly DynamicMethod finalCopyMethod;
		private readonly ILGenerator gen;
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
				gen.Emit(OpCodes.Ret);
				returnAdded = true;
			}
			return (ShallowCopyDelegate)finalCopyMethod.CreateDelegate(typeof(ShallowCopyDelegate));
		}

		public Func<ParameterType, ParameterType> FinishFunc()
		{
			if (!returnAdded)
			{
				gen.Emit(OpCodes.Ret);
				returnAdded = true;
			}
			return (Func<ParameterType, ParameterType>)finalCopyMethod.CreateDelegate(typeof(Func<ParameterType, ParameterType>));
		}
	}
}