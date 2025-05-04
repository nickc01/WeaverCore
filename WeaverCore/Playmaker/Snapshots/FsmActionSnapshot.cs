using System;
using System.Collections.Generic;
using System.Reflection;

namespace WeaverCore.Playmaker.Snapshots
{
    /// <summary>
    /// Represents a snapshot of an FSM action
    /// </summary>
    public class FsmActionSnapshot
		{        
			//public string ActionTypeName { get; private set; }
			public object Action { get; private set; }
			public Dictionary<string, object> Properties { get; private set; } = new Dictionary<string, object>();
			
			/// <summary>
			/// Creates a snapshot from the given action
			/// </summary>
			public static FsmActionSnapshot Create(object action)
			{            
				if (action == null) return null;
				
				var snapshot = new FsmActionSnapshot
				{
					//ActionTypeName = action.GetType().FullName,
					Action = action
				};
				
				// Capture all public properties and fields
				var properties = action.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
				foreach (var prop in properties)
				{                
					if (prop.CanRead && prop.GetIndexParameters().Length == 0)
					{                    
						try
						{                        
							var value = prop.GetValue(action, null);
							snapshot.Properties[prop.Name] = value;
						}
						catch
						{                        
							// Skip properties that can't be read
						}
					}
				}
				
				var fields = action.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach (var field in fields)
				{                
					try
					{                    
						var value = field.GetValue(action);
						snapshot.Properties[field.Name] = value;
					}
					catch
					{                    
						// Skip fields that can't be read
					}
				}
				
				return snapshot;
			}
			
			/// <summary>
			/// Restores the properties to the given action
			/// </summary>
			public void RestoreProperties(object action)
			{            
				if (action == null) return;
				
				foreach (var kvp in Properties)
				{                
					string propName = kvp.Key;
					object propValue = kvp.Value;
					
					// Try to set as property first                
					var property = action.GetType().GetProperty(propName);
					if (property != null && property.CanWrite)
					{                    
						try
						{                        
							if (propValue != null && propValue.GetType() != property.PropertyType &&
								property.PropertyType.IsEnum && propValue is int intValue)
							{                            
								// Convert int to enum
								property.SetValue(action, Enum.ToObject(property.PropertyType, intValue), null);
							}
							else
							{                            
								property.SetValue(action, propValue, null);
							}
						}
						catch
						{                        
							// Skip properties that can't be written
						}
						continue;
					}
					
					// Try to set as field
					var field = action.GetType().GetField(propName);
					if (field != null)
					{                    
						try
						{                        
							if (propValue != null && propValue.GetType() != field.FieldType &&
								field.FieldType.IsEnum && propValue is int intValue)
							{                            
								// Convert int to enum
								field.SetValue(action, Enum.ToObject(field.FieldType, intValue));
							}
							else
							{                            
								field.SetValue(action, propValue);
							}
						}
						catch
						{                        
							// Skip fields that can't be written
						}
					}
				}
			}
		}
		
}
