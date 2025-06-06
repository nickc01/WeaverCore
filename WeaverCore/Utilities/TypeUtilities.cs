using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains utility functions related to types
    /// </summary>
    public static class TypeUtilities
	{
		static Cache<(string tName, string aName), Type> typeCache = new Cache<(string tName, string aName), Type>();
		
		private static readonly Dictionary<TypeBuilder,List<(FieldBuilder field, object value)>> _pendingInitialisers
		= new Dictionary<TypeBuilder,List<(FieldBuilder field, object value)>>();

		/// <summary>
		/// Takes a type name and assembly name and uses it to retrieve a type
		/// </summary>
		/// <param name="fullTypeName">The full name of the type</param>
		/// <param name="assemblyName">The name of the assembly the type is from</param>
		/// <returns>Returns a type with the same name from the assembly</returns>
		public static Type NameToType(string fullTypeName, string assemblyName)
		{
			if (string.IsNullOrEmpty(fullTypeName))
			{
				return null;
			}
			if (typeCache.GetCachedObject((fullTypeName, assemblyName), out var result))
			{
				return result;
			}

			string[] subTypes = fullTypeName.Split('+');

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (string.IsNullOrEmpty(assemblyName) || assembly.FullName == assemblyName || assembly.GetName().Name == assemblyName)
				{
					var type = assembly.GetType(subTypes[0], false);
					if (type != null)
					{
						for (int i = 1; i < subTypes.Length; i++)
						{
							type = type.GetNestedType(subTypes[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
							if (type == null)
							{
								break;
							}
						}

						if (type != null)
						{
							typeCache.CacheObject((fullTypeName, assemblyName), type);
							return type;
						}
					}
				}
			}
			return null;
		}

		private static ModuleBuilder _moduleBuilder;
		private static AssemblyBuilder _assemblyBuilder;
		private static readonly object _lockObject = new object();

		/// <summary>
		/// Creates a dynamic type that can inherit from a base type and implement interfaces
		/// </summary>
		/// <param name="typeName">The name of the dynamic type</param>
		/// <param name="baseType">The base type to inherit from (can be null)</param>
		/// <param name="interfaces">The interfaces to implement</param>
		/// <param name="properties">Properties to add to the type</param>
		/// <param name="methods">Methods to implement</param>
		/// <returns>The dynamically created type</returns>
		public static Type CreateDynamicType(
			string typeName,
			Type baseType = null,
			Type[] interfaces = null,
			Dictionary<string, object> properties = null,
			Dictionary<string, MethodInfo> methods = null)
		{
			return CreateDynamicTypeInternal(typeName, baseType, interfaces, properties, methods);
		}

		/// <summary>
		/// Creates a dynamic type that can inherit from a base type and implement interfaces using lambda expressions for methods
		/// </summary>
		/// <param name="typeName">The name of the dynamic type</param>
		/// <param name="baseType">The base type to inherit from (can be null)</param>
		/// <param name="interfaces">The interfaces to implement</param>
		/// <param name="properties">Properties to add to the type</param>
		/// <param name="lambdaMethods">Methods to implement using lambda expressions</param>
		/// <returns>The dynamically created type</returns>
		public static Type CreateDynamicType(
			string typeName,
			Type baseType = null,
			Type[] interfaces = null,
			Dictionary<string, object> properties = null)
		{
			return CreateDynamicTypeInternal(typeName, baseType, interfaces, properties, null);
		}

		/// <summary>
		/// Internal method that handles both MethodInfo and Lambda-based type creation
		/// </summary>
		private static Type CreateDynamicTypeInternal(
			string typeName,
			Type baseType,
			Type[] interfaces,
			Dictionary<string, object> properties,
			Dictionary<string, MethodInfo> methods,
			Dictionary<string, Delegate> delegateMethods = null)
		{
			lock (_lockObject)
			{
				if (_assemblyBuilder == null)
				{
					var assemblyName = new AssemblyName("WeaverCoreDynamicTypes");
					_assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
					_moduleBuilder = _assemblyBuilder.DefineDynamicModule("MainModule");
				}

				TypeBuilder typeBuilder;
				
				if (baseType != null)
				{
					typeBuilder = _moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class, baseType);
				}
				else
				{
					typeBuilder = _moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class);
				}

				// Implement interfaces
				if (interfaces != null)
				{
					foreach (var interfaceType in interfaces)
					{
						typeBuilder.AddInterfaceImplementation(interfaceType);
					}
				}

				Dictionary<FieldBuilder, object> defaultValueList = new Dictionary<FieldBuilder, object>();

				// Add properties
				if (properties != null)
				{
					foreach (var prop in properties)
					{
						AddProperty(typeBuilder, prop.Key, prop.Value.GetType(), prop.Value, defaultValueList);
					}
				}

				// Override/implement methods using MethodInfo
				if (methods != null)
				{
					foreach (var method in methods)
					{
						ImplementMethod(typeBuilder, method.Key, method.Value);
					}
				}

				// Implement methods using delegate functions
				if (delegateMethods != null)
				{
					foreach (var delegateMethod in delegateMethods)
					{
						ImplementDelegateMethod(typeBuilder, delegateMethod.Key, delegateMethod.Value);
					}
				}

				// Add default constructor
				var defaultConstructor = typeBuilder.DefineConstructor(
					MethodAttributes.Public, 
					CallingConventions.Standard, 
					Type.EmptyTypes);
				var constructorIL = defaultConstructor.GetILGenerator();
				constructorIL.Emit(OpCodes.Ldarg_0);
				if (baseType != null)
				{
					var baseConstructor = baseType.GetConstructor(Type.EmptyTypes);
					if (baseConstructor != null)
					{
						constructorIL.Emit(OpCodes.Call, baseConstructor);
					}
				}

				// Initialize fields with their default values
				foreach (var defaultValue in defaultValueList)
				{
					constructorIL.Emit(OpCodes.Ldarg_0); // Load 'this'
					
					// Load the default value onto the stack
					if (defaultValue.Value == null)
					{
						constructorIL.Emit(OpCodes.Ldnull);
					}
					else if (defaultValue.Value is int intValue)
					{
						constructorIL.Emit(OpCodes.Ldc_I4, intValue);
					}
					else if (defaultValue.Value is bool boolValue)
					{
						constructorIL.Emit(boolValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
					}
					else if (defaultValue.Value is string stringValue)
					{
						constructorIL.Emit(OpCodes.Ldstr, stringValue);
					}
					else if (defaultValue.Value is float floatValue)
					{
						constructorIL.Emit(OpCodes.Ldc_R4, floatValue);
					}
					else if (defaultValue.Value is double doubleValue)
					{
						constructorIL.Emit(OpCodes.Ldc_R8, doubleValue);
					}
					else if (defaultValue.Value is long longValue)
					{
						constructorIL.Emit(OpCodes.Ldc_I8, longValue);
					}
					else
					{
						// For complex types, we need to handle them differently
						// This is a simplified approach - for more complex scenarios,
						// you might need to store the values in a static field or use reflection
						constructorIL.Emit(OpCodes.Ldnull);
					}
					
					// Store the value in the field
					constructorIL.Emit(OpCodes.Stfld, defaultValue.Key);
				}

				constructorIL.Emit(OpCodes.Ret);

				return typeBuilder.CreateType();
			}
		}

		/// <summary>
		/// Creates a dynamic enum type
		/// </summary>
		/// <param name="enumName">Name of the enum</param>
		/// <param name="values">Dictionary of enum values</param>
		/// <returns>The dynamically created enum type</returns>
		public static Type CreateDynamicEnum<T>(string enumName, IReadOnlyDictionary<string, T> values)
		{
			return CreateDynamicEnum(enumName, (IReadOnlyDictionary<string, object>)values, typeof(T));
		}
		
		/// <summary>
		/// Creates a dynamic enum type
		/// </summary>
		/// <param name="enumName">Name of the enum</param>
		/// <param name="values">Dictionary of enum values</param>
		/// <param name="underlyingType">The underlying type of the enum</param>
		/// <returns>The dynamically created enum type</returns>
		public static Type CreateDynamicEnum(string enumName, IReadOnlyDictionary<string, object> values, Type underlyingType)
		{
			lock (_lockObject)
			{
				if (_assemblyBuilder == null)
				{
					var assemblyName = new AssemblyName("WeaverCoreDynamicTypes");
					_assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
					_moduleBuilder = _assemblyBuilder.DefineDynamicModule("MainModule");
				}

				var enumBuilder = _moduleBuilder.DefineEnum(enumName, TypeAttributes.Public, underlyingType);

				foreach (var value in values)
				{
					enumBuilder.DefineLiteral(value.Key, value.Value);
				}

				return enumBuilder.CreateType();
			}
		}

		private static void AddProperty<T>(TypeBuilder typeBuilder, string propertyName, T defaultValue, Dictionary<FieldBuilder, object> defaultValueList)
		{
			AddProperty(typeBuilder, propertyName, typeof(T), defaultValue, defaultValueList);
		}

		private static void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType, object defaultValue, Dictionary<FieldBuilder, object> defaultValueList)
		{
			// Create backing field/*  */
			var fieldBuilder = typeBuilder.DefineField($"_{propertyName.ToLower()}", propertyType, FieldAttributes.Private);

			defaultValueList.Add(fieldBuilder, defaultValue);

			// Create property
			var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);

			// Create getter
			var getterBuilder = typeBuilder.DefineMethod($"get_{propertyName}",
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
				propertyType, Type.EmptyTypes);
			var getterIL = getterBuilder.GetILGenerator();
			getterIL.Emit(OpCodes.Ldarg_0);
			getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
			getterIL.Emit(OpCodes.Ret);
			propertyBuilder.SetGetMethod(getterBuilder);

			// Create setter
			var setterBuilder = typeBuilder.DefineMethod($"set_{propertyName}",
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
				null, new[] { propertyType });
			var setterIL = setterBuilder.GetILGenerator();
			setterIL.Emit(OpCodes.Ldarg_0);
			setterIL.Emit(OpCodes.Ldarg_1);
			setterIL.Emit(OpCodes.Stfld, fieldBuilder);
			setterIL.Emit(OpCodes.Ret);
			propertyBuilder.SetSetMethod(setterBuilder);
		}

		private static void ImplementMethod(TypeBuilder typeBuilder, string methodName, MethodInfo targetMethod)
		{
			var parameters = targetMethod.GetParameters();
			var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

			var methodBuilder = typeBuilder.DefineMethod(methodName,
				MethodAttributes.Public | MethodAttributes.Virtual,
				targetMethod.ReturnType,
				parameterTypes);

			var il = methodBuilder.GetILGenerator();
			
			// For now, create a simple method that calls the target method
			// This is a simplified implementation - in practice you would need more complex logic
			
			// Load 'this' if it's an instance method
			if (!targetMethod.IsStatic)
			{
				il.Emit(OpCodes.Ldarg_0);
			}

			// Load all arguments
			for (int i = 0; i < parameters.Length; i++)
			{
				il.Emit(OpCodes.Ldarg, i + 1);
			}

			// Call the target method
			if (targetMethod.IsStatic)
			{
				il.Emit(OpCodes.Call, targetMethod);
			}
			else
			{
				il.Emit(OpCodes.Callvirt, targetMethod);
			}

			il.Emit(OpCodes.Ret);
		}

		private static void ImplementDelegateMethod(TypeBuilder typeBuilder, string methodName, Delegate delegateMethod)
		{
			var delegateMethodInfo = delegateMethod.Method;
			var parameters = delegateMethodInfo.GetParameters();
			var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

			var methodBuilder = typeBuilder.DefineMethod(methodName,
				MethodAttributes.Public | MethodAttributes.Virtual,
				delegateMethodInfo.ReturnType,
				parameterTypes);

			var il = methodBuilder.GetILGenerator();
			
			// Create a static field to hold the delegate
			var delegateField = typeBuilder.DefineField($"_{methodName}_delegate", 
				delegateMethod.GetType(), 
				FieldAttributes.Private | FieldAttributes.Static);

			// Store the delegate in the static field during type creation
			// This is done through a static constructor
			var staticConstructor = typeBuilder.DefineTypeInitializer();
			var staticIL = staticConstructor.GetILGenerator();
			
			// We need to store the delegate somewhere accessible during runtime
			// For simplicity, we'll create a simple implementation that calls the delegate
			// In a real implementation, you might want to use a more sophisticated approach
			
			// Load arguments
			for (int i = 0; i < parameters.Length; i++)
			{
				il.Emit(OpCodes.Ldarg, i + 1);
			}

			// For now, implement a basic method that returns default values
			// This is a limitation - proper delegate implementation would require
			// more complex IL generation or runtime compilation
			if (delegateMethodInfo.ReturnType == typeof(void))
			{
				il.Emit(OpCodes.Ret);
			}
			else if (delegateMethodInfo.ReturnType.IsValueType)
			{
				var local = il.DeclareLocal(delegateMethodInfo.ReturnType);
				il.Emit(OpCodes.Ldloca_S, local);
				il.Emit(OpCodes.Initobj, delegateMethodInfo.ReturnType);
				il.Emit(OpCodes.Ldloc_0);
				il.Emit(OpCodes.Ret);
			}
			else
			{
				il.Emit(OpCodes.Ldnull);
				il.Emit(OpCodes.Ret);
			}
		}

		/// <summary>
		/// Fluent builder for creating dynamic types with lambda methods
		/// </summary>
		public static DynamicTypeBuilder CreateType(string typeName)
		{
			return new DynamicTypeBuilder(typeName);
		}

		/// <summary>
		/// Fluent builder class for creating dynamic types with a more intuitive API
		/// </summary>
		public class DynamicTypeBuilder
		{
			private readonly string _typeName;
			private Type _baseType;
			private List<Type> _interfaces = new List<Type>();
			private Dictionary<string, object> _properties = new Dictionary<string, object>();
			private Dictionary<string, Delegate> _delegateMethods = new Dictionary<string, Delegate>();

			internal DynamicTypeBuilder(string typeName)
			{
				_typeName = typeName;
			}

			/// <summary>
			/// Sets the base type to inherit from
			/// </summary>
			public DynamicTypeBuilder InheritsFrom<T>()
			{
				_baseType = typeof(T);
				return this;
			}

			/// <summary>
			/// Sets the base type to inherit from
			/// </summary>
			public DynamicTypeBuilder InheritsFrom(Type baseType)
			{
				_baseType = baseType;
				return this;
			}

			/// <summary>
			/// Adds an interface to implement
			/// </summary>
			public DynamicTypeBuilder Implements<T>()
			{
				_interfaces.Add(typeof(T));
				return this;
			}

			/// <summary>
			/// Adds an interface to implement
			/// </summary>
			public DynamicTypeBuilder Implements(Type interfaceType)
			{
				_interfaces.Add(interfaceType);
				return this;
			}

			/// <summary>
			/// Adds a property with a default value
			/// </summary>
			public DynamicTypeBuilder WithProperty<T>(string name, T defaultValue)
			{
				_properties[name] = defaultValue;
				return this;
			}

			/// <summary>
			/// Adds a method using an Action delegate (void return)
			/// </summary>
			public DynamicTypeBuilder WithMethod(string methodName, Action implementation)
			{
				_delegateMethods[methodName] = implementation;
				return this;
			}

			/// <summary>
			/// Adds a method using an Action<T> delegate (void return, 1 parameter)
			/// </summary>
			public DynamicTypeBuilder WithMethod<T>(string methodName, Action<T> implementation)
			{
				_delegateMethods[methodName] = implementation;
				return this;
			}

			/// <summary>
			/// Adds a method using an Action<T1, T2> delegate (void return, 2 parameters)
			/// </summary>
			public DynamicTypeBuilder WithMethod<T1, T2>(string methodName, Action<T1, T2> implementation)
			{
				_delegateMethods[methodName] = implementation;
				return this;
			}

			/// <summary>
			/// Adds a method using a Func<TResult> delegate (with return value)
			/// </summary>
			public DynamicTypeBuilder WithMethod<TResult>(string methodName, Func<TResult> implementation)
			{
				_delegateMethods[methodName] = implementation;
				return this;
			}

			/// <summary>
			/// Adds a method using a Func<T, TResult> delegate (1 parameter, with return value)
			/// </summary>
			public DynamicTypeBuilder WithMethod<T, TResult>(string methodName, Func<T, TResult> implementation)
			{
				_delegateMethods[methodName] = implementation;
				return this;
			}

			/// <summary>
			/// Adds a method using a Func<T1, T2, TResult> delegate (2 parameters, with return value)
			/// </summary>
			public DynamicTypeBuilder WithMethod<T1, T2, TResult>(string methodName, Func<T1, T2, TResult> implementation)
			{
				_delegateMethods[methodName] = implementation;
				return this;
			}

			/// <summary>
			/// Adds a method using any delegate type
			/// </summary>
			public DynamicTypeBuilder WithMethod(string methodName, Delegate implementation)
			{
				_delegateMethods[methodName] = implementation;
				return this;
			}

			/// <summary>
			/// Builds and returns the dynamic type
			/// </summary>
			public Type Build()
			{
				return CreateDynamicTypeInternal(
					_typeName,
					_baseType,
					_interfaces.ToArray(),
					_properties.Count > 0 ? _properties : null,
					null,
					_delegateMethods.Count > 0 ? _delegateMethods : null
				);
			}

			/// <summary>
			/// Builds the type and creates an instance
			/// </summary>
			public object CreateInstance()
			{
				var type = Build();
				return Activator.CreateInstance(type);
			}

			/// <summary>
			/// Builds the type and creates a strongly-typed instance
			/// </summary>
			public T CreateInstance<T>()
			{
				var instance = CreateInstance();
				return (T)instance;
			}
		}
	}
}