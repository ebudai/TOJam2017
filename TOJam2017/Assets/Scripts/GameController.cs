using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour 
{
	private static GameController instance;
	public static GameController Instance
	{
		get
		{
			if (instance == null)
			{
				Debug.LogWarning("No Master GameController, instantiating!");
				GameObject owner = new GameObject("GameController");
				instance = owner.AddComponent<GameController>();
			}
			return instance;
		}
	}
	
	//TODO: replace these public vars with getters
	public bool gameOver = false;
	public bool restart;

    public Rigidbody jellyMob;
    public Rigidbody crabMob;
    public int jellyDensity = 50;

	private Dictionary<string, int>score =  new Dictionary<string, int>();

	private float xMin = -5f;
	private float xMax = 5f;
	private float zMin = -14f;
	private float zMax = -6.5f;

	private Dictionary<string, Vector3> playerPositions = new Dictionary<string, Vector3>();
	private Dictionary<string, Vector3> playerVelocites = new Dictionary<string, Vector3>();
	private Dictionary<string, Transform> playerTransforms = new Dictionary<string, Transform>();
	private Dictionary<string, List<PlayerMoveRecord>> playerMoves = new Dictionary<string, List<PlayerMoveRecord>>();

	private GameObject[] hazards;
	private GameObject[] spawns;

    private List<Rigidbody> jellies = new List<Rigidbody>();
    private List<Rigidbody> crabs = new List<Rigidbody>();
    private int waveSize = 3;
    private int level = 0;
	//float theEnd = 0;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else if (instance != this)
		{
			Debug.LogWarning("Master GameController already exists, prejudicially exterminating new instance.");
			Destroy(this);
		}
	}

	void Start () 
	{
        StartCoroutine(SpawnJellies());
        StartCoroutine(SpawnWaves());
    }

	private int lastHazardCount = 0;
	private int newHazardCount;

	void Update ()
	{		
        /*
		if (newHazardCount != lastHazardCount)
		{
			Debug.Log ("No hazards changed to: " + newHazardCount);
			lastHazardCount = newHazardCount;
		}

		if(gameOver)
		{
			theEnd += Time.deltaTime;
			if(theEnd>0.667f)
				Time.timeScale = 0;
		}
        */      
	}

    IEnumerator SpawnJellies()
    {
        //find spawn Jellies
        GameObject[] jelliesArr = GameObject.FindGameObjectsWithTag("Jelly");
        var player = GameObject.Find("PlayerShip");
        if (jelliesArr.Length == 0)
        {
            //no jellies, create!
            int i = 0;
            for (i=0; i < jellyDensity; i++)
            {
                //Debug.Log("Spawning jelly: " + i);
                //spawn a jelly from 10-100 units away in a random dir...
                float xOffset = Random.Range(-60f, 60.0f);
                float yOffset = Random.Range(-60f, 60.0f);
                float zOffset = Random.Range(-60f, 60.0f);

                if (xOffset < 0 && xOffset > -10)
                {
                    xOffset -= 10;
                }
                if (xOffset > 0 && xOffset < -10)
                {
                    xOffset += 10;
                }
                if (yOffset < 0 && yOffset > -10)
                {
                    yOffset -= 10;
                }
                if (yOffset > 0 && yOffset < -10)
                {
                    yOffset += 10;
                }
                if (zOffset < 0 && zOffset > -10)
                {
                    zOffset -= 10;
                }
                if (zOffset > 0 && zOffset < -10)
                {
                    zOffset += 10;
                }
               // Debug.Log("spawnOffset: " + xOffset + "," + yOffset + "," + zOffset);
                Vector3 spawnOffset = new Vector3(xOffset, yOffset, zOffset);
                jellies.Add(Instantiate(jellyMob, player.transform.position + spawnOffset, jellyMob.transform.rotation));
            }
        }

        while (true)
        {
            List<Rigidbody> removeList = new List<Rigidbody>();
            foreach (Rigidbody jelly in jellies)
            {
                if (jelly != null)
                {
                    var jellyBrain = jelly.GetComponent<JellyBehaviour>();
                    if (jellyBrain.isDead())
                    {
                        Debug.Log("Removing dead jelly");
                        removeList.Add(jelly);
                    }
                }
            }
            foreach (Rigidbody jelly in removeList)
            {
                jellies.Remove(jelly);
            }

            if (jellies.Count < jellyDensity)
            {
                //not enough jellies, create!
                for (int i = jellies.Count; i < jellyDensity; i++)
                {
                    Debug.Log("Spawning jelly: " + i);
                    //spawn a jelly from 10-100 units away in a random dir...
                    float xOffset = Random.Range(-60f, 60f);
                    float yOffset = Random.Range(-60f, 60f);
                    float zOffset = Random.Range(-60f, 60f);

                    if (xOffset < 0 && xOffset > -20)
                    {
                        xOffset -= 20;
                    }
                    if (xOffset > 0 && xOffset < -20)
                    {
                        xOffset += 20;
                    }
                    if (yOffset < 0 && yOffset > -20)
                    {
                        yOffset -= 20;
                    }
                    if (yOffset > 0 && yOffset < -20)
                    {
                        yOffset += 20;
                    }
                    if (zOffset < 0 && zOffset > -20)
                    {
                        zOffset -= 20;
                    }
                    if (zOffset > 0 && zOffset < -20)
                    {
                        zOffset += 20;
                    }
                    //Debug.Log("spawnOffset: " + xOffset + "," + yOffset + "," + zOffset);
                    Vector3 spawnOffset = new Vector3(xOffset, yOffset, zOffset);
                    jellies.Add(Instantiate(jellyMob, player.transform.position + spawnOffset, jellyMob.transform.rotation));
                }
            }
            yield return new WaitForSeconds(5.0f);
        }
    }

    private bool waveSpawned = false;

    IEnumerator SpawnWaves()
    {
        var player = GameObject.Find("PlayerShip");
        while (true)
        {
            if (crabs.Count == 0 && !waveSpawned)
            {
                //no jellies, create!
                int i = 0;
                for (i = 0; i < waveSize; i++)
                {
                   //Debug.Log("Spawning crab: " + i);
                    float xOffset = Random.Range(-60f, 60.0f);
                    float yOffset = Random.Range(-60f, 60.0f);
                    float zOffset = Random.Range(-60f, 60.0f);

                    if (xOffset < 0 && xOffset > -10)
                    {
                        xOffset -= 10;
                    }
                    if (xOffset > 0 && xOffset < -10)
                    {
                        xOffset += 10;
                    }
                    if (yOffset < 0 && yOffset > -10)
                    {
                        yOffset -= 10;
                    }
                    if (yOffset > 0 && yOffset < -10)
                    {
                        yOffset += 10;
                    }
                    if (zOffset < 0 && zOffset > -10)
                    {
                        zOffset -= 10;
                    }
                    if (zOffset > 0 && zOffset < -10)
                    {
                        zOffset += 10;
                    }
                    //Debug.Log("spawnOffset: " + xOffset + "," + yOffset + "," + zOffset);
                    Vector3 spawnOffset = new Vector3(xOffset, yOffset, zOffset);
                    crabs.Add(Instantiate(crabMob, player.transform.position + spawnOffset, crabMob.transform.rotation));
                }
                waveSpawned = true;
            }
            yield return new WaitForSeconds(10.0f);
            //    if (hazardPrefabs.Length > 0)
            //    {
            //        for (int i = 0; i < hazardCount; i++)
            //        {
            //            var enemyIndex = i % 2;
            //            var spawnPoint = spawns[Random.Range(0, spawns.Length)];
            //            Instantiate(hazardPrefabs[enemyIndex], spawnPoint.transform.position, hazardPrefabs[enemyIndex].transform.rotation);

            //            if (GameController.Instance.gameOver)
            //            {
            //                GameController.Instance.restart = true;
            //                break;
            //            }
            //            yield return new WaitForSeconds(spawnWait);
            //        }
            //    }
            //    yield return new WaitForSeconds(waveWait);
        }
    }


    public void Reset () 
	{
		gameOver = false;
		restart = false;
		score["Player1"] = 0;
		playerMoves["Player1"] = new List<PlayerMoveRecord>();
	}

	public void AddScore(string playerId, int newScoreValue) 
	{
		score[playerId] += newScoreValue;
	}

	public void GameOver()
	{
		gameOver = true;
	}

	public void setPlayerPos (string playerId, Vector3 newPos) 
	{
		playerPositions[playerId] = newPos;
	}

	public Vector3 getPlayerPos(string playerId)
	{
		return playerPositions[playerId];
	}

	public void setPlayerVel (string playerId, Vector3 newVel) 
	{
		playerVelocites[playerId] = newVel;
	}
	
	public Vector3 getPlayerVel(string playerId)
	{
		return playerVelocites[playerId];
	}

	public void setPlayerTrans (string playerId, Transform newTrans) 
	{
		playerTransforms[playerId] = newTrans;
	}
	
	public Transform getPlayerTrans(string playerId)
	{
		return playerTransforms[playerId];
	}

	public float getPlayerHeading(string playerId)
	{
		var velocityRotation = Quaternion.FromToRotation (new Vector3(0f,0f,0f), playerVelocites[playerId]);
		return velocityRotation.eulerAngles.y;
	}

	public void addPlayerMove(string playerId, PlayerMoveRecord newMove)
	{

		if (playerMoves.ContainsKey(playerId))
	    {
			playerMoves[playerId].Insert (0, newMove);
		} 
		else
		{
			Debug.Log ("no move dict for: "+playerId);
		}
	}

    public Vector3 HeadOnTarget(GameObject shooter, string playerId)
    {
        return GameController.Instance.getPlayerPos (playerId);
    }
    
    public Vector3 LinearTarget(GameObject shooter, string playerId)
    {
		Vector3 predictedPosition;

        if (playerMoves[playerId].Count >= 2)
        {
            var currplayerPositions = getPlayerPos(playerId);
            PlayerMoveRecord currMove = playerMoves[playerId][0];
            PlayerMoveRecord prevMove = playerMoves[playerId][1];
			
			Vector3 deltaPosition = currMove.position - prevMove.position;
			float deltaTime = currMove.time - prevMove.time;
			Vector3 moveRate = deltaPosition / deltaTime;
			
			//get initial guess at time T
			float initialDist = Vector3.Distance (shooter.transform.position, currplayerPositions);
			float initialTime = initialDist / 7;
			float predictedX = currplayerPositions.x + (moveRate.x * initialTime);
			float predictedZ = currplayerPositions.z + (moveRate.z * initialTime);

			//get first refined guess
			float predictedDist = Vector3.Distance (shooter.transform.position, new Vector3(predictedX, currplayerPositions.y, predictedZ));
			float predictedTime = predictedDist / 7;
			float timeError = predictedTime - initialTime;
			int iterationCount = 0;
			//iterate to improve
			while (iterationCount < 10 && timeError > 0.1)
			{
				//re-predict using new time
				predictedX = currplayerPositions.x + (moveRate.x * predictedTime);
				predictedZ = currplayerPositions.z + (moveRate.z * predictedTime);
				initialTime = predictedTime;

				//get first refined guess
				predictedDist = Vector3.Distance (shooter.transform.position, new Vector3(predictedX, currplayerPositions.y, predictedZ));
				predictedTime = predictedDist / 7;
				timeError = predictedTime - initialTime;
				iterationCount++;
			}
			predictedPosition = new Vector3
				(
					Mathf.Clamp (predictedX, xMin+2, xMax-2), 
					currplayerPositions.y, 
					Mathf.Clamp (predictedZ, zMin+2, zMax-2)
				);
			return predictedPosition;
		} 
		else
		{
            return HeadOnTarget (shooter, playerId);
		}
	}

    public Vector3 CircularTarget(GameObject shooter, string playerId)
    {       
        Vector3 predictedPosition;
        //need to get delta rotation (x, y or z?) for past time frame
        //so need at least two entries in player move record
        if (playerMoves[playerId].Count >= 2)
		{
            var currplayerPositions = getPlayerPos(playerId);
            var currPlayerHeading = getPlayerHeading(playerId);
            PlayerMoveRecord currMove = playerMoves[playerId][0];
            PlayerMoveRecord prevMove = playerMoves[playerId][1];

			var deltaRotation = Quaternion.FromToRotation (prevMove.velocity, currMove.velocity);
			float deltaTime = currMove.time - prevMove.time;
			if (deltaRotation.eulerAngles.y <= Mathf.Epsilon)
			{
                return LinearTarget (shooter, playerId);
			}
			float turnRate = deltaRotation.eulerAngles.y / deltaTime;
			//rotation around the Y axis (up and down) is the 2d facing angle
			//Debug.Log ("Circular: Delta heading:"+deltaRotation.eulerAngles.y+", Delta Time:"+deltaTime+", Turn rate:"+turnRate);

			//get initial guess at time T
			float initialDist = Vector3.Distance (shooter.transform.position, currplayerPositions);
			float initialTime = initialDist/7;

			float predictedX = currplayerPositions.x - ( (3/turnRate) * (Mathf.Cos(currPlayerHeading - (turnRate*initialTime)) - Mathf.Cos(currPlayerHeading)));
			float predictedZ = currplayerPositions.z - ( (3/turnRate) * (Mathf.Sin(currPlayerHeading - (turnRate*initialTime)) - Mathf.Sin(currPlayerHeading)));
			//get first refined guess
			float predictedDist = Vector3.Distance (shooter.transform.position, new Vector3(predictedX, currplayerPositions.y, predictedZ));
			float predictedTime = predictedDist/7;
			float timeError = predictedTime - initialTime;

			int iterationCount = 0;
			//iterate to improve
			while (iterationCount < 10 && timeError > 0.1)
			{
				//re-predict using new time
				predictedX = currplayerPositions.x - ( (3/turnRate) * (Mathf.Cos(currPlayerHeading - (turnRate*predictedTime)) - Mathf.Cos(currPlayerHeading)));
				predictedZ = currplayerPositions.z - ( (3/turnRate) * (Mathf.Sin(currPlayerHeading - (turnRate*predictedTime)) - Mathf.Sin(currPlayerHeading)));
				initialTime = predictedTime;
				//get first refined guess
				predictedDist = Vector3.Distance (shooter.transform.position, new Vector3(predictedX, currplayerPositions.y, predictedZ));
				predictedTime = predictedDist/7;
				timeError = predictedTime - initialTime;
				iterationCount++;
			}
			predictedPosition = new Vector3
				(
					Mathf.Clamp (predictedX, xMin+2, xMax-2), 
					currplayerPositions.y, 
					Mathf.Clamp (predictedZ, zMin+2, zMax-2)
				);
			//Debug.Log ("Circularly predicted location:["+predictedPosition.x+","+currplayerPositions.y+","+predictedPosition.z+"], flight time:"+predictedTime);
			return predictedPosition;
		} 
		else
		{
            return HeadOnTarget (shooter, playerId);
		}
	}

	public void handleEnterCollision (GameObject trigger, Collider collided)  
	{
        //Debug.Log(trigger.tag + " hit " + collided.tag);
		//if (trigger.tag == "<SomeProjectile>") 
		//{
		//	if (collided.tag == "Wall") 
		//	{
		//		//...
		//	}
		//	if (collided.tag == "Player") 
		//	{
		//		//...
		//	}
		//}
		////...
	}

	public void handleExitCollision (GameObject trigger, Collider collided)  
	{
		//etc
	}
}

//Track player movements for targeting
public class PlayerMoveRecord
{
	public float time;
	public Vector3 position;	
	public Vector3 velocity;

	public PlayerMoveRecord(float currTime, Vector3 currPosition, Vector3 currVelocity) 
	{
		time = currTime;
		position = currPosition;
		velocity = currVelocity;
	}
}
