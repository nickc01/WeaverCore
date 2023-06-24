using System.Reflection;

namespace WeaverCore.Settings
{
    public static class SaveSpecificSettings_Extensions
	{
        public static bool HasField(this SaveSpecificSettings settings, string fieldName, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            if (settings == null)
            {
                return false;
            }

            var field = settings.GetType().GetField(fieldName, flags);

            if (field == null)
            {
                return false;
            }
            return true;
        }

        public static bool HasField<T>(this SaveSpecificSettings settings, string fieldName, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            if (settings == null)
            {
                return false;
            }

            var field = settings.GetType().GetField(fieldName, flags);

            if (field == null)
            {
                return false;
            }

            if (!typeof(T).IsAssignableFrom(field.FieldType))
            {
                return false;
            }

            return true;
        }

        public static T GetFieldValue<T>(this SaveSpecificSettings settings, string fieldName, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            return (T)GetField<T>(settings, fieldName, flags).GetValue(settings);
        }

        public static void SetFieldValue<T>(this SaveSpecificSettings settings, string fieldName, T newValue, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            GetField<T>(settings, fieldName, flags).SetValue(settings, newValue);
        }

        public static bool TryGetFieldValue<T>(this SaveSpecificSettings settings, string fieldName, out T result, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            if (HasField<T>(settings,fieldName, flags))
            {
                result = GetFieldValue<T>(settings, fieldName, flags);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        static FieldInfo GetField<T>(SaveSpecificSettings instance, string fieldName, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            var field = instance.GetType().GetField(fieldName, flags);

            if (field == null)
            {
                throw new System.Exception($"The field {fieldName} does not exist on {instance.GetType().FullName}");
            }

            if (!typeof(T).IsAssignableFrom(field.FieldType))
            {
                throw new System.Exception($"The field {fieldName} on {instance.GetType().FullName} is not of type {typeof(T).Name}");
            }

            return field;
        }
    }
}
