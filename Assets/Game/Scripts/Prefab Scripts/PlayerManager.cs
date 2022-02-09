using System;
using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private GameManager _gameManager;
    private AudioManager _audioManager;
    private GameManager.Player _playerData = new GameManager.Player();

    #region GM Controller

    public bool godMode = false;
    public bool forceOpenDoor = false;

    #endregion


    public Rigidbody2D rb;
    public GameObject bombPrefab;

    #region Scrips Variables

    private Animator _animator;
    private Collider2D[] _intersecting;
    private Vector2 _movement;
    private bool _playerDied = false;
    private bool _stageComplete = false;
    private float _stageTransitionTimer = 4.5f;
    private bool _playerTransparent = false;

    #endregion

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _audioManager = FindObjectOfType<AudioManager>();
        _playerData = _gameManager.PlayerData;
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (forceOpenDoor)
            _gameManager.IsDoorOpen = forceOpenDoor;

        if (_stageComplete)
        {
            if (_stageTransitionTimer < 0)
            {
                _gameManager.NextLevel();
            }

            _stageTransitionTimer -= Time.deltaTime;
            _animator.speed = 0;
            _movement = Vector2.zero;
            return;
        }

        if (_playerDied)
            return;

        if (Input.GetKeyDown("space") && (_gameManager.BombsOnScreen < _playerData.amountOfBombs))
            SpawnBomb();

        if (Input.GetKeyDown(KeyCode.LeftShift) && !_playerTransparent)
            StartCoroutine(Disappear());

        _movement.x = Input.GetAxisRaw("Horizontal");
        _movement.y = Input.GetAxisRaw("Vertical");

        if (Input.anyKey)
        {
            _animator.SetFloat("Horizontal", _movement.x);
            _animator.SetFloat("Vertical", _movement.y);
        }

        NudgePlayer();

        _animator.speed = _movement.magnitude > 0 ? 1 : 0;
    }

    // this func moves the player 
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + (_movement * _playerData.moveSpeed * Time.fixedDeltaTime));
    }

    // if player touched the door and the door is open we transition to the next stage
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Door") && _gameManager.IsDoorOpen)
        {
            _stageComplete = true;
            _audioManager.PauseSound("StageTheme");
            _audioManager.PlaySound("StageComplete");
            _gameManager.StageNumber += 1;
            _gameManager.IsDoorOpen = false;
            _gameManager.PlayerData = _playerData;
        }
    }

    // if player collided with a monster it dies
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Monster") && !godMode)
            PlayerDied();
    }

    // if player collided with the explosion it dies
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Explosion") && !godMode)
            PlayerDied();
    }

    // spawns a bomb if one doesnt exists yet
    private void SpawnBomb()
    {
        var bombPos = Vector2.zero;
        var playerPos = transform.position;
        bombPos.x = Mathf.Round(playerPos.x);
        bombPos.y = Mathf.Round(playerPos.y);
        if (!CheckForBomb(bombPos))
            return;
        _audioManager.PlaySound("PlaceBomb");
        var bomb = Instantiate(bombPrefab);
        bomb.transform.position = bombPos;
    }

    // check if there is a bomb on a given position
    private bool CheckForBomb(Vector2 bombPos)
    {
        _intersecting = Physics2D.OverlapCircleAll(bombPos, 0.1f);
        if (_intersecting.Length > 0)
            if (_intersecting[0].gameObject.CompareTag("Bomb"))
                return false;
        return true;
    }

    // audio functions triggered inside the player animations

    #region Animation Function

    private void PlayWalkingSoundRight()
    {
        _audioManager.PlaySound("WalkRight");
    }

    public void PlayWalkingSoundUp()
    {
        _audioManager.PlaySound("WalkUp");
    }

    public void PlayLifeLostSound()
    {
        _audioManager.PauseSound("StageTheme");
        _audioManager.PlaySound("LifeLost");
        gameObject.SetActive(false);
    }

    #endregion

    // player died function, stops its movement and plays the death sounds and animation
    // as well as updating the game manager data
    private void PlayerDied()
    {
        if (_playerDied) return;
        _movement = Vector2.zero;
        _playerDied = true;
        PlayerDiedAudioManagement();
        UnDetectMonsters();
        _animator.speed = 1;
        _playerData.lifePoints -= 1;
        _gameManager.PlayerData = _playerData;
        Destroy(this.gameObject, 3.5f);
    }
    
    // this func does all audio management for when a player dies
    private void PlayerDiedAudioManagement()
    {
        _audioManager.PauseSound("Chase");
        _audioManager.playerBeingChased = false;
        _audioManager.PlaySound("PlayerDied");
        _animator.Play("Player Died");
    }
    
    private void UnDetectMonsters()
    {
        foreach (var mon in GameObject.FindGameObjectsWithTag("Monster"))
        {
            mon.gameObject.GetComponent<MonsterManager>().PlayerUnDetected();
        }
    }

    // this func nudges the player over sharp corners
    private void NudgePlayer()
    {
        var nudge = 0.1;
        if (_movement == Vector2.down || _movement == Vector2.up)
        {
            nudge = CalcNudge(transform.position.x);
            NudgeHorizontally(nudge);
        }

        if (_movement == Vector2.left || _movement == Vector2.right)
        {
            nudge = CalcNudge(transform.position.y);
            NudgeVertically(nudge);
        }
    }

    // this func calculates the nudge (player distance from the center of a block value between (-1,1)
    private static double CalcNudge(float playerCoordinate)
    {
        var nudge = 0.1;
        var round = Math.Round(Math.Round(playerCoordinate / 2, MidpointRounding.AwayFromZero) * 2);
        if (Math.Truncate(playerCoordinate) % 2 == 0)
            nudge = Math.Floor(playerCoordinate) - playerCoordinate;
        else
            nudge = Math.Ceiling(playerCoordinate) - playerCoordinate;
        if (playerCoordinate < 0) return nudge * (-1);
        return nudge;
    }

    // nudges the player horizontally according to the nudge and movement direction
    private void NudgeHorizontally(double nudge)
    {
        if (-0.9 < nudge && nudge < -0.1)
            _movement = Vector2.left;
        if (0.1 < nudge && nudge < 0.9)
            _movement = Vector2.right;
    }

    // nudges the player vertically according to the nudge and movement direction
    private void NudgeVertically(double nudge)
    {
        if (-0.9 < nudge && nudge < -0.1)
            _movement = Vector2.down;
        if (0.1 < nudge && nudge < 0.9)
            _movement = Vector2.up;
    }

    // functions triggered by getting a power up 

    #region PowerUp Functions

    // player can place more bomb up to max of 10
    public void BombUp()
    {
        if (_playerData.amountOfBombs < 10)
            _playerData.amountOfBombs += 1;
    }

    // adds .2 to the player movement speed 
    public void SpeedUp()
    {
        _playerData.moveSpeed += 0.2f;
    }

    // adds +1 to the bomb radius up to max of 5
    public void FireUp()
    {
        if (_playerData.bombRadius < 5)
            _playerData.bombRadius += 1;
        _gameManager.PlayerData = _playerData;
    }

    #endregion

    // this coroutine makes the player disappear for the duration
    // when toggled bomb and monsters can harm the player
    private IEnumerator Disappear()
    {
        // making sure god mode is not toggled
        if (godMode) yield break;
        // setting variables
        var o = gameObject;
        var color = transform.GetComponent<SpriteRenderer>().color;
        TellMonPlayerDisappeared();
        // making player disappear for the amount of the duration
        o.layer = 0;
        godMode = true;
        color.a = 0.25f;
        transform.GetComponent<SpriteRenderer>().color = color;
        _playerTransparent = true;
        yield return new WaitForSeconds(_gameManager.transparencyDuration);
        // making player re-appear afterwards - 3 seconds by default
        o.layer = 3;
        godMode = false;
        color.a = 1f;
        transform.GetComponent<SpriteRenderer>().color = color;
        yield return new WaitForSeconds(_gameManager.transparencyCooldown);
        // recharges ability after the amount stated in the recharge - 15 seconds by default
        _playerTransparent = false;
        yield return null;
    }

    private static void TellMonPlayerDisappeared()
    {
        foreach (var mon in GameObject.FindGameObjectsWithTag("Monster"))
            mon.GetComponent<MonsterManager>().ActivatePlayerDisappeared();
    }

    // once a player died it activates the player died func of game manager
    private void OnDestroy()
    {
        _gameManager.PlayerDied();
    }
}