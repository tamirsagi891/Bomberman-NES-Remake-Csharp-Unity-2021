using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private GameManager _gameManager;
    private AudioManager _audioManager;

    #region Spawn Vectors

    private readonly Vector3 _startPos = new Vector3(-6, 4, -2); // the top left corner of the walkable grid
    private readonly List<Vector3> _emptyPlaces = new List<Vector3>();
    private readonly List<int> _spawnLocationsIndex = new List<int>();

    #endregion

    #region Level Variables

    private int _amountOfWalls;
    private int _amountOfMonsters;
    public float time = 200;
    private bool _spawnedCoins = false;

    #endregion

    #region Prefabs

    public GameObject wallPrefab;
    public GameObject monsterPrefab;
    public GameObject coinPrefab;
    public GameObject doorPrefab;
    public GameObject sirens;

    #endregion

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _audioManager = FindObjectOfType<AudioManager>();
    }

    private void Start()
    {
        SetInput();
        print(_amountOfMonsters);
        print(_gameManager.NumberOfWalls);
        FillEmptyPlacesList();
        FillSpawnIndexList();
        SpawnObjects();
    }

    private void Update()
    {
        if (IsPlayerChase())
            sirens.gameObject.SetActive(true);
        else
        {
            _audioManager.PauseSound("Detected");
            sirens.gameObject.SetActive(false);
        }

        if (time > 0)
            time -= Time.deltaTime;
        else
        {
            time = 0;
            if (_spawnedCoins) return;
            KillAllMonsters();
            SpawnCoins();
        }
    }

    // sets the amount of walls & monsters according the the values set in the game manager
    private void SetInput()
    {
        _amountOfWalls = _gameManager.numberOfWalls;
        _amountOfMonsters = _gameManager.numberOfMonsters;
    }

    // spawns the walls and monsters to the level as well as a door to the next stage
    private void SpawnObjects()
    {
        SpawnDoor();

        for (var i = 0; i < _amountOfWalls; i++)
            Instantiate(wallPrefab, _emptyPlaces[_spawnLocationsIndex[i]], Quaternion.identity);

        for (var i = _amountOfWalls; i < _amountOfMonsters + _amountOfWalls; i++)
            Instantiate(monsterPrefab, _emptyPlaces[_spawnLocationsIndex[i]], Quaternion.identity);
    }

    // spawns the door to the next stage underneath a random wall
    private void SpawnDoor()
    {
        var doorPos = _emptyPlaces[_spawnLocationsIndex[UnityEngine.Random.Range(0, _amountOfWalls)]];
        _gameManager.DoorPos = doorPos;
        doorPos.z = -1;
        Instantiate(doorPrefab, doorPos, Quaternion.identity);
    }

    // this func fill a list of vector with all the empty places on the level grid
    private void FillEmptyPlacesList()
    {
        // bomberman board size is 29x11
        for (var i = 0; i < 29; i++)
        for (var j = 0; j < 11; j++)
        {
            if ((i % 2 == 0 || j % 2 == 0) && !(i == 0 && j == 0) && !(i == 1 && j == 0) &&
                !(i == 0 && j == 1)) // in order to not include block indexes
                _emptyPlaces.Add(new Vector3(i, -j, 0) + _startPos);
        }
    }

    // this func spawns eight coin type monsters on the (almost) middle of the screen
    private void SpawnCoins()
    {
        for (var i = 0; i < 8; i++)
        {
            var coin = Instantiate(coinPrefab, new Vector3(6, 0, -3), quaternion.identity);
        }

        _spawnedCoins = true;
    }

    // this func kills all the monsters on the screen
    private static void KillAllMonsters()
    {
        foreach (var mon in GameObject.FindGameObjectsWithTag("Monster"))
        {
            Destroy(mon.gameObject);
        }
    }

    private bool IsPlayerChase()
    {
        return GameObject.FindGameObjectsWithTag("Monster")
            .Any(mon => mon.gameObject.GetComponent<MonsterManager>().chasePlayer);
    }


    // this func randomly chooses free spaces on the level grid to place the walls and monsters 
    private void FillSpawnIndexList()
    {
        while (_spawnLocationsIndex.Count < _amountOfWalls + _amountOfMonsters)
        {
            var tempNum = UnityEngine.Random.Range(0, _emptyPlaces.Count);
            if (!_spawnLocationsIndex.Contains(tempNum))
                _spawnLocationsIndex.Add(tempNum);
        }
    }
}