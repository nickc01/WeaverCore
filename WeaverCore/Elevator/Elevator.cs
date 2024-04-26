using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using WeaverCore.Components;
using WeaverCore.Utilities;

namespace WeaverCore.Elevator
{
    public class Elevator : MonoBehaviour
    {
        public const float INITIAL_DELAY_TIME = 0.25f;

        [Serializable]
        public struct ElevatorInfo
        {
            [NonSerialized]
            public Func<float> GetSpeed;
            public bool DoBob;
            public float BeginDelay;
            public float EndDelay;
            public bool DoActivateSound;
            public bool DoLoopSound;
            public bool DoFinishSound;

            public ElevatorInfo(ElevatorInfo source)
            {
                GetSpeed = source.GetSpeed;
                DoBob = source.DoBob;
                BeginDelay = source.BeginDelay;
                EndDelay = source.EndDelay;
                DoActivateSound = source.DoActivateSound;
                DoLoopSound = source.DoLoopSound;
                DoFinishSound = source.DoFinishSound;
            }

            public ElevatorInfo(Elevator instance)
            {
                GetSpeed = () => instance.Speed;
                DoBob = instance.DoBob;
                BeginDelay = instance.BeginDelay;
                EndDelay = instance.EndDelay;
                DoActivateSound = instance.LiftActivateSound != null;
                DoLoopSound = instance.LiftLoopSound != null;
                DoFinishSound = instance.LiftFinishSound != null;
            }

            public ElevatorInfo(Func<float> getSpeed, bool doBob, float beginDelay, float endDelay, bool doLiftSound, bool doLoopSound, bool doFinishSound)
            {
                GetSpeed = getSpeed;
                DoBob = doBob;
                BeginDelay = beginDelay;
                EndDelay = endDelay;
                DoActivateSound = doLiftSound;
                DoLoopSound = doLoopSound;
                DoFinishSound = doFinishSound;
            }
        }

        static int playerLayerID = -1;

        Coroutine movementRoutine;
        Coroutine playerLockRoutine;

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
        public bool AutoMove { get; set; } = true;

        [field: SerializeField]
        [field: Tooltip("If true, the elevator will automatically move when the player touches it")]
        public bool MoveOnContact { get; set; } = true;

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

        [field: SerializeField]
        [field: Tooltip("Used to control the volume of the sounds depending on how close the player is. The x serves as the distance where the sounds are loudest, and the y serves as the distance where the sounds are mute")]
        public Vector2 AudioRange = new Vector2(7.5f, 25f);

        /// <summary>
        /// If Moving == true, then this contains the destination the elevator is current moving to
        /// </summary>
        public Vector3 MovingDestination { get; private set; }

        public Vector3 MovingVelocity { get; private set; }

        public UnityEvent<Vector3> OnMove;

        AudioPlayer loopSoundInstance;

        /// <summary>
        /// If set to true, then bobs, the Activate Sound, and the Finish Sound will be disabled
        /// </summary>
        public bool SilentMode { get; set; } = false;

        [NonSerialized]
        float startTime = -1;

        protected virtual void Reset()
        {
            DefaultBottomPosition = transform.position - new Vector3(0f, 5f, 0f);
            DefaultTopPosition = transform.position + new Vector3(0f, 5f, 0f);

            LiftActivateSound = WeaverAssets.LoadWeaverAsset<AudioClip>("lift_activate");
            LiftLoopSound = WeaverAssets.LoadWeaverAsset<AudioClip>("lift_moving_loop");
            LiftFinishSound = WeaverAssets.LoadWeaverAsset<AudioClip>("lift_arrive");
        }

        public ElevatorInfo GetDefaultInfo() => new ElevatorInfo(this);

