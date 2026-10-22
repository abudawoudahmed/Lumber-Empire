using UnityEngine;

public class ReferenceTreePolish : MonoBehaviour
{
    readonly Color trunk=new Color(.43f,.24f,.10f), dark=new Color(.08f,.34f,.12f), mid=new Color(.12f,.46f,.16f), light=new Color(.18f,.56f,.20f);
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install(){new GameObject("Reference Pine Polish").AddComponent<ReferenceTreePolish>();}
    void Start(){Invoke(nameof(Polish),.35f);}
    void Polish(){foreach(var node in FindObjectsByType<TreeNode>(FindObjectsSortMode.None))MakePine(node.transform);}
    void MakePine(Transform root){if(root==null)return;for(int i=root.childCount-1;i>=0;i--)Destroy(root.GetChild(i).gameObject);
        float s=Random.Range(.92f,1.08f);root.localScale=Vector3.one*s;
        var t=GameObject.CreatePrimitive(PrimitiveType.Cylinder);t.name="Pine Trunk";t.transform.SetParent(root,false);t.transform.localPosition=new Vector3(0,.48f,0);t.transform.localScale=new Vector3(.32f,.48f,.32f);Paint(t,trunk);Destroy(t.GetComponent<Collider>());
        Cone(root,"Pine Lower",new Vector3(0,.95f,0),1.20f,1.55f,dark);
        Cone(root,"Pine Middle",new Vector3(0,1.65f,0),1.02f,1.45f,mid);
        Cone(root,"Pine Upper",new Vector3(0,2.28f,0),.78f,1.25f,light);
        Cone(root,"Pine Tip",new Vector3(0,2.82f,0),.50f,.95f,mid);
    }
    void Cone(Transform p,string n,Vector3 pos,float radius,float height,Color c){
        var g=new GameObject(n);g.transform.SetParent(p,false);g.transform.localPosition=pos;
        var mf=g.AddComponent<MeshFilter>();var mr=g.AddComponent<MeshRenderer>();
        const int sides=8;var v=new Vector3[sides+2];var tri=new int[sides*6];
        v[0]=new Vector3(0,height*.5f,0);v[1]=new Vector3(0,-height*.5f,0);
        for(int i=0;i<sides;i++){float a=Mathf.PI*2*i/sides;v[i+2]=new Vector3(Mathf.Cos(a)*radius,-height*.5f,Mathf.Sin(a)*radius);int j=(i+1)%sides;int k=i*6;tri[k]=0;tri[k+1]=i+2;tri[k+2]=j+2;tri[k+3]=1;tri[k+4]=j+2;tri[k+5]=i+2;}
        var mesh=new Mesh{name=n+" Mesh",vertices=v,triangles=tri};mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=mesh;PaintRenderer(mr,c);
    }
    void Paint(GameObject o,Color c){var r=o.GetComponent<Renderer>();if(r!=null)PaintRenderer(r,c);}
    void PaintRenderer(Renderer r,Color c){Shader s=Shader.Find("Universal Render Pipeline/Lit");if(s==null)s=Shader.Find("Sprites/Default");var m=new Material(s);m.color=c;r.material=m;}
}