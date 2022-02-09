using UnityEngine;

public class PlayerPowerupCollision : MonoBehaviour
{
    private PlayerManager _playerManager;
    private GameManager _gameManager;
    private AudioManager _audioManager;
    private string _powerUpName;

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _audioManager = FindObjectOfType<AudioManager>();
        _playerManager = GetComponent<PlayerManager>();
    }

    // this func call the power up func of the PlayerManager script based on the power
    // up name. it does so when player triggered a power up tagged object
    // it add 1000 point to the score and destroys the object
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Powerup")) return;
        _powerUpName = other.gameObject.GetComponent<PowerUpScript>().GetName();

        switch (_powerUpName)
        {
            case "Bomb Up":
                print("Bomb Up");
                _playerManager.BombUp();
                break;

            case "Speed Up":
                print("Speed Up");
                _playerManager.SpeedUp();
                break;

            case "Fire Up":
                print("Fire Up");
                _playerManager.FireUp();
                break;
        }

        _gameManager.Score += 1000;
        _audioManager.PlaySound("PowerUp");
        Destroy(other.gameObject);
    }
}