using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WeaverCore.Utilities
{
	public sealed class FieldAccessor<InstanceType, FieldType>
	{
		Func<InstanceType, FieldType> getter;
		Action<InstanceType, FieldType> setter;

		public InstanceType Instance { get; private set; }

		public FieldAccessor(InstanceType instance, FieldInfo field)
		{
			Instance = instance;
			getter = ReflectionUtilities.CreateFieldGetter<InstanceType, FieldType>(field);
			setter = ReflectionUtilities.CreateFieldSetter<InstanceType, FieldType>(field);
		}

		public FieldType Value
		{
			get => getter(Instance);
			set => setter(Instance, value);
		}
	}
	
	/// <summary>
	/// Makes it easier for accessing many private fields, properties, and methods of a type.
	/// It also uses caching to make access times as fast as possible
	/// </summary>
	/// <typeparam name="T">The type of the objec to be inspected. Use <see cref="object"/> if you don't know the type of the instance</typeparam>
	public sealed class ReflectionAccessor<T>
	{
		public const BindingFlags INSTANCE_BINDINGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		public T Instance { get; private set; }
		public Type ReflectedType { get; private set; }

		public ReflectionAccessor(T instance)
		{
			Instance = instance;
			ReflectedType = typeof(T);
			if (ReflectedType == typeof(object))
			{
				ReflectedType = instance.GetType();
			}
		}

		public FieldAccessor<T,FieldType> GetFieldAccessor<FieldType>(string fieldName, BindingFlags flags = INSTANCE_BINDINGS)
		{
			return new FieldAccessor<T, FieldType>(Instance, ReflectedType.GetField(fieldName, flags));
		}
	}
}
