using System;
using System.Reflection;

namespace WeaverCore.Editor.Settings
{
    public abstract class CustomPropertyAccessor_Base : IAccessor
    {
        public readonly Panel Panel;
        protected readonly string _description;
        protected readonly string _displayName;
        readonly MemberInfo _memberInfo;

        public CustomPropertyAccessor_Base(Panel panel, string displayName, string description, MemberInfo memberInfo = null)
        {
            Panel = panel;
            _description = description;
            _displayName = displayName;
            _memberInfo = memberInfo;
        }

        public string FieldName
        {
            get
            {
                return _displayName;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }

        public MemberInfo MemberInfo
        {
            get
            {
                return _memberInfo;
            }
        }

        public abstract Type MemberType { get; }

        public abstract object FieldValue { get; set; }
    }
}
