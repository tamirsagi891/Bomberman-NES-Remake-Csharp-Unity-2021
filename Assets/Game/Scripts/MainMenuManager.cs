using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private bool _arrowState = true;
    public GameObject mainMenu;
    public GameObject rules1;
    public GameObject rules2;
    private int _pageSwitcher = 0;

    // the purpose of this code is to move the option indicator on the main menu screen and transition 
    // to the 1st level
    private void Update()
    {
        var pos = transform.position;

        if (Input.GetKeyDown(KeyCode.RightArrow) && _pageSwitcher == 0)
        {
            _arrowState = false;
            pos.x = 0.2f;
            transform.position = pos;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && _pageSwitcher == 0)
        {
            _arrowState = true;
            pos.x = -3.8f;
            transform.position = pos;
        }

        if (!Input.GetKeyDown("space")) return;
        if (_arrowState)
            SceneManager.LoadScene("Transition", LoadSceneMode.Single);
        else
        {
            _pageSwitcher++;
            DisableAllMenuScreens();
            switch (_pageSwitcher)
            {
                case 1:
                    rules1.SetActive(true);
                    break;
                case 2:
                    rules2.SetActive(true);
                    break;
                case 3:
                    mainMenu.SetActive(true);
                    _pageSwitcher = 0;
                    break;
            }
        }
    }

    // this func disables all menu page screens.
    private void DisableAllMenuScreens()
    {
        mainMenu.SetActive(false);
        rules1.SetActive(false);
        rules2.SetActive(false);
    }
}