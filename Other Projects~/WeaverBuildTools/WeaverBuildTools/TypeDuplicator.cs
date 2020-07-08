using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverBuildTools
{
	public class TypeDuplicator
	{
		TypeDefinition Source;
		AssemblyDefinition NewAssembly;

		Dictionary<TypeReference, TypeReference> TypeReplacements = new Dictionary<TypeReference, TypeReference>();
		Dictionary<string, string> StringReplacements = new Dictionary<string, string>();

		public TypeDuplicator(TypeDefinition source,AssemblyDefinition newAssembly)
		{
			Source = source;
			NewAssembly = newAssembly;
			Console.WriteLine("Source = " + Source);
			Console.WriteLine("New Assembly = " + NewAssembly);
		}

		TypeReference GetType(TypeReference reference)
		{
			if (reference.Module.Assembly.FullName == NewAssembly.FullName)
			{
				return reference.Resolve();
			}
			else
			{
				return NewAssembly.MainModule.ImportReference(reference);
			}
		}

		TypeReference GetType(Type type)
		{
			return NewAssembly.MainModule.ImportReference(type);
		}

		public void AddTypeReplacement(TypeReference old, TypeReference type)
		{
			TypeReplacements.Add(old, type);
		}

		public void AddStringReplacement(string oldStirng,string newString)
		{
			StringReplacements.Add(oldStirng, newString);
		}

		TypeReference GetTypeReplacement(TypeReference old)
		{
			foreach (var replacements in TypeReplacements)
			{
				if (replacements.Key.FullName == old.FullName)
				{
					return replacements.Value;
				}
			}
			return null;
		}

		public TypeDefinition Create(string nameSpace, string name)
		{
			Console.WriteLine("A");
			Console.WriteLine("Namespace = " + nameSpace);
			Console.WriteLine("Name = " + name);
			var newType = new TypeDefinition(nameSpace, name, Source.Attributes, GetType(Source.BaseType));
			Console.WriteLine("B");
			if (Source.HasInterfaces)
			{
				foreach (var inter in Source.Interfaces)
				{
					newType.Interfaces.Add(new InterfaceImplementation(GetType(inter.InterfaceType)));
				}
			}
			Console.WriteLine("C");
			if (Source.HasFields)
			{
				foreach (var field in Source.Fields)
				{
					var newField = new FieldDefinition(field.Name, field.Attributes, GetType(field.FieldType));
					var replacement = GetTypeReplacement(newField.FieldType);
					if (replacement != null)
					{
						newField.FieldType = replacement;
					}
					/*if (TypeReplacements.ContainsKey(newField.Name))
					{
						newField.FieldType = GetType(TypeReplacements[newField.Name]);
					}*/

					newType.Fields.Add(newField);
				}
			}
			Console.WriteLine("D");
			if (Source.HasMethods)
			{
				foreach (var method in Source.Methods)
				{
					Console.WriteLine("E = " + method);
					var newMethod = new MethodDefinition(method.Name, method.Attributes, GetType(method.ReturnType));
					newMethod.Body.InitLocals = method.Body.InitLocals;
					//newMethod.Body.ThisParameter = new ParameterDefinition(method.Body.ThisParameter.Name, method.Body.ThisParameter.Attributes, GetType(method.Body.ThisParameter.ParameterType));

					Console.WriteLine("F = " + newMethod);
					if (method.Body.HasVariables)
					{
						foreach (var variable in method.Body.Variables)
						{
							Console.Write("G");
							var newVariable = new VariableDefinition(GetType(variable.VariableType));

							newMethod.Body.Variables.Add(newVariable);
						}
					}
					Console.Write("H");
					foreach (var instruction in method.Body.Instructions)
					{
						Console.Write("I");
						var operand = instruction.Operand;
						if (operand != null)
						{
							if (operand is TypeReference type)
							{
								Console.Write("J1");
								var replacement = GetTypeReplacement(type);
								if (replacement != null)
								{
									type = replacement;
								}
								newMethod.Body.Instructions.Add(Instruction.Create(instruction.OpCode,GetType(type)));
							}
							else if (operand is MethodReference methodRef)
							{
								Console.WriteLine("A Method = " + methodRef);
								Console.WriteLine("A Declaring Type = " + methodRef.DeclaringType);
								var replacement = GetTypeReplacement(methodRef.DeclaringType);
								if (replacement != null)
								{
									var oldRef = methodRef;
									//methodRef.DeclaringType = replacement;
									methodRef = new MethodReference(methodRef.Name, methodRef.ReturnType, replacement);
									//methodRef.CallingConvention = oldRef.CallingConvention;
									//methodRef.ExplicitThis = oldRef.ExplicitThis;
									methodRef.HasThis = oldRef.HasThis;
									//methodRef.MetadataToken = oldRef.MetadataToken;
								}
								Console.WriteLine("Method = " + methodRef);
								Console.WriteLine("Declaring Type = " + methodRef.DeclaringType);
								Console.Write("J2");
								var import = NewAssembly.MainModule.ImportReference(methodRef);
								//var resolved = import.Resolve();
								//Console.WriteLine("Resolved = " + resolved);
								Console.WriteLine("Import = " + import);
								newMethod.Body.Instructions.Add(Instruction.Create(instruction.OpCode,import));
							}
							else if (operand is string str)
							{
								Console.Write("J3");
								if (StringReplacements.ContainsKey(str))
								{
									str = StringReplacements[str];
								}
								newMethod.Body.Instructions.Add(Instruction.Create(instruction.OpCode,str));
							}
							else if (operand is FieldReference fieldRef)
							{
								newMethod.Body.Instructions.Add(Instruction.Create(instruction.OpCode,newType.Fields.First(f => f.Name == fieldRef.Name)));
							}
							else
							{
								Console.Write("J4");
								var newInstruction = Instruction.Create(OpCodes.Nop);
								newInstruction.OpCode = instruction.OpCode;
								newInstruction.Operand = instruction.Operand;
								newMethod.Body.Instructions.Add(newInstruction);
							}
						}
						else
						{
							Console.Write("J5");
							newMethod.Body.Instructions.Add(Instruction.Create(instruction.OpCode));
						}
					}
					newType.Methods.Add(newMethod);
				}
			}
			Console.WriteLine("Y");
			NewAssembly.MainModule.Types.Add(newType);
			Console.WriteLine("Z");
			return newType;
		}
	}
}
