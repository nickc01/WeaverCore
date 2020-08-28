using System;

namespace WeaverCore.Assets.Components
{
	public class IntProperty : A_InputFieldProperty
	{
		public override Type BindingFieldType
		{
			get
			{
				return typeof(int);
			}
		}

		protected override void UpdateField()
		{
			int value;
			if (int.TryParse(input.text, out value))
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
