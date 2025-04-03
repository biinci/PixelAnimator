using System.Collections.Generic;
using UnityEngine;
using binc.PixelAnimator.Editor.Windows;
using UnityEditor;

namespace binc.PixelAnimator.Editor.Preferences{
    public class PixelAnimatorPreferences : ScriptableObject{ 
        [SerializeReference] public List<Window> windows;
        public List<MonoScript> windowScripts;
    }

}