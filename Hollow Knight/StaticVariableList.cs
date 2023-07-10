using System.Collections.Generic;
using UnityEngine;

public static class StaticVariableList
{
    public static Dictionary<string, object> variables;

    public static void Clear()
    {
        if (variables != null)
        {
            variables.Clear();
        }
    }

    public static void SetValue<T>(string variableName, T value)
    {
        if (variables == null)
        {
            variables = new Dictionary<string, object>();
        }
        variables[variableName] = value;
    }

    public static T GetValue<T>(string variableName)
    {
        if (variables == null || !variables.ContainsKey(variableName))
        {
            Debug.LogError($"Attempt to get {variableName} from static variable list failed!");
            return default(T);
        }
        return (T)variables[variableName];
    }

    public static bool Exists(string variableName)
    {
        if (variables == null)
        {
            return false;
        }
        return variables.ContainsKey(variableName);
    }
}
