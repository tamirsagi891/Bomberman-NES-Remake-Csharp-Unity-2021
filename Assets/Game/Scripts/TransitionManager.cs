using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// this scrip transition between menus and level based on data from the game manager
// it plays the corresponding sound and displays the text
public class TransitionManager : MonoBehaviour
{
    private GameManager _gameManager;
    private AudioManager _audioManager;
    public GameObject msg;
    private TextMeshProUGUI _msgText;
    private float _time = 4.0f;

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _audioManager = FindObjectOfType<AudioManager>();
        _msgText = msg.GetComponent<TextMeshProUGUI>();

        _audioManager.PauseSound("Chase");
        
        if (_gameManager.StageNumber > 50)
            _msgText.SetText("GAME WON!");
        else if (_gameManager.PlayerData.lifePoints > 0)
        {
            _msgText.SetText("STAGE     " + _gameManager.StageNumber);
            _audioManager.PlaySound("StageStart");
        }
        else
        {
            _msgText.SetText("GAME OVER");
            _audioManager.PlaySound("GameOver");
            _time = 8.0f;
        }
    }

    private void Update()
    {
        _time -= Time.deltaTime;
        if (!(_time < 0)) return;
        if (_gameManager.StageNumber > 50)
            Destroy(_gameManager.gameObject);
        else if (_gameManager.PlayerData.lifePoints > 0)
        {
            _audioManager.PlaySound("StageTheme");
            SceneManager.LoadScene("Basic Level", LoadSceneMode.Single);
        }
        else
            Destroy(_gameManager.gameObject);
    }
}