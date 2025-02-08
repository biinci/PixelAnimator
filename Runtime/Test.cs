using binc.PixelAnimator;
using binc.PixelAnimator.DataManipulations;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class Test : MonoBehaviour
{
    public PixelAnimator animator;
    public PixelAnimation run;
    private string id;
    private Object obj;

    [SerializeReference]public BaseMethodData methodData;
    public MethodData data;
    // public MethodData<Collider> genericData;


    private void OnGUI()
    {
        GUILayout.Button("selam");
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
        id = GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();
        animator.Play(run);
        var id2 = run.PixelSprites[0].methodStorage.methodData[0].GlobalId;
        Debug.Log(id == id2);
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

    public void PlayAudio(AudioSource audioSource, float duration)
    {
        audioSource.Play();
    }

    public void Damage(Collider other, float damage)
    {
        
    }

    public void CollisionDamage(Collision other, float damage)
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        

    }
}

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("bind to collider method"))
        {
            var test = (Test)target;
            test.methodData = new MethodData<Collider>();
        }
        if (GUILayout.Button("bind to normal method"))
        {
            var test = (Test)target;
            test.methodData = new MethodData();
        }

        if (GUILayout.Button("reset data"))
        {
            var test = (Test)target;
            test.data = new MethodData();
        }
    }
}
