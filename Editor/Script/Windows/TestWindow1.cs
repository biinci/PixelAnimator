using System.Collections;
using System.Collections.Generic;
using binc.PixelAnimator.Editor.Windows;
using UnityEngine;

public class TestWindow1 : Window{



    public override void SetWindow(Event eventCurrent)
    {
        base.SetWindow(eventCurrent);
        
        
        GUI.Window(1, new Rect(100,100,200,100), _=>{} , "SELAM1");
    }


    public override void FocusFunctions()
    {


    }

    public override void UIOperations()
    {



    }
}
