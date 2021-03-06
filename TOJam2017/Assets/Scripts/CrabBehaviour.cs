﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabBehaviour : MonoBehaviour
{
    public float speed;
    public int scoreValue;
    public float fireRate;

    public GameObject shot;
    public GameObject explosion;

    public Transform LeftHardPt;
    public Transform RightHardPt;

    //Animator anim;
    AudioSource fireSound;
    AudioSource flybySound;

    private BotState myState = new BotState();
    private BotCommandStruct myCommands = new BotCommandStruct();
    //populate this list with the following array
    private SortedList<int, SubsumptionRule> SubsumptionRules = new SortedList<int, SubsumptionRule>();
    private List<SubsumptionRule> rulesList = new List<SubsumptionRule>();

    private float nextFire = 0;
    private float health = 30;
    private bool sentComms = false;

    private Animation anim;

    void Start()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        anim = GetComponent<Animation>();

        CollisionDelegator delegator = gameObject.AddComponent<CollisionDelegator>() as CollisionDelegator;
        delegator.attach(GameController.Instance.handleEnterCollision, GameController.Instance.handleExitCollision);
        //anim = GetComponent<Animator>();

        var aSources = gameObject.GetComponents<AudioSource>();
        fireSound = aSources[0];
        flybySound = aSources[1];

        myState.playerSpotted = false;
        myState.alive = true;
        //build subsumption 
        rulesList.Add(new SubsumptionRule(60, OrientDown));
        rulesList.Add(new SubsumptionRule(50, RunAway));
        rulesList.Add(new SubsumptionRule(30, Idle));
        rulesList.Add(new SubsumptionRule(10, AttackPlayer));
        foreach (SubsumptionRule currRule in rulesList)
        {
            SubsumptionRules.Add(currRule.priority, currRule);
        }

        var player = GameObject.Find("PlayerShip");
        if (player != null)
        {
            var playerBrain = player.GetComponent<PilotController>();
            if (player.activeInHierarchy && !playerBrain.invuln && !playerBrain.dying)
            {
                Vector3 playerPos = player.GetComponent<Rigidbody>().position;
                Vector3 deltaVectorP = playerPos - transform.position;
                //Debug.Log("deltaP: " + deltaVectorP.x + "," + deltaVectorP.y + "," + deltaVectorP.z);

                myState.desiredHeading = deltaVectorP;
                myState.dirToPlayer = deltaVectorP.normalized;
                //Debug.Log("deltaPnormal: " + myState.dirToPlayer.x + "," + myState.dirToPlayer.y + "," + myState.dirToPlayer.z);
                myState.distToPlayer = Vector3.Distance(playerPos, rigidBody.position);
            }
        }

        StartCoroutine(ProcessBehaviours());
    }

    // Update is called once per frame for animation
    void Update()
    {
        if (Time.time + 0.5 >= nextFire)
        {
            //Debug.Log("Stopping animation");
            anim["Take 001"].speed = -1;
            anim["Take 001"].time = 0;
           // anim.Rewind();
           // anim.Stop();
        }
    }

    public bool isAlive()
    {
        return myState.alive;
    }

    public void Die()
    {
        GameController.Instance.AddScore(scoreValue);

        //Rigidbody rigidBody = GetComponent<Rigidbody>();
        //rigidBody.AddTorque(new Vector3(0.0f,1000.0f,500.0f));
        //rigidBody.velocity = Vector3.zero;
        Instantiate(explosion, transform.position + (transform.up*3), Quaternion.identity);
        Destroy(gameObject);
        myState.alive = false;
    }

    private readonly VectorPid angularVelocityController = new VectorPid(33.7766f, 0, 0.2553191f);
    private readonly VectorPid headingController = new VectorPid(9.244681f, 0, 0.06382979f);

    public Transform target;

    //Physics callback, this is where sensation happens
    void FixedUpdate()
    {
        //Rigidbody rigidBody = GetComponent<Rigidbody>();
        Collider playerCollider = null;

        var player = GameObject.Find("PlayerShip");
        if (player != null)
        {
            var playerBrain = player.GetComponent<PilotController>();
            if (player.activeInHierarchy && !playerBrain.invuln && !playerBrain.dying)
            {
                Vector3 playerPos = player.GetComponent<Rigidbody>().position;
                Vector3 desiredHeading = playerPos - transform.position;

                //Debug.Log("deltaP: " + desiredHeading.x + "," + desiredHeading.y + "," + desiredHeading.z);

                myState.desiredHeading = desiredHeading;
                myState.dirToPlayer = desiredHeading.normalized;
                myState.distToPlayer = Vector3.Distance(playerPos, transform.position);
                // Debug.Log("distToPlayer: " + myState.distToPlayer);
                //visual debugging
                //Debug.DrawRay(transform.position, transform.forward * 15, Color.blue);
                //Debug.DrawRay(transform.position, rigidBody.angularVelocity * 10, Color.black);
                //Debug.DrawRay(transform.position, desiredHeading, Color.magenta);

                playerCollider = player.GetComponent<Collider>();
                var bounds = playerCollider.bounds;
                //raycast on target
                var lineToTarget = new Ray(transform.position, transform.forward);
                if (bounds.IntersectRay(lineToTarget))
                {
                    myState.playerSpotted = true;
                }
            }
        }
    }

    //Looped Bot brain "master behaviour"
    //executes subsumption rules and commands
    IEnumerator ProcessBehaviours()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1.5f, 3.0f));
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        while (myState.alive)
        {
            //clear commands
            myCommands = new BotCommandStruct();
            myCommands.thrust = 0;
            
            //Run behaviours
            foreach (KeyValuePair<int, SubsumptionRule> kvp in SubsumptionRules)
            {
                kvp.Value.behaviourScript();
            }

            ////Execute commands
            if (myCommands.thrust > 0)
            {
                rigidBody.AddForce(transform.forward * myCommands.thrust);
            }

            if (myCommands.angularCorrection != null)
            {
                //Debug.Log("Crab Angular correction: <"+ myCommands.angularCorrection.Value.magnitude + ">" + myCommands.angularCorrection);
                rigidBody.AddTorque(myCommands.angularCorrection.Value);
            }
            if (myCommands.torque != null)
            {
                if (myCommands.torque.Value.magnitude > 40)
                {
                    myCommands.torque = myCommands.torque.Value.normalized * 40.0f;
                }
                rigidBody.AddTorque(myCommands.torque.Value);
            }

            if (myCommands.fire && Time.time > nextFire)
            {
                anim["Take 001"].time = 1.0f;
                anim.Play();
                //fire
                fireSound.Play();

                var player = GameObject.Find("PlayerShip");

                //need two shots here, one on right, one on left
                //spawn them from the loc of hard pts
                GameObject leftShot = (GameObject)Instantiate(shot, LeftHardPt.position + transform.forward * 5, transform.rotation);
                GameObject rightShot = (GameObject)Instantiate(shot, RightHardPt.position + transform.forward * 5, transform.rotation);

                Vector3 desiredHeadingRight = player.transform.position - RightHardPt.position;
                Vector3 desiredHeadingLeft = player.transform.position - LeftHardPt.position;

                leftShot.GetComponent<Rigidbody>().AddForce(desiredHeadingLeft.normalized * 2000);
                rightShot.GetComponent<Rigidbody>().AddForce(desiredHeadingRight.normalized * 2000);

                nextFire = Time.time + fireRate;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }


    //========================================================================
    // SAMPLE BEHAVIOURS
    //========================================================================
    void RunAway()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        Collider playerCollider = null;

        if (myState.distToPlayer <= 25 && !myState.playerSpotted || myState.distToPlayer <= 10)
        {
            //change direction away from player
            var angularVelocityError = rigidBody.angularVelocity * -1;
            var angularVelocityCorrection = angularVelocityController.Update(angularVelocityError, 0.1f);

            angularVelocityCorrection = angularVelocityError * 40;
            myCommands.angularCorrection = angularVelocityCorrection;

            var headingError = Vector3.Cross(transform.forward, myState.desiredHeading * -1.0f);
            var headingCorrection = headingController.Update(headingError, 0.1f);

            headingCorrection = headingError.normalized * 40;
            myCommands.torque = headingCorrection;
            //run away fast
            myCommands.thrust = speed * 1.5f;

            //faster if he's right behind us
            var player = GameObject.Find("PlayerShip");
            if (player != null)
            {
                playerCollider = player.GetComponent<Collider>();
                var bounds = playerCollider.bounds;
                //raycast on target
                var lineToTarget = new Ray(transform.position, transform.forward);
                if (!bounds.IntersectRay(lineToTarget))
                {
                    myCommands.thrust = speed * 2.0f;
                }
            }
        }
    }

    void AttackPlayer()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        Collider playerCollider = null;

        Vector3 angularVelocityError = rigidBody.angularVelocity * -1;
        Vector3 angularVelocityCorrection = angularVelocityController.Update(angularVelocityError, 0.1f);

        angularVelocityCorrection = angularVelocityError * 40;
        myCommands.angularCorrection = angularVelocityCorrection;

        Vector3 headingError = Vector3.Cross(transform.forward, myState.desiredHeading);
        Vector3 headingCorrection = headingController.Update(headingError, 0.1f);

        headingCorrection = headingError.normalized * 40;
        myCommands.torque = headingCorrection;
        myCommands.thrust = speed;

        var player = GameObject.Find("PlayerShip");
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider>();
            if (playerCollider != null)
            {
                var bounds = playerCollider.bounds;
                //raycast on target
                var lineToTarget = new Ray(transform.position, transform.forward);
                if (bounds.IntersectRay(lineToTarget))
                {
                    myCommands.fire = true;
                    myCommands.thrust = speed;
                }
            }
        }
    }

    void Idle()
    {
        if (myState.distToPlayer < 15 && !flybySound.isPlaying)
        {
            flybySound.Play();
        }
    }

    void OrientDown()
    {
        var headingError = Vector3.Cross(transform.up, Vector3.up);

        //Vector3 headingCorrection = headingController.Update(headingError, 0.1f);
        //headingCorrection = headingCorrection.normalized * 30.0f;
        Vector3 headingCorrection = headingError.normalized * 10.0f;

        myCommands.torque += headingCorrection;
    }

    public void TakeHit()
    {
        health -= 5;
        myState.playerSpotted = false; //shit run away!
        if (health <= 0)
        {
            Die();
        } else if (health < 20 && !sentComms)
        {
            sentComms = true;
            var cmdScript = GameObject.Find("Canvas/CrabFace").GetComponent<CrabFaceAnimation>();
            //var cmdScript = GameObject.Find("Canvas/EelFace").GetComponent<EelFaceAnim>();
            cmdScript.Play();
        }
    }
}