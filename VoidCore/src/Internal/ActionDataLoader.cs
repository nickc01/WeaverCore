using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using HutongGames.PlayMaker;
using VoidCore.Helpers;



namespace VoidCore.Internal
{

    class ActionTracker : FsmStateAction
    {
        public override void OnEnter()
        {
            Modding.Logger.Log("ENTERED STATE = " + State.Name);

            Finish();
        }
    }


    static class ActionLoader
    {

        public static void ModifyActions(List<FsmStateAction> Actions,FsmState state)
        {
            /*Modding.Logger.Log("MODIFY START");
            foreach (var action in Actions)
            {
                Modding.Logger.Log("ACTIONS = " + action);
            }*/
            //Actions.Add(new ActionTracker());
        }
        /*[ModStart(typeof(VoidCore))]
        static void Start()
        {
            
        }

        [ModEnd(typeof(VoidCore))]
        static void End()
        {

        }*/
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
            //Modding.Logger.Log("RUNNING1");
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
        public static bool Prefix(ref bool __state,ref FsmStateAction[] ___actions)
        {
            //Modding.Logger.Log("RUNNING2");
            __state = ___actions == null;
            //Modding.Logger.Log("STATE = " + __state);
            return true;
        }

        public static void Postfix(ref bool __state, ref FsmStateAction[] ___actions, FsmState __instance)
        {
            if (__state)
            {
                List<FsmStateAction> actions;
                if (___actions == null)
                {
                    actions = new List<FsmStateAction>();
                }
                else
                {
                    actions = ___actions.ToList();
                }
                if (actions.Count == 0 || !(actions[0] is ActionTracker))
                {
                    ActionLoader.ModifyActions(actions, __instance);
                    actions.Insert(0, new ActionTracker());
                    foreach (var action in actions)
                    {
                        action.Init(__instance);
                    }
                }
                ___actions = actions.ToArray();
            }
        }
    }

    /*internal class StateProperties
    {
        public bool Modified = false;


        delegate bool tryGetDelegate(ActionData key, out StateProperties value);
        delegate void addDelegate(ActionData key, StateProperties value);

        static bool Initialized = false;
        static Type ConditionalWeakTable;
        static Func<ActionData, StateProperties> GetOrCreateValue;
        static tryGetDelegate TryGetValue;
        static addDelegate Add;
        static Func<FsmState, ActionData> GetActionData;

        static object Properties;

        //static WeakDictionary<FsmState,StateProperties> Properties = new WeakDictionary<FsmState, StateProperties>();

        //static object Properties = new object();

        public static StateProperties GetProperties(FsmState state)
        {
            if (ModuleInitializer.TheraotCore != null && !Initialized)
            {
                Initialized = true;
                Initialize();
            }
            
            return GetOrCreateValue(GetActionData(state));
        
        }

        static void Initialize()
        {
            try
            {
                Modding.Logger.Log("AA");
                ConditionalWeakTable = ModuleInitializer.TheraotCore.GetType("System.Runtime.CompilerServices.ConditionalWeakTable`2").MakeGenericType(typeof(ActionData), typeof(StateProperties));
                Modding.Logger.Log("ConditionalWeakTable = " + ConditionalWeakTable);
                Properties = Activator.CreateInstance(ConditionalWeakTable);
                Modding.Logger.Log("CC");
                GetOrCreateValue = (Func<ActionData, StateProperties>)Delegate.CreateDelegate(typeof(Func<ActionData, StateProperties>), Properties, ConditionalWeakTable.GetMethod("GetOrCreateValue"));
                TryGetValue = (tryGetDelegate)Delegate.CreateDelegate(typeof(tryGetDelegate), Properties, ConditionalWeakTable.GetMethod("TryGetValue"));
                Add = (addDelegate)Delegate.CreateDelegate(typeof(addDelegate), Properties, ConditionalWeakTable.GetMethod("Add"));
                GetActionData = Fields.CreateGetter<FsmState, ActionData>(typeof(FsmState).GetField("actionData", BindingFlags.NonPublic | BindingFlags.Instance));
                Modding.Logger.Log("DD");
            }
            catch (Exception e)
            {
                Modding.Logger.Log("Initialization Error = " + e);
            }
        }*/

        /*[HarmonyPatch]
        static class ConstructorPatch1
        {
            static MethodBase TargetMethod()
            {
                return typeof(FsmState).GetConstructor(new Type[] { typeof(Fsm) });
                //return typeof(FsmState).GetMethod("Finalize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }

            static void Postfix(FsmState __instance)
            {
                Modding.Logger.Log("CONSTRUCTOR1");
                if (!Properties.ContainsKey(__instance))
                {
                    Properties.Add(__instance, new StateProperties());
                }
            }
        }

        [HarmonyPatch]
        static class ConstructorPatch2
        {
            static MethodBase TargetMethod()
            {
                return typeof(FsmState).GetConstructor(new Type[] { typeof(FsmState) });
                //return typeof(FsmState).GetMethod("Finalize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }

            static void Postfix(FsmState __instance)
            {
                Modding.Logger.Log("CONSTRUCTOR2");
                if (!Properties.ContainsKey(__instance))
                {
                    Properties.Add(__instance, new StateProperties());
                }
            }
        }*/

