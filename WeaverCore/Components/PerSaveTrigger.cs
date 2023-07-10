using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using WeaverCore.Settings;

namespace WeaverCore.Components
{
    /// <summary>
    /// Causes a collision trigger event only once per save file
    /// </summary>
    public class PerSaveTrigger : MonoBehaviour
    {
        [SerializeField]
        SaveSpecificSettings settingsStorage;

        [SerializeField]
        string saveFieldName = "";

        [SerializeField]
        UnityEvent onTrigger;

        [NonSerialized]
        bool activated = false;

        [NonSerialized]
        FieldInfo field;

        void OnActivated()
        {
            if (!activated)
            {
                activated = true;
                field.SetValue(settingsStorage, true);
                onTrigger?.Invoke();
            }
        }

        public void ResetTrigger()
        {
            activated = true;
            if (field != null)
            {
                field.SetValue(settingsStorage, false);
            }
        }

        private void Awake()
        {
            if (settingsStorage == null)
            {
                throw new Exception($"No SaveSpecificSettings object specified on gameObject {gameObject.name}");
            }

            field = settingsStorage.GetType().GetField(saveFieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            if (field == null)
            {
                throw new Exception($"No field by the name {saveFieldName} was found on type {settingsStorage.GetType().FullName}");
            }

            if (field.FieldType != typeof(bool)) {
                field = null;
                throw new Exception($"The {saveFieldName} field on type {settingsStorage.GetType().FullName} must be a bool");
            }

            activated = (bool)field.GetValue(settingsStorage);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            OnActivated();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            OnActivated();
        }
    }
}
