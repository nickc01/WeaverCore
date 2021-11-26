using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Enums;

namespace WeaverCore.Editor.Implementations
{
	public class E_Player_I : Player_I
    {
        //int colliders = 0;

        //int possibleJumps = 2;

        //int jumpCounter = 0;

        //Rigidbody2D rigidBody;
        /*HeroController controller;

        bool OnGround
        {
            get
            {
                return controller.cState.onGround;
            }
        }*/

        public override bool HasDreamNail
        {
            get
            {
                return false;
            }
        }

        public override int EssenceCollected
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }
        public override int EssenceSpent
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

		//private void Awake()
		//{
            //controller = GetComponent<HeroController>();
        //}

		/*private void Awake()
		{
			//if (GetComponent<HeroController>() == null)
			//{
            //    gameObject.AddComponent<HeroController>();
			//}
		}

		void Start()
        {
           //rigidBody = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                if (possibleJumps > 0 && jumpCounter < possibleJumps)
                {
                    //rigidBody.velocity.y + 
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x,16.65f);
                    jumpCounter++;
                }
            }
            var velocity = rigidBody.velocity;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                velocity.x = 8.3f;

            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                velocity.x = -8.3f;
            }
            else
            {
                velocity.x = 0;
            }
            rigidBody.velocity = velocity;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "HeroWalkable" || collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                colliders++;
                jumpCounter = 0;
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "HeroWalkable" || collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                colliders--;
            }
        }*/

		public override void Initialize()
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
           //Debugger.Log("PLAYER START EDITOR");
        }

        public override void SoulGain()
        {
            
        }

        public override void RefreshSoulUI()
        {
            
        }

        public override void EnterParryState()
        {
            //TODO : EDITOR PARRYING
        }

        public override void RecoverFromParry()
        {
            //TODO : EDITOR PARRYING
        }

        public override void Recoil(CardinalDirection direction)
        {
            //TODO : PLAYER RECOIL
        }

        public override bool HasCharmEquipped(int charmNumber)
        {
            return false;
        }

        public override void EnterRoarLock()
        {
            
        }

        public override void ExitRoarLock()
        {
            
        }
    }
}
