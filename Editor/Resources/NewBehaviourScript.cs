using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class NewBehaviourScript : EditorWindow{ 

    public Rect windowRect;
    private Vector2 prevMousePos;
    private float scale=1;


    [MenuItem("Window/Test")]
    private static void InitWindow(){
        var window = CreateWindow<NewBehaviourScript>();
        window.Show();
        window.windowRect.position = Vector2.zero;
    }



    private void OnGUI(){

        windowRect.size = new Vector2(50*scale,50*scale);
        var e = Event.current;
        if(e.type == EventType.MouseDown){
            prevMousePos = e.mousePosition;
        }
        if(e.type == EventType.MouseDrag){
            var delta = e.mousePosition - prevMousePos;
            windowRect.position += delta;
            prevMousePos = e.mousePosition;
        }
        if(e.type == EventType.ScrollWheel){
            var delta = Mathf.Sign(e.delta.y) > 0 ? -1f : 1f;
            var p = scale;
            scale += delta;
            if(scale <=0) scale = 1;
            var ratio = scale/p;
            var relative = windowRect.position - e.mousePosition;
            var V = relative * ratio;
            var v = V - relative;
            windowRect.position += v;
        }
        if(scale <=0){
            scale = 1;
        }
        BeginWindows();
        GUI.Window(0,windowRect, _=>{
            EditorGUI.DrawRect(new Rect(0,0, windowRect.width/2, windowRect.height/2), Color.blue);


        }, GUIContent.none);
        EndWindows();
        // Debug.Log(e.button);
        Repaint();
    }


}