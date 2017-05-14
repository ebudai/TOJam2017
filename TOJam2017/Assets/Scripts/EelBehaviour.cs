using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EelBehaviour : MonoBehaviour
{
    public float speed;
    public int scoreValue;
    public GameObject explosion;

    //Animator anim;
    AudioSource biteSound;
    AudioSource flybySound;

    private BotState myState = new BotState();
    private BotCommandStruct myCommands = new BotCommandStruct();
    //populate this list with the following array
    private SortedList<int, SubsumptionRule> SubsumptionRules = new SortedList<int, SubsumptionRule>();
    private List<SubsumptionRule> rulesList = new List<SubsumptionRule>();

    private float health = 15;
    private bool sentComms = false;
    //1 left, -1 right
    private int finDir = 1;
    private float finCycleTime = 0.15f;
    private float lastFinFlap;
    private bool biting = false;
    private float biteTime;

    void Start()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        rigidBody.ResetCenterOfMass();
        CollisionDelegator delegator = gameObject.AddComponent<CollisionDelegator>() as CollisionDelegator;
        delegator.attach(GameController.Instance.handleEnterCollision, GameController.Instance.handleExitCollision);

        var aSources = gameObject.GetComponents<AudioSource>();
        biteSound = aSources[0];
        flybySound = aSources[1];

        myState.playerSpotted = false;
        myState.alive = true;
        //build subsumption 
        rulesList.Add(new SubsumptionRule(50, RunAway));
        rulesList.Add(new SubsumptionRule(30, Idle));
        rulesList.Add(new SubsumptionRule(10, AttackPlayer));
        foreach (SubsumptionRule currRule in rulesList)
        {
            SubsumptionRules.Add(currRule.priority, currRule);
        }
        var player = GameObject.Find("PlayerShip");
        var playerBrain = GameObject.Find("PlayerShip").GetComponent<PilotController>();
        if (player != null && playerBrain != null)
        {
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
        lastFinFlap = Time.time;
        StartCoroutine(FollowPlayer());
    }

    // Update is called once per frame for animation
    void Update()
    {

    }

    public bool isAlive()
    {
        return myState.alive;
    }

    public void Die()
    {
        GameController.Instance.AddScore(scoreValue);

        Rigidbody rigidBody = GetComponent<Rigidbody>();
        Instantiate(explosion, transform.position + (transform.up * -1.0f * 3), Quaternion.identity);
        Destroy(gameObject);
        myState.alive = false;
    }

    private readonly VectorPid angularVelocityController = new VectorPid(33.7766f, 0, 0.2553191f);
    private readonly VectorPid headingController = new VectorPid(9.244681f, 0, 0.06382979f);

    public Transform target;

    //Physics callback, this is where sensation happens
    void FixedUpdate()
    {
        Collider playerCollider = null;
        Rigidbody rigidBody = GetComponent<Rigidbody>();

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
                //Debug.DrawRay(transform.position, transform.up * -1.0 * 15, Color.blue);
                //Debug.DrawRay(transform.position, rigidBody.angularVelocity * 10, Color.black);
                //Debug.DrawRay(transform.position, desiredHeading, Color.magenta);
            }

            if (!myState.playerSpotted)
            {
                playerCollider = player.GetComponent<Collider>();
                var bounds = playerCollider.bounds;
                //raycast on target
                var lineToTarget = new Ray(transform.position, transform.up * -1.0f);
                if (bounds.IntersectRay(lineToTarget))
                {
                    myState.playerSpotted = true;
                }
            }
        }
        if (Time.time - 0.2f < hitTime)
        {
            rigidBody.AddRelativeTorque(0.0f, 0.0f, 400 * finDir);
        }
    }

    //Looped Bot brain "master behaviour"
    //executes subsumption rules and commands
    IEnumerator FollowPlayer()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1.5f, 3.0f));
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        while (myState.alive)
        {
            //clear commands
            myCommands = new BotCommandStruct();
            myCommands.thrust = 0.0f;
            finCycleTime = 0.15f;
            //Run behaviours
            foreach (KeyValuePair<int, SubsumptionRule> kvp in SubsumptionRules)
            {
                kvp.Value.behaviourScript();
            }
            if (!biting)
            {
                //Execute commands
                if (myCommands.thrust > 0)
                {
                    rigidBody.AddForce(transform.up * -1.0f * myCommands.thrust);
                    rigidBody.AddRelativeTorque(0.0f, 0.0f, 40 * finDir);
                    //also add torque to the fin side for moving...
                    if (Time.time - finCycleTime > lastFinFlap)
                    {
                        lastFinFlap = Time.time;
                        finDir = finDir * -1;
                    }
                }

                if (myCommands.angularCorrection != null)
                {
                    rigidBody.AddTorque(myCommands.angularCorrection.Value);
                }
                if (myCommands.torque != null)
                {
                    if (myCommands.torque.Value.magnitude > 20)
                    {
                        myCommands.torque = myCommands.torque.Value.normalized * 20.0f;
                    }
                    rigidBody.AddTorque(myCommands.torque.Value);
                }
            }
            else
            {
                rigidBody.AddRelativeTorque(40 * finDir, 0.0f, 0.0f);
                if (Time.time - finCycleTime > lastFinFlap)
                {
                    lastFinFlap = Time.time;
                    finDir = finDir * -1;
                }
                if (Time.time - 0.3f > biteTime)
                {
                    biting = false;
                }
            }

            yield return new WaitForSeconds(0.05f);
        }
    }


    //========================================================================
    // SAMPLE BEHAVIOURS
    //========================================================================
    void RunAway()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        Collider playerCollider = null;

        //don't run away if we have him in our sites!!!
        if ((myState.distToPlayer <= 25 && !myState.playerSpotted) || Time.time - 0.75f < hitTime)
        {
            //change direction away from player
            var angularVelocityError = rigidBody.angularVelocity * -1;
            var angularVelocityCorrection = angularVelocityController.Update(angularVelocityError, 0.1f);
            myCommands.angularCorrection = angularVelocityCorrection;

            var headingError = Vector3.Cross(transform.up * -1.0f, myState.desiredHeading * -1.0f);
            var headingCorrection = headingController.Update(headingError, 0.1f);
            myCommands.torque = headingCorrection;

            //go faster
            myCommands.thrust = speed * 1.5f;
            finCycleTime = 0.05f;

            var player = GameObject.Find("PlayerShip");
            if (player != null)
            {
                playerCollider = player.GetComponent<Collider>();
                if (playerCollider != null)
                {
                    var bounds = playerCollider.bounds;

                    //raycast on target
                    var lineToTarget = new Ray(transform.position, transform.up * -1.0f);
                    if (!bounds.IntersectRay(lineToTarget))
                    {
                        myCommands.thrust = speed * 2.0f;
                    }
                }
            }
        }
    }

    void AttackPlayer()
    {
        if (biting) return;
        Rigidbody rigidBody = GetComponent<Rigidbody>();

        Vector3 angularVelocityError = rigidBody.angularVelocity * -1;
        Vector3 angularVelocityCorrection = angularVelocityController.Update(angularVelocityError, 0.1f);
        myCommands.angularCorrection = angularVelocityCorrection;
        Vector3 headingError = Vector3.Cross(transform.up * -1.0f, myState.desiredHeading);
        Vector3 headingCorrection = headingController.Update(headingError, 0.1f);

        myCommands.torque = headingCorrection;
        myCommands.thrust = speed;
    }

    void Idle()
    {
        if (myState.distToPlayer < 15 && !flybySound.isPlaying)
        {
            flybySound.Play();
        }
    }

    public void DoBite()
    {
        if (!biting)
        {
            biting = true;
            transform.up = myState.dirToPlayer * -1.0f;
            biteTime = Time.time;
            //stop moving
            if (!biteSound.isPlaying)
            {
                biteSound.Play();
            }
            GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
            //rotate jaw?
        }
    }

    private float hitTime;
    public void TakeHit()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        myState.playerSpotted = false; //shit run away!
        hitTime = Time.time;

        rigidBody.AddRelativeTorque(0.0f, 0.0f, 400 * finDir);

        health -= 5;
        if (health <= 0)
        {
            Die();
        }
        else if (health < 15 && !sentComms)
        {
            sentComms = true;
            //var cmdScript = GameObject.Find("Canvas/CrabFace").GetComponent<CrabFaceAnimation>();
            var cmdScript = GameObject.Find("Canvas/EelFace").GetComponent<EelFaceAnim>();
            cmdScript.Play();
        }
    }
}
