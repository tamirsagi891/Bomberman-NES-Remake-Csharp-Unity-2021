using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    private GameManager _gameManager;

    public Rigidbody2D rb;

    #region Script Variables

    private Collider2D[] _intersecting;
    private readonly List<Vector2> _directions = new List<Vector2>();
    private Animator _animator;
    private Vector2 _movement;
    private Vector3 _temp; // a variable to hold a vector temporarily
    public float moveSpeed = 3.5f;
    private bool _monsterDied = false;
    private float _time = 0;

    #endregion

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _animator = GetComponent<Animator>();

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

        _time -= Time.deltaTime;
        if (!(_time <= 0)) return;
        _time = 2f;
        ChooseDirection();
    }

    // this func chooses a random direction every 2 seconds with 33% of keeping the same direction
    private void ChooseDirection()
    {
        if (Random.Range(1, 4) > 2) return;
        _movement = _directions[Random.Range(0, 4)];
        for (var i = 0; i < 4; i++)
        {
            if (FreeDirection()) return;
            _movement = _directions[Random.Range(0, 4)];
        }
    }

    // this func check if a direction is free for a monster. a free direction is a direction which is not 
    // blocked on the first step
    private bool FreeDirection()
    {
        _temp = transform.position;
        _temp.x += _movement.x;
        _temp.y += _movement.y;
        _intersecting = Physics2D.OverlapCircleAll(_temp, 0.1f);
        if (_intersecting.Length <= 0) return true;
        return _intersecting[0].gameObject.CompareTag("Player") || _intersecting[0].gameObject.CompareTag("Wall");
    }

    // this func moves the coin
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + (_movement * moveSpeed * Time.fixedDeltaTime));
    }

    // this func makes sure the coin wont collide with objects with the tags [monster,player,wall]
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Monster") || other.gameObject.CompareTag("Player") ||
            other.gameObject.CompareTag("Wall"))
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

    // kills a coin and adds 8000 points to the score
    private void MonsterDied()
    {
        if (_monsterDied) return;
        _monsterDied = true;
        _gameManager.Score += 8000;
        _animator.Play("Coin Death Anim");
        Destroy(gameObject, 1.6f);
    }

    // updates the game manager that the monster died
    private void OnDestroy()
    {
        _gameManager.KillMonster();
    }
}