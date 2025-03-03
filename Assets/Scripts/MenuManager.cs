using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    //public GameObject gameUIPanel;

    public void StartGame(int rows, int columns)
    {
        // Set game parameters based on the selected difficulty
        GameManager.Instance.SetGameParameters(rows, columns);

        // Hide Main Menu and Show Game UI
        mainMenuPanel.SetActive(false);
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
}
