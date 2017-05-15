using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileBehaviour : MonoBehaviour
{
    public AudioSource explosionSound;

    // Use this for initialization
    void Start ()
    {
        CollisionDelegator delegator = gameObject.AddComponent<CollisionDelegator>() as CollisionDelegator;
        delegator.attach(GameController.Instance.handleEnterCollision, GameController.Instance.handleExitCollision);

        StartCoroutine(TimeToLive());
    }

    private void Update()
    {
        Animation anim = GetComponent<Animation>();
        if (!anim.isPlaying)
        {
            anim.Play();
        }
    }

    private IEnumerator TimeToLive()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.5f);
            break;
        }
        Die();
    }

    public void Explode()
    {
    //    //explosionSound.Play();
    //    Die();
    }

    public void Die()
    {
        ////find dist to player       
        //var player = GameObject.Find("PlayerShip");
        //if (player != null && tag == "PlayerProjectile")
        //{
        //    var playerBrain = player.GetComponent<PilotController>();
        //    if (player.activeInHierarchy && !playerBrain.invuln && !playerBrain.dying)
        //    {
        //        Vector3 playerPos = player.GetComponent<Transform>().position;
        //        float distToPlayer = Vector3.Distance(playerPos, transform.position);
        //        Debug.Log("Missile dying at dist: " + distToPlayer.ToString());
        //    }
        //}
        Destroy(gameObject);
    }
}
