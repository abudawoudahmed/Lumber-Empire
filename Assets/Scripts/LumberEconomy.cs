using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class LumberEconomy : MonoBehaviour
{
    LumberPrototypeBootstrap game; SawmillProcessingLine line; Transform player,sellZone,upgradeZone;
    FieldInfo boardsField,playerField; int coins; int sawLevel=1; float nextSell,nextUpgrade; readonly List<GameObject> soldVisuals=new List<GameObject>();
    GUIStyle moneyStyle,tipStyle;
    const int BoardPrice=5, UpgradeCost=60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install(){new GameObject("Lumber Economy").AddComponent<LumberEconomy>();}

    void Start(){Invoke(nameof(Setup),1f);}
    void Setup(){game=FindFirstObjectByType<LumberPrototypeBootstrap>();line=FindFirstObjectByType<SawmillProcessingLine>();if(game==null)return;var f=BindingFlags.Instance|BindingFlags.NonPublic;boardsField=typeof(LumberPrototypeBootstrap).GetField("boards",f);playerField=typeof(LumberPrototypeBootstrap).GetField("player",f);player=playerField!=null?playerField.GetValue(game) as Transform:null;BuildZones();}

    void Update(){if(game==null||player==null||boardsField==null)return;int boards=(int)boardsField.GetValue(game);if(boards>0&&Near(sellZone,2.15f)&&Time.time>=nextSell){nextSell=Time.time+.18f;boardsField.SetValue(game,boards-1);coins+=BoardPrice;SellPop();}
        if(sawLevel==1&&coins>=UpgradeCost&&Near(upgradeZone,2.15f)&&Time.time>=nextUpgrade){nextUpgrade=Time.time+1f;coins-=UpgradeCost;sawLevel=2;if(line!=null)line.SetUpgradeLevel(2);UpgradePop();}}

    bool Near(Transform t,float r){if(t==null)return false;Vector3 d=player.position-t.position;d.y=0;return d.sqrMagnitude<r*r;}
    void BuildZones(){var root=new GameObject("Economy Yard");root.transform.position=new Vector3(9.2f,0,3.2f);sellZone=Pad(root.transform,"SELL BOARDS",new Vector3(4.0f,.03f,-4.2f),new Color(.22f,.68f,.28f));upgradeZone=Pad(root.transform,"SAW UPGRADE",new Vector3(0,.03f,-3.8f),new Color(.95f,.58f,.08f));}
    Transform Pad(Transform p,string n,Vector3 pos,Color c){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p);g.transform.localPosition=pos;g.transform.localScale=new Vector3(2.1f,.06f,2.1f);Paint(g,c);Destroy(g.GetComponent<Collider>());for(int s=-1;s<=1;s+=2){Box(p,pos+new Vector3(0,.06f,s*1.05f),new Vector3(2.15f,.09f,.08f),Color.white);Box(p,pos+new Vector3(s*1.05f,.06f,0),new Vector3(.08f,.09f,2.15f),Color.white);}return g.transform;}
    void Box(Transform p,Vector3 pos,Vector3 scale,Color c){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.transform.SetParent(p);g.transform.localPosition=pos;g.transform.localScale=scale;Paint(g,c);Destroy(g.GetComponent<Collider>());}
    void SellPop(){var c=GameObject.CreatePrimitive(PrimitiveType.Cylinder);c.name="Coin Pop";c.transform.position=sellZone.position+Vector3.up*.35f;c.transform.localScale=new Vector3(.18f,.035f,.18f);c.transform.rotation=Quaternion.Euler(90,0,0);Paint(c,new Color(1f,.78f,.08f));Destroy(c.GetComponent<Collider>());Destroy(c,.35f);}
    void UpgradePop(){for(int i=0;i<8;i++){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.transform.position=upgradeZone.position+Vector3.up*(.25f+i*.06f);g.transform.localScale=Vector3.one*.10f;Paint(g,new Color(1f,.65f,.08f));Destroy(g.GetComponent<Collider>());Destroy(g,.8f);}}
    void OnGUI(){if(moneyStyle==null){moneyStyle=new GUIStyle(GUI.skin.box){fontSize=24,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};tipStyle=new GUIStyle(GUI.skin.box){fontSize=17,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};}GUI.Box(new Rect(Screen.width-210,18,190,52),"COINS  "+coins,moneyStyle);if(player==null)return;if(Near(sellZone,2.3f))GUI.Box(new Rect(Screen.width/2-170,82,340,44),"SELLING BOARDS  +"+BoardPrice+" EACH",tipStyle);else if(sawLevel==1&&Near(upgradeZone,2.3f))GUI.Box(new Rect(Screen.width/2-190,82,380,44),coins>=UpgradeCost?"UPGRADING SAW  -"+UpgradeCost:"SAW LEVEL 2  •  "+UpgradeCost+" COINS",tipStyle);else if(sawLevel>1)GUI.Box(new Rect(Screen.width-210,76,190,40),"SAW LEVEL 2",tipStyle);}
    void Paint(GameObject o,Color c){var r=o.GetComponent<Renderer>();if(r==null)return;Shader s=Shader.Find("Universal Render Pipeline/Lit");if(s==null)s=Shader.Find("Sprites/Default");var m=new Material(s);m.color=c;r.material=m;}
}
