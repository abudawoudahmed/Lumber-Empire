using System.Collections;
using System.Reflection;
using UnityEngine;

public class ReferenceViewPolish : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install(){ new GameObject("Reference View Polish").AddComponent<ReferenceViewPolish>(); }

    IEnumerator Start()
    {
        LumberPrototypeBootstrap game=null;
        for(int i=0;i<300 && game==null;i++){game=FindFirstObjectByType<LumberPrototypeBootstrap>();if(game==null)yield return null;}
        yield return null;

        Camera cam=Camera.main;
        if(cam!=null){
            cam.orthographic=true;
            cam.orthographicSize=10.8f;
            cam.transform.rotation=Quaternion.Euler(43f,-42f,0f);
        }

        // Remove the oversized old covered sawmill. The open table-saw line remains.
        var old=GameObject.Find("SAWMILL");
        if(old!=null){
            var line=FindFirstObjectByType<SawmillProcessingLine>();
            if(line!=null)Destroy(old);
        }

        // Make the work area read as an open yard like the reference video.
        var yard=GameObject.CreatePrimitive(PrimitiveType.Cube);
        yard.name="Open Production Yard";
        yard.transform.position=new Vector3(5.3f,.025f,1.5f);
        yard.transform.localScale=new Vector3(11f,.05f,9f);
        Paint(yard,new Color(.48f,.57f,.61f));
        Destroy(yard.GetComponent<Collider>());

        // Keep production props above the yard surface.
        var station=GameObject.Find("Table Saw Processing Area");
        if(station!=null)station.transform.position=new Vector3(5.3f,.06f,1.5f);
    }

    Material Mat(Color c){Shader s=Shader.Find("Universal Render Pipeline/Lit");if(s==null)s=Shader.Find("Sprites/Default");var m=new Material(s);m.color=c;return m;}
    void Paint(GameObject o,Color c){var r=o.GetComponent<Renderer>();if(r!=null)r.material=Mat(c);}
}
