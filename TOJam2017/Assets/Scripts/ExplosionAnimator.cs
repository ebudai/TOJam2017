using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionAnimator : MonoBehaviour
{
    public Texture[] texList;
    int curTex = 0;

    void Start()
    {
        StartCoroutine(Switch());
    }

    IEnumerator Switch()
    {
        while (true)
        {
            var renderer = GetComponent<Renderer>();
            renderer.material.mainTexture = texList[curTex];
            curTex = (curTex + 1) % texList.Length; // A convenient way to loop an index
            yield return new WaitForSeconds(0.5f);
        }
    }
}