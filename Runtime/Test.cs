using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Linq.Expressions;
using binc.PixelAnimator;
using UnityEditor;

public class Test : MonoBehaviour
{
    public PixelAnimator animator;
    public PixelAnimation run;
    public List<Sprite> test;
    private void Start()
    {
        animator.Play(run);
        
    }
    public void Log(string msg)
    {
        Debug.Log(msg);
    }

}
