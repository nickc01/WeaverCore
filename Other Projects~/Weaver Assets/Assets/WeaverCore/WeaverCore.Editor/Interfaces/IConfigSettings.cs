using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Editor.Interfaces
{
	public interface IConfigSettings
	{
		/// <summary>
		/// Gets the settings info stored in a file
		/// </summary>
		void GetStoredSettings();

		/// <summary>
		/// Sets the settings info stored in a file
		/// </summary>
		void SetStoredSettings();
	}
}
