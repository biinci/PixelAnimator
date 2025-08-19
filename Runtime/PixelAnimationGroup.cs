using System.Collections.Generic;
using UnityEngine;

namespace binc.PixelAnimator
{
    [CreateAssetMenu(menuName = "Pixel Animation/ New Animation Controller")]

    public class PixelAnimationGroup : ScriptableObject
    {
         public IReadOnlyList<PixelAnimation> Animations => animations;
        [SerializeField] private List<PixelAnimation> animations;
        
    }
}