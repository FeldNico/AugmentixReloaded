using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Augmentix.Scripts.VR
{
    public class VRUI : MonoBehaviour
    {
        public static VRUI Instance = null;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

    }
}