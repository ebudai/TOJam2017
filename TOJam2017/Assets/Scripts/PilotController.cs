using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PilotController : MonoBehaviour {
    public bool invuln = false;
    public bool dying = false;
    public Image reticleUI;
    public Camera playerCamera;
    public GameObject shot;
    public Text scorePanel;

    public GameObject[] healthLevels;

    //private float Thrust { get; set; }
    private float MaxThrust { get; set; }
    private int health = 100;
    private Rigidbody ship;

    private Vector3 gunnerAim;
    private AudioSource laserSound;
    private AudioSource alarmSound;
    private AudioSource thrusterSound;
    private AudioSource hitSound;

    // Use this for initialization
    void Start ()
    {
        //FOG
        //Set the background color
        playerCamera.backgroundColor = new Color(0.0f, 0.4f, 0.7f, 1f);
        //=========
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.0f, 0.4f, 0.7f, 0.6f);
        RenderSettings.fogDensity = 0.04f;
        RenderSettings.skybox = null;
        //END FOG
        var aSources = gameObject.GetComponents<AudioSource>();
        alarmSound = aSources[0];
        thrusterSound = aSources[1];
        hitSound = aSources[2];

        MaxThrust = 100;
        ship = GetComponent<Rigidbody>();
        //cannon = GameObject.Find("Player Lasers").GetComponent<ParticleSystem>();
        //gunnerCannon = GameObject.Find("Gunner Lasers").GetComponent<ParticleSystem>();
        reticleUI.transform.position.Set(Screen.width / 2, Screen.height / 2, 0);
        laserSound = GameObject.Find("Laser Sound").GetComponent<AudioSource>();

        healthLevels[9].SetActive(false);
        healthLevels[8].SetActive(false);
        healthLevels[7].SetActive(false);
        healthLevels[6].SetActive(false);
        healthLevels[5].SetActive(false);
        healthLevels[4].SetActive(false);
        healthLevels[3].SetActive(false);
        healthLevels[2].SetActive(false);
        healthLevels[1].SetActive(false);
        healthLevels[0].SetActive(false);

        CollisionDelegator delegator = gameObject.AddComponent<CollisionDelegator>() as CollisionDelegator;
        delegator.attach(GameController.Instance.handleEnterCollision, GameController.Instance.handleExitCollision);

        StartCoroutine(HandleShooting());
    }
	
	// Update is called once per frame
	void Update () {
        HandleRotation();
        HandleThrusting();
        HandleShooting();
        //HandleLaserSounds();
        // HandleGunnerAiming();
        int score = GameController.Instance.GetScore();
        scorePanel.text = score.ToString();
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
            if (Input.GetAxis("Fire") != 0)
            {
                laserSound.Play();
                GameObject newShot = (GameObject)Instantiate(shot, transform.position + (transform.forward * 5.0f), transform.rotation);
                newShot.GetComponent<Rigidbody>().AddForce(transform.forward * 6000);
            }
            //if (Input.GetAxis("Gunner Fire") != 0) gunnerCannon.Play();
            //else gunnerCannon.Stop();
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void HandleThrusting()
    {
        float thrustInput = Input.GetAxis("Thrust");
        if (thrustInput > 0)
        {
            if (!thrusterSound.isPlaying || thrusterSound.time > 2.0)
            {
                thrusterSound.Play();
            }
            var thrust = 40 * thrustInput;
            thrust = Math.Min(thrust, MaxThrust);
            thrust = Math.Max(thrust, 0);
            ship.AddForce(transform.forward * thrust);
        }
    }

    private void HandleRotation()
    {
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

    public void TakeHit()
    {
        if (!hitSound.isPlaying)
        {
            hitSound.Play();
        }
        health -= 5;
        if (health < 100 && health > 90)
        {
            healthLevels[8].SetActive(true);
            healthLevels[9].SetActive(false);
        }
        else if (health <= 90 && health > 80)
        {
            healthLevels[7].SetActive(true);
            healthLevels[8].SetActive(false);
        }
        else if (health <= 80 && health > 70)
        {
            healthLevels[6].SetActive(true);
            healthLevels[7].SetActive(false);
        }
        else if (health <= 70 && health > 60)
        {
            healthLevels[5].SetActive(true);
            healthLevels[6].SetActive(false);
        }
        else if (health <= 60 && health > 50)
        {
            healthLevels[4].SetActive(true);
            healthLevels[5].SetActive(false);
        }
        else if (health <= 50 && health > 40)
        {
            healthLevels[3].SetActive(true);
            healthLevels[4].SetActive(false);
        }
        else if (health <= 40 && health > 30)
        {
            alarmSound.Play();
            healthLevels[2].SetActive(true);
            healthLevels[3].SetActive(false);
        }
        else if (health <= 30 && health > 20)
        {
            alarmSound.Play();
            healthLevels[1].SetActive(true);
            healthLevels[2].SetActive(false);
        }
        else if (health <= 20 && health > 10)
        {
            alarmSound.Play();
            healthLevels[0].SetActive(true);
            healthLevels[1].SetActive(false);
        }
    }
}
