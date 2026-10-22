using System.Collections;
using UnityEngine;

public class HandAxeBinder : MonoBehaviour
{
    Transform axe, hand, upperArm, lowerArm, spine, originalParent;
    Vector3 originalPos;
    Quaternion originalRot;
    bool lastActive, bound;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install(){new GameObject("Hand Axe Runtime Binder").AddComponent<HandAxeBinder>();}

    IEnumerator Start()
    {
        for(int i=0;i<600&&!bound;i++){
            var p=GameObject.Find("Lumberjack Player");
            var a=FindInactiveByName("Axe Pivot");
            if(p!=null&&a!=null){
                Animator anim=p.GetComponentInChildren<Animator>(true);
                if(anim!=null&&anim.isHuman){
                    hand=anim.GetBoneTransform(HumanBodyBones.RightHand);
                    upperArm=anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
                    lowerArm=anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
                    spine=anim.GetBoneTransform(HumanBodyBones.Spine);
                }
                if(hand==null)hand=FindHand(p.transform);
                if(hand!=null){axe=a;originalParent=axe.parent;originalPos=axe.localPosition;originalRot=axe.localRotation;bound=true;}
            }
            if(!bound)yield return null;
        }
    }

    Transform FindInactiveByName(string target){foreach(var t in Resources.FindObjectsOfTypeAll<Transform>())if(t.gameObject.scene.IsValid()&&t.name==target)return t;return null;}

    void LateUpdate(){if(!bound||axe==null||hand==null)return;bool active=axe.gameObject.activeSelf;if(active&&!lastActive)StartCoroutine(HandSwing());lastActive=active;}

    IEnumerator HandSwing()
    {
        axe.SetParent(hand,false);
        axe.localPosition=new Vector3(.015f,.035f,.015f);
        axe.localRotation=Quaternion.Euler(2,8,92);

        Quaternion h0=hand.localRotation;
        Quaternion u0=upperArm!=null?upperArm.localRotation:Quaternion.identity;
        Quaternion l0=lowerArm!=null?lowerArm.localRotation:Quaternion.identity;
        Quaternion s0=spine!=null?spine.localRotation:Quaternion.identity;

        Quaternion hWind=h0*Quaternion.Euler(-25,0,-25), hHit=h0*Quaternion.Euler(45,-8,28);
        Quaternion uWind=u0*Quaternion.Euler(-42,18,-38), uHit=u0*Quaternion.Euler(54,-10,30);
        Quaternion lWind=l0*Quaternion.Euler(-38,0,-22), lHit=l0*Quaternion.Euler(38,0,20);
        Quaternion sWind=s0*Quaternion.Euler(-8,-18,-4), sHit=s0*Quaternion.Euler(12,18,4);

        yield return Pose(.16f,h0,hWind,u0,uWind,l0,lWind,s0,sWind);
        yield return Pose(.10f,hWind,hHit,uWind,uHit,lWind,lHit,sWind,sHit);
        yield return Pose(.14f,hHit,h0,uHit,u0,lHit,l0,sHit,s0);

        hand.localRotation=h0;if(upperArm!=null)upperArm.localRotation=u0;if(lowerArm!=null)lowerArm.localRotation=l0;if(spine!=null)spine.localRotation=s0;
        if(axe!=null){axe.SetParent(originalParent,false);axe.localPosition=originalPos;axe.localRotation=originalRot;}
    }

    IEnumerator Pose(float duration,Quaternion ha,Quaternion hb,Quaternion ua,Quaternion ub,Quaternion la,Quaternion lb,Quaternion sa,Quaternion sb)
    {
        float t=0;while(t<1&&axe!=null&&axe.gameObject.activeSelf){t+=Time.deltaTime/duration;float e=Mathf.SmoothStep(0,1,t);hand.localRotation=Quaternion.Slerp(ha,hb,e);if(upperArm!=null)upperArm.localRotation=Quaternion.Slerp(ua,ub,e);if(lowerArm!=null)lowerArm.localRotation=Quaternion.Slerp(la,lb,e);if(spine!=null)spine.localRotation=Quaternion.Slerp(sa,sb,e);yield return null;}
    }

    Transform FindHand(Transform root){Transform fallback=null;foreach(var t in root.GetComponentsInChildren<Transform>(true)){string n=t.name.ToLowerInvariant().Replace(" ","");if(n.Contains("righthand")||n.Contains("hand_r")||n.Contains("hand.r")||n.Contains("r_hand"))return t;if(fallback==null&&n.Contains("hand")&&(n.Contains("right")||n.EndsWith("r")))fallback=t;}return fallback;}
}
