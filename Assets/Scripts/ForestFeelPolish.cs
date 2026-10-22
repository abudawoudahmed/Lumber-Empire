using System.Collections.Generic;
using UnityEngine;

public class ForestFeelPolish : MonoBehaviour
{
    class TreeData { public TreeNode tree; public Quaternion baseRot; public float phase, speed, amount; }
    readonly List<TreeData> trees = new List<TreeData>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install(){ new GameObject("Forest Feel Polish").AddComponent<ForestFeelPolish>(); }

    void Start(){ InvokeRepeating("Discover", .15f, 1f); }

    bool Known(TreeNode tree){ for(int i=0;i<trees.Count;i++) if(trees[i].tree==tree) return true; return false; }

    void Discover()
    {
        TreeNode[] found=FindObjectsByType<TreeNode>(FindObjectsSortMode.None);
        for(int n=0;n<found.Length;n++){
            TreeNode tree=found[n]; if(tree==null||Known(tree))continue;
            float x=.94f+((n*17)%13)*.01f;
            float y=.94f+((n*23)%17)*.012f;
            float z=.94f+((n*11)%13)*.01f;
            tree.transform.localScale=new Vector3(x,y,z);
            tree.transform.rotation=Quaternion.Euler(0,(n*47)%360,((n%5)-2)*.45f);
            Renderer[] renderers=tree.GetComponentsInChildren<Renderer>(true);
            for(int i=0;i<renderers.Length;i++){
                Transform t=renderers[i].transform;
                if(t!=tree.transform&&t.localPosition.y>2.2f){
                    float side=((i+n)%3-1)*.11f;
                    t.localPosition+=new Vector3(side,((i+n)%2)*.09f,-side*.55f);
                }
            }
            TreeData d=new TreeData(); d.tree=tree; d.baseRot=tree.transform.rotation; d.phase=n*.73f; d.speed=.55f+(n%4)*.08f; d.amount=.45f+(n%5)*.11f; trees.Add(d);
        }
    }

    void Update()
    {
        for(int i=trees.Count-1;i>=0;i--){
            TreeData d=trees[i]; if(d.tree==null){trees.RemoveAt(i);continue;} if(d.tree.felled)continue;
            float sway=Mathf.Sin(Time.time*d.speed+d.phase)*d.amount;
            d.tree.transform.rotation=d.baseRot*Quaternion.Euler(sway*.25f,0,sway);
        }
    }
}
