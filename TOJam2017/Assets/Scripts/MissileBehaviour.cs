using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileBehaviour : MonoBehaviour
{
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
            yield return new WaitForSeconds(5f);
            break;
        }
        Die();
    }

    public void Die()
    {
        Destroy(gameObject, 1f);
    }
}
