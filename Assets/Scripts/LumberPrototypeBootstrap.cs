using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LumberPrototypeBootstrap : MonoBehaviour
{
    Transform player;
    Camera cam;
    Animator animator;
    RuntimeAnimatorController normalController;
    AnimatorOverrideController runController;
    AnimationClip walkClip;
    AnimationClip runClip;
    bool wasMoving;
    bool wasRunning;
    float moveHeldTime;

    const float WalkSpeed = 2.6f;
    const float RunSpeed = 6.5f;
    const float RunDelay = 30.0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoStart(){if(FindFirstObjectByType<LumberPrototypeBootstrap>()==null)new GameObject("Lumber Empire Demo").AddComponent<LumberPrototypeBootstrap>();}

    void Start(){foreach(var c in FindObjectsByType<Camera>(FindObjectsSortMode.None))Destroy(c.gameObject);RenderSettings.ambientLight=new Color(.72f,.76f,.68f);MakeGround();MakeLight();MakeCamera();MakePlayer();MakeForest();MakeSawmill();}

    void Update()
    {
        if(player==null)return;
        Vector2 input=new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        Vector3 d=new Vector3(input.x,0,input.y).normalized;
        bool moving=d.sqrMagnitude>.01f;
        moveHeldTime=moving?moveHeldTime+Time.deltaTime:0f;
        bool running=moving&&moveHeldTime>=RunDelay;
        float speed=running?RunSpeed:WalkSpeed;

        if(moving){player.position+=d*speed*Time.deltaTime;player.rotation=Quaternion.Slerp(player.rotation,Quaternion.LookRotation(d),12f*Time.deltaTime);}

        if(animator!=null)
        {
            if(!moving&&wasMoving) FreezeStandingPose();
            else if(moving&&!wasMoving) StartWalking();
            else if(moving&&running&&!wasRunning) StartRunning();
        }
        wasMoving=moving;wasRunning=running;

        if(cam!=null){Vector3 target=player.position+new Vector3(9.5f,11f,-9.5f);cam.transform.position=Vector3.Lerp(cam.transform.position,target,5f*Time.deltaTime);}
    }

    void StartWalking()
    {
        animator.runtimeAnimatorController=normalController;
        animator.speed=1f;
        animator.Play("Walking",0,0f);
        animator.Update(0f);
    }

    void StartRunning()
    {
        if(runController==null||runClip==null){Debug.LogError("RUN CLIP NOT LOADED");return;}
        animator.runtimeAnimatorController=runController;
        animator.speed=1.35f;
        animator.Play("Walking",0,0f);
        animator.Update(0f);
        Debug.Log("RUNNING ACTIVATED AFTER 30 SECONDS");
    }

    void FreezeStandingPose()
    {
        animator.runtimeAnimatorController=normalController;
        animator.speed=1f;
        animator.Play("Walking",0,.08f);
        animator.Update(0f);
        animator.speed=0f;
    }

    void MakePlayer()
    {
        GameObject prefab=null;
#if UNITY_EDITOR
        prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Hodaart/HodaartLowPolyCharacterCollection3/Prefabs/Character 06.prefab");
        walkClip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Hodaart/HodaartLowPolyCharacterCollection3/Animations/Walking.anim");
        runClip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Hodaart/HodaartLowPolyCharacterCollection3/Animations/Running.anim");
#endif
        if(prefab==null)prefab=Resources.Load<GameObject>("Lumberjack/Character 06");
        if(prefab==null){Debug.LogError("Character 06 prefab not found.");return;}
        var root=new GameObject("Lumberjack Player");
        var obj=Instantiate(prefab,root.transform);obj.name="Character 06 Visual";obj.transform.localPosition=Vector3.zero;obj.transform.localRotation=Quaternion.identity;obj.transform.localScale=Vector3.one*1.45f;
        animator=obj.GetComponentInChildren<Animator>();
        if(animator!=null)
        {
            animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            normalController=animator.runtimeAnimatorController;
            if(normalController!=null&&runClip!=null)
            {
                runController=new AnimatorOverrideController(normalController);
                runController["Walking"]=runClip;
            }
            animator.Rebind();FreezeStandingPose();
        }
        GroundVisual(obj,0f);player=root.transform;
    }

    void GroundVisual(GameObject visual,float groundY){var rs=visual.GetComponentsInChildren<Renderer>();if(rs.Length==0)return;float minY=float.PositiveInfinity;foreach(var r in rs)minY=Mathf.Min(minY,r.bounds.min.y);visual.transform.position+=Vector3.up*(groundY-minY+.02f);}
    void MakeGround(){var g=GameObject.CreatePrimitive(PrimitiveType.Plane);g.name="Forest Clearing";g.transform.localScale=new Vector3(3.2f,1,3.2f);g.GetComponent<Renderer>().material.color=new Color(.36f,.64f,.25f);}
    void MakeLight(){var o=new GameObject("Warm Sun");var l=o.AddComponent<Light>();l.type=LightType.Directional;l.intensity=1.25f;l.color=new Color(1f,.91f,.76f);o.transform.rotation=Quaternion.Euler(48,-35,0);}
    void MakeCamera(){var o=new GameObject("Main Camera");o.tag="MainCamera";cam=o.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=7.5f;o.transform.position=new Vector3(9.5f,11,-9.5f);o.transform.rotation=Quaternion.Euler(38,-42,0);cam.backgroundColor=new Color(.65f,.82f,.92f);}
    void MakeForest(){Vector3[] p={new(-7,0,-5),new(-5,0,-7),new(-3,0,-6),new(5,0,-6),new(7,0,-3),new(-7,0,2),new(-6,0,6),new(5,0,6),new(7,0,4),new(3,0,7)};foreach(var v in p)Tree(v);}
    void Tree(Vector3 p){var r=new GameObject("Tree");r.transform.position=p;var t=GameObject.CreatePrimitive(PrimitiveType.Cylinder);t.transform.SetParent(r.transform);t.transform.localPosition=new Vector3(0,1.4f,0);t.transform.localScale=new Vector3(.42f,1.4f,.42f);t.GetComponent<Renderer>().material.color=new Color(.42f,.23f,.1f);for(int i=0;i<3;i++){var f=GameObject.CreatePrimitive(PrimitiveType.Sphere);f.transform.SetParent(r.transform);f.transform.localPosition=new Vector3((i-1)*.55f,3.1f+(i%2)*.35f,0);f.transform.localScale=new Vector3(1.5f,1.25f,1.5f);f.GetComponent<Renderer>().material.color=new Color(.12f,.48f,.16f);}}
    void MakeSawmill(){var root=new GameObject("Old Sawmill");root.transform.position=new Vector3(4,0,1);var table=GameObject.CreatePrimitive(PrimitiveType.Cube);table.transform.SetParent(root.transform);table.transform.localPosition=new Vector3(0,.65f,0);table.transform.localScale=new Vector3(4,.35f,1.7f);table.GetComponent<Renderer>().material.color=new Color(.38f,.23f,.12f);var blade=GameObject.CreatePrimitive(PrimitiveType.Cylinder);blade.transform.SetParent(root.transform);blade.transform.localPosition=new Vector3(0,1.45f,0);blade.transform.localScale=new Vector3(1.25f,.12f,1.25f);blade.transform.rotation=Quaternion.Euler(90,0,0);blade.GetComponent<Renderer>().material.color=Color.gray;}
}
