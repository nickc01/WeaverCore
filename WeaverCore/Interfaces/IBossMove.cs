using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Interfaces
{

	/// <summary>
	/// Used for implementing moves for a boss fight
	/// </summary>
	public interface IBossMove : IEnemyMove
	{
		/// <summary>
		/// Called when the enemy gets stunned
		/// </summary>
		void OnStun();
	}
}
