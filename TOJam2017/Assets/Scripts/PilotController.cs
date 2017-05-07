using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PilotController : MonoBehaviour {
    public bool invuln = false;
    public bool dying = false;
    public Image reticleUI;
    //private float Thrust { get; set; }
    private float MaxThrust { get; set; }
    private Rigidbody ship;
    public Camera playerCamera;
    private ParticleSystem cannon;
    private ParticleSystem gunnerCannon;
    private int lastFrameParticleCount;
    private Vector3 gunnerAim;
    private AudioSource laserSound;

    // Use this for initialization
    void Start ()
    {
        //FOG
        //Set the background color
        playerCamera.backgroundColor = new Color(0.0f, 0.4f, 0.7f, 1f);
        //=========
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.0f, 0.4f, 0.7f, 0.6f);
        RenderSettings.fogDensity = 0.02f;
        RenderSettings.skybox = null;
        //END FOG

        MaxThrust = 100;
        ship = GetComponent<Rigidbody>();
        cannon = GameObject.Find("Player Lasers").GetComponent<ParticleSystem>();
        gunnerCannon = GameObject.Find("Gunner Lasers").GetComponent<ParticleSystem>();
        reticleUI.transform.position.Set(Screen.width / 2, Screen.height / 2, 0);
        laserSound = GameObject.Find("Laser Sound").GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
        HandleRotation();
        HandleThrusting();
        HandleShooting();
        HandleLaserSounds();
        HandleGunnerAiming();
	}
    
    private void HandleGunnerAiming()
    {
        var horizontal = Clamp(Input.GetAxis("Horizontal Aim"), reticleUI.preferredWidth, Screen.width);
        var vertical = Clamp(Input.GetAxis("Vertical Aim"), reticleUI.preferredHeight, Screen.height);
       
        reticleUI.transform.position += new Vector3(horizontal, vertical, 0) * 14.65f;
        gunnerCannon.transform.Rotate(-vertical, horizontal, 0);
    }

    private void HandleLaserSounds()
    {
        var count = cannon.particleCount;
        if (count > lastFrameParticleCount) laserSound.Play();
        lastFrameParticleCount = cannon.particleCount;
    }

    private void HandleShooting()
    {
        if (Input.GetAxis("Fire") != 0) cannon.Play();
        else cannon.Stop();

        if (Input.GetAxis("Gunner Fire") != 0) gunnerCannon.Play();
        else gunnerCannon.Stop();
    }

    private void HandleThrusting()
    {
        var thrust = 40 * Input.GetAxis("Thrust");
        thrust = Math.Min(thrust, MaxThrust);
        thrust = Math.Max(thrust, 0);
        ship.AddForce(transform.forward * thrust);
    }

    private void HandleRotation()
    {
        var horizontalAmount = Input.GetAxis("Rotation");
        var verticalAmount = Input.GetAxis("Vertical");
        var rotationAmount = Input.GetAxis("Horizontal");

        ship.AddTorque(0, -horizontalAmount, 0);
        ship.AddRelativeTorque(-verticalAmount, 0, -rotationAmount);
    }

    private T Clamp<T>(T value, T low, T high) where T : IComparable<T>
    {
        if (value.CompareTo(low) > 0) return low;
        if (value.CompareTo(high) > 0) return high;
        return value;
    }
}
