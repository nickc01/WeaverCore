using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WeaverCore.Internal
{
    internal class EnemyHPBars_Interop
    {
        [ModImportName("EnemyHPBar")]
        private static class EnemyHPBarsImport
        {
            public static Action<GameObject> DisableHPBar = null;
            public static Action<GameObject> EnableHPBar = null;
            public static Action<GameObject> RefreshHPBar = null;
            public static Action<GameObject> MarkAsBoss = null;
            public static Action<GameObject> MarkAsNonBoss = null;
        }

        static EnemyHPBars_Interop()
        {
            typeof(EnemyHPBarsImport).ModInterop();
        }

        public static void DisableHPBar(GameObject go)
            => EnemyHPBarsImport.DisableHPBar?.Invoke(go);

        public static void EnableHPBar(GameObject go)
            => EnemyHPBarsImport.EnableHPBar?.Invoke(go);

        public static void RefreshHPBar(GameObject go)
            => EnemyHPBarsImport.RefreshHPBar?.Invoke(go);

        public static void MarkAsBoss(GameObject go)
            => EnemyHPBarsImport.MarkAsBoss?.Invoke(go);

        public static void MarkAsNonBoss(GameObject go)
            => EnemyHPBarsImport.MarkAsNonBoss?.Invoke(go);
    }
}
