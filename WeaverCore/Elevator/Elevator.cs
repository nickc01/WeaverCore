using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Elevator
{

    public class Elevator : MonoBehaviour
    {
        static int playerLayerID = -1;

        Coroutine movementRoutine;

        [field: SerializeField]
        public float Speed { get; set; } = 5f;

        [field: SerializeField]
        [field: Tooltip("The default position of the elevator when it's at the top")]
        public Vector3 DefaultTopPosition { get; set; }

        [field: SerializeField]
        [field: Tooltip("The default position of the elevator when it's at the bottom")]
        public Vector3 DefaultBottomPosition { get; set; }

        [field: SerializeField]
        [field: Tooltip("If the player is above The Default Top Position of the elevator, the elevator will automatically move to the top. If the player is below the Default Top Position, the elevator will automatically move to the bottom")]
        public bool AutoMove { get; protected set; } = true;

        [field: Tooltip("Does the elevator do a little bob motion when it's about to move?")]
        public bool DoBob { get; set; } = true;

        [field: Tooltip("The extra delay before the elevator starts moving")]
        public float BeginDelay { get; set; } = 0.5f;

        [field: Tooltip("The extra delay after the elevator stops moving")]
        public float EndDelay { get; set; } = 0.4f;

        public bool Moving => movementRoutine != null;

        public bool Ready { get; private set; } = false;

        [field: Space]
        [field: Header("Audio")]
        [field: SerializeField]
        public AudioClip LiftActivateSound { get; protected set; }

        [field: SerializeField]
        public Vector2 LiftActivateSoundPitchRange = new Vector2(0.9f, 1.1f);

        [field: SerializeField]
        public AudioClip LiftLoopSound { get; protected set; }

        [field: SerializeField]
        public AudioClip LiftFinishSound { get; protected set; }

        [field: SerializeField]
        public Vector2 LiftFinishSoundPitchRange = new Vector2(0.9f, 1.1f);

        /// <summary>
        /// If Moving == true, then this contains the destination the elevator is current moving to
        /// </summary>
        public Vector3 MovingDestination { get; private set; }

        AudioPlayer loopSoundInstance;

        protected virtual void Reset()
        {
            DefaultBottomPosition = transform.position - new Vector3(0f, 5f, 0f);
            DefaultTopPosition = transform.position + new Vector3(0f, 5f, 0f);

            LiftActivateSound = WeaverAssets.LoadWeaverAsset<AudioClip>("lift_activate");
            LiftLoopSound = WeaverAssets.LoadWeaverAsset<AudioClip>("lift_moving_loop");
            LiftFinishSound = WeaverAssets.LoadWeaverAsset<AudioClip>("lift_arrive");
        }

        protected virtual void OnValidate()
        {

        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(DefaultBottomPosition, DefaultTopPosition);
        }

        protected virtual void Awake()
        {
            if (HeroController.instance.isHeroInPosition)
            {
                InPosition();
            }
            else
            {
                HeroController.HeroInPosition whenInPosition = null;

                whenInPosition = (forceDirect) =>
                {
                    HeroController.instance.heroInPosition -= whenInPosition;
                    InPosition();
                };

                HeroController.instance.heroInPosition += whenInPosition;
            }
        }

        protected virtual void InPosition()
        {
            transform.position = GetStartPosition();
            Ready = true;
            //cogs = GetComponentsInChildren<ElevatorRotatable>();
        }

        private void Update()
        {
            if (AutoMove && !Moving && Ready)
            {
                if (Player.Player1.transform.position.y >= DefaultTopPosition.y)
                {
                    CallElevatorUp();
                }
                else
                {
                    CallElevatorDown();
                }
            }
        }

        protected virtual void OnPlayerTouch()
        {
            if (Moving)
            {
                return;
            }

            CallElevatorToPosition(GetOppositeDestination());
        }

        protected virtual void OnPlayerUntouch()
        {

        }

        public Vector3 GetOppositeDestination()
        {
            if (Moving)
            {
                var midPoint = Vector3.Lerp(DefaultBottomPosition, DefaultTopPosition, 0.5f);
                return MovingDestination.y >= midPoint.y ? DefaultBottomPosition : DefaultTopPosition;
            }
            else
            {
                var midPoint = Vector3.Lerp(DefaultBottomPosition, DefaultTopPosition, 0.5f);
                return transform.position.y >= midPoint.y ? DefaultBottomPosition : DefaultTopPosition;
            }
        }

        protected virtual Vector3 GetStartPosition()
        {
            if (AutoMove)
            {
                if (Player.Player1.transform.position.y >= DefaultTopPosition.y)
                {
                    //Start At Top
                    return DefaultTopPosition;
                }
                else
                {
                    //Start at Bottom

                    return DefaultBottomPosition;
                }
            }
            else
            {
                return transform.position;
            }
        }

        protected virtual float GetCogRotationIntensity(Vector3 source, Vector3 destination)
        {
            if (destination.y >= source.y)
            {
                return 1f;
            }
            else
            {
                return -1;
            }
        }

        public void CallElevatorUp()
        {
            CallElevatorToPosition(DefaultTopPosition, BeginDelay, DoBob);
        }

        public void CallElevatorUp(float delay, bool doBob)
        {
            CallElevatorToPosition(DefaultTopPosition, delay, doBob);
        }

        public void CallElevatorDown()
        {
            CallElevatorToPosition(DefaultBottomPosition, BeginDelay, DoBob);
        }

        public void CallElevatorDown(float delay, bool doBob)
        {
            CallElevatorToPosition(DefaultBottomPosition, delay, doBob);
        }

        public void CallElevatorToPosition(Vector3 destination)
        {
            CallElevatorToPosition(destination, BeginDelay, DoBob);
        }

        public void CallElevatorToPosition(Vector3 destination, float delay, bool doBob)
        {
            if (Moving && MovingDestination == destination)
            {
                return;
            }
            else if (!Moving && transform.position == destination)
            {
                return;
            }

            MovingDestination = destination;
            if (movementRoutine != null)
            {
                if (loopSoundInstance != null)
                {
                    loopSoundInstance.StopPlaying();
                    loopSoundInstance = null;
                }
                StopCoroutine(movementRoutine);
            }

            StopAllCoroutines();
            movementRoutine = StartCoroutine(ElevatorMovementRoutine(delay, doBob));
        }

        protected IEnumerator BobRoutine(Vector3 direction)
        {
            List<ElevatorBobable> bobObjects = new List<ElevatorBobable>();
            GetComponentsInChildren(bobObjects);

            IEnumerator RunBobber(ElevatorBobable bobable)
            {
                yield return bobable.OnBob(this, direction);
                bobObjects.Remove(bobable);
            }

            for (int i = 0; i < bobObjects.Count; i++)
            {
                StartCoroutine(RunBobber(bobObjects[i]));
            }

            yield return new WaitUntil(() => bobObjects.Count == 0);
            yield break;
        }

        protected virtual IEnumerator ElevatorMovementRoutine(float delay, bool doBob)
        {
            if (LiftActivateSound != null)
            {
                var instance = WeaverAudio.PlayAtPoint(LiftActivateSound, transform.position);
                instance.AudioSource.pitch = LiftActivateSoundPitchRange.RandomInRange();
            }

            var movementDirection = (MovingDestination - transform.position).normalized;

            if (doBob)
            {
                yield return BobRoutine(-movementDirection);
            }

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (LiftLoopSound != null)
            {
                loopSoundInstance = WeaverAudio.PlayAtPointLooped(LiftLoopSound, transform.position);
            }

            EventManager.BroadcastEvent("MOVING UP", gameObject);

            OnMovementStart();

            var start = transform.position;
            var originalLengthToEnd = Vector3.Distance(start, MovingDestination);

            var rotatables = GetComponentsInChildren<ElevatorRotatable>();

            while (true)
            {
                var direction = (MovingDestination - transform.position).normalized;
                transform.position += direction * Speed * Time.deltaTime;

                for (int i = 0; i < rotatables.Length; i++)
                {
                    rotatables[i].Rotate(this, movementDirection, Speed);
                }

                if (Vector3.Distance(start, transform.position) >= originalLengthToEnd)
                {
                    transform.position = MovingDestination;
                    break;
                }

                yield return null;
            }

            if (loopSoundInstance != null)
            {
                loopSoundInstance.StopPlaying();
                loopSoundInstance = null;
            }

            if (LiftFinishSound != null)
            {
                var instance = WeaverAudio.PlayAtPoint(LiftFinishSound, transform.position);
                instance.AudioSource.pitch = LiftFinishSoundPitchRange.RandomInRange();
            }

            EventManager.BroadcastEvent("STOP MOVING", gameObject);
            OnMovementEnd();

            if (doBob)
            {
                yield return BobRoutine(movementDirection);
            }

            if (EndDelay > 0f)
            {
                yield return new WaitForSeconds(EndDelay);
            }

            yield return 

            movementRoutine = null;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            WeaverLog.Log("OnCollisionEnter2D");
            if (!Ready)
            {
                return;
            }
            if (playerLayerID < 0)
            {
                playerLayerID = LayerMask.NameToLayer("Player");
            }

            if (collision.gameObject.layer == playerLayerID)
            {
                WeaverLog.Log("ON PLAYER TOUCH");
                OnPlayerTouch();
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!Ready)
            {
                return;
            }

            if (playerLayerID < 0)
            {
                playerLayerID = LayerMask.NameToLayer("Player");
            }

            if (collision.gameObject.layer == playerLayerID)
            {
                OnPlayerUntouch();
            }
        }

        protected virtual void OnMovementStart()
        {

        }

        protected virtual void OnMovementEnd()
        {

        }
    }
}