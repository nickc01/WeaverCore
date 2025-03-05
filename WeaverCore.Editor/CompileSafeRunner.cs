using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[InitializeOnLoad]
public class CompileSafeRunner
{
    static CompileSafeRunner()
    {
        File.WriteAllText("/tmp/unty1.txt", "Successful call 1");
        // Code to run even during script compilation failures
        EditorApplication.delayCall += RunSafeCode;
    }

    private static void RunSafeCode()
    {
        File.WriteAllText("/tmp/unty2.txt", "Successful call 2");
        Debug.Log("This will run despite compilation errors in game scripts.");
    }

    [DidReloadScripts]
    private static void RunSafeCode2()
    {
        File.WriteAllText("/tmp/unty3.txt", "Successful call 3");
        Debug.Log("This will run despite compilation errors in game scripts.");
    }
}