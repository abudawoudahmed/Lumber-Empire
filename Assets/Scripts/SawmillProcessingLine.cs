using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SawmillProcessingLine : MonoBehaviour
{
    LumberPrototypeBootstrap game;
    Transform player;
    Transform intake, sawPoint, output;
    readonly Queue<GameObject> waiting = new Queue<GameObject>();
    readonly List<GameObject> finished = new List<GameObject>();
    FieldInfo carriedField, boardsField, sawmillField, playerField;
    MethodInfo refreshBackpack;
    bool processing;
    const int MaxVisibleWaiting = 24;
    const int MaxVisibleFinished = 30;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install(){ new GameObject("Sawmill Processing Line").AddComponent<SawmillProcessingLine>(); }

    IEnumerator Start()
    {
        for(int i=0;i<600 && game==null;i++){
            game=FindFirstObjectByType<LumberPrototypeBootstrap>();
            if(game==null){yield return null;continue;}
            var type=typeof(LumberPrototypeBootstrap);
            carriedField=type.GetField("carriedLogs",BindingFlags.Instance|BindingFlags.NonPublic);
            boardsField=type.GetField("boards",BindingFlags.Instance|BindingFlags.NonPublic);
            sawmillField=type.GetField("sawmill",BindingFlags.Instance|BindingFlags.NonPublic);
            playerField=type.GetField("player",BindingFlags.Instance|BindingFlags.NonPublic);
            refreshBackpack=type.GetMethod("RefreshBackpack",BindingFlags.Instance|BindingFlags.NonPublic);
            player=playerField!=null?(Transform)playerField.GetValue(game):null;
        }
        if(game==null||player==null)yield break;
        BuildStation();
        // Disable the prototype's instant-delivery trigger. This line owns processing now.
        var dummy=new GameObject("Legacy Sawmill Trigger Disabled");
        dummy.transform.position=new Vector3(9999,0,9999);
        if(sawmillField!=null)sawmillField.SetValue(game,dummy.transform);
    }

    void Update()
    {
        if(game==null||player==null||intake==null)return;
        int carried=(int)carriedField.GetValue(game);
        Vector3 d=player.position-intake.position;d.y=0;
        if(carried>0 && d.sqrMagnitude<5.3f){
            Deposit(carried);
            carriedField.SetValue(game,0);
            refreshBackpack?.Invoke(game,null);
        }
        if(!processing&&waiting.Count>0)StartCoroutine(ProcessNext());
    }

    void Deposit(int count)
    {
        for(int i=0;i<count;i++){
            var log=MakeLog("Queued Log");
            int visible=Mathf.Min(waiting.Count,MaxVisibleWaiting);
            int row=visible/6, col=visible%6;
            log.transform.position=intake.position+new Vector3((col-2.5f)*.24f,.16f+row*.19f,0);
            waiting.Enqueue(log);
        }
    }

    IEnumerator ProcessNext()
    {
        processing=true;
        GameObject log=waiting.Dequeue();
        if(log==null){processing=false;yield break;}
        Vector3 start=log.transform.position;
        Vector3 mid=sawPoint.position+Vector3.up*.22f;
        float t=0;
        while(t<1&&log!=null){t+=Time.deltaTime*1.35f;log.transform.position=Vector3.Lerp(start,mid,Mathf.SmoothStep(0,1,t));yield return null;}
        yield return new WaitForSeconds(.22f);
        if(log!=null)Destroy(log);
        SpawnFinishedBoards();
        int boards=(int)boardsField.GetValue(game);
        boardsField.SetValue(game,boards+2);
        yield return new WaitForSeconds(.18f);
        RestackWaiting();
        processing=false;
    }

    void SpawnFinishedBoards()
    {
        for(int n=0;n<2;n++){
            var b=GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.name="Finished Board";
            int i=finished.Count%MaxVisibleFinished;
            int row=i/6,col=i%6;
            b.transform.position=output.position+new Vector3((col-2.5f)*.27f,.10f+row*.11f,n*.04f);
            b.transform.localScale=new Vector3(.23f,.08f,.72f);
            Paint(b,new Color(.78f,.55f,.29f));
            Destroy(b.GetComponent<Collider>());
            finished.Add(b);
        }
    }

    void RestackWaiting()
    {
        int i=0;
        foreach(var log in waiting){
            if(log==null)continue;
            int row=i/6,col=i%6;
            log.transform.position=intake.position+new Vector3((col-2.5f)*.24f,.16f+row*.19f,0);
            i++;
        }
    }

    void BuildStation()
    {
        var root=new GameObject("Table Saw Processing Area");
        root.transform.position=new Vector3(5.3f,0,1.5f);
        intake=Marker(root.transform,"RAW LOGS",new Vector3(-3.0f,.035f,0),new Vector3(2.5f,.07f,2.7f),new Color(.48f,.34f,.18f));
        output=Marker(root.transform,"FINISHED WOOD",new Vector3(3.0f,.035f,0),new Vector3(2.5f,.07f,2.7f),new Color(.69f,.52f,.29f));

        var table=GameObject.CreatePrimitive(PrimitiveType.Cube);table.name="Table Saw";table.transform.SetParent(root.transform);table.transform.localPosition=new Vector3(0,.72f,0);table.transform.localScale=new Vector3(2.25f,.18f,1.25f);Paint(table,new Color(.25f,.29f,.30f));
        for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2){var leg=GameObject.CreatePrimitive(PrimitiveType.Cube);leg.transform.SetParent(root.transform);leg.transform.localPosition=new Vector3(x*.85f,.35f,z*.42f);leg.transform.localScale=new Vector3(.13f,.7f,.13f);Paint(leg,new Color(.18f,.20f,.21f));}
        var blade=GameObject.CreatePrimitive(PrimitiveType.Cylinder);blade.name="Saw Blade";blade.transform.SetParent(root.transform);blade.transform.localPosition=new Vector3(0,1.02f,0);blade.transform.localScale=new Vector3(.48f,.055f,.48f);blade.transform.localRotation=Quaternion.Euler(90,0,0);Paint(blade,new Color(.72f,.75f,.76f));Destroy(blade.GetComponent<Collider>());
        var fence=GameObject.CreatePrimitive(PrimitiveType.Cube);fence.transform.SetParent(root.transform);fence.transform.localPosition=new Vector3(0,.93f,.42f);fence.transform.localScale=new Vector3(2.0f,.18f,.09f);Paint(fence,new Color(.82f,.58f,.12f));Destroy(fence.GetComponent<Collider>());
        sawPoint=new GameObject("Saw Feed Point").transform;sawPoint.SetParent(root.transform);sawPoint.localPosition=new Vector3(0,.88f,0);
    }

    Transform Marker(Transform parent,string name,Vector3 pos,Vector3 scale,Color color)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);g.transform.localPosition=pos;g.transform.localScale=scale;Paint(g,color);Destroy(g.GetComponent<Collider>());return g.transform;
    }

    GameObject MakeLog(string name)
    {
        var log=GameObject.CreatePrimitive(PrimitiveType.Cylinder);log.name=name;log.transform.localScale=new Vector3(.13f,.48f,.13f);log.transform.rotation=Quaternion.Euler(0,0,90);Paint(log,new Color(.52f,.29f,.12f));Destroy(log.GetComponent<Collider>());return log;
    }

    Material Mat(Color c){Shader s=Shader.Find("Universal Render Pipeline/Lit");if(s==null)s=Shader.Find("Sprites/Default");var m=new Material(s);m.color=c;return m;}
    void Paint(GameObject o,Color c){var r=o.GetComponent<Renderer>();if(r!=null)r.material=Mat(c);}
}