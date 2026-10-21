using System.Collections;
using UnityEngine;

public class HandAxeBinder : MonoBehaviour
{
    Transform axe, hand, originalParent;
    Vector3 originalPos;
    Quaternion originalRot;
    bool lastActive, bound;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install(){new GameObject("Hand Axe Runtime Binder").AddComponent<HandAxeBinder>();}

    IEnumerator Start()
    {
        for(int i=0;i<300&&!bound;i++){
            var p=GameObject.Find("Lumberjack Player");
            var a=GameObject.Find("Axe Pivot");
            if(p!=null&&a!=null){
                hand=FindHand(p.transform);
                if(hand!=null){
                    axe=a.transform; originalParent=axe.parent; originalPos=axe.localPosition; originalRot=axe.localRotation;
                    bound=true; Debug.Log("Axe hand binding ready: "+hand.name);
                }
            }
            if(!bound)yield return null;
        }
    }

    void LateUpdate()
    {
        if(!bound||axe==null||hand==null)return;
        bool active=axe.gameObject.activeSelf;
        if(active&&!lastActive)StartCoroutine(HandSwing());
        lastActive=active;
    }

    IEnumerator HandSwing()
    {
        // Keep the axe visibly locked to the real right hand while the existing chop timing runs.
        axe.SetParent(hand,false);
        axe.localPosition=new Vector3(.02f,.08f,.02f);
        axe.localRotation=Quaternion.Euler(0,0,95);
        Quaternion rest=hand.localRotation;
        Quaternion wind=rest*Quaternion.Euler(-45,5,-55);
        Quaternion hit=rest*Quaternion.Euler(60,-5,40);
        float t=0;
        while(t<1&&axe!=null&&axe.gameObject.activeSelf){t+=Time.deltaTime/0.14f;hand.localRotation=Quaternion.Slerp(rest,wind,t);yield return null;}
        t=0;
        while(t<1&&axe!=null&&axe.gameObject.activeSelf){t+=Time.deltaTime/0.12f;hand.localRotation=Quaternion.Slerp(wind,hit,t);yield return null;}
        t=0;
        while(t<1&&axe!=null&&axe.gameObject.activeSelf){t+=Time.deltaTime/0.16f;hand.localRotation=Quaternion.Slerp(hit,rest,t);yield return null;}
        hand.localRotation=rest;
        if(axe!=null){axe.SetParent(originalParent,false);axe.localPosition=originalPos;axe.localRotation=originalRot;}
    }

    Transform FindHand(Transform root)
    {
        Transform fallback=null;
        foreach(var t in root.GetComponentsInChildren<Transform>(true)){
            string n=t.name.ToLowerInvariant().Replace(" ","");
            if(n.Contains("righthand")||n.Contains("hand_r")||n.Contains("hand.r")||n.Contains("r_hand"))return t;
            if(fallback==null&&n.Contains("hand")&&(n.Contains("right")||n.EndsWith("r")))fallback=t;
        }
        return fallback;
    }
}