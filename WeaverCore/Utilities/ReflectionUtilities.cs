using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Utilities
{
	/*[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public abstract class FunctionHookAttribute : Attribute
	{

	}*/



	public static class ReflectionUtilities
	{
		public static IEnumerable<Type> GetChildrenOfType(Type parentType, bool includeAbstract = false)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (CoreInfo.LoadState == Enums.RunningState.Game && assembly.GetName().Name == "Assembly-CSharp")
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

		public static IEnumerable<Type> GetChildrenOfType<T>(bool includeAbstract = false)
		{
			return GetChildrenOfType(typeof(T), includeAbstract);
		}

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

		public static Func<SourceType, FieldType> CreateFieldGetter<SourceType, FieldType>(FieldInfo field)
		{
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
		}

		public static Func<SourceType, FieldType> CreateFieldGetter<SourceType, FieldType>(string fieldName, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		{
			return CreateFieldGetter<SourceType, FieldType>(typeof(SourceType).GetField(fieldName, flags));
		}

		public static Action<SourceType, FieldType> CreateFieldSetter<SourceType, FieldType>(FieldInfo field)
		{
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
		}

		public static Action<SourceType, FieldType> CreateFieldSetter<SourceType, FieldType>(string fieldName, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		{
			return CreateFieldSetter<SourceType, FieldType>(typeof(SourceType).GetField(fieldName, flags));
		}

		/// <summary>
		/// Converts a methodInfo object into a callable delegate
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
		/// Converts a methodInfo object into a callable delegate
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
		/// Converts a methodInfo object into a callable delegate
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
		/// Converts a methodInfo object into a callable delegate
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

		public static MethodInfo GetMethod<InstanceType>(string methodName, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
		{
			return typeof(InstanceType).GetMethod(methodName, flags);
		}

		public static IEnumerable<ValueTuple<MethodInfo, T>> GetMethodsWithAttribute<T>(BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) where T : Attribute
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var method in GetMethodsWithAttribute<T>(assembly, flags))
				{
					yield return method;
				}
			}
		}

		public static IEnumerable<ValueTuple<MethodInfo, T>> GetMethodsWithAttribute<T>(Assembly assembly, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) where T : Attribute
		{
			return GetMethodsWithAttribute<T>(assembly, null, flags);
			/*foreach (var type in assembly.GetTypes())
			{
				foreach (var method in type.GetMethods(flags))
				{
					var attributes = method.GetCustomAttributes(typeof(T), false);
					if (attributes.GetLength(0) > 0)
					{
						yield return new ValueTuple<MethodInfo, T>(method,(T)attributes[0]);
					}
				}
			}*/
		}


		public static IEnumerable<ValueTuple<MethodInfo, T>> GetMethodsWithAttribute<T>(Type[] paramTypes, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) where T : Attribute
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var method in GetMethodsWithAttribute<T>(assembly,paramTypes,flags))
				{
					yield return method;
				}
			}
		}

		public static IEnumerable<ValueTuple<MethodInfo, T>> GetMethodsWithAttribute<T>(Assembly assembly, Type[] paramTypes, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) where T : Attribute
		{
			if (assembly == null)
			{
				yield break;
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
				yield break;
			}

			foreach (var type in types)
			{
				foreach (var method in type.GetMethods(flags))
				{
					if (paramTypes != null)
					{
						var parameters = method.GetParameters();
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
					var attributes = method.GetCustomAttributes(typeof(T), false);
					if (attributes.GetLength(0) > 0)
					{
						yield return new ValueTuple<MethodInfo, T>(method, (T)attributes[0]);
					}
					/*if (method.GetCustomAttributes(typeof(T), false).GetLength(0) > 0)
					{
						yield return method;
					}*/
				}
			}
		}

		public static void ExecuteMethodsWithAttribute<T>(Assembly assembly, Func<MethodInfo, T,bool> ExecuteIf = null, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, bool throwOnError = false) where T : Attribute
		{
			List<ValueTuple<MethodInfo, T>> methods = new List<ValueTuple<MethodInfo, T>>();

			methods.AddRange(GetMethodsWithAttribute<T>(assembly, flags));

			if (methods.Count == 0)
			{
				return;
			}

			if (methods[0].Item2 is PriorityAttribute)
			{
				methods.Sort(new PriorityAttribute.PairSorter<T>());
			}

			while (methods.Count > 0)
			{
				var method = methods[0];
				methods.RemoveAt(0);
				try
				{
					if (ExecuteIf == null || ExecuteIf(method.Item1,method.Item2))
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
						WeaverLog.LogError("Error running function [" + method.Item1.DeclaringType.FullName + ":" + method.Item1.Name + "\n" + e);
					}
				}
			}
		}

		public static void ExecuteMethodsWithAttribute<T>(Func<MethodInfo, T, bool> ExecuteIf = null, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, bool throwOnError = false) where T : Attribute
		{
			List<ValueTuple<MethodInfo, T>> methods = new List<ValueTuple<MethodInfo, T>>();

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				methods.AddRange(GetMethodsWithAttribute<T>(assembly, flags));
			}

			if (methods.Count == 0)
			{
				return;
			}

			bool sortable = false;

			if (methods[0].Item2 is PriorityAttribute)
			{
				sortable = true;
				methods.Sort(new PriorityAttribute.PairSorter<T>());
			}

			AssemblyLoadEventHandler NewAssemblyLoad = (s, args) =>
			{
				methods.AddRange(GetMethodsWithAttribute<T>(args.LoadedAssembly, flags));
				if (sortable)
				{
					methods.Sort(new PriorityAttribute.PairSorter<T>());
				}
			};
			try
			{
				AppDomain.CurrentDomain.AssemblyLoad += NewAssemblyLoad;

				while (methods.Count > 0)
				{
					var method = methods[0];
					methods.RemoveAt(0);
					try
					{
						if (ExecuteIf == null || ExecuteIf(method.Item1,method.Item2))
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
							WeaverLog.LogError("Error running function [" + method.Item1.DeclaringType.FullName + ":" + method.Item1.Name + "\n" + e);
						}
					}
				}
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyLoad -= NewAssemblyLoad;
			}
		}

		/// <summary>
		/// Returns all the assemblies in the domain, similar to <see cref="AppDomain.GetAssemblies"/>
		/// 
		/// However, the problem with that function is that if there are any assemblies that get added afterwards, they won't appear in the array of assemblies returned
		/// 
		/// This function fixes that issue. If any new assemblies get loaded while using this IEnumerable, they will be automatically included
		/// 
		/// This function guarantees that all assemblies are enumerated over
		/// </summary>
		/// <param name="domain"></param>
		/// <returns></returns>
		public static IEnumerable<Assembly> AllAssemblies(this AppDomain domain)
		{
			return new AssemblyEnumerable(domain);
		}
	}


	sealed class AssemblyEnumerable : IEnumerable<Assembly>
	{
		AppDomain Domain;

		public AssemblyEnumerable(AppDomain domain)
		{
			Domain = domain;
		}

		IEnumerator<Assembly> IEnumerable<Assembly>.GetEnumerator()
		{
			return new AssemblyEnumerator(Domain);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new AssemblyEnumerator(Domain);
		}
	}

	sealed class AssemblyEnumerator : IEnumerator<Assembly>
	{
		AppDomain Domain;
		Queue<Assembly> Assemblies;
		bool disposedValue = false;

		Assembly current = null;

		public AssemblyEnumerator(AppDomain domain)
		{
			Domain = domain;
			Assemblies = new Queue<Assembly>(Domain.GetAssemblies());
			Domain.AssemblyLoad += OnNewAssemblyLoad;
		}

		private void OnNewAssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			Assemblies.Enqueue(args.LoadedAssembly);
		}

		Assembly IEnumerator<Assembly>.Current
		{
			get
			{
				return current;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				return current;
			}
		}

		bool IEnumerator.MoveNext()
		{
			if (Assemblies.Count == 0)
			{
				return false;
			}
			else
			{
				current = Assemblies.Dequeue();
				return true;
			}
		}

		void IEnumerator.Reset()
		{
			//Not possible to reset
		}

		void Dispose()
		{
			if (!disposedValue)
			{
				disposedValue = true;

				Domain.AssemblyLoad -= OnNewAssemblyLoad;
			}
		}

		~AssemblyEnumerator()
		{
			Dispose();
		}

		void IDisposable.Dispose()
		{
			Dispose();
			GC.SuppressFinalize(this);
		}


	}
}
