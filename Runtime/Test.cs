using UnityEngine;
using binc.PixelAnimator;
using UnityEditor;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    public PixelAnimator animator;
    public PixelAnimation run;
    private string id;
    private Object obj;
    private void Start()
    {
        DontDestroyOnLoad(this);
        id = GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();
        animator.Play(run);
        var id2 = run.PixelSprites[0].methodStorage.methodData[0].GlobalId;
        obj = run.PixelSprites[0].methodStorage.methodData[0].obj;
        Debug.Log(id.ToString() == id2);
        Debug.Log("burasi" + id);
        Debug.Log(id2);
        Debug.Log("obj: " + obj);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var id2 = GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();
            Debug.Log(id == id2);
            SceneManager.LoadScene(1);
            Debug.Log(obj);

        }
    }
    public void Log(string msg)
    {
        Debug.Log(msg);
    }

}
