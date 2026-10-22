using System.Reflection;
using UnityEngine;

public class GameplayPaceTuner : MonoBehaviour
{
    LumberPrototypeBootstrap game;
    MethodInfo addLogs;
    int lastTreeCount = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install(){ new GameObject("Gameplay Pace Tuner").AddComponent<GameplayPaceTuner>(); }

    void Update()
    {
        if(game==null){
            game=FindFirstObjectByType<LumberPrototypeBootstrap>();
            if(game==null)return;
            addLogs=typeof(LumberPrototypeBootstrap).GetMethod("AddLogs",BindingFlags.Instance|BindingFlags.NonPublic);
        }

        var trees=FindObjectsByType<TreeNode>(FindObjectsSortMode.None);
        foreach(var tree in trees) if(!tree.felled && tree.hitsLeft>2) tree.hitsLeft=2;

        int count=trees.Length;
        if(lastTreeCount>=0 && count<lastTreeCount && addLogs!=null){
            int felled=lastTreeCount-count;
            // Base game gives 3 logs. Add 5 more so every tree gives 8 logs.
            for(int i=0;i<felled;i++) addLogs.Invoke(game,new object[]{5});
        }
        lastTreeCount=count;
    }
}
