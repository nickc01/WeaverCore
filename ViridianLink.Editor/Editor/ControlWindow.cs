using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ViridianLink.Editor
{
    public class ControlWindow : EditorWindow
    {
        [SerializeField]
        string testString = "HelloWORLD!";

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.)]
        public static void Test()
        {

        }


        [MenuItem("Window/ControlWindow")]
        public static void Testing()
        {
            CreateAndShow();
        }

        public static ControlWindow Create()
        {
            return (ControlWindow)EditorWindow.GetWindow(typeof(ControlWindow));
        }

        public static ControlWindow CreateAndShow()
        {
            ControlWindow window = (ControlWindow)EditorWindow.GetWindow(typeof(ControlWindow));
            window.Show();
            return window;
        }
    }
}
