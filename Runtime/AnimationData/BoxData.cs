using UnityEngine;

namespace binc.PixelAnimator.AnimationData{

    [System.Serializable]
    public struct BoxData{
        public string boxName;
        public Color color;
        
        public int layer;
        public bool rounded;
        public PhysicsMaterial2D physicMaterial;

        public string Guid => guid;
        [ReadOnly, SerializeField]
        private string guid;
    }
}