        protected virtual void OnValidate()
        {

        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(DefaultBottomPosition, DefaultTopPosition);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, AudioRange.x);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, AudioRange.y);
        }

        protected virtual void Awake()
        {
            WeaverLog.Log("ELE AWAKE");
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
            if (startTime < 0)
            {
                startTime = Time.time;
            }
            if (!Moving)
            {
                transform.position = GetStartPosition();
            }

            Ready = true;
            //cogs = GetComponentsInChildren<ElevatorRotatable>();
        }

        private void Update()
        {
            if (startTime < 0)
            {
                startTime = Time.time;
            }

            if (AutoMove && !Moving && Ready && Time.time >= startTime + INITIAL_DELAY_TIME)
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
            if (Moving || !MoveOnContact)
            {
                return;
            }

            if (startTime < 0)
            {
                startTime = Time.time;
            }

            if (Time.time >= startTime + INITIAL_DELAY_TIME)
            {
                CallElevatorToPosition(GetOppositeDestination());
            }
            else
            {
                if (!Moving)
                {
                    CallElevatorToOpposite(new ElevatorInfo(GetDefaultInfo())
                    {
                        DoBob = false,
                        DoActivateSound = false,
                        BeginDelay = 0
                    });
                }
            }
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

        public void CallElevatorUp()
        {
            CallElevatorToPosition(DefaultTopPosition, GetDefaultInfo());
        }

        public void CallElevatorUp(ElevatorInfo info)
        {
            CallElevatorToPosition(DefaultTopPosition, info);
        }

        public void CallElevatorDown()
        {
            CallElevatorToPosition(DefaultBottomPosition, GetDefaultInfo());
        }

        public void CallElevatorDown(ElevatorInfo info)
        {
            CallElevatorToPosition(DefaultBottomPosition, info);
        }

        public void CallElevatorToOpposite()
        {
            CallElevatorToOpposite(GetDefaultInfo());
        }

        public void CallElevatorToOpposite(ElevatorInfo info)
        {
            CallElevatorToPosition(GetOppositeDestination(), info);
        }

        public void CallElevatorToPosition(Vector3 destination)
        {
            CallElevatorToPosition(destination, GetDefaultInfo());
        }

        public void CallElevatorToPosition(Vector3 destination, ElevatorInfo info)
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
            MovingVelocity = default;
            if (movementRoutine != null)
            {
                if (loopSoundInstance != null)
                {
                    loopSoundInstance.StopPlaying();
                    loopSoundInstance.Delete();
                    loopSoundInstance = null;
                }
                WeaverLog.Log("STOPPING MOVEMENT");
                StopCoroutine(movementRoutine);
                movementRoutine = null;
            }

            StopAllCoroutines();

            OnMove?.Invoke(destination);
            WeaverLog.Log("STARTING MOVEMENT");
            movementRoutine = StartCoroutine(ElevatorMovementRoutine(info));
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

        protected virtual IEnumerator ElevatorMovementRoutine(ElevatorInfo info)
        {
            if (!SilentMode && info.DoActivateSound && LiftActivateSound != null)
            {
                var instance = WeaverAudio.PlayAtPoint(LiftActivateSound, transform.position);
                instance.AudioSource.pitch = LiftActivateSoundPitchRange.RandomInRange();
                WeaverAudio.AddVolumeDistanceControl(instance, AudioRange);
            }

            var movementDirection = (MovingDestination - transform.position).normalized;

            if (info.DoBob && !SilentMode)
            {
                yield return BobRoutine(-movementDirection);
            }

            if (info.BeginDelay > 0f)
            {
                yield return new WaitForSeconds(info.BeginDelay);
            }

            if (info.DoLoopSound && LiftLoopSound != null)
            {
                loopSoundInstance = WeaverAudio.PlayAtPointLooped(LiftLoopSound, transform.position);
                WeaverAudio.AddVolumeDistanceControl(loopSoundInstance, AudioRange);
            }

            EventManager.BroadcastEvent("MOVING UP", gameObject);

            OnMovementStart();

            var start = transform.position;
            var originalLengthToEnd = Vector3.Distance(start, MovingDestination);

            var rotatables = GetComponentsInChildren<ElevatorRotatable>();

            while (true)
            {
                var direction = (MovingDestination - transform.position).normalized;
                MovingVelocity = direction * (info.GetSpeed?.Invoke() ?? Speed);
                transform.position += MovingVelocity * Time.deltaTime;

                for (int i = 0; i < rotatables.Length; i++)
                {
                    rotatables[i].Rotate(this, movementDirection, info.GetSpeed?.Invoke() ?? Speed);
                }

                if (Vector3.Distance(start, transform.position) >= originalLengthToEnd)
                {
                    transform.position = MovingDestination;
                    break;
                }

                loopSoundInstance.transform.position = transform.position;

                yield return null;
            }

            if (loopSoundInstance != null)
            {
                loopSoundInstance.StopPlaying();
                loopSoundInstance.Delete();
                loopSoundInstance = null;
            }

            if (!SilentMode && info.DoFinishSound && LiftFinishSound != null)
            {
                var instance = WeaverAudio.PlayAtPoint(LiftFinishSound, transform.position);
                instance.AudioSource.pitch = LiftFinishSoundPitchRange.RandomInRange();
                WeaverAudio.AddVolumeDistanceControl(instance, AudioRange);
            }

            EventManager.BroadcastEvent("STOP MOVING", gameObject);
            OnMovementEnd();

            if (!SilentMode && info.DoBob)
            {
                yield return BobRoutine(movementDirection);
            }
            if (info.EndDelay > 0f)
            {
                yield return new WaitForSeconds(info.EndDelay);
            }

            MovingVelocity = default;
            movementRoutine = null;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            //WeaverLog.Log("OnCollisionEnter2D");
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
                //WeaverLog.Log("ON PLAYER TOUCH");
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

        /// <summary>
        /// Locks the player to a certain range on the elevator. Useful for cross-scene transport
        /// </summary>
        /// <param name="horizontalRange">Locks the player to only a certain horizontal range relative to the elevator</param>
        /// <param name="duration">How long the lock should last</param>
        /// <param name="stickToElevator">Should the player stick to the elevator when the lock is complete?</param>
        /// <param name="stopPrematurely">Should the lock stop prematurely when the elevator stops moving?</param>
        public void LockPlayerToRange(Vector2 horizontalRange, float duration, bool stickToElevator = true, bool stopPrematurely = true)
        {
            var extraOffset = 0f;

#if !UNITY_EDITOR
            extraOffset = -0.3f;
#endif


            //LockPlayerToRange(horizontalRange, duration, new Vector2(2.470994f + Player.HEIGHT_FROM_FLOOR - 0.2665272f + extraOffset, 2.470994f + Player.HEIGHT_FROM_FLOOR - 0.2665272f + extraOffset), stickWhenDone, stopPrematurely);
            LockPlayerToRange(horizontalRange, duration, new Vector2(2.470994f + Player.Player1.GetHeightFromFloor(transform), 2.470994f + Player.Player1.GetHeightFromFloor(transform)), stickToElevator, stopPrematurely);
        }

        /// <summary>
        /// Locks the player to a certain range on the elevator. Useful for cross-scene transport
        /// </summary>
        /// <param name="horizontalRange">Locks the player to only a certain horizontal range relative to the elevator</param>
        /// <param name="floorAndCeiling">Locks the player to only a certain vertical range relative to the elevator</param>
        /// <param name="duration">How long the lock should last</param>
        /// <param name="stickToElevator">Should the player stick to the elevator when the lock is complete?</param>
        /// <param name="stopPrematurely">Should the lock stop prematurely when the elevator stops moving?</param>
        public void LockPlayerToRange(Vector2 horizontalRange, float duration, Vector2 floorAndCeiling, bool stickToElevator = true, bool stopPrematurely = true)
        {
            StopPlayerLock();
            playerLockRoutine = StartCoroutine(LockPlayerToRangeRoutine(horizontalRange, floorAndCeiling, duration, stickToElevator, stopPrematurely));
        }

        IEnumerator LockPlayerToRangeRoutine(Vector2 horizontalRange, Vector2 floorAndCeiling, float duration, bool stickToElevator, bool stopPrematurely)
        {
            void PositionPlayer()
            {
                var leftWorld = transform.TransformPoint(new Vector2(horizontalRange.x, floorAndCeiling.x));
                var rightWorld = transform.TransformPoint(new Vector2(horizontalRange.y, floorAndCeiling.y));

                var pPos = Player.Player1.transform.position;

                if (pPos.x < leftWorld.x)
                {
                    pPos.x = leftWorld.x;
                }

                if (pPos.x > rightWorld.x)
                {
                    pPos.x = rightWorld.x;
                }

                if (pPos.y < leftWorld.y)
                {
                    pPos.y = leftWorld.y;
                }

                if (pPos.y > rightWorld.y)
                {
                    pPos.y = rightWorld.y;
                }

                Player.Player1.transform.position = pPos;
            }

            if (stickToElevator)
            {
                var sticker = GetComponentInChildren<WeaverHeroPlatformStick>();
                if (sticker != null)
                {
                    sticker.enabled = false;
                    sticker.ForceStickPlayer();
                    PositionPlayer();
                }
            }
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                if (stopPrematurely && !Moving)
                {
                    break;
                }

                PositionPlayer();

                yield return null;
            }

            if (stickToElevator)
            {
                var sticker = GetComponentInChildren<WeaverHeroPlatformStick>();
                if (sticker != null)
                {
                    sticker.enabled = false;
                    var playerRB = Player.Player1.GetComponent<Rigidbody2D>();
                    sticker.ForceUnStickPlayer();
                    yield return null;
                    sticker.ForceStickPlayer();
                    playerRB.velocity = playerRB.velocity.With(y: 0f) + (Vector2)MovingVelocity;
                    HeroController.instance.SetBackOnGround();
                    PositionPlayer();
                    sticker.enabled = true;

                    for (float t = 0; t < 1f; t += Time.deltaTime)
                    {
                        if (stopPrematurely && !Moving)
                        {
                            break;
                        }

                        var leftWorld = transform.TransformPoint(new Vector2(horizontalRange.x, floorAndCeiling.x));
                        var rightWorld = transform.TransformPoint(new Vector2(horizontalRange.y, floorAndCeiling.y));

                        if (Player.Player1.transform.position.y >= leftWorld.y - 0.1f && Player.Player1.transform.position.y <= rightWorld.y + 0.1f)
                        {
                            playerRB.interpolation = RigidbodyInterpolation2D.None;
                        }
                        else
                        {
                            playerRB.interpolation = RigidbodyInterpolation2D.Interpolate;
                            break;
                        }

                        yield return null;
                    }

                    /*for (float t = 0; t < 0.1f; t += Time.deltaTime)
                    {
                        if (playerRB != null)
                        {
                            playerRB.interpolation = RigidbodyInterpolation2D.None;
                        }
                        yield return null;
                    }*/

                    /*for (float t = 0; t < 1f; t += Time.deltaTime)
                    {
                        sticker.ForceStickPlayer();
                        yield return null;
                    }*/

                    /*{
                        var leftWorld = transform.TransformPoint(new Vector2(horizontalRange.x, floorAndCeiling.x));
                        var rightWorld = transform.TransformPoint(new Vector2(horizontalRange.y, floorAndCeiling.y));

                        var pPos = Player.Player1.transform.position;

                        if (pPos.x < leftWorld.x)
                        {
                            pPos.x = leftWorld.x;
                        }

                        if (pPos.x > rightWorld.x)
                        {
                            pPos.x = rightWorld.x;
                        }

                        if (pPos.y < leftWorld.y)
                        {
                            pPos.y = leftWorld.y;
                        }

                        if (pPos.y > rightWorld.y)
                        {
                            pPos.y = rightWorld.y;
                        }

                        Player.Player1.transform.position = pPos;
                    }*/
                }
            }

            playerLockRoutine = null;
        }

        public void StopPlayerLock()
        {
            if (playerLockRoutine != null)
            {
                StopCoroutine(playerLockRoutine);
                playerLockRoutine = null;
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