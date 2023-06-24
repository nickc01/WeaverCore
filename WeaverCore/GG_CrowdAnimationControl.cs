using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;

namespace WeaverCore.Assets.Components
{


    public class GG_CrowdAnimationControl : MonoBehaviour
    {
        Sprite currentSprite;
        WeaverAnimationPlayer animator;

        private void Awake()
        {
            animator = GetComponent<WeaverAnimationPlayer>();
            currentSprite = animator.SpriteRenderer.sprite;
        }


        void LateUpdate()
        {
            if (currentSprite != animator.SpriteRenderer.sprite)
            {
                currentSprite = animator.SpriteRenderer.sprite;

                var playingFrame = animator.PlayingFrame;

                animator.SpriteRenderer.flipY = playingFrame == 0 || playingFrame == 2;
                animator.SpriteRenderer.flipX = playingFrame == 3 || playingFrame == 4;

            }
        }
    }
}
