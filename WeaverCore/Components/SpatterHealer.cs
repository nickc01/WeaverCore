using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Used to play a heal effect when a blue health bug is collected
    /// </summary>
    public class SpatterHealer : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("List of audio clips to play when bug is collected.")]
        System.Collections.Generic.List<AudioClip> collectSounds;

        float chooser = 0;

        float scaleModifier = 0;

        float attractSpeed = 0;

        [NonSerialized]
        Rigidbody2D _rigidbody;
        public Rigidbody2D Rigidbody2D => _rigidbody ??= GetComponent<Rigidbody2D>();

        GameObject healthGetEffect;

        Coroutine faceSquashRoutine = null;

        private void OnEnable()
        {
            healthGetEffect = transform.Find("Health Get Effect").gameObject;
            StartCoroutine(MainRoutine());
        }

        IEnumerator MainRoutine()
        {
            yield return null;
            //FALL STATE
            transform.parent = null;
            scaleModifier = UnityEngine.Random.Range(1.3f, 1.6f);

            var waitTime = UnityEngine.Random.Range(0.25f, 0.45f);

            faceSquashRoutine = StartCoroutine(FaceAngleAndSquash(float.PositiveInfinity, 0f, 1.4f, new Vector2(0.6f, 1.75f)));

            yield return new WaitForSeconds(waitTime);


            Rigidbody2D.gravityScale = 0f;

            yield return DecelerationRoutine(0.75f);

            //ATTRACT STATE
            yield return AttractRoutine();

            //GET STATE
            var selectedSound = collectSounds.GetRandomElement();
            var soundInstance = WeaverAudio.PlayAtPoint(selectedSound, transform.position);
            soundInstance.AudioSource.pitch = UnityEngine.Random.Range(0.9f,1.1f);

            Player.Player1.SendMessage("flashHealBlue");

            chooser = UnityEngine.Random.Range(0f, 360f);

            healthGetEffect.SetActive(true);

            healthGetEffect.transform.rotation = Quaternion.Euler(0f, 0f, chooser);

            chooser = UnityEngine.Random.Range(2f, 2.5f);

            healthGetEffect.transform.localScale = new Vector3(chooser,chooser,1f);

            healthGetEffect.transform.parent = null;

            gameObject.SetActive(false);

            yield break;
        }

        IEnumerator FaceAngleAndSquash(float duration, float angleOffset, float stretchFactor, Vector2 stretchMinMax)
        {
            for (float f = 0; f < duration; f += Time.deltaTime)
            {
                Vector2 velocity = Rigidbody2D.velocity;
                float z = Mathf.Atan2(velocity.y, velocity.x) * (180f / (float)Math.PI) + angleOffset;
                transform.localEulerAngles = new Vector3(0f, 0f, z);

                Squash(stretchFactor, stretchMinMax.x, stretchMinMax.y);
                yield return null;
            }
        }

        void Squash(float stretchFactor, float stretchMinX, float stretchMaxY)
        {
            var stretchY = 1f - Rigidbody2D.velocity.magnitude * stretchFactor * 0.01f;
            var stretchX = 1f + Rigidbody2D.velocity.magnitude * stretchFactor * 0.01f;
            if (stretchX < stretchMinX)
            {
                stretchX = stretchMinX;
            }
            if (stretchY > stretchMaxY)
            {
                stretchY = stretchMaxY;
            }
            stretchY *= scaleModifier;
            stretchX *= scaleModifier;
            transform.localScale = new Vector3(stretchX, stretchY, transform.localScale.z);
        }

        IEnumerator DecelerationRoutine(float deceleration)
        {
            while (Rigidbody2D.velocity.magnitude > 0.3f)
            {
                DecelerateSelf(deceleration);
            }
            yield break;
        }

        private void DecelerateSelf(float deceleration)
        {
            Vector2 velocity = Rigidbody2D.velocity;
            if (velocity.x < 0f)
            {
                velocity.x *= deceleration;
                if (velocity.x > 0f)
                {
                    velocity.x = 0f;
                }
            }
            else if (velocity.x > 0f)
            {
                velocity.x *= deceleration;
                if (velocity.x < 0f)
                {
                    velocity.x = 0f;
                }
            }
            if (velocity.y < 0f)
            {
                velocity.y *= deceleration;
                if (velocity.y > 0f)
                {
                    velocity.y = 0f;
                }
            }
            else if (velocity.y > 0f)
            {
                velocity.y *= deceleration;
                if (velocity.y < 0f)
                {
                    velocity.y = 0f;
                }
            }
            Rigidbody2D.velocity = velocity;
        }

        IEnumerator AttractRoutine()
        {
            while (true)
            {
                DoSetVelocity(attractSpeed);

                var distance = Vector3.Distance(transform.position, Player.Player1.transform.position);

                if (distance <= 0.6f)
                {
                    break;
                }
                else
                {
                    attractSpeed += (0.85f / Time.fixedDeltaTime) * Time.deltaTime;

                    attractSpeed = Mathf.Clamp(attractSpeed, 0f, 30f);
                }
                yield return null;

            }
            yield break;
        }

        private void DoSetVelocity(float speed)
        {
            if (!(Rigidbody2D == null))
            {
                float num = Player.Player1.transform.position.y + (-0.5f) - transform.position.y;
                float num2 = Player.Player1.transform.position.x + (0f) - transform.position.x;
                float num3 = Mathf.Atan2(num, num2) * (180f / (float)Math.PI);
                var x = speed * Mathf.Cos(num3 * ((float)Math.PI / 180f));
                var y = speed * Mathf.Sin(num3 * ((float)Math.PI / 180f));
                Vector2 velocity = default(Vector2);
                velocity.x = x;
                velocity.y = y;
                Rigidbody2D.velocity = velocity;
            }
        }
    }
}