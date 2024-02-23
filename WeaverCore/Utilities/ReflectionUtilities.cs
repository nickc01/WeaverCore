using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains many utility functions for reflection
	/// </summary>
	public static class ReflectionUtilities
	{
		/// <summary>
		/// Retrives all types that inherit from <paramref name="parentType"/>
		/// </summary>
		/// <param name="parentType">The common base class to check for</param>
		/// <param name="includeAbstract">Should abstract classes also be included?</param>
		/// <returns>Returns a list of all types that inherit from <paramref name="parentType"/></returns>
		public static IEnumerable<Type> GetChildrenOfType(Type parentType, bool includeAbstract = false)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (Initialization.Environment == Enums.RunningState.Game && assembly.GetName().Name == "Assembly-CSharp")
				{
					continue;
				}
				else
				{
					Type[] types = null;
					try
					{
						types = assembly.GetTypes();
					}
					catch (Exception)
					{
						//Assembly is broken, skip over it
					}
					if (types != null)
					{
						foreach (var type in assembly.GetTypes())
						{
							if (parentType.IsAssignableFrom(type))
							{
								if (includeAbstract)
								{
									yield return type;
								}
								else if (!type.IsAbstract && !type.ContainsGenericParameters && !type.IsInterface)
								{
									yield return type;
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Retrives all types that inherit from type <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">The common base class to check for</typeparam>
		/// <param name="includeAbstract">Should abstract classes also be included?</param>
		/// <returns>Returns a list of all types that inherit from type <typeparamref name="T"/></returns>
		public static IEnumerable<Type> GetChildrenOfType<T>(bool includeAbstract = false)
		{
			return GetChildrenOfType(typeof(T), includeAbstract);
		}

		/// <summary>
		/// Retrieves all types that inherit from type <typeparamref name="T"/>, and create instances of each of them
		/// </summary>
		/// <typeparam name="T">The common base class to check for</typeparam>
		/// <param name="throwOnFailure">If an object fails to instantiate, should an exception be thrown? If false, that object will be skipped over</param>
		/// <returns>Returns a list of all objects that inherit from type <typeparamref name="T"/></returns>
		public static IEnumerable<T> GetObjectsOfType<T>(bool throwOnFailure = false)
		{
			foreach (var type in GetChildrenOfType(typeof(T)))
			{
				bool created = false;
				T instance = default(T);
				try
				{
					instance = (T)Activator.CreateInstance(type);
					created = true;
				}
				catch (Exception)
				{
					if (throwOnFailure)
					{
						throw;
					}
				}
				if (created)
				{
					yield return instance;
				}
			}
		}

		/// <summary>
		/// Creates a function that retrives a field value
		/// </summary>
		/// <typeparam name="SourceType">The type that contains the field</typeparam>
		/// <typeparam name="FieldType">The type of the field</typeparam>
		/// <param name="field">The field to create a getter for</param>
		/// <returns>Returns a function that when invoked, will retrieve the field value</returns>
		public static Func<SourceType, FieldType> CreateFieldGetter<SourceType, FieldType>(FieldInfo field)
		{
#if NET_4_6
			string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, typeof(FieldType), new Type[1] { typeof(SourceType) }, true);
			ILGenerator gen = setterMethod.GetILGenerator();
			if (field.IsStatic)
			{
				gen.Emit(OpCodes.Ldsfld, field);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldfld, field);
			}
			gen.Emit(OpCodes.Ret);
			return (Func<SourceType, FieldType>)setterMethod.CreateDelegate(typeof(Func<SourceType, FieldType>));
#else
			return (source) =>
			{
				return (FieldType)field.GetValue(source);
			};
#endif
		}

		/// <summary>
		/// Creates a function that retrives a field value
		/// </summary>
		/// <typeparam name="SourceType">The type that contains the field</typeparam>
		/// <typeparam name="FieldType">The type of the field</typeparam>
		/// <param name="fieldName">The name of the field to create a getter for</param>
		/// <param name="flags">The binding flags used to find the field</param>
		/// <returns>Returns a function that when invoked, will retrieve the field value</returns>
		public static Func<SourceType, FieldType> CreateFieldGetter<SourceType, FieldType>(string fieldName, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		{
			return CreateFieldGetter<SourceType, FieldType>(typeof(SourceType).GetField(fieldName, flags));
		}

		/// <summary>
		/// Creates a funciton that sets a field value
		/// </summary>
		/// <typeparam name="SourceType">The type that contains the field</typeparam>
		/// <typeparam name="FieldType">The type of the field</typeparam>
		/// <param name="field">The field to create a setter for</param>
		/// <returns>Returns a function that when invoked, will set the field value</returns>
		public static Action<SourceType, FieldType> CreateFieldSetter<SourceType, FieldType>(FieldInfo field)
		{
#if NET_4_6
			string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[2] { typeof(SourceType), typeof(FieldType) }, true);
			ILGenerator gen = setterMethod.GetILGenerator();
			if (field.IsStatic)
			{
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Stsfld, field);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Stfld, field);
			}
			gen.Emit(OpCodes.Ret);
			return (Action<SourceType, FieldType>)setterMethod.CreateDelegate(typeof(Action<SourceType, FieldType>));
#else
			return (source, value) =>
			{
				field.SetValue(source, value);
				//return (FieldType)field.GetValue(source);
			};
#endif
		}

		/// <summary>
		/// Creates a funciton that sets a field value
		/// </summary>
		/// <typeparam name="SourceType">The type that contains the field</typeparam>
		/// <typeparam name="FieldType">The type of the field</typeparam>
		/// <param name="fieldName">The name of the field to create a setter for</param>
		/// <param name="flags">The binding flags used to find the field</param>
		/// <returns>Returns a function that when invoked, will set the field value</returns>
		public static Action<SourceType, FieldType> CreateFieldSetter<SourceType, FieldType>(string fieldName, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		{
			return CreateFieldSetter<SourceType, FieldType>(typeof(SourceType).GetField(fieldName, flags));
		}

		/// <summary>
		/// Converts a MethodInfo object into a callable delegate
		/// </summary>
		/// <typeparam name="DelegateType">The delegate type to convert type</typeparam>
		/// <param name="method">The method info</param>
		/// <returns>The delegate that will call the method</returns>
		public static DelegateType MethodToDelegate<DelegateType>(MethodInfo method)
		{
			if (!typeof(Delegate).IsAssignableFrom(typeof(DelegateType)))
			{
				throw new Exception(typeof(DelegateType).FullName + " is not a delegate type");
			}
			return (DelegateType)(object)Delegate.CreateDelegate(typeof(DelegateType), method);
		}

		/// <summary>
		/// Converts a MethodInfo object into a callable delegate
		/// </summary>
		/// <typeparam name="DelegateType">The delegate type to convert type</typeparam>
		/// <param name="method">The method info</param>
		/// <param name="instance">The object instance that the method is bound to</param>
		/// <returns>The delegate that will call the method</returns>
		public static DelegateType MethodToDelegate<DelegateType>(MethodInfo method, object instance)
		{
			if (!typeof(Delegate).IsAssignableFrom(typeof(DelegateType)))
			{
				throw new Exception(typeof(DelegateType).FullName + " is not a delegate type");
			}
			return (DelegateType)(object)Delegate.CreateDelegate(typeof(DelegateType), instance, method);
		}

		/// <summary>
		/// Converts a MethodInfo object into a callable delegate
		/// </summary>
		/// <typeparam name="DelegateType">The delegate type to convert type</typeparam>
		/// <typeparam name="InstanceType">The instance type the method is bound to</typeparam>
		/// <param name="methodName">The name of the method</param>
		/// <param name="Flags">The binding flags used to find the method on the object</param>
		/// <returns>The delegate that will call the method</returns>
		public static DelegateType MethodToDelegate<DelegateType, InstanceType>(string methodName, BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
		{
			return MethodToDelegate<DelegateType>(typeof(InstanceType).GetMethod(methodName, Flags));
		}

		/// <summary>
		/// Converts a MethodInfo object into a callable delegate
		/// </summary>
		/// <typeparam name="DelegateType">The delegate type to convert type</typeparam>
		/// <typeparam name="InstanceType">The instance type the method is bound to</typeparam>
		/// <param name="methodName">The name of the method</param>
		/// <param name="instance">The instance object the method is bound to</param>
		/// <param name="Flags">The binding flags used to find the method on the object</param>
		/// <returns>The delegate that will call the method</returns>
		public static DelegateType MethodToDelegate<DelegateType, InstanceType>(string methodName, InstanceType instance, BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
		{
			if (!typeof(Delegate).IsAssignableFrom(typeof(DelegateType)))
			{
				throw new Exception(typeof(DelegateType).FullName + " is not a delegate type");
			}
			return (DelegateType)(object)Delegate.CreateDelegate(typeof(DelegateType), instance, typeof(InstanceType).GetMethod(methodName, Flags));
		}

		/// <summary>
		/// Finds a method on a given type and returns a MethodInfo object for it
		/// </summary>
		/// <typeparam name="InstanceType">The instance type to find the method under</typeparam>
		/// <param name="methodName">The name of the method to find</param>
		/// <param name="flags">The binding flags to find the method with</param>
		/// <returns>Returns a method info object for the fmound method</returns>
		public static MethodInfo GetMethod<InstanceType>(string methodName, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
		{
			return typeof(InstanceType).GetMethod(methodName, flags);
		}

        /// <summary>
        /// Finds all methods that have a certain attribute type applied to them
        /// </summary>
        /// <typeparam name="AttriType">The type attribute to look for</typeparam>
        /// <param name="flags">The binding flags to determine what kind of methods to find</param>
        /// <returns>Returns all methods with the specified attribute type applied to them</returns>
        public static IEnumerable<(MethodInfo method, AttriType attribute)> GetMethodsWithAttribute<AttriType>(BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) where AttriType : Attribute
        {
			return GetMethodsWithAttribute<AttriType>(AppDomain.CurrentDomain.GetAssemblies(), flags);
        }

        /// <summary>
        /// Finds all methods that have a certain attribute type applied to them
        /// </summary>
        /// <typeparam name="AttriType">The type attribute to look for</typeparam>
		/// <param name="assemblies">The assemblies to check</param>
        /// <param name="flags">The binding flags to determine what kind of methods to find</param>
        /// <returns>Returns all methods with the specified attribute type applied to them</returns>
        public static IEnumerable<(MethodInfo method, AttriType attribute)> GetMethodsWithAttribute<AttriType>(IEnumerable<Assembly> assemblies, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) where AttriType : Attribute
        {
            foreach (var assembly in assemblies)
            {
                foreach (var method in GetMethodsWithAttribute<AttriType>(assembly, flags))
                {
                    yield return method;
                }
            }
        }

		/// <summary>
		/// Finds all methods in an assembly that have a certain attribute type applied to them
		/// </summary>
		/// <typeparam name="AttriType">The type attribute to look for</typeparam>
		/// <param name="assembly">The assembly to look for the methods under</param>
		/// <param name="flags">The binding flags to determine what kind of methods to find</param>
		/// <returns>Returns all methods in the assembly with the specified attribute type applied to them</returns>
		public static IEnumerable<(MethodInfo method, AttriType attribute)> GetMethodsWithAttribute<AttriType>(Assembly assembly, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) where AttriType : Attribute
		{
			return GetMethodsWithAttribute<AttriType>(assembly, null, flags);
		}


		/// <summary>
		/// Finds all methods that have a certain attribute type applied to them and accept certain parameters
		/// </summary>
		/// <typeparam name="AttriType">The type attribute to look for</typeparam>
		/// <param name="paramTypes">The type of parameters that the methods must have</param>
		/// <param name="flags">The binding flags to determine what kind of methods to find</param>
		/// <returns>Returns all methods with the specified attribute type applied to them</returns>
		public static IEnumerable<(MethodInfo method, AttriType attribute)> GetMethodsWithAttribute<AttriType>(Type[] paramTypes, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) where AttriType : Attribute
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (!Initialization.IsAssemblyExcluded(assembly))
				{
                    foreach (var method in GetMethodsWithAttribute<AttriType>(assembly, paramTypes, flags))
                    {
                        yield return method;
                    }
                }
			}
		}

		/// <summary>
		/// Finds all methods in an assembly that have a certain attribute type applied to them and accept certain parameters
		/// </summary>
		/// <typeparam name="AttriType">The type attribute to look for</typeparam>
		/// <param name="assembly">The assembly to look for the methods under</param>
		/// <param name="paramTypes">The type of parameters that the methods must have</param>
		/// <param name="flags">The binding flags to determine what kind of methods to find</param>
		/// <returns>Returns all methods with the specified attribute type applied to them</returns>
		public static IEnumerable<ValueTuple<MethodInfo, AttriType>> GetMethodsWithAttribute<AttriType>(Assembly assembly, Type[] paramTypes, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) where AttriType : Attribute
		{
			List<ValueTuple<MethodInfo, AttriType>> methodsWithAttributes = new List<(MethodInfo, AttriType)>();

            if (assembly == null)
			{
				return methodsWithAttributes;
                //yield break;
            }
			Type[] types = null;

			try
			{
				types = assembly.GetTypes();
			}
			catch (Exception)
			{
#if !UNITY_EDITOR
				if (assembly.GetName().Name != "Assembly-CSharp")
				{
					WeaverLog.Log("Broken Assembly Found [" + assembly.GetName().Name + "]");
				}
#endif
                return methodsWithAttributes;
                //yield break;
            }

			foreach (var type in types)
			{
				MethodInfo[] methods = null;

				try
				{
					methods = type.GetMethods(flags);
				}
				catch (Exception e)
				{
					Debug.LogWarning($"WeaverCore Warning: Failed to obtain methods for type {type.FullName} in assembly {assembly.GetName().Name}. This may signify that {assembly.GetName().Name} has a problem");
                    Debug.LogWarning(e);
					continue;
				}

				//try
				//{
				foreach (var method in methods)
				{
					if (paramTypes != null)
					{
						ParameterInfo[] parameters = null;
						try
                        {
							parameters = method.GetParameters();
						}
						catch (Exception e)
                        {
							try
                            {
                                Debug.LogWarning($"WeaverCore Warning: Failed to obtain method parameters for {type.FullName}:{method.Name} in assembly {assembly.GetName().Name}. This may signify that {assembly.GetName().Name} has a problem");
                                Debug.LogWarning(e);
							}
							catch (Exception)
                            {
                                Debug.LogWarning(e);
							}
							continue;
						}

						if (parameters.Length != paramTypes.GetLength(0))
						{
							continue;
						}
						for (int i = 0; i < paramTypes.GetLength(0); i++)
						{
							if (parameters[i].ParameterType != paramTypes[i])
							{
								continue;
							}
						}
					}
					object[] attributes = null;
					try
					{
						attributes = method.GetCustomAttributes(typeof(AttriType), false);
					}
					catch (Exception e)
					{
						try
						{
                            Debug.LogWarning($"There was an error reading attribute info from {type.FullName}:{method.Name}() in assembly {assembly.GetName().Name}, see output.log for more details");
                            Debug.LogWarning(e);
						}
						catch (Exception)
						{
                            Debug.LogWarning("Error Reading Attribute info from unknown method");
                            Debug.LogWarning(e);
						}
					}
					if (attributes != null && attributes.GetLength(0) > 0)
					{
						methodsWithAttributes.Add(new ValueTuple<MethodInfo, AttriType>(method, (AttriType)attributes[0]));
                        //yield return new ValueTuple<MethodInfo, AttriType>(method, (AttriType)attributes[0]);
                    }
				}
				/*}*/
			}

			return methodsWithAttributes;
		}

		/// <summary>
		/// Finds all methods in an assembly that have a certain attribute type applied to them, and executes them
		/// </summary>
		/// <typeparam name="AttriType">The type attribute to look for</typeparam>
		/// <param name="assembly">The assembly to look for the methods under</param>
		/// <param name="ExecuteIf">If a delegate is specified here, then only the methods that satify this delegate will be executed</param>
		/// <param name="flags">The binding flags to determine what kind of methods to find</param>
		/// <param name="throwOnError">Should an exception be thrown if a method fails?</param>
		public static void ExecuteMethodsWithAttribute<AttriType>(Assembly assembly, Func<MethodInfo, AttriType, bool> ExecuteIf = null, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, bool throwOnError = false) where AttriType : Attribute
		{
			List<ValueTuple<MethodInfo, AttriType>> methods = new List<ValueTuple<MethodInfo, AttriType>>();

			methods.AddRange(GetMethodsWithAttribute<AttriType>(assembly, flags));

			if (methods.Count == 0)
			{
				return;
			}

			if (methods[0].Item2 is PriorityAttribute)
			{
				methods.Sort(new PriorityAttribute.MethodSorter<AttriType>());
			}

			while (methods.Count > 0)
			{
				var method = methods[0];
				methods.RemoveAt(0);
				try
				{
					if (ExecuteIf == null || ExecuteIf(method.Item1, method.Item2))
					{
						method.Item1.Invoke(null, null);
					}
				}
				catch (Exception e)
				{
					if (throwOnError)
					{
						throw;
					}
					else
					{
						try
						{
							WeaverLog.LogError("Error running function [" + method.Item1.DeclaringType.FullName + ":" + method.Item1.Name);
							UnityEngine.Debug.LogException(e);
						}
						catch (Exception)
						{
							Debug.LogWarning("Error running unknown method");
							Debug.LogException(e);
						}
					}
				}
			}
		}

        /// <summary>
        /// Finds all methods that have a certain attribute type applied to them, and executes them
        /// </summary>
        /// <typeparam name="AttriType">The type attribute to look for</typeparam>
		/// <param name="assemblies">The assemblies to check</param>
        /// <param name="ExecuteIf">If a delegate is specified here, then only the methods that satify this delegate will be executed</param>
        /// <param name="flags">The binding flags to determine what kind of methods to find</param>
        /// <param name="throwOnError">Should an exception be thrown if a method fails?</param>
        public static void ExecuteMethodsWithAttribute<AttriType>(IEnumerable<Assembly> assemblies, Func<MethodInfo, AttriType, bool> ExecuteIf = null, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, bool throwOnError = false) where AttriType : Attribute
		{
            Initialization.PerformanceLog($"Running methods with Attribute {typeof(AttriType)}");

            List<ValueTuple<MethodInfo, AttriType>> methods = new List<ValueTuple<MethodInfo, AttriType>>();

            foreach (var assembly in assemblies)
            {
                methods.AddRange(GetMethodsWithAttribute<AttriType>(assembly, flags));
                /*if (!Initialization.IsAssemblyExcluded(assembly))
                {
                    
                }*/
            }

            if (methods.Count == 0)
            {
                return;
            }
            if (typeof(PriorityAttribute).IsAssignableFrom(typeof(AttriType)))
            {
                methods.Sort(new PriorityAttribute.MethodSorter<AttriType>());
            }
            while (methods.Count > 0)
            {
                var method = methods[0];
                methods.RemoveAt(0);
                try
                {
                    if (ExecuteIf == null || ExecuteIf(method.Item1, method.Item2))
                    {
                        Initialization.PerformanceLog($"Running method {method.Item1.DeclaringType.Name}:{method.Item1.Name}");
                        method.Item1.Invoke(null, null);
                        Initialization.PerformanceLog($"Done Running method {method.Item1.DeclaringType.Name}:{method.Item1.Name}");
                    }
                }
                catch (Exception e)
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                    else
                    {
                        //WeaverLog.LogError("Error running function [" + method.Item1.DeclaringType.FullName + ":" + method.Item1.Name + "\n" + e);
                        try
                        {
                            WeaverLog.LogError("Error running function [" + method.Item1.DeclaringType.FullName + ":" + method.Item1.Name);
                            UnityEngine.Debug.LogException(e);
                        }
                        catch (Exception)
                        {
                            Debug.LogWarning("Error running unknown method");
                            Debug.LogException(e);
                        }
                    }
                }
            }

            Initialization.PerformanceLog($"Finished running methods with Attribute {typeof(AttriType)}");
        }

        /// <summary>
        /// Finds all methods that have a certain attribute type applied to them, and executes them
        /// </summary>
        /// <typeparam name="AttriType">The type attribute to look for</typeparam>
        /// <param name="ExecuteIf">If a delegate is specified here, then only the methods that satify this delegate will be executed</param>
        /// <param name="flags">The binding flags to determine what kind of methods to find</param>
        /// <param name="throwOnError">Should an exception be thrown if a method fails?</param>
        public static void ExecuteMethodsWithAttribute<AttriType>(Func<MethodInfo, AttriType, bool> ExecuteIf = null, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, bool throwOnError = false) where AttriType : Attribute
		{
            ExecuteMethodsWithAttribute(AppDomain.CurrentDomain.GetAssemblies(), ExecuteIf, flags, throwOnError);
        }

        /// <summary>
        /// Finds a loaded assembly by its full name or simple name.
        /// </summary>
        /// <param name="assemblyName">The full name or simple name of the assembly.</param>
        /// <returns>The loaded assembly or null if not found.</returns>
        public static Assembly FindLoadedAssembly(string assemblyName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName == assemblyName || assembly.GetName().Name == assemblyName)
                {
                    return assembly;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the value of a field using reflection.
        /// </summary>
        /// <param name="obj">The object containing the field.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="flags">Binding flags for reflection (default is Static, NonPublic, Public, Instance).</param>
        /// <returns>The value of the specified field.</returns>
        public static object ReflectGetField(this object obj, string fieldName, BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            return obj.GetType().GetField(fieldName, flags).GetValue(obj);
        }

        /// <summary>
        /// Sets the value of a field using reflection.
        /// </summary>
        /// <param name="obj">The object containing the field.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="flags">Binding flags for reflection (default is Static, NonPublic, Public, Instance).</param>
        public static void ReflectSetField(this object obj, string fieldName, object value, BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            obj.GetType().GetField(fieldName, flags).SetValue(obj, value);
        }

        /// <summary>
        /// Gets the value of a property using reflection.
        /// </summary>
        /// <param name="obj">The object containing the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="flags">Binding flags for reflection (default is Static, NonPublic, Public, Instance).</param>
        /// <returns>The value of the specified property.</returns>
        public static object ReflectGetProperty(this object obj, string propertyName, BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            return obj.GetType().GetProperty(propertyName, flags).GetValue(obj);
        }

        /// <summary>
        /// Sets the value of a property using reflection.
        /// </summary>
        /// <param name="obj">The object containing the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="flags">Binding flags for reflection (default is Static, NonPublic, Public, Instance).</param>
        /// <returns>True if the property was successfully set; otherwise, false.</returns>
        public static bool ReflectSetProperty(this object obj, string propertyName, object value, BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            var property = obj.GetType().GetProperty(propertyName, flags);
            property?.SetValue(obj, value);
            return property != null;
        }

        /// <summary>
        /// Calls a method using reflection.
        /// </summary>
        /// <param name="obj">The object containing the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="parameters">An array of parameters to pass to the method.</param>
        /// <param name="flags">Binding flags for reflection (default is Static, NonPublic, Public, Instance).</param>
        /// <returns>The result of the method invocation.</returns>
        public static object ReflectCallMethod(this object obj, string methodName, object[] parameters = null, BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            return obj.GetType().GetMethod(methodName, flags).Invoke(obj, parameters);
        }

        /// <summary>
        /// Gets the MethodInfo object for a method using reflection.
        /// </summary>
        /// <param name="obj">The object containing the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="flags">Binding flags for reflection (default is Static, NonPublic, Public, Instance).</param>
        /// <returns>The MethodInfo object for the specified method.</returns>
        public static MethodInfo ReflectGetMethod(this object obj, string methodName, BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            return obj.GetType().GetMethod(methodName, flags);
        }
    }
}
