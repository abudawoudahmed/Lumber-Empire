using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GameplayPaceTuner : MonoBehaviour
{
    LumberPrototypeBootstrap game;
    MethodInfo addLogs;
    FieldInfo carriedLogsField;
    FieldInfo playerField;
    int lastTreeCount = -1;
    int lastVisualCount = -1;
    Transform extraStack;
    readonly List<GameObject> extraLogs = new List<GameObject>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install(){ new GameObject("Gameplay Pace Tuner").AddComponent<GameplayPaceTuner>(); }

    void Update()
    {
        if(game==null){
            game=FindFirstObjectByType<LumberPrototypeBootstrap>();
            if(game==null)return;
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            addLogs=typeof(LumberPrototypeBootstrap).GetMethod("AddLogs",flags);
            carriedLogsField=typeof(LumberPrototypeBootstrap).GetField("carriedLogs",flags);
            playerField=typeof(LumberPrototypeBootstrap).GetField("player",flags);
        }

        var trees=FindObjectsByType<TreeNode>(FindObjectsSortMode.None);
        foreach(var tree in trees) if(!tree.felled && tree.hitsLeft>1) tree.hitsLeft=1;

        int count=trees.Length;
        if(lastTreeCount>=0 && count<lastTreeCount && addLogs!=null){
            int felled=lastTreeCount-count;
            // Base game gives 3 logs; add 5 more so each tree gives 8 total.
            for(int i=0;i<felled;i++) addLogs.Invoke(game,new object[]{5});
        }
        lastTreeCount=count;

        int carried=carriedLogsField!=null?(int)carriedLogsField.GetValue(game):0;
        if(carried!=lastVisualCount){ RefreshExtraStack(carried); lastVisualCount=carried; }
    }

    void RefreshExtraStack(int carried)
    {
        foreach(var g in extraLogs) if(g!=null) Destroy(g);
        extraLogs.Clear();
        if(carried<=12)return;
        Transform player=playerField!=null?playerField.GetValue(game) as Transform:null;
        if(player==null)return;
        if(extraStack==null){
            var root=new GameObject("Extra Wood Stack");
            root.transform.SetParent(player,false);
            root.transform.localPosition=new Vector3(0,1.0f,-.42f);
            extraStack=root.transform;
        }
        int visible=Mathf.Min(carried,32);
        for(int i=12;i<visible;i++){
            var log=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.name="Stack Log";
            log.transform.SetParent(extraStack,false);
            int layer=i/4,slot=i%4;
            log.transform.localPosition=new Vector3((slot-1.5f)*.18f,layer*.20f,0);
            log.transform.localRotation=Quaternion.Euler(0,0,90);
            log.transform.localScale=new Vector3(.12f,.42f,.12f);
            var r=log.GetComponent<Renderer>();
            if(r!=null){var shader=Shader.Find("Universal Render Pipeline/Lit");if(shader==null)shader=Shader.Find("Sprites/Default");var m=new Material(shader);m.color=new Color(.57f,.33f,.14f);r.material=m;}
            Destroy(log.GetComponent<Collider>());
            extraLogs.Add(log);
        }
    }
}
