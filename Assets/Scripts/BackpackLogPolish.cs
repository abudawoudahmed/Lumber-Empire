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
            int layer=i/4,slot=i%4;
            log.localPosition=new Vector3((slot-1.5f)*.09f,layer*.095f,0);
            log.localRotation=Quaternion.Euler(0,0,90);
            // Force one fixed compact size every frame so later logs never grow back.
            log.localScale=new Vector3(.065f,.17f,.065f);
            i++;
        }
    }
}
