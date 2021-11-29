using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WeaverCore.Editor.Settings
{

    public class FieldAccessor : IAccessor
    {
        public readonly Panel Panel;
        public readonly FieldInfo Field;
        readonly string _description;
        readonly string _displayName;

        public FieldAccessor(Panel panel, FieldInfo field, string displayName, string description)
        {
            Panel = panel;
            Field = field;
            _description = description;
            _displayName = displayName;
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

        public object FieldValue
        {
            get
            {
                return Field.GetValue(Panel);
            }
            set
            {
                Field.SetValue(Panel, value);
            }
        }

        public MemberInfo MemberInfo
        {
            get
            {
                return Field;
            }
        }

        public Type MemberType
        {
            get
            {
                return Field.FieldType;
            }
        }
    }
}
