using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Playmaker;

namespace WeaverCore.Utilities
{
	public static partial class PlayMakerUtilities
	{
		static readonly object DebugRegistryLock = new object();
		static readonly Dictionary<object, Component> DebuggedFsms = new Dictionary<object, Component>();
		static readonly Dictionary<object, string> CurrentEventByFsm = new Dictionary<object, string>();
		static readonly Dictionary<object, string> CurrentEventSourceByFsm = new Dictionary<object, string>();

		static MethodInfo debugProcessEventMethod;
		static MethodInfo debugSwitchStateMethod;
		static MethodInfo debugEnterStateMethod;
		static PropertyInfo debugStateNameProperty;
		static PropertyInfo debugEventNameProperty;
		static PropertyInfo debugFsmNameProperty;
		static PropertyInfo debugFsmGameObjectProperty;
		static PropertyInfo debugFsmComponentProperty;
		static PropertyInfo debugActionNameProperty;
		static FieldInfo debugCurrentEventDataField;
		static FieldInfo debugSentByGameObjectField;
		static FieldInfo debugSentByFsmField;
		static FieldInfo debugSentByStateField;
		static FieldInfo debugSentByActionField;

		public static bool EnableDebugging(Component playMakerFSM)
		{
			if (!IsPlayMakerFSM(playMakerFSM))
			{
				return false;
			}

			object fsm = GetFsm(playMakerFSM);
			if (fsm == null)
			{
				return false;
			}

			bool added;
			lock (DebugRegistryLock)
			{
				added = !DebuggedFsms.ContainsKey(fsm);
				DebuggedFsms[fsm] = playMakerFSM;
			}

			if (added)
			{
				WeaverLog.Log($"[FSM DEBUG][ENABLE] {GetDebugSourceLabel(playMakerFSM)}");
			}

			return true;
		}

		public static bool EnableDebugging(PlayMakerFsmWrapper playMakerFSM)
		{
			return EnableDebugging(playMakerFSM.InternalComponent);
		}

		public static int EnableDebugging(GameObject gameObject, bool includeChildren = true)
		{
			if (gameObject == null || PlayMakerFSMType == null)
			{
				return 0;
			}

			Component[] fsms = includeChildren
				? gameObject.GetComponentsInChildren(PlayMakerFSMType, true).OfType<Component>().ToArray()
				: gameObject.GetComponents(PlayMakerFSMType).OfType<Component>().ToArray();

			int enabledCount = 0;
			for (int i = 0; i < fsms.Length; i++)
			{
				if (EnableDebugging(fsms[i]))
				{
					enabledCount++;
				}
			}

			return enabledCount;
		}

		public static bool DisableDebugging(Component playMakerFSM)
		{
			if (!IsPlayMakerFSM(playMakerFSM))
			{
				return false;
			}

			object fsm = GetFsm(playMakerFSM);
			if (fsm == null)
			{
				return false;
			}

			bool removed;
			lock (DebugRegistryLock)
			{
				removed = DebuggedFsms.Remove(fsm);
				CurrentEventByFsm.Remove(fsm);
				CurrentEventSourceByFsm.Remove(fsm);
			}

			if (removed)
			{
				WeaverLog.Log($"[FSM DEBUG][DISABLE] {GetDebugSourceLabel(playMakerFSM)}");
			}

			return removed;
		}

		public static bool DisableDebugging(PlayMakerFsmWrapper playMakerFSM)
		{
			return DisableDebugging(playMakerFSM.InternalComponent);
		}

		public static void DisableAllDebugging()
		{
			lock (DebugRegistryLock)
			{
				DebuggedFsms.Clear();
				CurrentEventByFsm.Clear();
				CurrentEventSourceByFsm.Clear();
			}
		}

