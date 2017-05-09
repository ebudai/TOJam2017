using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehaviour : MonoBehaviour
{
    private GameObject player;
    private GameObject cam;

    private Texture[] frameArray;
    private int curTex = 0;
    private bool finishedAnim = false;
    private float startTime;
    // Use this for initialization
    void Start ()
    {
        startTime = Time.time;
        player = GameObject.Find("PlayerShip");
        cam = GameObject.Find("Camera");
        frameArray = new Texture[7];
        frameArray[0] = Resources.Load("bubblesExplosion1") as Texture;
        frameArray[1] = Resources.Load("bubblesExplosion2") as Texture;
        frameArray[2] = Resources.Load("bubblesExplosion3") as Texture;
        frameArray[3] = Resources.Load("bubblesExplosion4") as Texture;
        frameArray[4] = Resources.Load("bubblesExplosion5") as Texture;
        frameArray[5] = Resources.Load("bubblesExplosion6") as Texture;
        frameArray[6] = Resources.Load("bubblesExplosion7") as Texture;

        StartCoroutine(Switch());
        StartCoroutine(TimeToLive());
        gameObject.GetComponent<AudioSource>().Play();
    }

    IEnumerator Switch()
    {
        
        var renderer = GetComponent<Renderer>();
        while (!finishedAnim)
        {

            renderer.material.mainTexture = frameArray[curTex];
            //don't want looping
            //curTex = (curTex + 1) % frameArray.Length; // A convenient way to loop an index

            curTex += 1;
            if (curTex >= frameArray.Length)
            {
                finishedAnim = true;
            }
            yield return new WaitForSeconds(0.075f);
        }
        renderer.enabled = false;
    }

    void Update()
    {
        //billboard
        transform.rotation = cam.transform.rotation;
        //Debug.DrawRay(transform.position, transform.forward * 100, Color.magenta);
    }

    private IEnumerator TimeToLive()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.3f);
            break;
        }
        Die();
    }

    public void Die()
    {
        //Debug.Log("Explosion - dying");
        Destroy(gameObject);
    }
}
