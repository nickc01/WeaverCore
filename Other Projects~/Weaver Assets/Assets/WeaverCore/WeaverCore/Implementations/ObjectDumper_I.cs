using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class ObjectDumper_I : IImplementation
	{
		public abstract void Dump(GameObject obj);
	}
}
