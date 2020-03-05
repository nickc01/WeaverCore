using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;

namespace WeaverCore
{
    public class Player : MonoBehaviour
    {
        static Player _instance;
        public static Player Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject playerObject = new GameObject("Player");
                    _instance = playerObject.AddComponent<Player>();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }


        PlayerImplementation playerImpl;

        void Start()
        {
            var playerImplType = ImplFinder.GetImplementationType<PlayerImplementation>();

            playerImpl = (PlayerImplementation)gameObject.AddComponent(playerImplType);
            Instance = this;
            playerImpl.Initialize();
        }
    }
}
