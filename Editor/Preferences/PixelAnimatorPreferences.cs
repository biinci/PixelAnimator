using System.Collections.Generic;
using UnityEngine;
using binc.PixelAnimator.Editor.Windows;
using UnityEditor;


namespace binc.PixelAnimator.Editor.Preferences{

    [CreateAssetMenu(menuName = "Pixel Animation/ New Animator Preferences")]
    public class PixelAnimatorPreferences : ScriptableObject{ 

        [SerializeReference] public List<Window> windows;
        public List<MonoScript> windowScripts;





    }

}