using UnityEngine;
using binc.PixelAnimator;

public class Test : MonoBehaviour
{
    public PixelAnimator animator;
    public PixelAnimation run;
    private void Start()
    {
        animator.Play(run);
        
    }
    public void Log(string msg)
    {
        Debug.Log(msg);
    }

}
