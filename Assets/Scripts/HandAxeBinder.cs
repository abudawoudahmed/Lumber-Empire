using System.Collections;
using UnityEngine;

public class HandAxeBinder : MonoBehaviour
{
    Transform axe;
    Transform hand;
    Vector3 handBaseEuler;
    Coroutine swing;

    public void Setup(Transform axeTransform)
    {
        axe = axeTransform;
        hand = FindHand(transform);
        if (hand == null || axe == null) return;

        handBaseEuler = hand.localEulerAngles;
        axe.SetParent(hand, false);
        axe.localPosition = new Vector3(0.03f, 0.10f, 0.02f);
        axe.localRotation = Quaternion.Euler(0f, 0f, 95f);
        axe.gameObject.SetActive(false);
    }

    public void Chop()
    {
        if (hand == null || axe == null) return;
        if (swing != null) StopCoroutine(swing);
        swing = StartCoroutine(Swing());
    }

    IEnumerator Swing()
    {
        axe.gameObject.SetActive(true);
        Quaternion rest = Quaternion.Euler(handBaseEuler);
        Quaternion windup = rest * Quaternion.Euler(-35f, 0f, -45f);
        Quaternion strike = rest * Quaternion.Euler(55f, 0f, 35f);

        for (float t = 0; t < 1; t += Time.deltaTime / 0.16f) {
            hand.localRotation = Quaternion.Slerp(rest, windup, t);
            yield return null;
        }
        for (float t = 0; t < 1; t += Time.deltaTime / 0.12f) {
            hand.localRotation = Quaternion.Slerp(windup, strike, t);
            yield return null;
        }
        for (float t = 0; t < 1; t += Time.deltaTime / 0.18f) {
            hand.localRotation = Quaternion.Slerp(strike, rest, t);
            yield return null;
        }
        hand.localRotation = rest;
        axe.gameObject.SetActive(false);
        swing = null;
    }

    Transform FindHand(Transform root)
    {
        Transform best = null;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) {
            string n = t.name.ToLowerInvariant();
            if (n.Contains("righthand") || n.Contains("right_hand") || n.Contains("hand_r") || n == "hand.r") return t;
            if (best == null && n.Contains("hand") && (n.Contains("right") || n.Contains("_r") || n.EndsWith(" r"))) best = t;
        }
        return best;
    }
}