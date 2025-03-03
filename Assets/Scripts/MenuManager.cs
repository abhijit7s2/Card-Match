using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    public GameObject mainMenuPanel;
    //public GameObject gameUIPanel;
    public Button loadButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public void StartGame(int rows, int columns)
    {
        // Set game parameters based on the selected difficulty
        GameManager.Instance.SetGameParameters(rows, columns);

        // Hide Main Menu and Show Game UI
        HideMainMenu();
        //gameUIPanel.SetActive(true);

        // Initialize or reset the game setup
        GameManager.Instance.GenerateCards(rows, columns);
    }
    public void StartGameEasy()
    {
        StartGame(2, 2);  // Easy settings
    }

    public void StartGameMedium()
    {
        StartGame(3, 4);  // Medium settings
    }

    public void StartGameHard()
    {
        StartGame(5, 6);  // Hard settings
    }
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
       // gameUIPanel.SetActive(false);
    }
    public void HideMainMenu()
    {
        mainMenuPanel.SetActive(false);
    }

    public void DisableLoadButton()
    {
        if (loadButton != null)
            loadButton.interactable = false;
    }
}
