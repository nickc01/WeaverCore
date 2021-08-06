using System.Collections;

namespace WeaverCore.Interfaces
{
	/// <summary>
	/// Used for implementing moves for an enemy
	/// </summary>
	public interface IEnemyMove
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
		/// Called when the boss dies
		/// </summary>
		void OnDeath();


		/// <summary>
		/// Called when this move gets cancelled before it finishes
		/// </summary>
		void OnCancel();
	}
}
