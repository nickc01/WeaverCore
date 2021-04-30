/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Assets.Components
{
	public class FloatProperty : A_InputFieldProperty
	{
		public override Type BindingFieldType
		{
			get
			{
				return typeof(float);
			}
		}

		protected override void UpdateField()
		{
			float value;
			if (float.TryParse(input.text,out value))
			{
				base.UpdateField();
			}
			else
			{
				input.text = previousInput;
			}
		}
	}
}
*/