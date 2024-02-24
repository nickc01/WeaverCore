using System.Collections;
using System.IO;
using UnityEngine.Profiling;
using WeaverCore.Assets;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore.Internal
{
    public sealed class WeaverCoreGlobalSettings : GlobalSettings
    {
        public override string TabName => "WeaverCore";


        static UnboundCoroutine profileRoutine;

        [SettingField(EnabledType.AlwaysVisible)]
        public void OpenDebugTools()
        {
            SettingsScreen.Instance.Hide();
            if (!WeaverCoreDebugTools.IsOpen)
            {
                WeaverCoreDebugTools.Open();
            }
        }

        [SettingField(EnabledType.AlwaysVisible, "Start Profiling")]
        public void StartProfiling()
        {
            Profiler.SetAreaEnabled(ProfilerArea.CPU, true);
            Profiler.SetAreaEnabled(ProfilerArea.GPU, true);
            Profiler.SetAreaEnabled(ProfilerArea.Rendering, true);
            Profiler.SetAreaEnabled(ProfilerArea.Memory, true);
            Profiler.enableAllocationCallstacks = true;


            Profiler.logFile = $"PROFILE_{System.DateTime.Now:dd-MM-yyyy HH-mm-ss}"; //Also supports passing "myLog.raw"
            Profiler.enableBinaryLog = true;
            Profiler.enabled = true;

            if (profileRoutine == null)
            {
                profileRoutine = UnboundCoroutine.Start(MemoryProfiling($"MEMORY_{System.DateTime.Now:dd-MM-yyyy HH-mm-ss}.csv"));
            }
        }

        [SettingField(EnabledType.AlwaysVisible, "Stop Profiling")]
        public void StopProfiling()
        {
            Profiler.enableBinaryLog = false;
            Profiler.enabled = false;
            if (profileRoutine != null)
            {
                UnboundCoroutine.Stop(profileRoutine);
            }
        }

        static double BytesToGigabytes(long size)
        {
            return (size / 1000) / 1000_000.0;
        }

        static IEnumerator MemoryProfiling(string memoryFileName)
        {
            using (var file = File.CreateText(memoryFileName))
            {
                file.WriteLine("Mono Heap Size,Mono Used Size,Allocated Graphics Mem,Total Alloc Mem,Total Reserved Mem,Total Unused Reserve Mem,Used Heap Size");

                while (true)
                {
                    file.WriteLine($"{BytesToGigabytes(Profiler.GetMonoHeapSizeLong())},{BytesToGigabytes(Profiler.GetMonoUsedSizeLong())},{BytesToGigabytes(Profiler.GetAllocatedMemoryForGraphicsDriver())},{BytesToGigabytes(Profiler.GetTotalAllocatedMemoryLong())},{BytesToGigabytes(Profiler.GetTotalReservedMemoryLong())},{BytesToGigabytes(Profiler.GetTotalUnusedReservedMemoryLong())},{BytesToGigabytes(Profiler.usedHeapSizeLong)}");

                    yield return null;
                }
            }
        }
    }
}
