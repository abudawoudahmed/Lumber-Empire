using System.Collections.Generic;
using UnityEngine;

public class ForestFeelPolish : MonoBehaviour
{
    readonly Dictionary<TreeNode,TreeData> trees=new Dictionary<TreeNode,TreeData>();
    class TreeData{public Quaternion baseRot;public float phase;public float speed;public float amount;}

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install(){new GameObject("Forest Feel Polish").AddComponent<ForestFeelPolish>();}

    void Start(){InvokeRepeating(nameof(Discover),.15f,1f);}

    void Discover()
    {
        foreach(var tree in FindObjectsByType<TreeNode>(FindObjectsSortMode.None)){
            if(trees.ContainsKey(tree))continue;
            int seed=tree.GetInstanceID();Random.State old=Random.state;Random.InitState(seed);
            tree.transform.localScale=new Vector3(Random.Range(.88f,1.12f),Random.Range(.90f,1.16f),Random.Range(.88f,1.12f));
            tree.transform.rotation=Quaternion.Euler(0,Random.Range(0f,360f),Random.Range(-1.5f,1.5f));
            var renderers=tree.GetComponentsInChildren<Renderer>(true);
            for(int i=0;i<renderers.Length;i++){
                Transform t=renderers[i].transform;
                if(t==tree.transform)continue;
                if(t.localPosition.y>2.2f){
                    t.localPosition+=new Vector3(Random.Range(-.18f,.18f),Random.Range(-.12f,.20f),Random.Range(-.18f,.18f));
                    t.localScale=Vector3.Scale(t.localScale,new Vector3(Random.Range(.88f,1.15f),Random.Range(.88f,1.18f),Random.Range(.88f,1.15f)));
                }
            }
            trees[tree]=new TreeData{baseRot=tree.transform.rotation,phase=Random.Range(0f,6.28f),speed=Random.Range(.55f,.9f),amount=Random.Range(.45f,1.05f)};
            Random.state=old;
        }
    }

    void Update()
    {
        var dead=new List<TreeNode>();
        foreach(var kv in trees){
            if(kv.Key==null){dead.Add(kv.Key);continue;}
            if(kv.Key.felled)continue;
            TreeData d=kv.Value;float sway=Mathf.Sin(Time.time*d.speed+d.phase)*d.amount;
            kv.Key.transform.rotation=d.baseRot*Quaternion.Euler(sway*.32f,0,sway);
        }
        foreach(var k in dead)trees.Remove(k);
    }
}
