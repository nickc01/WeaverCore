using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
#if USE_EMIT
using System.Reflection.Emit;
#endif

//If USE_EMIT is defined, then it will use Reflection.Emit to create a fast copier.
//If USE_EMIT is not defined, then it will use a reflection-based method every time the copier function is called, which will end up being slower

namespace WeaverCore.Utilities
{
	public class FieldCopier<TypeToCopyFields>
	{
#if USE_EMIT
		DynamicMethod finalCopyMethod;
		ILGenerator gen;
#else
		List<FieldInfo> fieldsToCopy = new List<FieldInfo>();
		Action<TypeToCopyFields, TypeToCopyFields> finalFunction;
#endif
		bool returnAdded = false;

		public FieldCopier()
		{
#if USE_EMIT
			finalCopyMethod = new DynamicMethod(typeof(TypeToCopyFields).FullName + "_fieldCopier", null, new Type[2] { typeof(TypeToCopyFields), typeof(TypeToCopyFields) }, true);
			gen = finalCopyMethod.GetILGenerator();
#endif
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
#if USE_EMIT
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldfld, field);
			gen.Emit(OpCodes.Stfld, field);
#else
			fieldsToCopy.Add(field);
#endif
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
#if USE_EMIT
				gen.Emit(OpCodes.Ret);
#endif
				returnAdded = true;
			}
#if USE_EMIT
			return (Action<TypeToCopyFields, TypeToCopyFields>)finalCopyMethod.CreateDelegate(typeof(Action<TypeToCopyFields, TypeToCopyFields>));
#else
			if (finalFunction == null)
			{
				finalFunction = (source, dest) =>
				{
					for (int i = 0; i < fieldsToCopy.Count; i++)
					{
						fieldsToCopy[i].SetValue(dest, fieldsToCopy[i].GetValue(source));
					}
				};
			}
			return finalFunction;
#endif
		}
	}
}
