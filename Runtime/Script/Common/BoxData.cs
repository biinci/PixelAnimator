using UnityEngine;
using UnityEngine.Serialization;

namespace binc.PixelAnimator.Common{

    [System.Serializable]
    public struct BoxData{
        public string boxName;
        public Color color;
        [Tooltip("LayerMask Index")]
        public int activeLayer;
        public bool rounded;
        public PhysicsMaterial2D physicMaterial;

        public string Guid => guid;
        [ReadOnly, SerializeField]
        private string guid;

    }



}