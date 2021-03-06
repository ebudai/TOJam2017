﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyBehaviour : MonoBehaviour
{
    public BotState myState;
    public float distThreshold;
    private void Start()
    {
        CollisionDelegator delegator = gameObject.AddComponent<CollisionDelegator>() as CollisionDelegator;
        delegator.attach(GameController.Instance.handleEnterCollision, GameController.Instance.handleExitCollision);

        myState.alive = true;
        //start a coroutine that will "Bob" up and down
        StartCoroutine(Bob());
    }

    // Update is called once per frame
    void FixedUpdate()
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
                    //Debug.Log("Killing jelly");
                    Die();
                }
            }
        }
    }

    private float bobDir = 1.0f;
    IEnumerator Bob()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        while (true)
        {
            //transform.forward == up for this model
            float dieRoll = Random.Range(0f, 6.0f);
            if (dieRoll > 4)
            {
                rigidBody.AddForce(transform.forward * 400 * bobDir);
                bobDir *= -1.0f;
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    public bool isDead()
    {
        return myState.alive;
    }

    public void Die()
    {
        myState.alive = false;
        Destroy(gameObject, 1f);
    }
}
