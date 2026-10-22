using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SawmillProcessingLine : MonoBehaviour
{
    LumberPrototypeBootstrap game; Transform player,intake,sawPoint,output,blade;
    readonly Queue<GameObject> waiting=new Queue<GameObject>(); readonly List<GameObject> finished=new List<GameObject>();
    FieldInfo carriedField,boardsField,sawmillField,playerField; MethodInfo refreshBackpack; bool processing;
    readonly Color orange=new Color(.95f,.36f,.06f), dark=new Color(.10f,.12f,.13f), steel=new Color(.34f,.38f,.40f), wood=new Color(.72f,.43f,.20f), board=new Color(.88f,.67f,.38f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install(){new GameObject("Sawmill Processing Line").AddComponent<SawmillProcessingLine>();}
    IEnumerator Start(){for(int i=0;i<600&&game==null;i++){game=FindFirstObjectByType<LumberPrototypeBootstrap>();if(game==null){yield return null;continue;}var t=typeof(LumberPrototypeBootstrap);carriedField=t.GetField("carriedLogs",BindingFlags.Instance|BindingFlags.NonPublic);boardsField=t.GetField("boards",BindingFlags.Instance|BindingFlags.NonPublic);sawmillField=t.GetField("sawmill",BindingFlags.Instance|BindingFlags.NonPublic);playerField=t.GetField("player",BindingFlags.Instance|BindingFlags.NonPublic);refreshBackpack=t.GetMethod("RefreshBackpack",BindingFlags.Instance|BindingFlags.NonPublic);player=playerField!=null?(Transform)playerField.GetValue(game):null;}if(game==null||player==null)yield break;BuildStation();var dummy=new GameObject("Legacy Sawmill Trigger Disabled");dummy.transform.position=new Vector3(9999,0,9999);if(sawmillField!=null)sawmillField.SetValue(game,dummy.transform);}
    void Update(){if(blade!=null&&processing)blade.Rotate(Vector3.forward,720f*Time.deltaTime,Space.Self);if(game==null||player==null||intake==null)return;int carried=(int)carriedField.GetValue(game);Vector3 d=player.position-intake.position;d.y=0;if(carried>0&&d.sqrMagnitude<5.5f){Deposit(carried);carriedField.SetValue(game,0);refreshBackpack?.Invoke(game,null);}if(!processing&&waiting.Count>0)StartCoroutine(ProcessNext());}
    void Deposit(int count){for(int i=0;i<count;i++)waiting.Enqueue(MakeLog("Raw Log"));RestackWaiting();}
    IEnumerator ProcessNext(){processing=true;GameObject log=waiting.Dequeue();if(log==null){processing=false;yield break;}Vector3 a=log.transform.position,b=sawPoint.position+Vector3.up*.13f;yield return Move(log,a,b,.72f);yield return new WaitForSeconds(.10f);if(log!=null)Destroy(log);for(int n=0;n<2;n++)SpawnBoard();int boards=(int)boardsField.GetValue(game);boardsField.SetValue(game,boards+2);yield return new WaitForSeconds(.18f);RestackWaiting();processing=false;}
    IEnumerator Move(GameObject g,Vector3 a,Vector3 b,float seconds){float t=0;while(t<1&&g!=null){t+=Time.deltaTime/seconds;g.transform.position=Vector3.Lerp(a,b,Mathf.SmoothStep(0,1,t));yield return null;}}
    void SpawnBoard(){var b=GameObject.CreatePrimitive(PrimitiveType.Cube);b.name="Finished Board";int i=finished.Count%42,row=i/7,col=i%7;b.transform.position=output.position+new Vector3((col-3f)*.24f,.08f+row*.10f,0);b.transform.localScale=new Vector3(.22f,.07f,.72f);Paint(b,board);Destroy(b.GetComponent<Collider>());finished.Add(b);}
    void RestackWaiting(){int i=0;foreach(var log in waiting){if(log==null)continue;int layer=i/7,slot=i%7;log.transform.position=intake.position+new Vector3((slot-3f)*.22f,.13f+layer*.17f,0);i++;}}

    void BuildStation(){var root=new GameObject("Cartoon Lumber Processing Line");root.transform.position=new Vector3(5.1f,0,1.3f);
        intake=Zone(root.transform,"RAW STORAGE",new Vector3(-4.15f,.025f,0),new Vector3(2.45f,.05f,2.45f));
        output=Zone(root.transform,"BOARD STORAGE",new Vector3(4.15f,.025f,0),new Vector3(2.45f,.05f,2.45f));
        // Long black belt, deliberately plain: no arrows or copied markings.
        AddBox(root.transform,"Black Feed Conveyor",new Vector3(-2.05f,.48f,0),new Vector3(3.45f,.15f,1.12f),dark);
        AddBox(root.transform,"Black Output Conveyor",new Vector3(2.05f,.48f,0),new Vector3(3.45f,.15f,1.12f),dark);
        for(int x=-3;x<=3;x+=2){Leg(root.transform,new Vector3(x*.66f,.23f,-.43f));Leg(root.transform,new Vector3(x*.66f,.23f,.43f));}
        for(int side=-1;side<=1;side+=2){AddBox(root.transform,"Conveyor Rail",new Vector3(-2.05f,.62f,side*.52f),new Vector3(3.55f,.11f,.10f),steel);AddBox(root.transform,"Conveyor Rail",new Vector3(2.05f,.62f,side*.52f),new Vector3(3.55f,.11f,.10f),steel);}
        // Central low-poly saw housing inspired by the reference image.
        AddBox(root.transform,"Saw Body",new Vector3(0,.46f,0),new Vector3(1.48f,.72f,1.38f),steel);
        AddBox(root.transform,"Orange Left Tower",new Vector3(-.72f,.76f,0),new Vector3(.22f,1.05f,1.48f),orange);
        AddBox(root.transform,"Orange Right Tower",new Vector3(.72f,.76f,0),new Vector3(.22f,1.05f,1.48f),orange);
        AddBox(root.transform,"Orange Top Guard",new Vector3(0,1.27f,0),new Vector3(1.62f,.22f,1.48f),orange);
        var wheel=GameObject.CreatePrimitive(PrimitiveType.Cylinder);wheel.name="Circular Blade";wheel.transform.SetParent(root.transform);wheel.transform.localPosition=new Vector3(0,.92f,0);wheel.transform.localScale=new Vector3(.72f,.055f,.72f);wheel.transform.localRotation=Quaternion.Euler(90,0,0);Paint(wheel,new Color(.76f,.78f,.79f));Destroy(wheel.GetComponent<Collider>());blade=wheel.transform;
        var hub=GameObject.CreatePrimitive(PrimitiveType.Cylinder);hub.name="Blade Hub";hub.transform.SetParent(root.transform);hub.transform.localPosition=new Vector3(0,.92f,-.075f);hub.transform.localScale=new Vector3(.16f,.09f,.16f);hub.transform.localRotation=Quaternion.Euler(90,0,0);Paint(hub,steel);Destroy(hub.GetComponent<Collider>());
        AddBox(root.transform,"Motor",new Vector3(0,.34f,-.92f),new Vector3(.76f,.56f,.48f),dark);
        AddBox(root.transform,"Control Post",new Vector3(.96f,.53f,-.70f),new Vector3(.25f,.82f,.28f),steel);
        AddBox(root.transform,"Control Button",new Vector3(.96f,.77f,-.86f),new Vector3(.12f,.12f,.05f),orange);
        sawPoint=new GameObject("Saw Feed Point").transform;sawPoint.SetParent(root.transform);sawPoint.localPosition=new Vector3(0,.62f,0);
    }
    void Leg(Transform p,Vector3 pos){AddBox(p,"Conveyor Leg",pos,new Vector3(.13f,.46f,.13f),steel);}
    Transform Zone(Transform p,string n,Vector3 pos,Vector3 scale){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p);g.transform.localPosition=pos;g.transform.localScale=scale;Paint(g,new Color(.66f,.55f,.36f));Destroy(g.GetComponent<Collider>());for(int side=-1;side<=1;side+=2){AddBox(p,n+" Border",pos+new Vector3(0,.045f,side*scale.z*.5f),new Vector3(scale.x,.07f,.07f),new Color(.92f,.88f,.72f));AddBox(p,n+" Border",pos+new Vector3(side*scale.x*.5f,.045f,0),new Vector3(.07f,.07f,scale.z),new Color(.92f,.88f,.72f));}return g.transform;}
    void AddBox(Transform p,string n,Vector3 pos,Vector3 scale,Color c){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p);g.transform.localPosition=pos;g.transform.localScale=scale;Paint(g,c);Destroy(g.GetComponent<Collider>());}
    GameObject MakeLog(string n){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;g.transform.localScale=new Vector3(.14f,.54f,.14f);g.transform.rotation=Quaternion.Euler(0,0,90);Paint(g,wood);Destroy(g.GetComponent<Collider>());return g;}
    Material Mat(Color c){Shader s=Shader.Find("Universal Render Pipeline/Lit");if(s==null)s=Shader.Find("Sprites/Default");var m=new Material(s);m.color=c;return m;}void Paint(GameObject o,Color c){var r=o.GetComponent<Renderer>();if(r!=null)r.material=Mat(c);}
}