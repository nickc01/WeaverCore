using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
    public abstract class DreamnailUtilities_I : IImplementation
	{
		public abstract void DisplayEnemyDreamMessage(int convoAmount, string convoTitle, string sheetTitle);
		public abstract void DisplayRegularDreamMessage(string convoTitle, string sheetTitle);
		public abstract void CancelRegularDreamnailMessage();

		public abstract void PlayDreamnailEffects(Vector3 position);
    }
}
