using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour 
{
    public string loadLevel;
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
    public Rigidbody eelMob;
    public int jellyDensity = 100;

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
    private float waveStartTime;
    private int numCrabs = 3;
    private int numEels = 2;
    private int level = 1;
    private int score = 0;
    private int pLives = 3;
    //float theEnd = 0;

    private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			//DontDestroyOnLoad(gameObject);
		}
		else if (instance != this)
		{
			Debug.LogWarning("Master GameController already exists, prejudicially exterminating new instance.");
			Destroy(this);
		}
	}

	void Start () 
	{
        level = 1;
        score = 0;
        pLives = 3;
        StartCoroutine(SpawnJellies());
        StartCoroutine(SpawnWaves());
        StartCoroutine(WaveNotice());
    }

	private int lastHazardCount = 0;
	private int newHazardCount;

	void Update ()
	{
        if (Input.GetKey(KeyCode.Escape))
        {
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        if (gameOver)
        {
            GameObject persistentGameObject = GameObject.Find("PlayerInfoStore");
            var persistentScript = persistentGameObject.GetComponent<PlayerInfo>();
            persistentScript.score = score;
            SceneManager.LoadScene(loadLevel);
        }
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
                Instantiate(jellyMob, player.transform.position + spawnOffset, jellyMob.transform.rotation);
            }
        }

        while (true)
        { 
            jelliesArr = GameObject.FindGameObjectsWithTag("Jelly");
            if (jelliesArr.Length < jellyDensity)
            {
                //not enough jellies, create!
                for (int i = jelliesArr.Length; i < jellyDensity; i++)
                {
                    //Debug.Log("Spawning jelly: " + i);
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
                    Instantiate(jellyMob, player.transform.position + spawnOffset, jellyMob.transform.rotation);
                }
            }
            yield return new WaitForSeconds(5.0f);
        }
    }

    IEnumerator WaveNotice()
    {
        var waveNotice = GameObject.Find("Canvas/WaveNotice").GetComponent<Text>();
        while (true)
        {
            if (Time.time - 1.3f < waveStartTime)
            {
                waveNotice.text = "WAVE " + level.ToString();
            }
            else
            {
                waveNotice.text = "";
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private bool waveSpawned = false;
    IEnumerator SpawnWaves()
    {
        var player = GameObject.Find("PlayerShip");
        while (true)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies.Length == 0) 
            {
                if (!waveSpawned)
                {
                    waveStartTime = Time.time;
                    //no crabs, create!
                    int i;
                    for (i = 0; i < numCrabs; i++)
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
                        Vector3 spawnOffset = new Vector3(xOffset, yOffset, zOffset);
                        Instantiate(crabMob, player.transform.position + spawnOffset + (player.transform.forward * 35), crabMob.transform.rotation);
                    }
                    for (i = 0; i < numEels; i++)
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
                        Vector3 spawnOffset = new Vector3(xOffset, yOffset, zOffset);
                        Instantiate(eelMob, player.transform.position + spawnOffset + (player.transform.forward * 35), eelMob.transform.rotation);
                    }
                    waveSpawned = true;
                }
                else
                {
                    //go to next wave
                    waveSpawned = false;
                    numCrabs += 3;
                    numEels += 2;
                    level += 1;
                }
            }
            yield return new WaitForSeconds(6.0f);
        }
    }

	public void AddScore(int newScoreValue) 
	{
		score += newScoreValue;
	}

    public void LoseLife()
    {
        pLives -= 1;
        Debug.Log("Lives: " +pLives);
        if (pLives == 0)
        {
            GameOver();
        }
    }

    public int GetLives()
    {
        return pLives;
    }

    public int GetScore()
    {
        return score;
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
        switch (trigger.tag)
        {
            case "Projectile" :
                if (collided.tag == "Player")
                {
                    var playerBrain = collided.GetComponent<PilotController>();
                    playerBrain.TakeHit();
                    trigger.GetComponent<MissileBehaviour>().Explode();
                }
                else
                {
                    trigger.GetComponent<MissileBehaviour>().Die();
                }
                break;
            case "PlayerProjectile" :
                if (collided.tag == "Enemy")
                {

                    var player = GameObject.Find("PlayerShip");
                    if (player != null)
                    {
                        var playerBrain = player.GetComponent<PilotController>();
                        playerBrain.playHit();
                    }

                    var enemyBrain = collided.gameObject.GetComponent<CrabBehaviour>();
                    var eelBrain = collided.gameObject.GetComponent<EelBehaviour>();
                    if (eelBrain != null && eelBrain.isAlive())
                    {
                        eelBrain.TakeHit();
                        trigger.GetComponent<MissileBehaviour>().Explode();
                    }
                    if (enemyBrain != null && enemyBrain.isAlive())
                    { 
                        enemyBrain.TakeHit();
                        trigger.GetComponent<MissileBehaviour>().Explode();
                    }
                }
                else
                {
                    trigger.GetComponent<MissileBehaviour>().Die();
                }
                break;
            case "Enemy":
                if (collided.tag == "Player")
                {
                    //Debug.Log(trigger.tag + " hit " + collided.tag);
                    //is it an eel?
                    var eelBrain = trigger.GetComponent<EelBehaviour>();
                    if (eelBrain != null)
                    {
                        //play collision sound
                        eelBrain.DoBite();
                        var playerBrain = collided.GetComponent<PilotController>();
                        playerBrain.TakeHit();
                    }
                }
                break;
            case "Player" :
                if (collided.tag == "Jelly")
                {
                    //Debug.Log(trigger.tag + " hit " + collided.tag);
                    //trigger.GetComponent<PilotController>().Stop();
                }
                //Debug.Log(trigger.tag + " hit " + collided.tag);
                break;
        }
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