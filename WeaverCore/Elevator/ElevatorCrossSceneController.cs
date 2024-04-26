using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Utilities;
using static WeaverCore.Elevator.Elevator;

namespace WeaverCore.Elevator
{
    public class ElevatorCrossSceneController : MonoBehaviour, ISerializationCallbackReceiver
    {
        [OnHarmonyPatch]
        static void OnHarmonyPatch(HarmonyPatcher patcher)
        {
            {
                var orig = typeof(HeroController).GetMethod(nameof(HeroController.EnterScene));
                var prefix = typeof(ElevatorCrossSceneController).GetMethod(nameof(EnterScene_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
                patcher.Patch(orig, prefix, null);
            }
        }

        static bool EnterScene_Prefix(HeroController __instance, TransitionPoint enterGate, float delayBeforeEnter)
        {
            //foreach (var crossScene in GameObject.FindObjectsOfType<ElevatorCrossSceneController>())
            foreach (var crossScene in ComponentUtilities.GetComponentsInChildrenList<ElevatorCrossSceneController>(enterGate.gameObject.scene.GetRootGameObjects()))
            {
                for (int i = 0; i < crossScene.crossSceneEntries.Count; i++)
                {
                    var entry = crossScene.crossSceneEntries[i];
                    if (entry.WhenExitingFromTransition == enterGate)
                    {
                        crossScene.transform.position = entry.ElevatorPosition;
                        if (entry.AttachPlayerToElevator)
                        {
                            if (crossScene.TryGetComponent<WeaverHeroPlatformStick>(out var stick))
                            {
                                stick.ForceStickPlayer();
                            }
                        }

                        var elevator = crossScene.GetComponent<Elevator>();

                        switch (entry.Action)
                        {
                            case CrossSceneEntry.MoveAction.MoveUp:
                                elevator.CallElevatorUp(entry.Info);


                                float delay = 0f;

                                if (entry.ForceCancelDesolateDive)
                                {
                                    HeroController.instance.exitedQuake = false;
                                }

                                if (HeroController.instance.exitedQuake)
                                {
                                    delay = 0.25f;
                                }
                                else
                                {
                                    delay = 0.33f;
                                }

                                elevator.LockPlayerToRange(new Vector2(-5f, 5f), delayBeforeEnter + 0.165f + enterGate.entryDelay + 0.4f + delay, entry.AttachPlayerToElevator, entry.StopStickingPrematurely);
                                break;
                            case CrossSceneEntry.MoveAction.MoveDown:

                                HeroController.instance.exitedQuake = false;
                                elevator.CallElevatorDown(entry.Info);
                                elevator.LockPlayerToRange(new Vector2(-5f, 5f), delayBeforeEnter + HeroController.instance.TIME_TO_ENTER_SCENE_BOT + 0.165f + enterGate.entryDelay + 0.4f, entry.AttachPlayerToElevator, entry.StopStickingPrematurely);
                                break;
                            default:
                                break;
                        }

                        break;
                    }
                }
            }
            return true;
        }


        [Serializable]
        public class CrossSceneEntry
        {
            public enum MoveAction
            {
                None,
                MoveUp,
                MoveDown
            }

            [Tooltip("When the player enters into this scene through this transition Point")]
            public TransitionPoint WhenExitingFromTransition;

            [Tooltip("The position the elevator should teleport to when the player enters from this transition point")]
            public Vector3 ElevatorPosition;

            [Tooltip("Attaches the player automatically to the elevator (assuming the elevator has a WeaverHeroPlatformStick component")]
            public bool AttachPlayerToElevator = true;

            [Tooltip("The action the elevator should immediately do when the player enters from this transition point")]
            public MoveAction Action = MoveAction.None;

            [Tooltip("Should the elevator stop sticking the player to the platform when the elevator stops moving? You should keep this to true unless you know what you are doing")]
            public bool StopStickingPrematurely = true;

            [Tooltip("If the player is desolate diving into the doorway, should that be immediately canceled when entering this doorway (Should be true to prevent major issues)")]
            public bool ForceCancelDesolateDive = true;

            [Tooltip("Extra info about the elevator movement")]
            public ElevatorInfo Info;
        }

        [SerializeField]
        [HideInInspector]
        string infoJson;

        [SerializeField]
        [HideInInspector]
        List<UnityEngine.Object> infoObjects;

        [SerializeField]
        List<CrossSceneEntry> crossSceneEntries = new List<CrossSceneEntry>();

        //[SerializeField]
        //List<CrossSceneEntry> resultSceneEntries = new List<CrossSceneEntry>();

        [Serializable]
        class Container
        {
            public List<CrossSceneEntry> entries;
        }

        private void OnValidate()
        {
            WeaverSerializer.Serialize(new Container { entries = crossSceneEntries }, out infoJson, out infoObjects);
        }

        public void OnBeforeSerialize()
        {
/*#if UNITY_EDITOR
            WeaverSerializer.Serialize(new Container { entries = crossSceneEntries }, out infoJson, out infoObjects);
#endif*/
        }

        public void OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            crossSceneEntries = WeaverSerializer.Deserialize<Container>(infoJson, infoObjects).entries;
#endif
        }
    }
}