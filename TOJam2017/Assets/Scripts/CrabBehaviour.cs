using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabBehaviour : MonoBehaviour
{
    public float speed;
    public int scoreValue;
    public float fireRate;

    public GameObject shot;
    public Transform LeftHardPt;
    public Transform RightHardPt;

    //Animator anim;
    AudioSource fireSound;
    AudioSource flybySound;
    AudioSource deathSound;

    private BotState myState = new BotState();
    private BotCommandStruct myCommands = new BotCommandStruct();
    //populate this list with the following array
    private SortedList<int, SubsumptionRule> SubsumptionRules = new SortedList<int, SubsumptionRule>();
    private List<SubsumptionRule> rulesList = new List<SubsumptionRule>();

    private float nextFire = 0;
    private float health = 30;
    void Start()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();

        CollisionDelegator delegator = gameObject.AddComponent<CollisionDelegator>() as CollisionDelegator;
        delegator.attach(GameController.Instance.handleEnterCollision, GameController.Instance.handleExitCollision);
        //anim = GetComponent<Animator>();

        var aSources = gameObject.GetComponents<AudioSource>();
        fireSound = aSources[0];
        flybySound = aSources[1];
        deathSound = aSources[2];

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
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        rigidBody.AddTorque(new Vector3(0.0f,1.0f,0.0f));
        rigidBody.velocity = Vector3.zero;
        myState.alive = false;
        //anim.StopPlayback();

        GameController.Instance.AddScore(scoreValue);

        //death sound
        deathSound.Play();
        Destroy(gameObject, 1f);
    }

    private readonly VectorPid angularVelocityController = new VectorPid(33.7766f, 0, 0.2553191f);
    private readonly VectorPid headingController = new VectorPid(9.244681f, 0, 0.06382979f);

    public Transform target;

    //Physics callback, this is where sensation happens
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
            }
        }
    }

    //Looped Bot brain "master behaviour"
    //executes subsumption rules and commands
    IEnumerator FollowPlayer()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1.5f, 3.0f));
        var anim = GetComponent<Animation>();
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        while (myState.alive)
        {
            //clear commands
            myCommands = new BotCommandStruct();
            myCommands.thrust = 0;
            anim.Stop();

            //Run behaviours
            foreach (KeyValuePair<int, SubsumptionRule> kvp in SubsumptionRules)
            {
                kvp.Value.behaviourScript();
            }

            //Execute commands
            if (myCommands.thrust > 0)
            {
                rigidBody.AddForce(transform.forward * myCommands.thrust);
                //rigidBody.AddForce(transform.up * myCommands.thrust);
            }

            if (myCommands.angularCorrection != null)
            {
                rigidBody.AddTorque(myCommands.angularCorrection.Value);
            }
            if (myCommands.torque != null)
            {
                rigidBody.AddTorque(myCommands.torque.Value);
            }

            if (myCommands.fire && Time.time > nextFire)
            {
                //fire
                fireSound.Play();
                //need two shots here, one on right, one on left
                //spawn them from the loc of hard pts
                var player = GameObject.Find("PlayerShip");
                anim["Take 001"].time = 0.50f;
                anim.Play();
                GameObject leftShot = (GameObject)Instantiate(shot, LeftHardPt.position + transform.forward * 5, transform.rotation);
                GameObject rightShot = (GameObject)Instantiate(shot, RightHardPt.position + transform.forward * 5, transform.rotation);

                Vector3 desiredHeadingRight = player.transform.position - RightHardPt.position;
                Vector3 desiredHeadingLeft = player.transform.position - LeftHardPt.position;

                leftShot.GetComponent<Rigidbody>().AddForce(desiredHeadingLeft.normalized * 6000);
                rightShot.GetComponent<Rigidbody>().AddForce(desiredHeadingRight.normalized * 6000);

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

        if (myState.distToPlayer <= 25)
        {
            if (myState.distToPlayer < 15 && !flybySound.isPlaying)
            {
                flybySound.Play();
            }

            //change direction away from player
            var angularVelocityError = rigidBody.angularVelocity * -1;
            var angularVelocityCorrection = angularVelocityController.Update(angularVelocityError, 0.1f);
            myCommands.angularCorrection = angularVelocityCorrection;

            var headingError = Vector3.Cross(transform.forward, myState.desiredHeading * -1.0f);
            var headingCorrection = headingController.Update(headingError, 0.1f);
            myCommands.torque = headingCorrection;

            var player = GameObject.Find("PlayerShip");
            if (player != null)
            {
                playerCollider = player.GetComponent<Collider>();
                if (playerCollider != null)
                {
                    var bounds = playerCollider.bounds;

                    //raycast on target
                    var lineToTarget = new Ray(transform.position, transform.forward);
                    if (!bounds.IntersectRay(lineToTarget))
                    {
                        myCommands.thrust = speed * 2.0f;
                        //myCommands.firingAngle = direction;
                    }
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
        myCommands.angularCorrection = angularVelocityCorrection;
        Vector3 headingError = Vector3.Cross(transform.forward, myState.desiredHeading);
        Vector3 headingCorrection = headingController.Update(headingError, 0.1f);

        if (headingCorrection.magnitude > 25)
        {
            headingCorrection = headingCorrection.normalized * 25.0f;
        } 

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
        //Rigidbody rigidBody = GetComponent<Rigidbody>();
        //Vector3 vecRight = Vector3.Cross(Vector3.up, Vector3.forward);
        //Vector3 vecLeft = Vector3.Cross(Vector3.forward, Vector3.up);
        //if (UnityEngine.Random.Range(-1.0f, 1.0f) > 0)
        //{
        //    var headingError = Vector3.Cross(transform.forward, vecLeft);
        //    var headingCorrection = headingController.Update(headingError, 0.1f);
        //    myCommands.torque = headingCorrection;
        //}
        //else
        //{
        //    var headingError = Vector3.Cross(transform.forward, vecRight);
        //    var headingCorrection = headingController.Update(headingError, 0.1f);
        //    myCommands.torque = headingCorrection;
        //}

        //myCommands.thrust = speed;
    }

    public void TakeHit()
    {
        health -= 5;
        if (health <= 0)
        {
            Die();
        }
    }
}

public class VectorPid
{
    public float pFactor, iFactor, dFactor;

    private Vector3 integral;
    private Vector3 lastError;

    public VectorPid(float pFactor, float iFactor, float dFactor)
    {
        this.pFactor = pFactor;
        this.iFactor = iFactor;
        this.dFactor = dFactor;
    }

    public Vector3 Update(Vector3 currentError, float timeFrame)
    {
        integral += currentError * timeFrame;
        var deriv = (currentError - lastError) / timeFrame;
        lastError = currentError;
        return currentError * pFactor
            + integral * iFactor
            + deriv * dFactor;
    }
}