using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class PilotController : MonoBehaviour
{
    public GameObject laserPt;
    public GameObject shot;
    public Text scorePanel;
    public GameObject noticePanel;
    public Text noticeText;
    public Image screenFlash;

    public bool invuln = false;
    public bool dying = false;
    public GameObject[] healthLevels;
    private float MaxThrust { get; set; }
    private Rigidbody ship;

    public Camera playerCamera;
    public Camera rearView;

    private ParticleSystem cannon;
    private ParticleSystem gunnerCannon;
    private int lastFrameParticleCount;
    private Vector3 gunnerAim;
    private AudioSource laserSound;

    private int health = 100;
    private AudioSource alarmSound;
    private AudioSource thrusterSound;
    private AudioSource hitSound;
    private AudioSource deathSound;
    private AudioSource hitEnemySound;

    // Use this for initialization
    void Start()
    {
        //FOG
        //Set the background color
        playerCamera.backgroundColor = new Color(0.0f, 0.4f, 0.7f, 1f);
        rearView.backgroundColor = new Color(0.0f, 0.4f, 0.7f, 1f);
        //=========
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.0f, 0.4f, 0.7f, 0.6f);
        RenderSettings.fogDensity = 0.03f;
        RenderSettings.skybox = null;
        //END FOG

        var aSources = gameObject.GetComponents<AudioSource>();
        alarmSound = aSources[0];
        thrusterSound = aSources[1];
        hitSound = aSources[2];
        deathSound = aSources[3];
        hitEnemySound = aSources[4];

        MaxThrust = 100;
        ship = GetComponent<Rigidbody>();
        laserSound = GameObject.Find("Laser Sound").GetComponent<AudioSource>();

        int i;
        for(i=0;i<10;i++)
        {
            healthLevels[i].SetActive(false);
        }

        CollisionDelegator delegator = gameObject.AddComponent<CollisionDelegator>() as CollisionDelegator;
        delegator.attach(GameController.Instance.handleEnterCollision, GameController.Instance.handleExitCollision);

        StartCoroutine(HandleShooting());
        screenFlash.CrossFadeAlpha(0.0f, 0.0f, false);
    }

    // Update is called once per frame
    void Update()
    {
        // HandleLaserSounds();
        // HandleGunnerAiming();
        int score = GameController.Instance.GetScore();
        scorePanel.text = score.ToString();
        if (dying && Input.GetAxis("Fire") != 0)
        // if (dying && Input.anyKeyDown)
        {
            noticeText.text = "";
            noticePanel.SetActive(false);
            transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            transform.rotation = transform.rotation = Quaternion.identity;
            invuln = false;
            dying = false;

            health = 100;
            healthLevels[10].SetActive(true);
            int i;
            for (i = 0; i < 10; i++)
            {
                healthLevels[i].SetActive(false);
            }
        }
    }

   void FixedUpdate()
    {
        //physics updates go in FixedUpdate
        HandleRotation();
        HandleThrusting();
        ship.angularDrag = (float)(2 + (3 * ship.velocity.magnitude / 11.2));
    }

    //private void HandleGunnerAiming()
    //{
    //    var horizontal = Clamp(Input.GetAxis("Horizontal Aim"), reticleUI.preferredWidth, Screen.width);
    //    var vertical = Clamp(Input.GetAxis("Vertical Aim"), reticleUI.preferredHeight, Screen.height);

    //    reticleUI.transform.position += new Vector3(horizontal, vertical, 0) * 14.65f;
    //    gunnerCannon.transform.Rotate(-vertical, horizontal, 0);
    //}

    //private void HandleLaserSounds()
    //{
    //    var count = cannon.particleCount;
    //    if (count > lastFrameParticleCount) laserSound.Play();
    //    lastFrameParticleCount = cannon.particleCount;
    //}

    IEnumerator HandleShooting()
    {
        while (true)
        {
            if (Input.GetAxis("Fire") != 0 || Input.GetKeyDown("joystick button 0"))
            {
                laserSound.Play();
                GameObject newShot = (GameObject)Instantiate(shot, laserPt.transform.position, laserPt.transform.rotation);
                newShot.transform.Rotate(new Vector3(-80.0f, 0.0f, -90.0f));
                newShot.GetComponent<Rigidbody>().AddForce(laserPt.transform.forward * -1500);

                //leftShot.GetComponent<Rigidbody>().AddForce(desiredHeadingLeft.normalized * 6000);
                //rightShot.GetComponent<Rigidbody>().AddForce(desiredHeadingRight.normalized * 6000);

                //GameObject newShot = Instantiate(shot, transform.position + (transform.up * -2.0f), transform.rotation);
                //GameObject newShot = Instantiate(shot, transform.position, transform.rotation);
                // newShot.transform.Rotate(new Vector3(-90.0f,0.0f,-90.0f));

                //Vector3 direction = (transform.forward * 150) - newShot.transform.position;
                //Debug.DrawRay(transform.position + (transform.up * -2.0f), transform.forward * 150, Color.blue);
                //newShot.GetComponent<Rigidbody>().AddForce(direction.normalized * 3000);
                // newShot.GetComponent<Rigidbody>().AddForce(transform.forward * 3000);

                //Debug.Log("deltaP: " + desiredHeading.x + "," + desiredHeading.y + "," + desiredHeading.z);
                //GameObject newShot = Instantiate(shot, transform.position + (transform.forward * 5.0f), transform.rotation);
                //newShot.GetComponent<Rigidbody>().AddForce(transform.forward * 3000);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void HandleThrusting()
    {
        if (dying) return;
        float thrustInput = Input.GetAxis("Thrust");
        if (thrustInput <= 0)
        {
            if (Input.GetKeyDown("joystick button 1"))
            {
                thrustInput = 1.0f;
            }
        }
        if (thrustInput > 0)
        {
            if (!thrusterSound.isPlaying)
            {
                thrusterSound.Play();
            }
            var thrust = 40 * thrustInput;
            thrust = Math.Min(thrust, MaxThrust);
            thrust = Math.Max(thrust, 0);
            ship.AddForce(transform.forward * thrust);
        }
        else
        {
            thrusterSound.Stop();
        }
    }

    private void HandleRotation()
    {
        if (dying) return;
        var horizontalAmount = Input.GetAxis("Rotation");
        var verticalAmount = Input.GetAxis("Vertical");
        var rotationAmount = Input.GetAxis("Horizontal");

        ship.AddRelativeTorque(0, -horizontalAmount, 0);
        ship.AddRelativeTorque(-verticalAmount, 0, -rotationAmount);
    }

    private T Clamp<T>(T value, T low, T high) where T : IComparable<T>
    {
        if (value.CompareTo(low) > 0) return low;
        if (value.CompareTo(high) > 0) return high;
        return value;
    }

    public void Stop()
    {
        GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
    }

    public void playHit()
    {
        hitEnemySound.Play();
    }

    IEnumerator ScreenFlash()
    {
        screenFlash.CrossFadeAlpha(0.5f, 0.0f, false);
        screenFlash.CrossFadeAlpha(0.0f, 0.2f, false);
        yield return new WaitForSeconds(0.0f);
    }

    public void TakeHit()
    {
        if (dying) return;

        StartCoroutine(ScreenFlash());

        if (!hitSound.isPlaying)
        {
            hitSound.Play();
        }
        health -= 3;
        if (health <= 90 && health > 80)
        {
            healthLevels[8].SetActive(true);
            healthLevels[9].SetActive(false);

        }
        else if (health <= 80 && health > 70)
        {
            healthLevels[7].SetActive(true);
            healthLevels[8].SetActive(false);

        }
        else if (health <= 70 && health > 60)
        {
            healthLevels[6].SetActive(true);
            healthLevels[7].SetActive(false);

        }
        else if (health <= 60 && health > 50)
        {
            healthLevels[5].SetActive(true);
            healthLevels[6].SetActive(false);

        }
        else if (health <= 50 && health > 40)
        {
            healthLevels[4].SetActive(true);
            healthLevels[5].SetActive(false);

        }
        else if (health <= 40 && health > 30)
        {

            healthLevels[3].SetActive(true);
            healthLevels[4].SetActive(false);

        }
        else if (health <= 30 && health > 20)
        {
            alarmSound.Play();
            healthLevels[2].SetActive(true);
            healthLevels[3].SetActive(false);

        }
        else if (health <= 20 && health > 10)
        {
            alarmSound.Play();
            healthLevels[1].SetActive(true);
            healthLevels[2].SetActive(false);

        }
        else if (health < 10)
        {
            alarmSound.Play();
            healthLevels[0].SetActive(true);
            healthLevels[1].SetActive(false);
        }
        if (health <= 0 && !dying)
        {
            Die();
        }
    }

    void Die()
    {
        deathSound.Play();
        dying = true;
        invuln = true;
        Stop();
        GameController.Instance.LoseLife();

        noticePanel.SetActive(true);
        int lives = GameController.Instance.GetLives();
        
        if (lives == 1)
        {
            noticeText.text = "LAST LIFE!!!";
        }
        else if (lives > 0)
        {
            noticeText.text = lives.ToString() + " LIVES LEFT";
        }
    }
}