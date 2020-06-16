﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class URoutine_I : IImplementation
	{
		public abstract URoutineData Start(IEnumerator<IUAwaiter> function);

		public abstract void Stop(URoutineData routine);

		public abstract float DT { get; }

		public abstract class URoutineData
		{

		}
	}
}