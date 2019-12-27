using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ViridianLink.Game
{
    public class Initializer : InitializerImplementation
    {
        public override void Initialize()
        {
            Debug.Log("GAME INITIALIZER");
            Modding.Logger.Log("GAME INITIALIZER2");
        }
    }
}
