using System;
using System.Reflection;

namespace WeaverCore.Editor.Settings
{
    public class CustomPropertyAccessor<T> : CustomPropertyAccessor_Base
    {
        public readonly Func<T> Getter;
        public readonly Action<T> Setter;

        public CustomPropertyAccessor(Panel panel, Func<T> getter, Action<T> setter, string displayName, string description, MemberInfo memberInfo = null) : base(panel, displayName, description, memberInfo)
        {
            Getter = getter;
            Setter = setter;
        }

        public override Type MemberType
        {
            get
            {
                return typeof(T);
            }
        }

        public override object FieldValue
        {
            get
            {
                return Getter();
            }
            set
            {
                Setter((T)value);
            }
        }
    }

    public class CustomPropertyAccessor : CustomPropertyAccessor_Base
    {
        public readonly Func<object> Getter;
        public readonly Action<object> Setter;

        public CustomPropertyAccessor(Type memberType, Panel panel, Func<object> getter, Action<object> setter, string displayName, string description, MemberInfo memberInfo = null) : base(panel, displayName, description, memberInfo)
        {
            _memberType = memberType;
            Getter = getter;
            Setter = setter;
        }

        Type _memberType;
        public override Type MemberType
        {
            get => _memberType;
        }

        public override object FieldValue
        {
            get
            {
                return Getter();
            }
            set
            {
                Setter(value);
            }
        }
    }
}
