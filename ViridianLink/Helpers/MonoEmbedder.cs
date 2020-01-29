using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;


namespace ViridianLink.Helpers
{
	/*public static class MonoEmbedder
	{
		static Func<Stream, IDisposable> ReadAssembly;

		static MonoEmbedder()
		{
			CosturaUtility.Initialize();
			var eventInfo = typeof(AppDomain).GetEvent("AssemblyResolve", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
			var fieldInfo = typeof(AppDomain).GetField(eventInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			var dl = fieldInfo.GetValue(AppDomain.CurrentDomain) as Delegate;

			var InvoList = dl.GetInvocationList();

			for (int i = InvoList.Length - 1; i >= 0; i--)
			{
				var invo = InvoList[i];
				Debugger.Log("Invo = " + invo.Method.Name);
				Debugger.Log("Declaring Type = " + invo.Method.DeclaringType);
				Debugger.Log("Assembly = " + invo.Method.Module.Assembly.FullName);
			}
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Debugger.Log($"Assembly = {assembly.FullName}, Version = {assembly.GetName().Version}");
			}
			var name = new AssemblyName("Mono.Cecil, Version=0.10.4.0, Culture=neutral, PublicKeyToken=50cebf1cceb9d05e");
			var cecil = System.Reflection.Assembly.Load(name);
			Debugger.Log("Version = " + cecil.GetName().Version);
			var AssemblyDefinitionT = cecil.GetType("Mono.Cecil.AssemblyDefinition");
			Debugger.Log(nameof(AssemblyDefinitionT) + " = " + AssemblyDefinitionT);
			Debugger.Log("Method Count = " + AssemblyDefinitionT.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance).Length);
			foreach (var method in AssemblyDefinitionT.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				Debugger.Log("Method = " + method.Name);
			}
			Debugger.Log("Method = " + AssemblyDefinitionT.GetMethod("ReadAssembly", new Type[] { typeof(Stream) }));
			Debugger.Log("Method 2 = " + AssemblyDefinitionT.GetMethod("ReadAssembly", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
			ReadAssembly = Methods.GetFunction<Func<Stream, IDisposable>>(AssemblyDefinitionT.GetMethod("ReadAssembly", new Type[] { typeof(Stream) }));
		}


		public static void Embed(string sourceFile,string additionResource,string resourceName)
		{
			using (FileStream sourceStream = new FileStream(sourceFile,FileMode.Open,FileAccess.ReadWrite))
			{
				using (FileStream resourceStream = new FileStream(additionResource,FileMode.Open,FileAccess.Read))
				{
					Embed(sourceStream, resourceStream, resourceName);
				}
			}
		}

		public static void Embed(Stream source,Stream addition,string resourceName)
		{

		}
	}*/
}
