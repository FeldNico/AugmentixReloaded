using UnityEngine;

namespace Augmentix.Scripts.AR
{
    public class MapTarget : MonoBehaviour
    {
        public static MapTarget Instance = null;

        public float PlayerScale;
        public float Scale;
        public Vector3 MapOffset = new Vector3(1.694f,0f,1.261f);
        public GameObject Scaler { private set; get; }

        void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
    }
}
