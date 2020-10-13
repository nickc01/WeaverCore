using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace WeaverCore.Utilities
{
	public class FieldCopier<TypeToCopyFields>
	{
		DynamicMethod finalCopyMethod;
		ILGenerator gen;
		bool returnAdded = false;

		public FieldCopier()
		{
			finalCopyMethod = new DynamicMethod(typeof(TypeToCopyFields).FullName + "_fieldCopier", null, new Type[2] { typeof(TypeToCopyFields), typeof(TypeToCopyFields) }, true);
			gen = finalCopyMethod.GetILGenerator();
		}

		public void AddField(FieldInfo field)
		{
			if (returnAdded)
			{
				throw new Exception("No new fields can be added once the method has been created");
			}
			if (field.ReflectedType != typeof(TypeToCopyFields))
			{
				throw new Exception("The field " + field.Name + " is not declared in the type " + typeof(TypeToCopyFields).FullName);
			}
			if (field.IsStatic)
			{
				throw new Exception("The field " + field.Name + " is static. Static fields are not allowed in the copier");
			}
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldfld, field);
			gen.Emit(OpCodes.Stfld, field);
		}

		public void AddField(string fieldName, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		{
			var field = typeof(TypeToCopyFields).GetField(fieldName, flags);
			if (field == null)
			{
				throw new Exception("The type " + typeof(TypeToCopyFields).FullName + " does not have a field called " + fieldName);
			}
			AddField(field);
		}

		public void AddRange(IEnumerable<FieldInfo> fields)
		{
			foreach (var field in fields)
			{
				AddField(field);
			}
		}

		/// <summary>
		/// Creates the finished method that copies the fields from one type to another. The first parameter is the source type, and the second parameter is the destination type
		/// </summary>
		/// <returns></returns>
		public Action<TypeToCopyFields, TypeToCopyFields> Finish()
		{
			if (!returnAdded)
			{
				gen.Emit(OpCodes.Ret);
				returnAdded = true;
			}
			return (Action<TypeToCopyFields, TypeToCopyFields>)finalCopyMethod.CreateDelegate(typeof(Action<TypeToCopyFields, TypeToCopyFields>));
		}
	}
}
