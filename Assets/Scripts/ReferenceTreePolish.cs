using UnityEngine;

public class ReferenceTreePolish : MonoBehaviour
{
    readonly Color trunk=new Color(.43f,.24f,.10f), dark=new Color(.08f,.34f,.12f), mid=new Color(.12f,.46f,.16f), light=new Color(.18f,.56f,.20f);
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install(){new GameObject("Reference Pine Polish").AddComponent<ReferenceTreePolish>();}
    void Start(){Invoke(nameof(Polish),.35f);}
    void Polish(){foreach(var node in FindObjectsByType<TreeNode>(FindObjectsSortMode.None))MakePine(node.transform);}
    void MakePine(Transform root){if(root==null)return;for(int i=root.childCount-1;i>=0;i--)Destroy(root.GetChild(i).gameObject);
        float s=Random.Range(.92f,1.08f);root.localScale=Vector3.one*s;
        var t=GameObject.CreatePrimitive(PrimitiveType.Cylinder);t.name="Pine Trunk";t.transform.SetParent(root,false);t.transform.localPosition=new Vector3(0,1.35f,0);t.transform.localScale=new Vector3(.30f,1.35f,.30f);Paint(t,trunk);Destroy(t.GetComponent<Collider>());
        Cone(root,"Pine Lower",new Vector3(0,2.15f,0),new Vector3(2.05f,1.55f,2.05f),dark);
        Cone(root,"Pine Middle",new Vector3(0,2.90f,0),new Vector3(1.62f,1.42f,1.62f),mid);
        Cone(root,"Pine Upper",new Vector3(0,3.58f,0),new Vector3(1.15f,1.25f,1.15f),light);
        Cone(root,"Pine Tip",new Vector3(0,4.08f,0),new Vector3(.68f,.88f,.68f),mid);
    }
    void Cone(Transform p,string n,Vector3 pos,Vector3 scale,Color c){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;Paint(g,c);Destroy(g.GetComponent<Collider>());}
    void Paint(GameObject o,Color c){var r=o.GetComponent<Renderer>();if(r==null)return;Shader s=Shader.Find("Universal Render Pipeline/Lit");if(s==null)s=Shader.Find("Sprites/Default");var m=new Material(s);m.color=c;r.material=m;}
}