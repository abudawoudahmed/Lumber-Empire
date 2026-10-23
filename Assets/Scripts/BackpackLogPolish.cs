using UnityEngine;

public class BackpackLogPolish : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install(){new GameObject("Compact Backpack Logs").AddComponent<BackpackLogPolish>();}

    void LateUpdate()
    {
        var anchor=GameObject.Find("Log Backpack");
        if(anchor==null)return;
        anchor.transform.localPosition=new Vector3(0,.88f,-.27f);
        int i=0;
        foreach(Transform log in anchor.transform)
        {
            if(log.name!="Back Log")continue;
            int layer=i/3, slot=i%3;
            log.localPosition=new Vector3((slot-1f)*.12f,layer*.13f,0);
            log.localRotation=Quaternion.Euler(0,0,90);
            // Every piece uses exactly the same short, chunky proportions.
            log.localScale=new Vector3(.095f,.20f,.095f);
            i++;
        }
    }
}
