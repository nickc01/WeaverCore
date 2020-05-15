using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Awaiters;
using WeaverCore.Editor.Helpers;
using WeaverCore.Helpers;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class EditorPropertyManagerImplementation : PropertyManagerImplementation
	{
		List<IPropertyTableBase> Tables = new List<IPropertyTableBase>();

		URoutine routine = null;

		int index = 0;

		public override void AddTable(IPropertyTableBase table)
		{
			Tables.Add(table);
		}

		public override void End()
		{
			if (routine != null)
			{
				routine.Stop();
				routine = null;
			}
		}

		public override void RemoveTable(IPropertyTableBase table)
		{
			Tables.Remove(table);
		}

		public override void Start()
		{
			if (routine == null)
			{
				routine = URoutine.Start(Update());
			}
		}


		IEnumerator<IUAwaiter> Update()
		{
			while (true)
			{
				index++;
				if (index >= Tables.Count)
				{
					index = 0;
				}
				if (Tables.Count > 0)
				{
					CleanTable(Tables[index]);
				}
				yield return new WaitForSeconds(10.0f / (Tables.Count + 1));
			}
		}
	}
}
