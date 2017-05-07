using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyBehaviour : MonoBehaviour
{
    public float distThreshold;
    private void Start()
    {
        //start a coroutine that will "Bob" up and down
        StartCoroutine(Bob());
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        var player = GameObject.Find("PlayerShip");
        if (player != null)
        {
            var playerBrain = player.GetComponent<PilotController>();
            if (player.activeInHierarchy && !playerBrain.invuln && !playerBrain.dying)
            {
                Vector3 playerPos = player.GetComponent<Rigidbody>().position;

                float distToPlayer = Vector3.Distance(playerPos, rigidBody.position);
                if (distToPlayer > distThreshold)
                {
                    Die();
                }
            }
        }
	}

    private float bobDir = 1.0f;
    IEnumerator Bob()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        while(true)
        {
            //transform.forward == up for this model
            float dieRoll = Random.Range(0f, 6.0f);
            if (dieRoll > 2)
            {
                rigidBody.AddForce(transform.forward * 20 * bobDir);
                bobDir *= -1.0f;
            }

            yield return new WaitForSeconds(2.0f);
        }
    }

    public void Die()
    {
        //myState.alive = false;
        //anim.StopPlayback();

        //death sound
        //audio2.Play();

        Destroy(gameObject, 1f);
    }
}
