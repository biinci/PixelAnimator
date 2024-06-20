using System.Collections;
using System.Collections.Generic;
using binc.PixelAnimator.Editor.Windows;
using UnityEngine;

public class TestWindow2 : Window
{

    public override void SetWindow(Event eventCurrent)
    {
        base.SetWindow(eventCurrent);
        GUI.Window(2, new Rect(10,130,100,100), _=>{} , "SELAM2");
    }


    public override void FocusFunctions()
    {
        throw new System.NotImplementedException();
    }

    public override void UIOperations()
    {
        throw new System.NotImplementedException();
    }
}