        /*[HarmonyPatch]
        static class FinializerPatch
        {
            static MethodBase TargetMethod()
            {
                return typeof(FsmState).GetMethod("Finalize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }

            static void Prefix(FsmState __instance)
            {
                __instance.GetHashCode();
                Modding.Logger.Log("FINALIZE");
                //lock (Lock)
                //{
                //    Modding.Logger.Log("Removing State = " + __instance.Name);
                //    Properties.Remove(__instance.GetHashCode());
                //}
                //Properties.Add(__instance,new StateProperties());
            }
        }*/
    //}
}


/*namespace VoidCore.Internal
{
    public class TestAction : FsmStateAction
    {
        public FsmOwnerDefault gameObject;

        public override void OnEnter()
        {
            Modding.Logger.Log("ENTERED STATE = " + State.Name);
            base.OnEnter();
        }
    }


    public static class ActionDataLoader
    {
        static Func<ActionData, List<string>> ActionNamesGet = FindFieldGetter<ActionData,List<string>>("actionNames");
        static Action<ActionData, List<string>> ActionNamesSet = FindFieldSetter<ActionData,List<string>>("actionNames");

        static Func<ActionData, List<string>> CustomNamesGet = FindFieldGetter<ActionData, List<string>>("customNames");
        static Action<ActionData, List<string>> CustomNamesSet = FindFieldSetter<ActionData, List<string>>("customNames");

        static Func<ActionData, List<bool>> ActionEnabledGet = FindFieldGetter<ActionData, List<bool>>("actionEnabled");
        static Action<ActionData, List<bool>> ActionEnabledSet = FindFieldSetter<ActionData, List<bool>>("actionEnabled");

        static Func<ActionData, List<bool>> ActionIsOpenGet = FindFieldGetter<ActionData, List<bool>>("actionIsOpen");
        static Action<ActionData, List<bool>> ActionIsOpenSet = FindFieldSetter<ActionData, List<bool>>("actionIsOpen");

        static Func<ActionData, List<int>> ActionStartIndexGet = FindFieldGetter<ActionData, List<int>>("actionStartIndex");
        static Action<ActionData, List<int>> ActionStartIndexSet = FindFieldSetter<ActionData, List<int>>("actionStartIndex");

        static Func<ActionData, List<int>> ActionHashCodesGet = FindFieldGetter<ActionData, List<int>>("actionHashCodes");
        static Action<ActionData, List<int>> ActionHashCodesSet = FindFieldSetter<ActionData, List<int>>("actionHashCodes");

        static Func<ActionData, List<int>> ParamDataPosGet = FindFieldGetter<ActionData, List<int>>("paramDataPos");
        static Action<ActionData, List<int>> ParamDataPosSet = FindFieldSetter<ActionData, List<int>>("paramDataPos");

        static Func<ActionData, List<string>> ParamNameGet = FindFieldGetter<ActionData, List<string>>("paramName");
        static Action<ActionData, List<string>> ParamNameSet = FindFieldSetter<ActionData, List<string>>("paramName");

        static Func<ActionData, List<int>> ParamDataTypeGet = FindFieldGetter<ActionData, List<int>>("paramDataType");
        static Action<ActionData, List<int>> ParamDataTypeSet = FindFieldSetter<ActionData, List<int>>("paramDataType");

        static Func<ActionData, List<int>> ParamByteDataSizeGet = FindFieldGetter<ActionData, List<int>>("paramByteDataSize");
        static Action<ActionData, List<int>> ParamByteDataSizeSet = FindFieldSetter<ActionData, List<int>>("paramByteDataSize");

        static Func<ActionData, List<FsmOwnerDefault>> FsmOwnerDefaultParamsGet = FindFieldGetter<ActionData, List<FsmOwnerDefault>>("fsmOwnerDefaultParams");
        static Action<ActionData, List<FsmOwnerDefault>> FsmOwnerDefaultParamsSet = FindFieldSetter<ActionData, List<FsmOwnerDefault>>("fsmOwnerDefaultParams");


        static Func<Type, int> GetActionTypeHashCode = (Func<Type,int>)Delegate.CreateDelegate(typeof(Func<Type, int>), typeof(ActionData).GetMethod("GetActionTypeHashCode", BindingFlags.NonPublic | BindingFlags.Static));

        static bool Started = false;

        [ModStart(typeof(VoidCore))]
        public static void Start()
        {
            if (!Started)
            {
                Started = true;
                Modding.Logger.Log("START BEGIN");
                var harmony = ModuleInitializer.GetVoidCoreHarmony() as HarmonyInstance;
                harmony.Patch(ActionsPatch.TargetMethod(), new HarmonyMethod(typeof(ActionsPatch).GetMethod(nameof(ActionsPatch.Prefix))));
                harmony.Patch(ActionsPatch2.TargetMethod(), new HarmonyMethod(typeof(ActionsPatch2).GetMethod(nameof(ActionsPatch2.Prefix))));
                //On.HutongGames.PlayMaker.FsmState.LoadActions += FsmState_LoadActions;
                //On.HutongGames.PlayMaker.ActionData.LoadActions += LoadActions;
                On.HutongGames.PlayMaker.ReflectionUtils.GetGlobalType += GetType;
                Modding.Logger.Log("START END");
            }
        }

        private static Type GetType(On.HutongGames.PlayMaker.ReflectionUtils.orig_GetGlobalType orig, string typeName)
        {
            Type type = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    break;
                }
            }
            if (type != null)
            {
                return type;
            }
            return orig(typeName);
        }

        [ModEnd(typeof(VoidCore))]
        static void End()
        {
            if (Started)
            {
                Started = false;
                Modding.Logger.Log("START BEGIN");
                //On.HutongGames.PlayMaker.ActionData.LoadActions += LoadActions;
                //On.HutongGames.PlayMaker.ReflectionUtils.GetGlobalType -= GetType;
                Modding.Logger.Log("START END");
            }
        }

        private static void FsmState_LoadActions(On.HutongGames.PlayMaker.FsmState.orig_LoadActions orig, FsmState self)
        {
            LoadActions(self.ActionData, self);
            orig(self);
        }

        class ActionsPatch
        {
            public static MethodBase TargetMethod()
            {
                return typeof(FsmState).GetProperty("Actions", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
            }

            public static bool Prefix(FsmState __instance)
            {
                LoadActions(__instance.ActionData, __instance);
                return true;
            }
        }

        class ActionsPatch2
        {
            public static MethodBase TargetMethod()
            {
                return typeof(FsmState).GetMethod("LoadActions", BindingFlags.Public | BindingFlags.Instance);
                //return typeof(FsmState).GetProperty("Actions", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
            }

            public static bool Prefix(FsmState __instance)
            {
                LoadActions(__instance.ActionData, __instance);
                return true;
            }
        }

        private static void LoadActions(ActionData self, FsmState state)
        {
            Modding.Logger.Log("A");
            var actionNames = ActionNamesGet(self);
            List<string> actionNamesCopy = new List<string>(actionNames);
            var actionHashCodes = ActionHashCodesGet(self);
            var actionIsOpen = ActionIsOpenGet(self);
            var actionEnabled = ActionEnabledGet(self);
            var customNames = CustomNamesGet(self);
            var startIndexes = ActionStartIndexGet(self);
            //var fsmOwnerDefault = FsmOwnerDefaultParamsGet(self);
            var paramDataPos = ParamDataPosGet(self);
            var paramName = ParamNameGet(self);
            var paramDataType = ParamDataTypeGet(self);
            var paramByteDataSize = ParamByteDataSizeGet(self);
            var fsmOwnerDefaultParams = FsmOwnerDefaultParamsGet(self);

            var paramNameCopy = new List<string>(paramName);
            var count1 = paramName.Count;
            var count2 = paramNameCopy.Count;
            var count3 = paramName.Count();
            var capacity = paramName.Capacity;

            if (ActionNamesGet(self)[0] != typeof(TestAction).FullName)
            {
                Modding.Logger.Log("B");
                actionNames.Insert(0, typeof(TestAction).FullName);
                Modding.Logger.Log("C");
                actionHashCodes.Insert(0, GetActionTypeHashCode(ReflectionUtils.GetGlobalType(typeof(TestAction).FullName)));
                actionIsOpen.Insert(0, true);
                actionEnabled.Insert(0, true);
                customNames.Insert(0, CustomNamesGet(self)[0]);

                for (int i = 0; i < startIndexes.Count; i++)
                {
                    startIndexes[i] = startIndexes[i] + 1;
                }
                startIndexes.Insert(0, 0);
                fsmOwnerDefaultParams.Add(new FsmOwnerDefault());
                paramDataPos.Insert(0, fsmOwnerDefaultParams.Count - 1);
                Modding.Logger.Log("BEFORE = " + paramName.Count);
                foreach (var item in paramName)
                {
                    Modding.Logger.Log("CONTENT = " + item);
                }
                paramName.Insert(0, nameof(TestAction.gameObject));
                Modding.Logger.Log("AFTER = " + paramName.Count);
                paramName.Capacity = paramName.Count;
                foreach (var item in paramName)
                {
                    Modding.Logger.Log("CONTENT = " + item);
                }
                paramDataType.Insert(0, 20);
                paramByteDataSize.Insert(0, 0);
                //ParamDataPosGet(self).Insert(0, 3);

            }
            Modding.Logger.Log("Z");
            //Debugger.Break();
            //return orig(self, state);
        }

        static Func<Target,T> FindFieldGetter<Target,T>(string fieldName,BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return Fields.CreateGetter<Target,T>(typeof(Target).GetField(fieldName, flags));
        }

        static Action<Target, T> FindFieldSetter<Target, T>(string fieldName, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return Fields.CreateSetter<Target, T>(typeof(Target).GetField(fieldName, flags));
        }
    }
}*/
