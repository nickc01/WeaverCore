using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Playmaker
{
    /// <summary>
    /// A reflection-based wrapper around the PlayMaker FsmEvent class.
    /// </summary>
    public class WeaverFsmEvent
    {
        // Static reflection caching
        private static Type fsmEventType;
        private static MethodInfo findEventMethod;
        private static MethodInfo getFsmEventMethod;
        private static PropertyInfo nameProperty;
        private static PropertyInfo isSystemEventProperty;
        private static PropertyInfo isGlobalProperty;
        private static PropertyInfo pathProperty;
        
        // Static event fields
        //private static Dictionary<string, PropertyInfo> systemEventFields = new Dictionary<string, PropertyInfo>();
        
        // The wrapped FsmEvent instance
        public object FsmEventBase { get; set; }

        static WeaverFsmEvent()
        {
            if (PlayMakerUtilities.IsAvailable)
            {
                if (fsmEventType == null)
                {
                    fsmEventType = TypeUtilities.NameToType("HutongGames.PlayMaker.FsmEvent", PlayMakerUtilities.PlayMakerFSMType.Assembly.GetName().Name);
                }
                
                if (fsmEventType != null)
                {
                    // Cache basic properties
                    nameProperty = fsmEventType.GetProperty("Name");
                    isSystemEventProperty = fsmEventType.GetProperty("IsSystemEvent");
                    isGlobalProperty = fsmEventType.GetProperty("IsGlobal");
                    pathProperty = fsmEventType.GetProperty("Path");
                    
                    // Cache methods
                    findEventMethod = fsmEventType.GetMethod("FindEvent");
                    getFsmEventMethod = fsmEventType.GetMethod("GetFsmEvent", new[] { typeof(string) });
                    
                    // Cache system event static properties
                    /*string[] systemEvents = new string[]
                    {
                        "Finished", "BecameInvisible", "BecameVisible", "CollisionEnter", "CollisionExit",
                        "CollisionStay", "CollisionEnter2D", "CollisionExit2D", "CollisionStay2D",
                        "ControllerColliderHit", "Finished", "LevelLoaded", "MouseDown", "MouseDrag",
                        "MouseEnter", "MouseExit", "MouseOver", "MouseUp", "MouseUpAsButton", "TriggerEnter",
                        "TriggerExit", "TriggerStay", "TriggerEnter2D", "TriggerExit2D", "TriggerStay2D",
                        "ApplicationFocus", "ApplicationPause", "ApplicationQuit", "ParticleCollision",
                        "JointBreak", "JointBreak2D", "Disable", "PlayerConnected", "ServerInitialized",
                        "ConnectedToServer", "PlayerDisconnected", "DisconnectedFromServer", "FailedToConnect",
                        "FailedToConnectToMasterServer", "MasterServerEvent", "NetworkInstantiate",
                        "UiBeginDrag", "UiDrag", "UiEndDrag", "UiClick", "UiDrop", "UiPointerClick",
                        "UiPointerDown", "UiPointerEnter", "UiPointerExit", "UiPointerUp", "UiBoolValueChanged",
                        "UiFloatValueChanged", "UiIntValueChanged", "UiVector2ValueChanged", "UiEndEdit"
                    };
                    
                    foreach (string eventName in systemEvents)
                    {
                        var prop = fsmEventType.GetProperty(eventName, BindingFlags.Public | BindingFlags.Static);
                        if (prop != null)
                        {
                            systemEventFields.Add(eventName, prop);
                        }
                    }*/
                }
            }
        }

        public WeaverFsmEvent(string eventName)
        {
            if (fsmEventType != null && getFsmEventMethod != null)
            {
                FsmEventBase = getFsmEventMethod.Invoke(null, new object[] { eventName });
            }
        }

        public WeaverFsmEvent(object fsmEvent)
        {
            FsmEventBase = fsmEvent;
        }

        public string Name
        {
            get => FsmEventBase != null && nameProperty != null ? nameProperty.GetValue(FsmEventBase) as string : null;
            set
            {
                if (FsmEventBase != null && nameProperty != null)
                {
                    nameProperty.SetValue(FsmEventBase, value);
                }
            }
        }

        public bool IsSystemEvent
        {
            get => FsmEventBase != null && isSystemEventProperty != null && (bool)isSystemEventProperty.GetValue(FsmEventBase);
            set
            {
                if (FsmEventBase != null && isSystemEventProperty != null)
                {
                    isSystemEventProperty.SetValue(FsmEventBase, value);
                }
            }
        }

        public bool IsGlobal
        {
            get => FsmEventBase != null && isGlobalProperty != null && (bool)isGlobalProperty.GetValue(FsmEventBase);
            set
            {
                if (FsmEventBase != null && isGlobalProperty != null)
                {
                    isGlobalProperty.SetValue(FsmEventBase, value);
                }
            }
        }

        public string Path
        {
            get => FsmEventBase != null && pathProperty != null ? pathProperty.GetValue(FsmEventBase) as string : null;
            set
            {
                if (FsmEventBase != null && pathProperty != null)
                {
                    pathProperty.SetValue(FsmEventBase, value);
                }
            }
        }

        /// <summary>
        /// Find an event by name
        /// </summary>
        public static WeaverFsmEvent FindEvent(string eventName)
        {
            if (findEventMethod != null)
            {
                var result = findEventMethod.Invoke(null, new object[] { eventName });
                return result != null ? new WeaverFsmEvent(result) : null;
            }
            return null;
        }

        /// <summary>
        /// Get or create an event with the specified name
        /// </summary>
        public static WeaverFsmEvent GetFsmEvent(string eventName)
        {
            if (getFsmEventMethod != null)
            {
                var result = getFsmEventMethod.Invoke(null, new object[] { eventName });
                return result != null ? new WeaverFsmEvent(result) : null;
            }
            return null;
        }

        /// <summary>
        /// Get a system event by field name
        /// </summary>
        public static WeaverFsmEvent GetSystemEventField(string fieldName)
        {
            var e = fsmEventType.ReflectGetProperty(fieldName);
            if (e == null)
            {
                return null;
            }
            return new WeaverFsmEvent(e);
        }

        /// <summary>
        /// Get a system event by name
        /// </summary>
        public static WeaverFsmEvent GetSystemEvent(string eventName) => GetSystemEventField(EventNameToFieldName(eventName));

        static string EventNameToFieldName(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentException(nameof(eventName));

            // Normalise the text and split on spaces/underscores
            var parts = eventName
                .Trim()
                .Replace('_', ' ')
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder(parts.Length * 6);

            foreach (var part in parts)
            {
                // Preserve all‑numeric (e.g. “2D” ⇒ “2D”)
                if (part.Length == 0) continue;

                if (char.IsDigit(part[0]))
                {
                    sb.Append(part);
                    continue;
                }

                sb.Append(char.ToUpperInvariant(part[0]));
                if (part.Length > 1)
                    sb.Append(part.Substring(1).ToLowerInvariant());
            }

            return sb.ToString();
        }

        public static WeaverFsmEvent BecameInvisible            => GetSystemEventField(nameof(BecameInvisible));
        public static WeaverFsmEvent BecameVisible              => GetSystemEventField(nameof(BecameVisible));
        public static WeaverFsmEvent CollisionEnter             => GetSystemEventField(nameof(CollisionEnter));
        public static WeaverFsmEvent CollisionExit              => GetSystemEventField(nameof(CollisionExit));
        public static WeaverFsmEvent CollisionStay              => GetSystemEventField(nameof(CollisionStay));
        public static WeaverFsmEvent CollisionEnter2D           => GetSystemEventField(nameof(CollisionEnter2D));
        public static WeaverFsmEvent CollisionExit2D            => GetSystemEventField(nameof(CollisionExit2D));
        public static WeaverFsmEvent CollisionStay2D            => GetSystemEventField(nameof(CollisionStay2D));
        public static WeaverFsmEvent ControllerColliderHit      => GetSystemEventField(nameof(ControllerColliderHit));
        public static WeaverFsmEvent TriggerEnter               => GetSystemEventField(nameof(TriggerEnter));
        public static WeaverFsmEvent TriggerExit                => GetSystemEventField(nameof(TriggerExit));
        public static WeaverFsmEvent TriggerStay                => GetSystemEventField(nameof(TriggerStay));
        public static WeaverFsmEvent TriggerEnter2D             => GetSystemEventField(nameof(TriggerEnter2D));
        public static WeaverFsmEvent TriggerExit2D              => GetSystemEventField(nameof(TriggerExit2D));
        public static WeaverFsmEvent TriggerStay2D              => GetSystemEventField(nameof(TriggerStay2D));
        public static WeaverFsmEvent Finished                   => GetSystemEventField(nameof(Finished));
        public static WeaverFsmEvent Disable                    => GetSystemEventField(nameof(Disable));
        public static WeaverFsmEvent LevelLoaded                => GetSystemEventField(nameof(LevelLoaded));
        public static WeaverFsmEvent MouseDown                  => GetSystemEventField(nameof(MouseDown));
        public static WeaverFsmEvent MouseDrag                  => GetSystemEventField(nameof(MouseDrag));
        public static WeaverFsmEvent MouseEnter                 => GetSystemEventField(nameof(MouseEnter));
        public static WeaverFsmEvent MouseExit                  => GetSystemEventField(nameof(MouseExit));
        public static WeaverFsmEvent MouseOver                  => GetSystemEventField(nameof(MouseOver));
        public static WeaverFsmEvent MouseUp                    => GetSystemEventField(nameof(MouseUp));
        public static WeaverFsmEvent MouseUpAsButton            => GetSystemEventField(nameof(MouseUpAsButton));
        public static WeaverFsmEvent PlayerConnected            => GetSystemEventField(nameof(PlayerConnected));
        public static WeaverFsmEvent ServerInitialized          => GetSystemEventField(nameof(ServerInitialized));
        public static WeaverFsmEvent ConnectedToServer          => GetSystemEventField(nameof(ConnectedToServer));
        public static WeaverFsmEvent PlayerDisconnected         => GetSystemEventField(nameof(PlayerDisconnected));
        public static WeaverFsmEvent DisconnectedFromServer     => GetSystemEventField(nameof(DisconnectedFromServer));
        public static WeaverFsmEvent FailedToConnect            => GetSystemEventField(nameof(FailedToConnect));
        public static WeaverFsmEvent FailedToConnectToMasterServer => GetSystemEventField(nameof(FailedToConnectToMasterServer));
        public static WeaverFsmEvent MasterServerEvent          => GetSystemEventField(nameof(MasterServerEvent));
        public static WeaverFsmEvent NetworkInstantiate         => GetSystemEventField(nameof(NetworkInstantiate));
        public static WeaverFsmEvent ApplicationFocus           => GetSystemEventField(nameof(ApplicationFocus));
        public static WeaverFsmEvent ApplicationPause           => GetSystemEventField(nameof(ApplicationPause));
        public static WeaverFsmEvent ApplicationQuit           => GetSystemEventField(nameof(ApplicationQuit));
        public static WeaverFsmEvent ParticleCollision          => GetSystemEventField(nameof(ParticleCollision));
        public static WeaverFsmEvent JointBreak                 => GetSystemEventField(nameof(JointBreak));
        public static WeaverFsmEvent JointBreak2D               => GetSystemEventField(nameof(JointBreak2D));
        public static WeaverFsmEvent UiBeginDrag                => GetSystemEventField(nameof(UiBeginDrag));
        public static WeaverFsmEvent UiDrag                     => GetSystemEventField(nameof(UiDrag));
        public static WeaverFsmEvent UiEndDrag                  => GetSystemEventField(nameof(UiEndDrag));
        public static WeaverFsmEvent UiClick                    => GetSystemEventField(nameof(UiClick));
        public static WeaverFsmEvent UiDrop                     => GetSystemEventField(nameof(UiDrop));
        public static WeaverFsmEvent UiPointerClick             => GetSystemEventField(nameof(UiPointerClick));
        public static WeaverFsmEvent UiPointerDown              => GetSystemEventField(nameof(UiPointerDown));
        public static WeaverFsmEvent UiPointerEnter             => GetSystemEventField(nameof(UiPointerEnter));
        public static WeaverFsmEvent UiPointerExit              => GetSystemEventField(nameof(UiPointerExit));
        public static WeaverFsmEvent UiPointerUp                => GetSystemEventField(nameof(UiPointerUp));
        public static WeaverFsmEvent UiBoolValueChanged         => GetSystemEventField(nameof(UiBoolValueChanged));
        public static WeaverFsmEvent UiFloatValueChanged        => GetSystemEventField(nameof(UiFloatValueChanged));
        public static WeaverFsmEvent UiIntValueChanged          => GetSystemEventField(nameof(UiIntValueChanged));
        public static WeaverFsmEvent UiVector2ValueChanged      => GetSystemEventField(nameof(UiVector2ValueChanged));
        public static WeaverFsmEvent UiEndEdit                  => GetSystemEventField(nameof(UiEndEdit));
    }
}