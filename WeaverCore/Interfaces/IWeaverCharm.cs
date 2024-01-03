using UnityEngine;

namespace WeaverCore.Interfaces
{
    /// <summary>
    /// Interface used for implementing custom charms.
    /// </summary>
    public interface IWeaverCharm
    {
        /// <summary>
        /// The name of the charm.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The description of the charm.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The notch cost of the charm.
        /// </summary>
        int NotchCost { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the charm has been acquired.
        /// </summary>
        bool Acquired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the charm is equipped.
        /// </summary>
        bool Equipped { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the charm has been newly collected.
        /// </summary>
        bool NewlyCollected { get; set; }

        /// <summary>
        /// The sprite of the charm.
        /// </summary>
        Sprite CharmSprite { get; }
    }

}
