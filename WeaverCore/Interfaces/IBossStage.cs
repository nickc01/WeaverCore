using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Interfaces
{
	public interface IBossMove
	{
		/// <summary>
		/// Whether the move is enabled. If it's not enabled, then it won't be selected by the move randomizer
		/// </summary>
		bool MoveEnabled { get; }

		/// <summary>
		/// Used to execute the move
		/// </summary>
		/// <returns></returns>
		IEnumerator DoMove();

		/// <summary>
		/// Called when the enemy gets stunned
		/// </summary>
		void OnStun();

		/// <summary>
		/// Called when this move gets cancelled before it finishes
		/// </summary>
		void OnCancel();

		/// <summary>
		/// Called when the boss dies
		/// </summary>
		void OnDeath();
	}
}
