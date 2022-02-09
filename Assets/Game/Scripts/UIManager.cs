using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// this scrip lets us update the UI text boxes on the game level 
public class UIManager : MonoBehaviour
{
    private GameManager _gameManager;
    private LevelManager _levelManager;
    public GameObject time;
    private TextMeshProUGUI _timeText;
    public GameObject score;
    private TextMeshProUGUI _scoreText;
    public GameObject lifePoint;
    private TextMeshProUGUI _lifePointText;

    private bool _playerTransparent = false;
    public Slider transparencyBar;
    private float _timeRemaining = 1;
    private float _timeMax = 1;

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _levelManager = FindObjectOfType<LevelManager>();
        _timeText = time.GetComponent<TextMeshProUGUI>();
        _scoreText = score.GetComponent<TextMeshProUGUI>();
        _lifePointText = lifePoint.GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        var lifePointString = (_gameManager.PlayerData.lifePoints - 1).ToString();
        _lifePointText.SetText("LEFT  " + lifePointString);
        
        transparencyBar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.green;
    }

    private void Update()
    {
        
        var timeString = _levelManager.time.ToString("0");
        if (_levelManager.time <= 9.5f)
            timeString = "0" + timeString;
        _timeText.SetText("TIME  " + timeString);

        var scoreString = _gameManager.Score.ToString();
        if (_gameManager.Score == 0)
            scoreString = "00";
        _scoreText.SetText(scoreString);
        

        if (Input.GetKeyDown(KeyCode.LeftShift) && !_playerTransparent)
            StartCoroutine(DisappearActivated());

        if (!_playerTransparent)
            transparencyBar.value = 1;
        else
            transparencyBar.value = _timeRemaining / _timeMax;

        if (_timeRemaining <= 0)
            _timeRemaining = 0;
        else
            _timeRemaining -= Time.deltaTime;

    }

    // this coroutine activates the transparency cooldown bar
    private IEnumerator DisappearActivated()
    {
        _playerTransparent = true;
        
        _timeMax = _gameManager.transparencyDuration;
        _timeRemaining = _gameManager.transparencyDuration;
        yield return new WaitForSeconds(_gameManager.transparencyDuration);
        
        transparencyBar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.red;
        _timeMax = _gameManager.transparencyCooldown;
        _timeRemaining = _gameManager.transparencyCooldown;
        yield return new WaitForSeconds(_gameManager.transparencyCooldown);

        _playerTransparent = false;
        transparencyBar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.green;
        yield return null;
    }
    
}