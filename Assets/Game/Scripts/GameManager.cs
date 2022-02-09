using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _shared;

    public int Score { get; set; }
    public int StageNumber { get; set; }
    public int NumberOfWalls { get; set; }
    public int NumberOfMonsters { get; set; }
    public int BombsOnScreen { get; set; }
    public bool IsDoorOpen { get; set; }
    public Vector3 DoorPos { get; set; }

    // a player struct containing the data for a player
    public struct Player
    {
        public bool detonator;
        public float moveSpeed;
        public int amountOfBombs;
        public int bombRadius;
        public int lifePoints;

        public Player(bool detonator, float moveSpeed, int amountOfBombs, int bombRadius, int lifePoints)
        {
            this.detonator = detonator;
            this.moveSpeed = moveSpeed;
            this.amountOfBombs = amountOfBombs;
            this.bombRadius = bombRadius;
            this.lifePoints = lifePoints;
        }
    }

    #region Player Data

    // this region updates only during the first launch of the game
    public float moveSpeed = 3;
    [Range(1, 10)] public int amountOfBombs = 1;
    [Range(1, 5)] public int bombRadius = 1;
    [Range(1, 30)] public int lifePoints = 3;
    [Range(1, 50)] public int stageNum = 1;

    #endregion

    public Player PlayerData;

    #region Inspector Control

    [Range(1, 80)] [SerializeField] public int numberOfWalls = 55;
    [Range(0, 20)] [SerializeField] public int numberOfMonsters = 6;
    [Range(1, 99)] public int powerUpSpawnRate = 10;
    
    [Range(1, 50)] public int transparencyDuration = 3;
    [Range(1, 99)] public int transparencyCooldown = 15;

    #endregion

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SetDefaultVariables();
    }

    // this func sets the default variables for the public vars
    private void SetDefaultVariables()
    {
        PlayerData = new Player(false, moveSpeed, amountOfBombs, bombRadius, lifePoints);
        IsDoorOpen = false;
        BombsOnScreen = 0;
        Score = 0;
        StageNumber = stageNum;
        DoorPos = Vector3.zero;
        NumberOfWalls = numberOfWalls;
        NumberOfMonsters = numberOfMonsters;
    }

    // this func changes to fitting scene with regards to the player data. it also resets some
    // of the inputs.
    public void PlayerDied()
    {
        if (PlayerData.lifePoints == 0)
        {
            SceneManager.LoadScene("Transition", LoadSceneMode.Single);
        }
        else
        {
            NumberOfWalls = numberOfWalls;
            NumberOfMonsters = numberOfMonsters;
            BombsOnScreen = 0;
            SceneManager.LoadScene("Transition", LoadSceneMode.Single);
        }
    }

    // this func take the player to the next stage
    public void NextLevel()
    {
        NumberOfWalls = numberOfWalls;
        NumberOfMonsters = numberOfMonsters;
        BombsOnScreen = 0;
        PlayerData.lifePoints += 1;
        SceneManager.LoadScene("Transition", LoadSceneMode.Single);
    }

    // updates the amount of walls after a wall is destroyed, checks if the door is open for the player
    public void DestroyWall()
    {
        var wallList = GameObject.FindGameObjectsWithTag("Wall");
        NumberOfWalls = wallList.Length;
        print("Number of walls left is: " + NumberOfWalls);
        OpenDoor();
    }

    // updates the amount of monsters after a monster died, checks if the door is open for the player
    public void KillMonster()
    {
        var monList = GameObject.FindGameObjectsWithTag("Monster");
        NumberOfMonsters = monList.Length;
        print("Number of monster left is: " + NumberOfMonsters);
        OpenDoor();
    }

    // open the door to the next stage if there are no walls/monsters
    // this func can be overriden using the "Open Door" feature in the player prefab inspector
    private void OpenDoor()
    {
        if (NumberOfMonsters + NumberOfWalls != 0) return;
        print("Door is open!");
        IsDoorOpen = true;
    }
    
    // one destroyed go to main menu
    private void OnDestroy()
    {
        SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
    }
}