		[OnHarmonyPatch]
		static void PatchDebugHooks(HarmonyPatcher patcher)
		{
			if (!PlayMakerAvailable)
			{
				return;
			}

			MethodInfo processEventMethod = GetDebugProcessEventMethod();
			MethodInfo switchStateMethod = GetDebugSwitchStateMethod();
			MethodInfo enterStateMethod = GetDebugEnterStateMethod();

			MethodInfo processEventPrefix = typeof(PlayMakerUtilities).GetMethod(nameof(DebugProcessEventPrefix), BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo processEventPostfix = typeof(PlayMakerUtilities).GetMethod(nameof(DebugProcessEventPostfix), BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo switchStatePrefix = typeof(PlayMakerUtilities).GetMethod(nameof(DebugSwitchStatePrefix), BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo enterStatePrefix = typeof(PlayMakerUtilities).GetMethod(nameof(DebugEnterStatePrefix), BindingFlags.NonPublic | BindingFlags.Static);

			if (processEventMethod != null)
			{
				patcher.Patch(processEventMethod, processEventPrefix, processEventPostfix);
			}

			if (switchStateMethod != null)
			{
				patcher.Patch(switchStateMethod, switchStatePrefix, null);
			}

			if (enterStateMethod != null)
			{
				patcher.Patch(enterStateMethod, enterStatePrefix, null);
			}
		}

		static bool DebugProcessEventPrefix(object __instance, object fsmEvent, object eventData)
		{
			if (!TryGetDebuggedComponent(__instance, out var component))
			{
				return true;
			}

			string eventName = GetDebugEventName(fsmEvent);
			string activeStateName = GetActiveStateName(__instance) ?? "[no active state]";
			string eventSource = GetDebugEventSourceLabel(eventData);

			lock (DebugRegistryLock)
			{
				CurrentEventByFsm[__instance] = eventName;
				CurrentEventSourceByFsm[__instance] = eventSource;
			}

			if (string.IsNullOrEmpty(eventSource))
			{
				WeaverLog.Log($"[FSM DEBUG][EVENT] {GetDebugSourceLabel(component)} :: state={activeStateName} event={eventName}");
			}
			else
			{
				WeaverLog.Log($"[FSM DEBUG][EVENT] {GetDebugSourceLabel(component)} :: state={activeStateName} event={eventName} from={eventSource}");
			}
			return true;
		}

		static void DebugProcessEventPostfix(object __instance)
		{
			lock (DebugRegistryLock)
			{
				CurrentEventByFsm.Remove(__instance);
				CurrentEventSourceByFsm.Remove(__instance);
			}
		}

		static bool DebugSwitchStatePrefix(object __instance, object toState)
		{
			if (!TryGetDebuggedComponent(__instance, out var component))
			{
				return true;
			}

			string fromStateName = GetActiveStateName(__instance) ?? "[no active state]";
			string toStateName = GetDebugStateName(toState);
			string eventName;
			string eventSource;

			lock (DebugRegistryLock)
			{
				CurrentEventByFsm.TryGetValue(__instance, out eventName);
				CurrentEventSourceByFsm.TryGetValue(__instance, out eventSource);
			}

			if (string.IsNullOrEmpty(eventName))
			{
				WeaverLog.Log($"[FSM DEBUG][TRANSITION] {GetDebugSourceLabel(component)} :: {fromStateName} -> {toStateName}");
			}
			else if (string.IsNullOrEmpty(eventSource))
			{
				WeaverLog.Log($"[FSM DEBUG][TRANSITION] {GetDebugSourceLabel(component)} :: {fromStateName} --({eventName})-> {toStateName}");
			}
			else
			{
				WeaverLog.Log($"[FSM DEBUG][TRANSITION] {GetDebugSourceLabel(component)} :: {fromStateName} --({eventName} from={eventSource})-> {toStateName}");
			}

			return true;
		}

		static bool DebugEnterStatePrefix(object __instance, object state)
		{
			if (!TryGetDebuggedComponent(__instance, out var component))
			{
				return true;
			}

			WeaverLog.Log($"[FSM DEBUG][ENTER] {GetDebugSourceLabel(component)} :: {GetDebugStateName(state)}");
			return true;
		}

		static bool TryGetDebuggedComponent(object fsm, out Component component)
		{
			lock (DebugRegistryLock)
			{
				return DebuggedFsms.TryGetValue(fsm, out component) && component != null;
			}
		}

		static MethodInfo GetDebugProcessEventMethod()
		{
			if (debugProcessEventMethod == null && FsmType != null && FsmEventType != null)
			{
				debugProcessEventMethod = FsmType
					.GetMethods(BindingFlags.Instance | BindingFlags.Public)
					.FirstOrDefault(m =>
					{
						if (m.Name != "ProcessEvent")
						{
							return false;
						}

						ParameterInfo[] parameters = m.GetParameters();
						return parameters.Length == 2 && parameters[0].ParameterType == FsmEventType;
					});
			}

			return debugProcessEventMethod;
		}

		static MethodInfo GetDebugSwitchStateMethod()
		{
			if (debugSwitchStateMethod == null && FsmType != null && FsmStateType != null)
			{
				debugSwitchStateMethod = FsmType.GetMethod("SwitchState", BindingFlags.Instance | BindingFlags.Public, null, new[] { FsmStateType }, null);
			}

			return debugSwitchStateMethod;
		}

		static MethodInfo GetDebugEnterStateMethod()
		{
			if (debugEnterStateMethod == null && FsmType != null && FsmStateType != null)
			{
				debugEnterStateMethod = FsmType.GetMethod("EnterState", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { FsmStateType }, null);
			}

			return debugEnterStateMethod;
		}

		static string GetDebugStateName(object state)
		{
			if (state == null)
			{
				return "[null state]";
			}

			if (debugStateNameProperty == null && FsmStateType != null)
			{
				debugStateNameProperty = FsmStateType.GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
			}

			return debugStateNameProperty?.GetValue(state) as string ?? state.ToString();
		}

		static string GetDebugEventName(object fsmEvent)
		{
			if (fsmEvent == null)
			{
				return "[null event]";
			}

			if (debugEventNameProperty == null && FsmEventType != null)
			{
				debugEventNameProperty = FsmEventType.GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
			}

			return debugEventNameProperty?.GetValue(fsmEvent) as string ?? fsmEvent.ToString();
		}

		static string GetDebugSourceLabel(Component playMakerFSM)
		{
			string objectPath = GetTransformPath(playMakerFSM != null ? playMakerFSM.transform : null);
			string fsmName = playMakerFSM != null ? GetFsmName(playMakerFSM) : null;

			if (string.IsNullOrEmpty(fsmName))
			{
				return objectPath;
			}

			return $"{objectPath} [{fsmName}]";
		}

		static string GetDebugEventSourceLabel(object eventDataOverride)
		{
			object eventData = GetCurrentEventData(eventDataOverride);
			if (eventData == null)
			{
				return null;
			}

			EnsureDebugEventDataReflection();

			GameObject sentByGameObject = debugSentByGameObjectField?.GetValue(eventData) as GameObject;
			object sentByFsm = debugSentByFsmField?.GetValue(eventData);
			object sentByState = debugSentByStateField?.GetValue(eventData);
			object sentByAction = debugSentByActionField?.GetValue(eventData);

			if (sentByGameObject == null && sentByFsm == null && sentByState == null && sentByAction == null)
			{
				return null;
			}

			string objectPath = sentByGameObject != null ? GetTransformPath(sentByGameObject.transform) : GetDebugFsmObjectPath(sentByFsm);
			string fsmName = GetDebugFsmName(sentByFsm);
			string stateName = GetDebugStateName(sentByState);
			string actionName = GetDebugActionName(sentByAction);

			List<string> parts = new List<string>();
			if (!string.IsNullOrEmpty(objectPath))
			{
				parts.Add(objectPath);
			}
			if (!string.IsNullOrEmpty(fsmName))
			{
				parts.Add($"[{fsmName}]");
			}
			if (!string.IsNullOrEmpty(stateName))
			{
				parts.Add($"state={stateName}");
			}
			if (!string.IsNullOrEmpty(actionName))
			{
				parts.Add($"action={actionName}");
			}

			return parts.Count > 0 ? string.Join(" ", parts) : null;
		}

		static object GetCurrentEventData(object eventDataOverride)
		{
			if (eventDataOverride != null)
			{
				return eventDataOverride;
			}

			if (debugCurrentEventDataField == null && FsmType != null)
			{
				debugCurrentEventDataField = FsmType.GetField("EventData", BindingFlags.Public | BindingFlags.Static);
			}

			return debugCurrentEventDataField?.GetValue(null);
		}

		static void EnsureDebugEventDataReflection()
		{
			if (debugSentByGameObjectField != null || FsmType == null)
			{
				return;
			}

			Type eventDataType = FsmType.Assembly.GetType("HutongGames.PlayMaker.FsmEventData");
			if (eventDataType == null)
			{
				return;
			}

			debugSentByGameObjectField = eventDataType.GetField("SentByGameObject", BindingFlags.Instance | BindingFlags.Public);
			debugSentByFsmField = eventDataType.GetField("SentByFsm", BindingFlags.Instance | BindingFlags.Public);
			debugSentByStateField = eventDataType.GetField("SentByState", BindingFlags.Instance | BindingFlags.Public);
			debugSentByActionField = eventDataType.GetField("SentByAction", BindingFlags.Instance | BindingFlags.Public);
		}

		static string GetDebugFsmName(object fsm)
		{
			if (fsm == null)
			{
				return null;
			}

			if (debugFsmNameProperty == null && FsmType != null)
			{
				debugFsmNameProperty = FsmType.GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
			}

			return debugFsmNameProperty?.GetValue(fsm) as string ?? fsm.ToString();
		}

		static string GetDebugFsmObjectPath(object fsm)
		{
			if (fsm == null)
			{
				return null;
			}

			if (debugFsmComponentProperty == null && FsmType != null)
			{
				debugFsmComponentProperty = FsmType.GetProperty("FsmComponent", BindingFlags.Instance | BindingFlags.Public);
			}

			Component component = debugFsmComponentProperty?.GetValue(fsm) as Component;
			if (component != null)
			{
				return GetTransformPath(component.transform);
			}

			if (debugFsmGameObjectProperty == null && FsmType != null)
			{
				debugFsmGameObjectProperty = FsmType.GetProperty("GameObject", BindingFlags.Instance | BindingFlags.Public);
			}

			GameObject gameObject = debugFsmGameObjectProperty?.GetValue(fsm) as GameObject;
			return gameObject != null ? GetTransformPath(gameObject.transform) : null;
		}

		static string GetDebugActionName(object action)
		{
			if (action == null)
			{
				return null;
			}

			if (debugActionNameProperty == null && FsmStateActionType != null)
			{
				debugActionNameProperty = FsmStateActionType.GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
			}

			object wrappedAction = null;
			FieldInfo wrappedActionField = action.GetType().GetField("_action", BindingFlags.Instance | BindingFlags.NonPublic);
			if (wrappedActionField != null)
			{
				wrappedAction = wrappedActionField.GetValue(action);
			}

			string actionTypeName = wrappedAction != null ? wrappedAction.GetType().Name : action.GetType().Name;
			string actionName = debugActionNameProperty?.GetValue(action) as string;

			if (!string.IsNullOrEmpty(actionName) && actionName != actionTypeName)
			{
				return $"{actionTypeName} ({actionName})";
			}

			return actionTypeName;
		}

		static string GetTransformPath(Transform transform)
		{
			if (transform == null)
			{
				return "[missing object]";
			}

			List<string> parts = new List<string>();
			while (transform != null)
			{
				parts.Add(transform.name);
				transform = transform.parent;
			}

			parts.Reverse();
			return string.Join("/", parts);
		}
	}
}
