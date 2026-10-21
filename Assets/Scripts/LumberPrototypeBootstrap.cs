using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LumberPrototypeBootstrap : MonoBehaviour
{
    Transform player; Camera cam; Animator animator;
    RuntimeAnimatorController normalController;
    AnimationClip runClip;
    PlayableGraph runGraph;
    bool runGraphActive, wasMoving, wasRunning, chopping;
    float moveHeldTime, nextChopTime;
    Vector2 touchStart;
    bool dragging;
    int wood;
    TreeNode nearbyTree;
    GUIStyle hudStyle, hintStyle;

    const float WalkSpeed=2.6f;
    const float RunSpeed=7.2f;
    const float RunDelay=3f;
    const float ChopRange=2.15f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoStart(){if(FindFirstObjectByType<LumberPrototypeBootstrap>()==null)new GameObject("Lumber Empire Demo").AddComponent<LumberPrototypeBootstrap>();}

    void Start(){
        foreach(var c in FindObjectsByType<Camera>(FindObjectsSortMode.None))Destroy(c.gameObject);
        RenderSettings.ambientLight=new Color(.72f,.76f,.68f);
        MakeGround();MakeLight();MakeCamera();MakePlayer();MakeForest();MakeSawmill();
    }

    Vector2 ReadMoveInput(){
        Vector2 input=new(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        if(Input.touchCount>0){
            Touch t=Input.GetTouch(0);
            if(t.phase==TouchPhase.Began){touchStart=t.position;dragging=true;}
            if(dragging&&(t.phase==TouchPhase.Moved||t.phase==TouchPhase.Stationary)){Vector2 delta=t.position-touchStart;if(delta.magnitude>12f)input=Vector2.ClampMagnitude(delta/90f,1f);}
            if(t.phase==TouchPhase.Ended||t.phase==TouchPhase.Canceled)dragging=false;
        }else if(Input.GetMouseButtonDown(0)){touchStart=Input.mousePosition;dragging=true;}
        else if(Input.GetMouseButton(0)&&dragging){Vector2 delta=(Vector2)Input.mousePosition-touchStart;if(delta.magnitude>12f)input=Vector2.ClampMagnitude(delta/90f,1f);}
        else if(Input.GetMouseButtonUp(0))dragging=false;
        return input;
    }

    void Update(){
        if(player==null)return;
        nearbyTree=FindClosestTree();
        if(!chopping){
            Vector2 input=ReadMoveInput();Vector3 d=new Vector3(input.x,0,input.y);if(d.sqrMagnitude>1)d.Normalize();
            bool moving=d.sqrMagnitude>.01f;moveHeldTime=moving?moveHeldTime+Time.deltaTime:0f;bool running=moving&&moveHeldTime>=RunDelay;float speed=running?RunSpeed:WalkSpeed;
            if(moving){player.position+=d*speed*Time.deltaTime;player.rotation=Quaternion.Slerp(player.rotation,Quaternion.LookRotation(d),12f*Time.deltaTime);}
            if(animator!=null){if(!moving&&wasMoving)FreezeStandingPose();else if(moving&&!wasMoving)StartWalking();else if(running&&!wasRunning)StartRunning();}
            wasMoving=moving;wasRunning=running;
        }
        if(nearbyTree!=null && Time.time>=nextChopTime && ChopPressed()){StartCoroutine(ChopTree(nearbyTree));nextChopTime=Time.time+.55f;}
        if(cam!=null){Vector3 target=player.position+new Vector3(9.5f,11f,-9.5f);cam.transform.position=Vector3.Lerp(cam.transform.position,target,5f*Time.deltaTime);}
    }

    bool ChopPressed(){
        if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.E))return true;
        if(Input.touchCount>0){Touch t=Input.GetTouch(0);if(t.phase==TouchPhase.Ended&&(t.position-touchStart).magnitude<18f)return true;}
        return Input.GetMouseButtonUp(0)&&((Vector2)Input.mousePosition-touchStart).magnitude<18f;
    }

    TreeNode FindClosestTree(){
        TreeNode best=null;float bestD=ChopRange*ChopRange;
        foreach(var t in FindObjectsByType<TreeNode>(FindObjectsSortMode.None)){if(t.felled)continue;Vector3 delta=t.transform.position-player.position;delta.y=0;float d=delta.sqrMagnitude;if(d<bestD){bestD=d;best=t;}}
        return best;
    }

    IEnumerator ChopTree(TreeNode tree){
        if(tree==null||tree.felled)yield break;chopping=true;wasMoving=false;wasRunning=false;moveHeldTime=0;
        Vector3 face=tree.transform.position-player.position;face.y=0;if(face.sqrMagnitude>.01f)player.rotation=Quaternion.LookRotation(face);
        if(animator!=null)FreezeStandingPose();
        tree.Hit();
        yield return new WaitForSeconds(.16f);
        if(tree.hitsLeft<=0){tree.felled=true;yield return StartCoroutine(FellAndCollect(tree));}
        chopping=false;
    }

    IEnumerator FellAndCollect(TreeNode tree){
        Transform tr=tree.transform;Quaternion start=tr.rotation;Vector3 axis=Vector3.Cross(Vector3.up,(tr.position-player.position).normalized);if(axis.sqrMagnitude<.01f)axis=Vector3.right;Quaternion end=Quaternion.AngleAxis(78f,axis)*start;
        float t=0;while(t<1){t+=Time.deltaTime*1.7f;tr.rotation=Quaternion.Slerp(start,end,Mathf.SmoothStep(0,1,t));yield return null;}
        yield return new WaitForSeconds(.25f);wood+=3;
        Vector3 stumpPos=tr.position;Destroy(tr.gameObject);
        var stump=GameObject.CreatePrimitive(PrimitiveType.Cylinder);stump.name="Stump";stump.transform.position=stumpPos+Vector3.up*.18f;stump.transform.localScale=new Vector3(.5f,.18f,.5f);Paint(stump,new Color(.48f,.28f,.12f));Destroy(stump.GetComponent<Collider>());
    }

    void OnGUI(){
        if(hudStyle==null){hudStyle=new GUIStyle(GUI.skin.box){fontSize=24,alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold};hintStyle=new GUIStyle(GUI.skin.box){fontSize=19,alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold};}
        GUI.Box(new Rect(18,18,170,52),"WOOD  "+wood,hudStyle);
        if(nearbyTree!=null&&!nearbyTree.felled){string msg=nearbyTree.hitsLeft+" HITS  •  TAP / SPACE";GUI.Box(new Rect(Screen.width/2-145,Screen.height-82,290,52),msg,hintStyle);}
    }

    void StopRunGraph(){if(runGraphActive&&runGraph.IsValid()){runGraph.Destroy();runGraphActive=false;}if(animator!=null)animator.runtimeAnimatorController=normalController;}
    void StartWalking(){StopRunGraph();animator.speed=1f;animator.Play("Walking",0,0f);animator.Update(0f);}
    void StartRunning(){if(runClip==null){animator.speed=1.45f;return;}StopRunGraph();var playable=AnimationPlayableUtilities.PlayClip(animator,runClip,out runGraph);playable.SetSpeed(1.25);runGraphActive=true;}
    void FreezeStandingPose(){StopRunGraph();animator.speed=1f;animator.Play("Walking",0,.08f);animator.Update(0f);animator.speed=0f;}
    void OnDestroy(){if(runGraphActive&&runGraph.IsValid())runGraph.Destroy();}

    void MakePlayer(){
        animator=FindFirstObjectByType<Animator>();GameObject obj=null;if(animator!=null)obj=animator.gameObject;
#if UNITY_EDITOR
        if(obj==null){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Hodaart/HodaartLowPolyCharacterCollection3/Prefabs/Character 06.prefab");if(prefab!=null)obj=Instantiate(prefab);}
        runClip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Hodaart/HodaartLowPolyCharacterCollection3/Animations/Running.anim");
#endif
        if(obj==null){Debug.LogError("No playable character found in scene/build.");return;}
        var root=new GameObject("Lumberjack Player");root.transform.position=Vector3.zero;obj.transform.SetParent(root.transform,true);obj.transform.position=Vector3.zero;obj.transform.rotation=Quaternion.identity;
        animator=obj.GetComponentInChildren<Animator>();if(animator!=null){animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;normalController=animator.runtimeAnimatorController;animator.Rebind();FreezeStandingPose();}
        GroundVisual(obj,0.05f);player=root.transform;
    }

    void GroundVisual(GameObject visual,float groundY){if(animator!=null)animator.Update(0f);var rs=visual.GetComponentsInChildren<Renderer>();if(rs.Length==0)return;float minY=float.PositiveInfinity;foreach(var r in rs){if(r is SkinnedMeshRenderer s)s.updateWhenOffscreen=true;minY=Mathf.Min(minY,r.bounds.min.y);}if(float.IsFinite(minY))visual.transform.position+=Vector3.up*(groundY-minY);}
    Material Mat(Color color){Shader s=Shader.Find("Universal Render Pipeline/Lit");if(s==null)s=Shader.Find("Sprites/Default");var m=new Material(s);m.color=color;return m;}
    void Paint(GameObject o,Color c){var r=o.GetComponent<Renderer>();if(r!=null)r.material=Mat(c);}
    void MakeGround(){var g=GameObject.CreatePrimitive(PrimitiveType.Plane);g.name="Forest Clearing";g.transform.localScale=new Vector3(3.2f,1,3.2f);Paint(g,new Color(.36f,.64f,.25f));}
    void MakeLight(){var o=new GameObject("Warm Sun");var l=o.AddComponent<Light>();l.type=LightType.Directional;l.intensity=1.25f;l.color=new Color(1f,.91f,.76f);o.transform.rotation=Quaternion.Euler(48,-35,0);}
    void MakeCamera(){var o=new GameObject("Main Camera");o.tag="MainCamera";cam=o.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=7.5f;o.transform.position=new Vector3(9.5f,11,-9.5f);o.transform.rotation=Quaternion.Euler(38,-42,0);cam.backgroundColor=new Color(.65f,.82f,.92f);}
    void MakeForest(){Vector3[] p={new(-7,0,-5),new(-5,0,-7),new(-3,0,-6),new(5,0,-6),new(7,0,-3),new(-7,0,2),new(-6,0,6),new(5,0,6),new(7,0,4),new(3,0,7)};foreach(var v in p)Tree(v);}
    void Tree(Vector3 p){var r=new GameObject("Tree");r.transform.position=p;var node=r.AddComponent<TreeNode>();node.owner=this;var t=GameObject.CreatePrimitive(PrimitiveType.Cylinder);t.transform.SetParent(r.transform);t.transform.localPosition=new Vector3(0,1.4f,0);t.transform.localScale=new Vector3(.42f,1.4f,.42f);Paint(t,new Color(.42f,.23f,.1f));for(int i=0;i<3;i++){var f=GameObject.CreatePrimitive(PrimitiveType.Sphere);f.transform.SetParent(r.transform);f.transform.localPosition=new Vector3((i-1)*.55f,3.1f+(i%2)*.35f,0);f.transform.localScale=new Vector3(1.5f,1.25f,1.5f);Paint(f,new Color(.12f,.48f,.16f));}}
    void MakeSawmill(){var root=new GameObject("Old Sawmill");root.transform.position=new Vector3(4,0,1);var table=GameObject.CreatePrimitive(PrimitiveType.Cube);table.transform.SetParent(root.transform);table.transform.localPosition=new Vector3(0,.65f,0);table.transform.localScale=new Vector3(4,.35f,1.7f);Paint(table,new Color(.38f,.23f,.12f));var blade=GameObject.CreatePrimitive(PrimitiveType.Cylinder);blade.transform.SetParent(root.transform);blade.transform.localPosition=new Vector3(0,1.45f,0);blade.transform.localScale=new Vector3(1.25f,.12f,1.25f);blade.transform.rotation=Quaternion.Euler(90,0,0);Paint(blade,Color.gray);}

    public class TreeNode:MonoBehaviour{
        public LumberPrototypeBootstrap owner;public int hitsLeft=3;public bool felled;
        public void Hit(){hitsLeft=Mathf.Max(0,hitsLeft-1);StartCoroutine(Shake());}
        IEnumerator Shake(){Vector3 p=transform.localPosition;for(int i=0;i<4;i++){transform.localPosition=p+new Vector3((i%2==0?.08f:-.08f),0,0);yield return new WaitForSeconds(.035f);}transform.localPosition=p;}
    }
}
