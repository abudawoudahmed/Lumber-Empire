using UnityEngine;

public class LumberPrototypeBootstrap : MonoBehaviour
{
    void Start()
    {
        RenderSettings.ambientLight = new Color(0.65f,0.72f,0.62f);
        MakeGround(); MakeLight(); MakeCamera(); MakePlayer(); MakeForest(); MakeSawmill();
    }
    void MakeGround(){ var g=GameObject.CreatePrimitive(PrimitiveType.Plane); g.name="Forest Clearing"; g.transform.localScale=new Vector3(3,1,3); g.GetComponent<Renderer>().material.color=new Color(0.34f,0.62f,0.25f); }
    void MakeLight(){ var o=new GameObject("Warm Sun"); var l=o.AddComponent<Light>(); l.type=LightType.Directional; l.intensity=1.25f; l.color=new Color(1f,.9f,.72f); o.transform.rotation=Quaternion.Euler(48,-35,0); }
    void MakeCamera(){ var o=new GameObject("Main Camera"); o.tag="MainCamera"; var c=o.AddComponent<Camera>(); c.orthographic=true; c.orthographicSize=8; o.transform.position=new Vector3(10,12,-10); o.transform.rotation=Quaternion.Euler(38,-42,0); }
    void MakePlayer(){ var root=new GameObject("Lumberjack"); root.transform.position=new Vector3(0,1,0); var body=GameObject.CreatePrimitive(PrimitiveType.Capsule); body.transform.SetParent(root.transform); body.transform.localScale=new Vector3(.65f,1f,.65f); body.GetComponent<Renderer>().material.color=new Color(.12f,.32f,.62f); var head=GameObject.CreatePrimitive(PrimitiveType.Sphere); head.transform.SetParent(root.transform); head.transform.localPosition=new Vector3(0,1.25f,0); head.transform.localScale=Vector3.one*.65f; head.GetComponent<Renderer>().material.color=new Color(.88f,.64f,.45f); }
    void MakeForest(){ Vector3[] p={new(-7,0,-5),new(-5,0,-7),new(-3,0,-6),new(5,0,-6),new(7,0,-3),new(-7,0,2),new(-6,0,6),new(5,0,6),new(7,0,4),new(3,0,7)}; foreach(var v in p) Tree(v); }
    void Tree(Vector3 p){ var r=new GameObject("Tree"); r.transform.position=p; var t=GameObject.CreatePrimitive(PrimitiveType.Cylinder); t.transform.SetParent(r.transform); t.transform.localPosition=new Vector3(0,1.4f,0); t.transform.localScale=new Vector3(.42f,1.4f,.42f); t.GetComponent<Renderer>().material.color=new Color(.42f,.23f,.1f); for(int i=0;i<3;i++){var f=GameObject.CreatePrimitive(PrimitiveType.Sphere);f.transform.SetParent(r.transform);f.transform.localPosition=new Vector3((i-1)*.55f,3.1f+(i%2)*.35f,0);f.transform.localScale=new Vector3(1.5f,1.25f,1.5f);f.GetComponent<Renderer>().material.color=new Color(.12f,.48f,.16f);}}
    void MakeSawmill(){ var root=new GameObject("Old Sawmill"); root.transform.position=new Vector3(4,0,1); var table=GameObject.CreatePrimitive(PrimitiveType.Cube); table.transform.SetParent(root.transform); table.transform.localPosition=new Vector3(0,.65f,0); table.transform.localScale=new Vector3(4,.35f,1.7f); table.GetComponent<Renderer>().material.color=new Color(.38f,.23f,.12f); var blade=GameObject.CreatePrimitive(PrimitiveType.Cylinder); blade.transform.SetParent(root.transform); blade.transform.localPosition=new Vector3(0,1.45f,0); blade.transform.localScale=new Vector3(1.25f,.12f,1.25f); blade.transform.rotation=Quaternion.Euler(90,0,0); blade.GetComponent<Renderer>().material.color=Color.gray; }
}
