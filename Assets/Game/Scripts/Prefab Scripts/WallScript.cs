using UnityEngine;
using Random = UnityEngine.Random;

public class WallScript : MonoBehaviour
{
    private GameManager _gameManager;
    private Animator _animator;
    public GameObject[] powerUps = new GameObject[3];
    private int _spawnRate;

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _animator = gameObject.GetComponent<Animator>();
        _spawnRate = _gameManager.powerUpSpawnRate;
    }

    // updates the power up spawns rate from exploding a wall accordingly to the data
    // give at the game manager
    private void Update()
    {
        _spawnRate = _gameManager.powerUpSpawnRate;
    }


    // explodes a wall by playing the animation and choosing to spawn a power up
    public void Explode()
    {
        _animator.Play("Wall Destroy");
        SpawnPowerUp();
        Destroy(gameObject, 0.9f);
    }

    // randomly spawns a power up with (_spawnRate)% 
    private void SpawnPowerUp()
    {
        if (transform.position == _gameManager.DoorPos) return;
        if (UnityEngine.Random.Range(1, 100) > _spawnRate) return;
        var powerUp = Instantiate(powerUps[Random.Range(0, powerUps.Length)]);
        powerUp.transform.position = transform.position;
    }

    // updates the game manager that a wall got destroyed
    private void OnDestroy()
    {
        _gameManager.DestroyWall();
    }
}