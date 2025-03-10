using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;

[InitializeOnLoad]
public class HKRecomp
{
    [DllImport ("HKDLLInitializer")]
    private static extern float RunDotNetApp();

    static HKRecomp()
    {
        RunDotNetApp();
    }
}