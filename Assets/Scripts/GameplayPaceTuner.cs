using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GameplayPaceTuner : MonoBehaviour
{
    LumberPrototypeBootstrap game;
    MethodInfo addLogs, refreshBackpack;
    FieldInfo carriedLogsField;
    readonly Dictionary<TreeNode,int> previousHits = new Dictionary<TreeNode,int>();
    int previousTreeCount = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install(){ new GameObject("Three Hit Harvest Balance").AddComponent<GameplayPaceTuner>(); }

    void Update()
    {
        if(game==null)
        {
            game=FindFirstObjectByType<LumberPrototypeBootstrap>();
            if(game==null)return;
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            addLogs=typeof(LumberPrototypeBootstrap).GetMethod("AddLogs",flags);
            refreshBackpack=typeof(LumberPrototypeBootstrap).GetMethod("RefreshBackpack",flags);
            carriedLogsField=typeof(LumberPrototypeBootstrap).GetField("carriedLogs",flags);
        }

        var trees=FindObjectsByType<TreeNode>(FindObjectsSortMode.None);
        foreach(var tree in trees)
        {
            if(tree==null||tree.felled)continue;
            int old;
            if(!previousHits.TryGetValue(tree,out old)) previousHits[tree]=tree.hitsLeft;
            else if(tree.hitsLeft<old)
            {
                int hits=old-tree.hitsLeft;
                for(int i=0;i<hits;i++) addLogs?.Invoke(game,new object[]{1});
                previousHits[tree]=tree.hitsLeft;
            }
        }

        // The original prototype awards three logs when the tree object disappears.
        // Remove that legacy reward: the player already received exactly one log per hit.
        if(previousTreeCount>=0 && trees.Length<previousTreeCount && carriedLogsField!=null)
        {
            int removed=previousTreeCount-trees.Length;
            int carried=(int)carriedLogsField.GetValue(game);
            carriedLogsField.SetValue(game,Mathf.Max(0,carried-3*removed));
            refreshBackpack?.Invoke(game,null);
        }
        previousTreeCount=trees.Length;

        var oldExtra=GameObject.Find("Extra Wood Stack");
        if(oldExtra!=null)Destroy(oldExtra);
    }
}
