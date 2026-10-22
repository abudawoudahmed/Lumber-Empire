using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SawmillProcessingLine : MonoBehaviour
{
    LumberPrototypeBootstrap game; Transform player,intake,sawPoint,output,blade;
    readonly Queue<GameObject> waiting=new Queue<GameObject>(); readonly List<GameObject> finished=new List<GameObject>();
    FieldInfo carriedField,boardsField,sawmillField,playerField; MethodInfo refreshBackpack; bool processing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install(){new GameObject("Sawmill Processing Line").AddComponent<SawmillProcessingLine>();}
    IEnumerator Start(){for(int i=0;i<600&&game==null;i++){game=FindFirstObjectByType<LumberPrototypeBootstrap>();if(game==null){yield return null;continue;}var t=typeof(LumberPrototypeBootstrap);carriedField=t.GetField("carriedLogs",BindingFlags.Instance|BindingFlags.NonPublic);boardsField=t.GetField("boards",BindingFlags.Instance|BindingFlags.NonPublic);sawmillField=t.GetField("sawmill",BindingFlags.Instance|BindingFlags.NonPublic);playerField=t.GetField("player",BindingFlags.Instance|BindingFlags.NonPublic);refreshBackpack=t.GetMethod("RefreshBackpack",BindingFlags.Instance|BindingFlags.NonPublic);player=playerField!=null?(Transform)playerField.GetValue(game):null;}if(game==null||player==null)yield break;BuildStation();var dummy=new GameObject("Legacy Sawmill Trigger Disabled");dummy.transform.position=new Vector3(9999,0,9999);if(sawmillField!=null)sawmillField.SetValue(game,dummy.transform);}
    void Update(){if(blade!=null)blade.Rotate(Vector3.forward,540f*Time.deltaTime,Space.Self);if(game==null||player==null||intake==null)return;int carried=(int)carriedField.GetValue(game);Vector3 d=player.position-intake.position;d.y=0;if(carried>0&&d.sqrMagnitude<5.3f){Deposit(carried);carriedField.SetValue(game,0);refreshBackpack?.Invoke(game,null);}if(!processing&&waiting.Count>0)StartCoroutine(ProcessNext());}
    void Deposit(int count){for(int i=0;i<count;i++){var log=MakeLog("Queued Log");waiting.Enqueue(log);}RestackWaiting();}
    IEnumerator ProcessNext(){processing=true;GameObject log=waiting.Dequeue();if(log==null){processing=false;yield break;}Vector3 a=log.transform.position,b=sawPoint.position+Vector3.up*.18f,c=output.position+new Vector3(-.65f,.18f,0);float t=0;while(t<1&&log!=null){t+=Time.deltaTime*1.6f;log.transform.position=Vector3.Lerp(a,b,Mathf.SmoothStep(0,1,t));yield return null;}yield return new WaitForSeconds(.12f);if(log!=null)Destroy(log);for(int n=0;n<2;n++)SpawnBoard(c);int boards=(int)boardsField.GetValue(game);boardsField.SetValue(game,boards+2);yield return new WaitForSeconds(.16f);RestackWaiting();processing=false;}
    void SpawnBoard(Vector3 start){var b=GameObject.CreatePrimitive(PrimitiveType.Cube);b.name="Finished Board";int i=finished.Count%36,row=i/6,col=i%6;b.transform.position=output.position+new Vector3((col-2.5f)*.28f,.09f+row*.105f,0);b.transform.localScale=new Vector3(.24f,.075f,.78f);Paint(b,new Color(.79f,.57f,.31f));Destroy(b.GetComponent<Collider>());finished.Add(b);}
    void RestackWaiting(){int i=0;foreach(var log in waiting){if(log==null)continue;int layer=i/6,slot=i%6;log.transform.position=intake.position+new Vector3((slot-2.5f)*.23f,.15f+layer*.18f,0);i++;}}

    void BuildStation(){var root=new GameObject("Reference Lumber Saw");root.transform.position=new Vector3(5.3f,0,1.5f);
        intake=Marker(root.transform,"RAW LOGS",new Vector3(-3.15f,.025f,0),new Vector3(2.4f,.05f,2.45f),new Color(.46f,.34f,.20f));
        output=Marker(root.transform,"FINISHED WOOD",new Vector3(3.15f,.025f,0),new Vector3(2.4f,.05f,2.45f),new Color(.64f,.49f,.28f));
        // Low, open industrial saw like the reference: feed rails -> guarded circular blade -> output rails.
        AddBox(root.transform,"Machine Base",Vector3.zero+new Vector3(0,.20f,0),new Vector3(2.35f,.38f,1.55f),new Color(.18f,.25f,.23f));
        AddBox(root.transform,"Feed Bed",new Vector3(-1.65f,.48f,0),new Vector3(2.5f,.16f,1.18f),new Color(.34f,.39f,.36f));
        AddBox(root.transform,"Output Bed",new Vector3(1.65f,.48f,0),new Vector3(2.5f,.16f,1.18f),new Color(.34f,.39f,.36f));
        for(int side=-1;side<=1;side+=2){AddBox(root.transform,"Feed Rail",new Vector3(-1.65f,.62f,side*.48f),new Vector3(2.55f,.12f,.10f),new Color(.62f,.45f,.18f));AddBox(root.transform,"Output Rail",new Vector3(1.65f,.62f,side*.48f),new Vector3(2.55f,.12f,.10f),new Color(.62f,.45f,.18f));}
        var wheel=GameObject.CreatePrimitive(PrimitiveType.Cylinder);wheel.name="Large Circular Saw Blade";wheel.transform.SetParent(root.transform);wheel.transform.localPosition=new Vector3(0,1.02f,0);wheel.transform.localScale=new Vector3(.72f,.055f,.72f);wheel.transform.localRotation=Quaternion.Euler(90,0,0);Paint(wheel,new Color(.78f,.80f,.79f));Destroy(wheel.GetComponent<Collider>());blade=wheel.transform;
        AddBox(root.transform,"Blade Hub",new Vector3(0,1.02f,-.04f),new Vector3(.18f,.18f,.16f),new Color(.22f,.24f,.24f));
        AddBox(root.transform,"Left Guard",new Vector3(-.62f,.94f,0),new Vector3(.28f,.86f,1.30f),new Color(.82f,.48f,.10f));
        AddBox(root.transform,"Right Guard",new Vector3(.62f,.94f,0),new Vector3(.28f,.86f,1.30f),new Color(.82f,.48f,.10f));
        AddBox(root.transform,"Top Guard",new Vector3(0,1.40f,0),new Vector3(1.52f,.18f,1.30f),new Color(.82f,.48f,.10f));
        AddBox(root.transform,"Motor",new Vector3(0,.40f,-.92f),new Vector3(.82f,.58f,.55f),new Color(.20f,.30f,.29f));
        sawPoint=new GameObject("Saw Feed Point").transform;sawPoint.SetParent(root.transform);sawPoint.localPosition=new Vector3(0,.62f,0);
    }
    void AddBox(Transform p,string n,Vector3 pos,Vector3 scale,Color c){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p);g.transform.localPosition=pos;g.transform.localScale=scale;Paint(g,c);Destroy(g.GetComponent<Collider>());}
    Transform Marker(Transform p,string n,Vector3 pos,Vector3 scale,Color c){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p);g.transform.localPosition=pos;g.transform.localScale=scale;Paint(g,c);Destroy(g.GetComponent<Collider>());return g.transform;}
    GameObject MakeLog(string n){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;g.transform.localScale=new Vector3(.13f,.48f,.13f);g.transform.rotation=Quaternion.Euler(0,0,90);Paint(g,new Color(.52f,.29f,.12f));Destroy(g.GetComponent<Collider>());return g;}
    Material Mat(Color c){Shader s=Shader.Find("Universal Render Pipeline/Lit");if(s==null)s=Shader.Find("Sprites/Default");var m=new Material(s);m.color=c;return m;}void Paint(GameObject o,Color c){var r=o.GetComponent<Renderer>();if(r!=null)r.material=Mat(c);}
}