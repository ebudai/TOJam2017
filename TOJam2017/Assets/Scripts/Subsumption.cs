using UnityEngine;
using System.Collections;

public delegate void ExecuteRule();

public class SubsumptionRule
{
	public int priority;
	public ExecuteRule behaviourScript;

	public SubsumptionRule (int initPriority, ExecuteRule initScript)
	{
		priority = initPriority;
		behaviourScript = initScript;
	}
}

public class BotCommandStruct
{
	public bool fire = false;
	public Vector3 firingAngle;	
	public Vector3 velocity;
    public Vector3 torque;
    public float thrust;
	public Quaternion rotation;
    public Vector3 angularCorrection;
}

public struct BotState
{
	public float distToPlayer;
	public Vector3 desiredHeading;

	public Vector3 dirToPlayer;
	public Vector3 firingAngle;
	public Vector3 closestMoveDir;
	public bool playerSpotted;
	public bool alive;
}

//private Vector3[] cardinalDirs = {
//    Vector3.forward,
//    Vector3.right,
//    Vector3.back,
//    Vector3.left,
//    (Vector3.forward+Vector3.right).normalized,
//    (Vector3.back+Vector3.right).normalized,
//    (Vector3.back+Vector3.left).normalized,
//    (Vector3.forward+Vector3.left).normalized,
//};