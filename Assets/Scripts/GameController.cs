using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance = null;

    const float width = 510f;
    const float height = 960f;

    public float snakeSpeed = 100f;

    public BodyPart bodyPrefab = null;
    public GameObject rockPrefab = null;
    public GameObject eggPrefab = null;
    public GameObject goldEggPrefab = null;
    public GameObject spikePrefab = null;

    public Sprite tailSprite = null;
    public Sprite bodySprite = null;

    public SnakeHead snakeHead = null;

    public bool alive = true;

    public bool waitingToPlay = true;

    List<Egg> eggs = new List<Egg>();
    List<Spike> spikes = new List<Spike>();

    int level = 0;
    int noOfEggsForNextLevel = 0;

    public int score   = 0;
    public int highScore = 0;

    public TMP_Text scoreText = null;
    public TMP_Text highScoreText = null;
    public TMP_Text gameOverText = null;
    public TMP_Text tapToPlayText = null;
    public TMP_Text levelText = null;

    public AudioSource levelUpSound = null;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        Debug.Log("Starting Snake Game");
        CreateWalls();
        alive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(waitingToPlay)
        {
            foreach(Touch touch in Input.touches)
            {
                if(touch.phase == TouchPhase.Ended)
                {
                    StartGamePlay();
                }
            }
            if (Input.GetMouseButtonUp(0))
                StartGamePlay();
        }
    }

    public void GameOver()
    {
        alive = false;
        waitingToPlay = true;
        gameOverText.gameObject.SetActive(true);
        tapToPlayText.gameObject.SetActive(true);
    }

    void StartGamePlay()
    {
        score = 0;
        level = 0;

        scoreText.text = "Score: " + score.ToString();
        highScoreText.text = "High Score: " + highScore.ToString();

        gameOverText.gameObject.SetActive(false);
        tapToPlayText.gameObject.SetActive(false);

        waitingToPlay = false;
        alive = true;

        KillOldEggs();
        KillOldSpikes();
        LevelUp();
    }

    void LevelUp()
    {
        level++;
        if(level > 1)
            levelUpSound.Play();
        levelText.text = "Level: " + level.ToString();
        noOfEggsForNextLevel = 4 + (level * 2);

        snakeSpeed = 100f + (level*100/8f);
        if (snakeSpeed >= 500) snakeSpeed = 500f;

        snakeHead.ResetSnake();
        CreateEgg();
        KillOldSpikes();
        CreateSpikes();
    }
    public void EggEaten(Egg egg)
    {
        score += 5;
        noOfEggsForNextLevel--;
        if (noOfEggsForNextLevel == 0)
        {
            score += level * 10;
            LevelUp();
        }
        else if (noOfEggsForNextLevel == 1)
            CreateEgg(true);
        else
            CreateEgg(false);

        if (score > highScore)
        {
            highScore = score;
            highScoreText.text = "High Score: " + highScore.ToString();
        }
        scoreText.text = "Score: " + score.ToString();

        eggs.Remove(egg);
        Destroy(egg.gameObject);
    }
    void CreateWalls()
    {
        float z = -1f;
        Vector3 start = new Vector3(-width, -height, z);
        Vector3 finish = new Vector3(-width, height, z);
        CreateWall(start, finish);

        start = new Vector3(width, -height, z);
        finish = new Vector3(width, height, z);
        CreateWall(start, finish);

        start = new Vector3(-width, -height, z);
        finish = new Vector3(width, -height, z);
        CreateWall(start, finish);

        start = new Vector3(-width, height, z);
        finish = new Vector3(width, height, z);
        CreateWall(start, finish);
    }

    void CreateWall(Vector3 start, Vector3 finish)
    {
        float distance = Vector3.Distance(start, finish);
        int noOfRocks = (int)(distance * 0.025f);
        Vector3 delta = (finish - start)/noOfRocks;
        Vector3 position = start;
        for(int rock = 0; rock <= noOfRocks; rock++)
        {
            float rotation = Random.Range(0, 360f);
            float scale = Random.Range(1.5f, 2f);
            CreateRock(position, scale, rotation);
            position += delta;
        }
    }

    void CreateRock(Vector3 position, float scale, float rotation)
    {
        GameObject rock = Instantiate(rockPrefab, position, Quaternion.Euler(0,0,rotation));
        rock.transform.localScale = new Vector3(scale, scale, 1);
    }

    void CreateEgg(bool golden = false)
    {
        Vector3 position = Vector3.zero;
        bool positionValid = false;

        // Loop until a valid position for the egg is found
        while (!positionValid)
        {
            position.x = -width + Random.Range(100f, (width * 2) - 200f);
            position.y = -height + Random.Range(100f, (height * 2) - 200f);
            position.z = -1f;

            // Check if the position is not within the area where spikes are instantiated
            bool validPosition = true;
            foreach (Spike spike in spikes)
            {
                if (Vector3.Distance(position, spike.transform.position) < 20f)
                {
                    validPosition = false;
                    break;
                }
            }

            // If the position is valid, break the loop
            if (validPosition)
                positionValid = true;
        }

        // Instantiate the egg at the valid position
        Egg egg = null;
        if (golden)
            egg = Instantiate(goldEggPrefab, position, Quaternion.identity).GetComponent<Egg>();
        else
            egg = Instantiate(eggPrefab, position, Quaternion.identity).GetComponent<Egg>();
        eggs.Add(egg);
    }

    void CreateSpikes()
    {
        Vector3 defaultPosition = Vector3.zero; // Default position of the snake head

        for (int i = 1; i <= level; i++)
        {
            Vector3 position = Vector3.zero;
            bool positionValid = false;

            // Loop until a valid position for the spike is found
            while (!positionValid)
            {
                position.x = -width + Random.Range(100f, (width * 2) - 200f);
                position.y = -height + Random.Range(100f, (height * 2) - 200f);
                position.z = -1f;

                // Check if the position is not in front of the default position of the snake head
                if (Vector3.Distance(position, defaultPosition) > 150f)
                {
                    bool validPosition = true;

                    // Check if the position is not where other spikes are instantiated
                    foreach (Spike spik in spikes)
                    {
                        if (Vector3.Distance(position, spik.transform.position) < 20f)
                        {
                            validPosition = false;
                            break;
                        }
                    }

                    // If the position is valid, break the loop
                    if (validPosition)
                        positionValid = true;
                }
            }

            // Instantiate the spike at the valid position
            Spike spike = Instantiate(spikePrefab, position, Quaternion.Euler(0, 0, Random.Range(0, 360))).GetComponent<Spike>();
            spikes.Add(spike);
        }
    }


    void KillOldEggs()
    {
        foreach(Egg egg in eggs)
        {
            Destroy(egg.gameObject);
        }
        eggs.Clear();
    }

    void KillOldSpikes()
    {
        foreach(Spike spike in spikes)
        {
            Destroy(spike.gameObject);
        }
        spikes.Clear();
    }
}
