using System.Collections;
using UnityEngine;

public class TreeNode : MonoBehaviour
{
    public int hitsLeft = 3;
    public bool felled;

    public void Hit()
    {
        hitsLeft = Mathf.Max(0, hitsLeft - 1);
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        Vector3 p = transform.localPosition;
        for(int i=0;i<5;i++)
        {
            float amount = i % 2 == 0 ? .055f : -.055f;
            transform.localPosition = p + new Vector3(amount,0,0);
            yield return new WaitForSeconds(.035f);
        }
        transform.localPosition = p;
    }
}
