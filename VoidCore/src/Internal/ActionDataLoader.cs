using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Harmony;
using HutongGames.PlayMaker;
using UnityEngine;
using VoidCore.Helpers;



namespace VoidCore.Internal
{
    static class ActionLoader
    {

        public static void ModifyActions(List<FsmStateAction> Actions,FsmState state,GameObject obj)
        {
            try
            {

            }
            catch(Exception e)
            {
                Modding.Logger.Log("Modify Actions Exception = " + e);
                StackTrace trace = new StackTrace(true);
                Modding.Logger.Log("Stack Trace = " + trace);
            }
        }
    }


    [HarmonyPatch]
    static class ActionsGetterPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(FsmState).GetProperty("Actions").GetGetMethod();
        }

        static bool Prefix(ref bool __state,ref FsmStateAction[] ___actions)
        {
            return ActionsLoaderPatch.Prefix(ref __state, ref ___actions);
        }

        static void Postfix(ref bool __state,ref FsmStateAction[] ___actions,FsmState __instance)
        {
            ActionsLoaderPatch.Postfix(ref __state, ref ___actions, __instance);
        }
    }

    [HarmonyPatch(typeof(FsmState))]
    [HarmonyPatch("LoadActions")]
    static class ActionsLoaderPatch
    {
        class Props
        {
            public bool Modified = false;
        }

        static PropertyTable<FsmState, Props> PropertyTable = new PropertyTable<FsmState, Props>();


        public static bool Prefix(ref bool __state,ref FsmStateAction[] ___actions)
        {
            __state = ___actions == null;
            return true;
        }

        public static void ApplyActionMods(FsmState state, ref FsmStateAction[] ___actions)
        {
            var properties = PropertyTable.GetOrCreate(state);
            if (!properties.Modified)
            {
                properties.Modified = true;
                List<FsmStateAction> actions;
                if (___actions == null)
                {
                    actions = new List<FsmStateAction>();
                }
                else
                {
                    actions = ___actions.ToList();
                }
                ActionLoader.ModifyActions(actions, state,state.Fsm.GameObject);
                foreach (var action in actions)
                {
                    action.Init(state);
                }
                ___actions = actions.ToArray();
            }
        }

        public static void Postfix(ref bool __state, ref FsmStateAction[] ___actions, FsmState __instance)
        {
            if (__state && !PlayMakerFSM.NotMainThread)
            {
                ApplyActionMods(__instance, ref ___actions);
            }
        }
    }
}
