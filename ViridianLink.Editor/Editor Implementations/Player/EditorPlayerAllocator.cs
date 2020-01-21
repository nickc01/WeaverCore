using UnityEngine;
using ViridianLink.Implementations;
using ViridianLink.Implementations.Allocators;

namespace ViridianLink.Editor.Implementations.Allocators
{
    public class EditorPlayerAllocator : PlayerAllocator
    {
        public override PlayerImplementation Allocate()
        {
            var player = GameObject.FindObjectOfType<Player>();
            if (player != null)
            {
                if (player.GetComponent<EditorPlayerImplementation>() == null)
                {
                    player.gameObject.AddComponent<EditorPlayerImplementation>();
                }
                return player.GetComponent<EditorPlayerImplementation>();
            }
            else
            {
                var gameObject = new GameObject("Player");
                var implementation = gameObject.AddComponent<EditorPlayerImplementation>();
                gameObject.AddComponent<Player>();
                return implementation;
            }
        }
    }
}
