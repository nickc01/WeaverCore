using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverCore.Interfaces;
using WeaverCore.Settings;

namespace WeaverCore.Implementations
{
	public abstract class SaveSpecificSettings_I : IImplementation
	{
		public abstract int CurrentSaveSlot { get; }
		public abstract void SaveSettings(SaveSpecificSettings settings);
		public abstract void LoadSettings(SaveSpecificSettings settings);
	}
}
