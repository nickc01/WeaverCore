using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace AssemblyManipulator
{
	public sealed class TypeReplacer : IDisposable
	{
		bool disposed = false;

		List<AssemblyDefinition> Assemblies = new List<AssemblyDefinition>();
		List<AssemblyDefinition> ForeignAssemblies = new List<AssemblyDefinition>();

		AssemblyDefinition SourceAssembly;
		ModuleDefinition SourceModule;

		List<TypeDefinition> TypesToReplace = new List<TypeDefinition>();

		public TypeReplacer(string sourceAssemblyPath, IEnumerable<string> otherAssemblies)
		{
			SourceAssembly = AssemblyDefinition.ReadAssembly(sourceAssemblyPath, new ReaderParameters() { ReadWrite = true, AssemblyResolver = new MainResolver(Assemblies) });
			SourceModule = SourceAssembly.MainModule;
			Assemblies.Add(SourceAssembly);
			foreach (var assembly in otherAssemblies)
			{
				var otherAssembly = AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters() { AssemblyResolver = new MainResolver(Assemblies) });
				Assemblies.Add(otherAssembly);
				ForeignAssemblies.Add(otherAssembly);
			}
		}

		TypeReference GetTypeReference(TypeReference reference)
		{
			if (reference.Module.Assembly.FullName == SourceAssembly.FullName)
			{
				return reference.Resolve();
			}
			else
			{
				return SourceModule.ImportReference(reference);
			}
		}

		TypeReference GetTypeElseWhere(TypeReference type)
		{
			TypeReference foreignT = null;

			foreach (var assembly in ForeignAssemblies)
			{
				//var t = assembly.MainModule.GetType(typeName);
				var t = FindTypeInAssembly(assembly.MainModule, type);
				if (t != null)
				{
					foreignT = GetTypeReference(t);
					break;
				}
			}
			return foreignT;
		}

		bool GetTypeElseWhere(TypeDefinition t, out TypeReference result)
		{
			result = GetTypeElseWhere(t);
			return result != null;
		}

		public void WriteChanges()
		{
			//SourceAssembly.Write();
			SourceAssembly.MainModule.Write();
		}

		TypeDefinition FindTypeInAssembly(ModuleDefinition module, TypeReference type)
		{
			/*type.isGe
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				type = type.GetGenericTypeDefinition();
			}*/
			return module.Types.FirstOrDefault(t => t.FullName == type.FullName/* && t.HasGenericParameters == type.ContainsGenericParameters*/);
		}

		int FindTypeIndexInAssembly(ModuleDefinition module, TypeDefinition type)
		{
			for (int i = 0; i < module.Types.Count; i++)
			{
				if (module.Types[i].FullName == type.FullName)
				{
					return i;
				}
			}
			return -1;
		}

		TypeSpecification ChangeTypeSpec(TypeSpecification source, TypeReference newType)
		{
			if (source is ArrayType at)
			{
				return new ArrayType(ReplaceTypeWith(at.ElementType,newType), at.Rank);
			}
			else if (source is ByReferenceType brt)
			{
				return new ByReferenceType(ReplaceTypeWith(brt.ElementType, newType));
			}
			else if (source is FunctionPointerType fpt)
			{
				var newFPT = new FunctionPointerType();
				newFPT.HasThis = fpt.HasThis;
				newFPT.ExplicitThis = fpt.ExplicitThis;
				newFPT.ReturnType = ReplaceTypeWith(fpt.ReturnType, newType);
				return newFPT;
			}
			else if (source is GenericInstanceType git)
			{
				var newGit = new GenericInstanceType(ReplaceTypeWith(git.ElementType, newType));
				for (int i = 0; i < git.GenericArguments.Count; i++)
				{
					newGit.GenericArguments.Add(ReplaceTypeWith(git.GenericArguments[i],newType));
				}
				return newGit;
			}
			else if (source is OptionalModifierType omt)
			{
				return new OptionalModifierType(ReplaceTypeWith(omt.ModifierType, newType),ReplaceTypeWith(omt.ElementType,newType));
			}
			else if (source is PinnedType pt)
			{
				return new PinnedType(ReplaceTypeWith(pt.ElementType, newType));
			}
			else if (source is PointerType ptr)
			{
				return new PointerType(ReplaceTypeWith(ptr.ElementType, newType));
			}
			else if (source is RequiredModifierType rmt)
			{
				return new RequiredModifierType(ReplaceTypeWith(rmt.ModifierType, newType), ReplaceTypeWith(rmt.ElementType, newType));
			}
			else if (source is SentinelType st)
			{
				return new SentinelType(ReplaceTypeWith(st.ElementType, newType));
			}
			else
			{
				return source;
			}
		}

		TypeReference ReplaceTypeWith(TypeReference source, TypeReference dest)
		{
			if (source == null)
			{
				return null;
			}
			if (dest == null)
			{
				return source;
			}

			if (source is TypeSpecification typeSpec)
			{
				//typeSpec.ElementType = ReplaceTypeWith(typeSpec.ElementType,)
				return SourceModule.ImportReference(ChangeTypeSpec(typeSpec, dest));
			}

			/*if (source is GenericParameter gp)
			{
				var newGP = new GenericParameter(gp.Name, dest);
			}*/

			if (!(source is GenericParameter) && source.DeclaringType != null)
			{
				source.DeclaringType = ReplaceTypeWith(source.DeclaringType, dest);
			}
			//var sourceDef = source.Resolve();
			//var destDef = dest.Resolve();

			if (source.IsGenericParameter)
			{
				return source;
			}
			else if (source is GenericInstanceType git)
			{
				if (dest.FullName == git.ElementType.FullName)
				{
					var foundDest = GetTypeElseWhere(dest);
					if (foundDest == null)
					{
						throw new Exception("The type " + dest.FullName + " could not be found in any other assemblies");
					}
					Console.WriteLine("Importing 1 = " + foundDest);
					var replacement = new GenericInstanceType(SourceModule.ImportReference(foundDest));
					foreach (var argument in git.GenericArguments)
					{
						replacement.GenericArguments.Add(ReplaceTypeWith(argument, dest));
					}
					return SourceModule.ImportReference(replacement);
				}
				else
				{
					for (int i = 0; i < git.GenericArguments.Count; i++)
					{
						git.GenericArguments[i] = ReplaceTypeWith(git.GenericArguments[i], dest);
					}
					return SourceModule.ImportReference(git);
				}
			}
			else if (source.FullName == dest.FullName)
			{
				var foundDest = GetTypeElseWhere(dest);
				if (foundDest == null)
				{
					throw new Exception("The type " + dest.FullName + " could not be found in any other assemblies");
				}
				Console.WriteLine("Importing 2 = " + foundDest);
				return SourceModule.ImportReference(foundDest);
			}
			else
			{
				return source;
			}
		}

		public void ReplaceTypes(IEnumerable<TypeDefinition> types)
		{
			if (types is List<TypeDefinition> list)
			{
				TypesToReplace = list;
			}
			else
			{
				TypesToReplace = types.ToList();
			}
			/*Console.WriteLine("TYPES TO REPLACE {{{");
			foreach (var t in TypesToReplace)
			{
				Console.WriteLine(t.FullName);
			}
			Console.WriteLine("}}}}}}}}}}}}}}}}");
			Console.WriteLine("TYPES BEFORE {{{");
			foreach (var t in SourceModule.Types)
			{
				Console.WriteLine(t.FullName);
			}
			Console.WriteLine("}}}}}}}}}}}}}}}}");*/

			foreach (var type in types)
			{
				ReplaceType(type);
			}

			/*Console.WriteLine("TYPES AFTER {{{");
			foreach (var t in SourceModule.Types)
			{
				Console.WriteLine(t.FullName);
			}
			Console.WriteLine("}}}}}}}}}}}}}}}}");*/
		}

		void ReplaceType(TypeDefinition t)
		{
			if (t.FullName == "<Module>" || t.FullName == "<PrivateImplementationDetails>")
			{
				return;
			}
			var sourceTypeIndex = FindTypeIndexInAssembly(SourceModule, t);
			if (sourceTypeIndex < 0)
			{
				throw new Exception("The type " + t + " does not exist in the assembly " + SourceAssembly.FullName);
			}

			var sourceType = GetTypeReference(SourceModule.Types[sourceTypeIndex]);

			TypeReference replacementType = null;

			foreach (var type in SourceModule.Types)
			{
				if (TypesToReplace.Any(ttr => ttr.FullName == type.FullName))
				{
					continue;
				}

				//Console.WriteLine($"{type.FullName} matches any = " + TypesToReplace.Any(ttr => ttr.FullName == type.FullName));

				if (replacementType == null)
				{
					
					if (!GetTypeElseWhere(t, out replacementType))
					{
						//new AssemblyNameReference("Assembly-CSharp", new Version("0.0.0.0"))
						replacementType = new TypeReference(t.Namespace, t.Name, ForeignAssemblies[0].MainModule, new AssemblyNameReference("Assembly-CSharp", new Version("0.0.0.0")));
					}
				}

				PatchType(sourceType, replacementType, type);

				if (type.HasNestedTypes)
				{
					foreach (var nestedType in type.NestedTypes)
					{
						PatchType(nestedType, replacementType, type);
					}
				}
			}
			//throw new Exception("Type Removal = " + sourceTypeDefinition.FullName);
			Console.WriteLine("Removing Type = " + sourceType.FullName);
			SourceModule.Types.RemoveAt(sourceTypeIndex);
		}

		private void PatchType(TypeReference sourceType, TypeReference replacementType, TypeDefinition type)
		{
			type.BaseType = ReplaceTypeWith(type.BaseType, replacementType);


			if (type.HasGenericParameters)
			{
				foreach (var gParam in type.GenericParameters)
				{
					for (int i = 0; i < gParam.Constraints.Count; i++)
					{
						gParam.Constraints[i] = ReplaceTypeWith(gParam.Constraints[i], replacementType);
					}
				}
			}

			if (type.HasInterfaces)
			{
				for (int i = 0; i < type.Interfaces.Count; i++)
				{
					var inter = type.Interfaces[i];
					type.Interfaces.RemoveAt(i);
					type.Interfaces.Insert(i, new InterfaceImplementation(ReplaceTypeWith(inter.InterfaceType, replacementType)));
				}
			}

			if (type.HasFields)
			{
				foreach (var field in type.Fields)
				{
					field.FieldType = ReplaceTypeWith(field.FieldType, replacementType);
				}
			}

			if (type.HasProperties)
			{
				foreach (var prop in type.Properties)
				{
					prop.PropertyType = ReplaceTypeWith(prop.PropertyType, replacementType);
					if (prop.GetMethod != null)
					{
						PatchOutMethod(prop.GetMethod, sourceType, replacementType);
					}
					if (prop.SetMethod != null)
					{
						PatchOutMethod(prop.SetMethod, sourceType, replacementType);
					}
					if (prop.HasOtherMethods)
					{
						foreach (var otherMethod in prop.OtherMethods)
						{
							PatchOutMethod(otherMethod, sourceType, replacementType);
						}
					}
				}
			}

			if (type.HasMethods)
			{
				foreach (var method in type.Methods)
				{
					PatchOutMethod(method, sourceType, replacementType);
				}
			}
		}

		void PatchOutMethod(MethodDefinition method, TypeReference sourceType, TypeReference replacementType)
		{
			/*if (method.ReturnType == sourceType)
			{
				method.ReturnType = replacementType;
			}*/

			if (method.HasGenericParameters)
			{
				foreach (var gParam in method.GenericParameters)
				{
					if (gParam.HasConstraints)
					{
						for (int i = 0; i < gParam.Constraints.Count; i++)
						{
							gParam.Constraints[i] = ReplaceTypeWith(gParam.Constraints[i], replacementType);
						}
					}
				}
			}

			method.ReturnType = ReplaceTypeWith(method.ReturnType, replacementType);

			if (method.HasParameters)
			{
				foreach (var parameter in method.Parameters)
				{
					parameter.ParameterType = ReplaceTypeWith(parameter.ParameterType, replacementType);
					/*if (parameter.ParameterType == sourceType)
					{
						parameter.ParameterType = replacementType;
					}*/
				}
			}

			if (method.HasBody)
			{
				if (method.Body.HasVariables)
				{
					if (method.Body.ThisParameter != null)
					{
						method.Body.ThisParameter.ParameterType = ReplaceTypeWith(method.Body.ThisParameter.ParameterType, replacementType);
					}
					foreach (var v in method.Body.Variables)
					{
						v.VariableType = ReplaceTypeWith(v.VariableType, replacementType);
						/*if (v.VariableType == sourceType)
						{
							v.VariableType = replacementType;
						}*/
					}
				}
				//foreach (var instruction in method.Body.Instructions)
				for (int i = 0; i < method.Body.Instructions.Count; i++)
				{
					method.Body.Instructions[i] = PatchInstruction(replacementType, method.Body.Instructions[i]);
				}
			}
		}

		private Instruction PatchInstruction(TypeReference replacementType, Instruction instruction)
		{
			var operand = instruction.Operand;

			if (operand is TypeReference type)
			{
				//type.DeclaringType = ReplaceTypeWith(type.DeclaringType, replacementType);
				instruction.Operand = ReplaceTypeWith(type, replacementType);
				/*if (type == sourceType)
				{
					instruction.Operand = replacementType;
				}*/
			}
			else if (operand is MethodReference methodRef)
			{
				//if (methodRef.DeclaringType == sourceType)
				//{

				if (methodRef is GenericInstanceMethod gim)
				{
					var method = gim.ElementMethod;
					method.ReturnType = ReplaceTypeWith(method.ReturnType, replacementType);
					method.DeclaringType = ReplaceTypeWith(method.DeclaringType, replacementType);
					var newGim = new GenericInstanceMethod(method);
					for (int i = 0; i < gim.GenericArguments.Count; i++)
					{
						newGim.GenericArguments.Add(ReplaceTypeWith(gim.GenericArguments[i], replacementType));
					}
					methodRef = SourceModule.ImportReference(newGim);
				}
				else
				{
					//var newMethod = new MethodReference(methodRef.Name,)
					var newMethod = new MethodReference(methodRef.Name, ReplaceTypeWith(methodRef.DeclaringType, replacementType), ReplaceTypeWith(methodRef.ReturnType, replacementType));
					newMethod.CallingConvention = methodRef.CallingConvention;
					newMethod.ExplicitThis = methodRef.ExplicitThis;
					if (methodRef.HasGenericParameters)
					{
						//IGenericParameterProvider genericProvider = null;

						foreach (var genericParam in methodRef.GenericParameters)
						{
							newMethod.GenericParameters.Add(new GenericParameter(genericParam.Name, genericParam.Owner));
						}
					}
					newMethod.HasThis = methodRef.HasThis;
					//methodRef.DeclaringType = ReplaceTypeWith(methodRef.DeclaringType, replacementType);
					//methodRef.ReturnType = ReplaceTypeWith(methodRef.ReturnType, replacementType);
					if (methodRef.HasParameters)
					{
						for (int i = 0; i < methodRef.Parameters.Count; i++)
						{
							var sourceP = methodRef.Parameters[i];
							var p = new ParameterDefinition(sourceP.Name, sourceP.Attributes, ReplaceTypeWith(sourceP.ParameterType, replacementType));
							newMethod.Parameters.Add(p);

							//newMethod.Parameters[i].ParameterType = ReplaceTypeWith(methodRef.Parameters[i].ParameterType, replacementType);
						}
					}
					methodRef = SourceModule.ImportReference(newMethod);
				}

				if (methodRef == null)
				{
					Console.WriteLine("WARNING : NULL METHOD!");
				}

				instruction.Operand = methodRef;

				//TODO TODO TODO
				//var newMethodRef = new MethodReference(methodRef.Name, methodRef.ReturnType, replacementType);
				//}
			}
			else if (operand is Instruction inst)
			{
				instruction.Operand = PatchInstruction(replacementType,inst);
			}
			else if (operand is Instruction[] insts)
			{
				List<Instruction> PatchedInsts = new List<Instruction>();
				foreach (var i in insts)
				{
					PatchedInsts.Add(PatchInstruction(replacementType, i));
				}
				instruction.Operand = PatchedInsts.ToArray();
			}
			else if (operand is CallSite cs)
			{
				cs.ReturnType = ReplaceTypeWith(cs.ReturnType, replacementType);
				cs.MethodReturnType.ReturnType = ReplaceTypeWith(cs.MethodReturnType.ReturnType, replacementType);
				if (cs.HasParameters)
				{
					foreach (var param in cs.Parameters)
					{
						param.ParameterType = ReplaceTypeWith(param.ParameterType, replacementType);
					}
				}
				instruction.Operand = cs;
			}
			else if (operand is FieldReference fieldRef)
			{
				fieldRef.DeclaringType = ReplaceTypeWith(fieldRef.DeclaringType, replacementType);
				fieldRef.FieldType = ReplaceTypeWith(fieldRef.FieldType, replacementType);

				instruction.Operand = fieldRef;
			}

			return instruction;
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				//Dispose Stuff Here

				foreach (var assembly in Assemblies)
				{
					assembly.Dispose();
				}

				GC.SuppressFinalize(this);
			}
		}

		~TypeReplacer()
		{
			Dispose();
		}
	}
}
