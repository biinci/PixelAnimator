using System.Collections.Generic;
using UnityEngine;

namespace binc.PixelAnimator
{
    public class PixelAnimationController : ScriptableObject
    {
         public IReadOnlyList<PixelAnimation> Animations => animations;
        [SerializeField] private List<PixelAnimation> animations;
        
    }
}