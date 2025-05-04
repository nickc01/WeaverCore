using UnityEngine;

namespace WeaverCore.Playmaker.Snapshots
{
    /// <summary>
    /// Represents a snapshot of an FSM variable
    /// </summary>
    public class FsmVariableSnapshot
		{        
			public string Name { get; private set; }
			public string Type { get; private set; }
			public object Value { get; private set; }
			
			/// <summary>
			/// Creates a snapshot from the given variable
			/// </summary>
			public static FsmVariableSnapshot Create(object variable)
			{            
				if (variable == null) return null;
				
				var snapshot = new FsmVariableSnapshot();
				
				// Get variable name            
				var nameProperty = variable.GetType().GetProperty("Name");            
				if (nameProperty == null) return null;
				
				snapshot.Name = (string)nameProperty.GetValue(variable, null);
				snapshot.Type = variable.GetType().Name.Replace("Fsm", "").ToLower();
				
				// Get variable value            
				var valueProperty = variable.GetType().GetProperty("Value");            
				if (valueProperty != null)
				{                
					snapshot.Value = valueProperty.GetValue(variable, null);
				}
				
				return snapshot;
			}
			
			/// <summary>
			/// Restores the variable to the given FSM
			/// </summary>
			public void Restore(object fsm)
			{            
				if (fsm == null || string.IsNullOrEmpty(Name)) return;
				
				switch (Type.ToLower())
				{                
					case "bool":
						PlayMakerUtilities.SetBoolVariable(fsm, Name, (bool)Value);
						break;
					case "int":
						PlayMakerUtilities.SetIntVariable(fsm, Name, (int)Value);
						break;
					case "float":
						PlayMakerUtilities.SetFloatVariable(fsm, Name, (float)Value);
						break;
					case "string":
						PlayMakerUtilities.SetStringVariable(fsm, Name, (string)Value);
						break;
					case "vector3":
						PlayMakerUtilities.SetVector3Variable(fsm, Name, (Vector3)Value);
						break;
					case "gameobject":
						PlayMakerUtilities.SetGameObjectVariable(fsm, Name, (GameObject)Value);
						break;
				}
			}
		}
		
}
