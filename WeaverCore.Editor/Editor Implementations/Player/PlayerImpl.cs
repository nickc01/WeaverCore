using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Helpers;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class EditorPlayerImplementation : PlayerImplementation
    {
        /*[SerializeField]
        //List<Collision2D> floorColliders = new List<Collision2D>();
        int colliders = 0;

        Rigidbody2D rigidBody;

        bool OnGround => colliders > 0;


        void Start()
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                if (OnGround)
                {
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x,rigidBody.velocity.y + 16.65f);
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
            if (collision.gameObject.layer == LayerMask.NameToLayer("Soft Terrain"))
            {
                //floorColliders.Add(collision);
                colliders++;
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Soft Terrain"))
            {
                colliders--;
                //floorColliders.Remove(collision);
            }
        }*/

        public override void Initialize()
        {
            Debugger.Log("PLAYER START EDITOR");
        }
    }
}
