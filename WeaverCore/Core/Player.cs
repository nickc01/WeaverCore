using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;
using WeaverCore.WeaverAssets;

namespace WeaverCore
{
    public class Player : MonoBehaviour
    {
        static List<Player> Players = new List<Player>();

        public static IEnumerable<Player> AllPlayers => Players;

        public static Player Player1 => Players.Count > 0 ? Players[0] : null;

        public static Player NearestPlayer(Vector3 position)
        {
            float shortestDistance = Mathf.Infinity;
            Player nearestPlayer = null;

            foreach (var player in Players)
            {
                var distance = Vector3.Distance(player.transform.position, position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestPlayer = player;
                }
            }
            return nearestPlayer;
        }

        public static Player NearestPlayer(Component component) => NearestPlayer(component.transform.position);
        public static Player NearestPlayer(Transform transform) => NearestPlayer(transform.position);
        public static Player NearestPlayer(GameObject gameObject) => NearestPlayer(gameObject.transform.position);

        /// <summary>
        /// Takes a child object, and will return the Player Object that this object is a child of. If the object is not a child object of a player, then this will return null
        /// </summary>
        /// <param name="childObject"></param>
        /// <returns></returns>
        public static Player GetPlayerFromChild(GameObject childObject)
        {
            var playerLayer = LayerMask.NameToLayer("Player");
            var parent = childObject.transform;

            while (parent != null)
            {
                if ((parent.gameObject.layer | playerLayer) == playerLayer)
                {
                    var playerComponent = parent.gameObject.GetComponent<Player>();
                    if (playerComponent != null)
                    {
                        return playerComponent;
                    }
                }
                parent = parent.parent;
            }
            return null;
        }


        public virtual void PlayAttackSlash(GameObject target, HitInfo hit, Vector3 effectsOffset = default)
        {
            PlayAttackSlash((transform.position + target.transform.position) * 0.5f + effectsOffset,hit);
        }

        public virtual void PlayAttackSlash(Vector3 target, HitInfo hit)
        {
            Instantiate(EffectAssets.NailStrikePrefab, target, Quaternion.identity);
            var slashImpact = Instantiate(EffectAssets.SlashImpactPrefab, target, Quaternion.identity);

            var attackDirection = DirectionUtilities.DegreesToDirection(hit.Direction);

            switch (attackDirection)
            {
                case CardinalDirection.Up:
                    SetRotation2D(slashImpact.transform, UnityEngine.Random.Range(70, 110));
                    slashImpact.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                    break;
                case CardinalDirection.Down:
                    SetRotation2D(slashImpact.transform, UnityEngine.Random.Range(70, 110));
                    slashImpact.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                    break;
                case CardinalDirection.Left:
                    SetRotation2D(slashImpact.transform, UnityEngine.Random.Range(340, 380));
                    slashImpact.transform.localScale = new Vector3(-1.5f, 1.5f, 1f);
                    break;
                case CardinalDirection.Right:
                    SetRotation2D(slashImpact.transform, UnityEngine.Random.Range(340, 380));
                    slashImpact.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                    break;
                default:
                    break;
            }
            //OTHER ATTACK TYPES : TODO
        }

        void SetRotation2D(Transform t, float rotation)
        {
            Vector3 eulerAngles = t.eulerAngles;
            eulerAngles.z = rotation;
            t.eulerAngles = eulerAngles;
        }



        PlayerImplementation playerImpl;

        void Start()
        {
            var playerImplType = ImplFinder.GetImplementationType<PlayerImplementation>();

            playerImpl = (PlayerImplementation)gameObject.AddComponent(playerImplType);
            Players.AddConditional(this);
            playerImpl.Initialize();
        }

        void OnEnable()
        {
            Players.AddConditional(this);
        }

        void OnDisable()
        {
            Players.Remove(this);
        }

        void OnDestroy()
        {
            Players.Remove(this);
        }
    }
}
