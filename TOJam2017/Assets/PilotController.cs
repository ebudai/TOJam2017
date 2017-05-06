using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PilotController : MonoBehaviour {

    //private float Thrust { get; set; }
    private float MaxThrust { get; set; }

    private Rigidbody ship;
    private ParticleSystem cannon;

    // Use this for initialization
    void Start () {
        MaxThrust = 100;
        ship = GetComponent<Rigidbody>();
        cannon = GameObject.Find("Player Lasers").GetComponent<ParticleSystem>();
    }
	
	// Update is called once per frame
	void Update () {
        HandleRotation();
        HandleThrusting();
        HandleShooting();
	}
    
    private void HandleShooting()
    {
        if (Input.GetAxis("Fire") != 0) cannon.Play();
        else cannon.Stop();
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
}
