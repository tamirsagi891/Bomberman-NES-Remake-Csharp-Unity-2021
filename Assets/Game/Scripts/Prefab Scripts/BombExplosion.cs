using UnityEngine;

public class BombExplosion : MonoBehaviour
{
    private GameManager _gameManager;
    private AudioManager _audioManager;

    #region Scrips Prefabs
    public GameObject explosion;
    #endregion

    #region Script Variables
    private Collider2D[] _intersecting;
    private GameObject _explosion;
    private int _expRadios;
    private Vector3 _position;
    private Vector3 _temp; // a variable to hold a vector temporarily
    private float _time = 3;
    #endregion

    // sets the managers and bomb data
    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _audioManager = FindObjectOfType<AudioManager>();
        _position = transform.position;
        _gameManager.BombsOnScreen += 1;
        _expRadios = _gameManager.PlayerData.bombRadius;
    }

    private void Update()
    {
        _time -= Time.deltaTime;
        if (_time < 0)
        {
            _explosion = Instantiate(explosion);
            _audioManager.PlaySound("BombExplode");
            _gameManager.BombsOnScreen -= 1;
            _explosion.transform.position = _position;
            Destroy(_explosion, 1);

            ExplosionHorizontal(-1);
            ExplosionHorizontal(1);
            ExplosionVertical(-1);
            ExplosionVertical(1);
            gameObject.SetActive(false);
            _time = 1000; // just to make sure it wont happen again
        }
    }

    // ignites the bomb immediately 
    private void IgniteBomb()
    {
        _time = -1;
    }

    // after the player moved from the bomb it turns the trigger off
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
    }

    // this func check for horizontal explosion path using the bomb radius
    // if there is no wall/block it triggers the explosion
    private void ExplosionHorizontal(int direction)
    {
        for (var i = 1; i <= _expRadios; i++)
        {
            _temp = _position;
            _temp.y += i * direction;
            if (!IntersectionWithExplosion())
                return;
            InstantiateExplosion(i, direction, "Exp Vertical", "Exp Up", "Exp Down");
        }
    }

    // this func check for vertical explosion path using the bomb radius
    // if there is no wall/block it triggers the explosion
    private void ExplosionVertical(int direction)
    {
        for (var i = 1; i <= _expRadios; i++)
        {
            _temp = _position;
            _temp.x += i * direction;
            if (!IntersectionWithExplosion())
                return;
            InstantiateExplosion(i, direction, "Exp Horizontal", "Exp Right", "Exp Left");
        }
    }

    // this func gets a Vector3 position on the level grid and checks if a bomb can explode there
    // if there is a door/block the bomb explosion cant get there. if there is a wall it destroys it
    // if there is a bomb it ignites that bomb
    private bool IntersectionWithExplosion()
    {
        _intersecting = Physics2D.OverlapCircleAll(_temp, 0.2f);
        if (_intersecting.Length <= 0) return true;
        if (_intersecting[0].gameObject.CompareTag("Bomb"))
        {
            _intersecting[0].gameObject.GetComponent<BombExplosion>().IgniteBomb();
            return false;
        }

        if (_intersecting[0].gameObject.CompareTag("Block") || _intersecting[0].gameObject.CompareTag("Door"))
            return false;
        if (_intersecting[0].gameObject.CompareTag("Wall"))
        {
            _intersecting[0].gameObject.GetComponent<WallScript>().Explode();
            return false;
        }

        return true;
    }

    // spawns the explosion prefab and plays the corresponding animation 
    private void InstantiateExplosion(int index, int direction, string anim1, string anim2, string anim3)
    {
        _explosion = Instantiate(explosion, _temp, Quaternion.identity);
        _explosion.GetComponent<Animator>().Play(anim1);
        if ((index == _expRadios) && (direction == 1))
            _explosion.GetComponent<Animator>().Play(anim2);
        if ((index == _expRadios) && (direction == -1))
            _explosion.GetComponent<Animator>().Play(anim3);
        Destroy(_explosion, 0.9f);
    }
}