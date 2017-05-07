using UnityEngine;
using System.Collections;

public class CollisionDelegator : MonoBehaviour
{
	public const string NAME = "CollisionDelegator";
	public string filterTag;
	public delegate void TriggerCallback (GameObject trigger, Collider collided);
	private event TriggerCallback callbackEnter;
	private event TriggerCallback callbackExit;
	
	void OnTriggerEnter (Collider col) 
	{
		if (callbackEnter != null && (filterTag == null || col.gameObject.tag == filterTag)) 
		{
			callbackEnter (gameObject, col);
		}
	}
	
	void OnTriggerExit (Collider col) 
	{
		if (callbackExit != null && (filterTag == null || col.gameObject.tag == filterTag)) 
		{
			callbackExit (gameObject, col);
		}
	}
	
	public void attach (TriggerCallback newCallbackEnter = null, TriggerCallback newCallbackExit = null)
	{
		callbackEnter = newCallbackEnter;
		callbackExit = newCallbackExit;
	}
}