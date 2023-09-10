using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WeaverCore.Attributes;
using WeaverCore.Utilities;
using static UnityEngine.GraphicsBuffer;

namespace WeaverCore.Components
{
    public class SpatterHealer : MonoBehaviour
    {
        [SerializeField]
        List<AudioClip> collectSounds;

        //[SerializeField]
        //float acceleration = 0.1f;

        //[SerializeField]
        float chooser = 0;

        //[SerializeField]
        //float distance = 0;

        float scaleModifier = 0;


        //[SerializeField]
        //float speed = 0;

        float attractSpeed = 0;

        [NonSerialized]
        Rigidbody2D _rigidbody;
        public Rigidbody2D Rigidbody2D => _rigidbody ??= GetComponent<Rigidbody2D>();

        GameObject healthGetEffect;

        /*[OnInit]
        static void Init()
        {
            //1.3
            var minFloat = BitConverter.ToSingle(new byte[] { 102, 102, 166, 63, 0 }, 0);

            //1.6
            var maxFloat = BitConverter.ToSingle(new byte[] { 205, 204, 204, 63, 0 }, 0);

            //0.25
            var waitTimeMin = BitConverter.ToSingle(new byte[] { 0, 0, 128, 62, 0 }, 0);

            //0.45
            var waitTimeMax = BitConverter.ToSingle(new byte[] { 102, 102, 230, 62, 0 }, 0);

            //1.4
            var stretchFactor = BitConverter.ToSingle(new byte[] { 51, 51, 179, 63, 0 }, 0);

            //0.6
            var stretchMinX = BitConverter.ToSingle(new byte[] { 154, 153, 25, 63 }, 0);

            //1.75
            var stretchMaxY = BitConverter.ToSingle(new byte[] { 0, 0, 224, 63 }, 0);

            //0.75
            var deceleration = BitConverter.ToSingle(new byte[] { 0,0,64, 63}, 0);

            //0.3
            var float2 = BitConverter.ToSingle(new byte[] { 154, 153, 153, 62 }, 0);

            //-0.5
            var positionY = BitConverter.ToSingle(new byte[] { 0, 0, 0, 191 }, 0);

            //var varTest = new string(new char[] { (char)68, (char)105, (char)115, (char)116, (char)97, (char)110 });

            //0.3
            var distanceFloat2 = BitConverter.ToSingle(new byte[] { 154, 153, 153, 62 }, 0);

            //0.6
            var tolerance = BitConverter.ToSingle(new byte[] { 154, 153, 25, 63 }, 0);

            //0.85
            var attractIncrement = BitConverter.ToSingle(new byte[] { 154, 153, 89, 63 }, 0);

            //30
            var maxClamp = BitConverter.ToSingle(new byte[] { 0,0,240,65 }, 0);

            //1
            var weight = BitConverter.ToSingle(new byte[] { 0,0,128,63 }, 0);

            var pitchMin = BitConverter.ToSingle(new byte[] { 102,102,102,63 }, 0);
            var pitchMax = BitConverter.ToSingle(new byte[] { 205,204,140,63 }, 0);

            var maxRandomVal = BitConverter.ToSingle(new byte[] { 0, 0, 180, 67 }, 0);

            //2
            var chooserMin = BitConverter.ToSingle(new byte[] { 0, 0, 0, 64 }, 0);

            //2.5
            var chooserMax = BitConverter.ToSingle(new byte[] { 0, 0, 32, 64 }, 0);



            //WeaverLog.Log($"{nameof(positionY)} = {positionY}");
            //WeaverLog.Log($"{nameof(distanceFloat2)} = {distanceFloat2}");
            WeaverLog.Log($"{nameof(chooserMin)} = {chooserMin}");
            WeaverLog.Log($"{nameof(chooserMax)} = {chooserMax}");
        }

        //69 Var = Distance aka element 19
        //83 Var = Distance aka element 21*/


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

            var healthGetEffectChild = transform.Find("Health Get Effect");

            var waitTime = UnityEngine.Random.Range(0.25f, 0.45f);

            faceSquashRoutine = StartCoroutine(FaceAngleAndSquash(float.PositiveInfinity, 0f, 1.4f, new Vector2(0.6f, 1.75f)));

            yield return new WaitForSeconds(waitTime);


            //DECEL STATE
            Rigidbody2D.gravityScale = 0f;
            /*faceSquashRoutine = StartCoroutine(FaceAngleAndSquash(waitTime, 0f, 1.4f, new Vector2(0.6f, 1.75f)));*/

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

            //healthGetEffect.transform.localScale




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