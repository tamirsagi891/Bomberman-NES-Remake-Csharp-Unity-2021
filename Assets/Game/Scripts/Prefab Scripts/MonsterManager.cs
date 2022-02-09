using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterManager : MonoBehaviour
{
    private GameManager _gameManager;
    private AudioManager _audioManager;

    public Rigidbody2D rb;
    public GameObject pointsText;

    #region Script Variables

    private Collider2D[] _intersecting;
    private readonly List<Vector2> _directions = new List<Vector2>();
    private Animator _animator;
    private Vector2 _movement;
    private Vector3 _temp; // a variable to hold a vector temporarily
    public float moveSpeed = 2f;
    private bool _monsterDied = false;
    private float _time = 0;

    #endregion

    #region Detect Player

    public GameObject exclamationMark;
    public GameObject questionMark;
    public float detectionDistance = 5;
    public float detectionSpeedBooster = 1.25f;
    [HideInInspector] public bool chasePlayer;
    private Vector3 _playerPos;

    #endregion

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _audioManager = FindObjectOfType<AudioManager>();
        _animator = GetComponent<Animator>();

        StartCoroutine(CasualReset());

        // fill the _direction vector with all four direction
        _directions.Add(Vector2.up);
        _directions.Add(Vector2.right);
        _directions.Add(Vector2.down);
        _directions.Add(Vector2.left);
        _movement = _directions[Random.Range(0, 4)];
    }

    private void Update()
    {
        if (_monsterDied)
        {
            _movement = Vector2.zero;
            return;
        }

        if (chasePlayer) return;

        _time -= Time.deltaTime;
        if (!(_time <= 0)) return;
        _time = 1;
        ChooseDirection();
        NudgeMon();
        PlayWalkingAnim();
    }

    // this func chooses a random direction every 2 seconds with 33% of keeping the same direction
    private void ChooseDirection()
    {
        if (Random.Range(1, 4) == 3)
            _movement = _directions[Random.Range(0, 4)];
        for (var i = 0; i < 4; i++)
        {
            if (FreeDirection()) return;
            _movement = _directions[Random.Range(0, 4)];
        }
    }

    // this func plays the walking animation according to the _movement direction
    private void PlayWalkingAnim()
    {
        if (_movement == Vector2.up || _movement == Vector2.right)
            _animator.Play("Balloon Walking Up and Right");
        if (_movement == Vector2.down || _movement == Vector2.left)
            _animator.Play("Balloon Walking Down and Left");
    }

    // this func check if a direction is free for a monster. a free direction is a direction which is not 
    // blocked on the first step
    private bool FreeDirection()
    {
        _temp = transform.position;
        _temp.x += _movement.x;
        _temp.y += _movement.y;
        _intersecting = Physics2D.OverlapCircleAll(_temp, 0.1f);
        if (_intersecting.Length > 0)
            if (!_intersecting[0].gameObject.CompareTag("Player"))
                return false;
        return true;
    }

    // this func moves the monster
    private void FixedUpdate()
    {
        var monPrevPos = transform.position;
        rb.MovePosition(rb.position + (_movement * moveSpeed * Time.fixedDeltaTime));

        // this piece of code sets detection on and off as well as applying the speed booster
        if (!chasePlayer)
        {
            DetectPlayer();
            if (chasePlayer) PlayerDetected();
        }
        else
        {
            ChasePlayer();
            if (!chasePlayer) PlayerUnDetected();
        }
    }

    // this func nudges the monster over sharp corners
    private void NudgeMon()
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

    // this func calculates the nudge (monster distance from the center of a block value between (-1,1)
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

    // nudges the monster horizontally according to the nudge and movement direction
    private void NudgeHorizontally(double nudge)
    {
        if (-0.9 < nudge && nudge < -0.1)
            _movement = Vector2.left;
        if (0.1 < nudge && nudge < 0.9)
            _movement = Vector2.right;
    }

    // nudges the monster vertically according to the nudge and movement direction
    private void NudgeVertically(double nudge)
    {
        if (-0.9 < nudge && nudge < -0.1)
            _movement = Vector2.down;
        if (0.1 < nudge && nudge < 0.9)
            _movement = Vector2.up;
    }

    // this func makes sure the coin wont collide with objects with the tags [monster,player]
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Monster") || other.gameObject.CompareTag("Player"))
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other.gameObject.GetComponent<Collider2D>());
    }

    // if player is hiding behind a bomb the coin will try to go the other way
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bomb"))
            _movement *= (-1);
    }

    // kills a coin that touched an explosion
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Explosion"))
            MonsterDied();
    }

    // kills a monster and adds 100 points to the score
    private void MonsterDied()
    {
        if (_monsterDied) return;
        _monsterDied = true;
        _gameManager.Score += 100;
        _animator.Play("Balloon Death Anim");
        ShowPoints();
        PlayerUnDetected();
        Destroy(gameObject, 1.5f);
    }

    // this func shows the points the player got on the screen
    private void ShowPoints()
    {
        var points = Instantiate(pointsText, transform);
        var posPoints = transform.position;
        posPoints.x += 0.3f;
        posPoints.y += 0.3f;
        points.transform.position = posPoints;
    }

    // this func detect a player within the detection distance set at the inspector
    // it return true if a player was detected and sets the variable _playerPos to the 
    // nearest aligned grid to the player detection pos. else returns false.
    private bool DetectPlayer()
    {
        var monPos = transform.position;
        var endPos = monPos;
        endPos.x += _movement.x * detectionDistance;
        endPos.y += _movement.y * detectionDistance;
        var hit = Physics2D.Linecast(monPos, endPos, 1 << LayerMask.NameToLayer("Player"));
        Debug.DrawLine(monPos, endPos);
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
        {
            chasePlayer = true;
            _playerPos = GameObject.FindWithTag("Player").transform.position;
            _playerPos.x = Mathf.Round(_playerPos.x);
            _playerPos.y = Mathf.Round(_playerPos.y);
            return true;
        }

        if (hit.collider != null && hit.collider.gameObject.CompareTag("Bomb"))
        {
            chasePlayer = false;
            return false;
        }

        return false;
    }

    // a set of action the play when player is being detected
    private void PlayerDetected()
    {
        moveSpeed *= detectionSpeedBooster;
        _audioManager.PlayerBeingChased();
        _audioManager.PlaySound("Detected");
        exclamationMark.SetActive(true);
    }

    // a set of action the play when player is being undetected
    public void PlayerUnDetected()
    {
        _audioManager.PlayerBeingChased();
        moveSpeed /= detectionSpeedBooster;
        exclamationMark.SetActive(false);
    }

    private IEnumerator PlayerDisappeared()
    {
        if (!(moveSpeed > 2) || !chasePlayer) yield break;
        chasePlayer = false;
        PlayerUnDetected();
        questionMark.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        questionMark.SetActive(false);
        yield return null;
    }

    public void ActivatePlayerDisappeared()
    {
        StartCoroutine(PlayerDisappeared());
    }


    // this func chases the player when detected, it stops the random movement of the enemy.
    // once the enemy doesnt see the player any longer it gets to the last seen position and
    // scans all four directions
    private void ChasePlayer()
    {
        if (DetectPlayer()) return;
        print(_playerPos);
        if (Vector2.Distance(transform.position, _playerPos) < 0.1f)
        {
            foreach (var direction in _directions)
            {
                _movement = direction;
                if (DetectPlayer()) return;
            }

            chasePlayer = false;
        }
    }

    // this coroutine resets enemy chase mechanic every 5 seconds
    // it was made in order to make the game a bit less difficult.
    private IEnumerator CasualReset()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (!(moveSpeed > 2) || !chasePlayer) continue;
            chasePlayer = false;
            PlayerUnDetected();
        }
        // ReSharper disable once IteratorNeverReturns (the while condition is always true).
    }

    // updates the game manager that the monster died
    private void OnDestroy()
    {
        _gameManager.KillMonster();
    }
}