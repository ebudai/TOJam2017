using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
	public GameObject[] hazardPrefabs;
	public int hazardCount;
	public float spawnWait;
	public float startWait;
	public float waveWait;

	public Texture2D[] gui_screens;
	public GUIStyle gui_menu;

	int gameMode = 1;
	int plives = 3;

    void Start () 
    {
        GameController.Instance.Reset ();   
        StartCoroutine( SpawnWavesL1 ());
	}

	IEnumerator SpawnWavesL1 () 
    {
        //find spawn points
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("SpawnPointL1");
        foreach(GameObject spawn in spawns)
        {
            Instantiate (hazardPrefabs[1], spawn.transform.position, hazardPrefabs[1].transform.rotation);
        }
        yield return new WaitForSeconds(startWait);
        while (true)
        {
            if (hazardPrefabs.Length > 0) 
            {
                for (int i=0;i<hazardCount;i++) 
                {
                    var enemyIndex = i % 2;
                    var spawnPoint = spawns[Random.Range(0, spawns.Length)];
                    Instantiate (hazardPrefabs[enemyIndex], spawnPoint.transform.position, hazardPrefabs[enemyIndex].transform.rotation);

					if (GameController.Instance.gameOver) 
					{
						GameController.Instance.restart = true;
						break;
					}
                    yield return new WaitForSeconds(spawnWait);
                }
            }
            yield return new WaitForSeconds(waveWait);
        }
    }

	// Update is called once per animation frame
	void Update () 
	{
		//plives = GameObject.Find("Player").GetComponent<PlayerBehaviour>().lives;

		if (GameObject.Find("Player") == null) 
		{
			gameMode = 2;
		}
			
		if(gameMode == 0)
		{
			Time.timeScale = 0;
		}

		if (Input.GetKeyDown (KeyCode.R)) 
		{
			Application.LoadLevel (Application.loadedLevel);
		}

		if(Input.GetKeyDown(KeyCode.Joystick1Button7) || Input.GetKeyDown(KeyCode.Joystick2Button7))
		{
			if(gameMode == 0)
			{
				gameMode = 1;
				Time.timeScale = 1;
			}
			else if(gameMode == 1)
			{
				gameMode = 0;
				Time.timeScale = 0;
			}
			else
			{
				Application.LoadLevel (Application.loadedLevel);
			}
		}
	}

	void OnGUI()
	{
		if (gameMode == 0)
		{
			//...GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),gui_screens[0],ScaleMode.ScaleToFit);
		}
		else
		if (gameMode == 1)
		{
			//...
		}
		else
		{
			//...GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),gui_screens[1],ScaleMode.ScaleToFit);
		}
	}


}
