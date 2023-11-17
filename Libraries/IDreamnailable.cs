namespace WeaverCore.Interfaces
{
    /// <summary>
    /// Interface used for receiving dreamnail events
    /// </summary>
    public interface IDreamnailable
	{
        /// <summary>
        /// Called when the player dreamnails this object. Returns how much soul to heal the player, or zero to not give any soul
        /// </summary>
        /// <param name="player"> the player that dreamnailed this object</param>
        /// <returns> Returns how much soul to heal the player, or zero to not give any soul</returns>
        int DreamnailHit(Player player);
	}
}
