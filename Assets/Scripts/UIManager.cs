using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TMP_Text matchText;
    public TMP_Text turnText;
    public TMP_Text victoryText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        victoryText.gameObject.SetActive(false);
    }

    public void UpdateMatchText(int currentMatches, int totalMatches)
    {
        matchText.text = "Matches: " + currentMatches + "/" + totalMatches;
    }

    public void UpdateTurnText(int turnsTaken)
    {
        turnText.text = "Turns: " + turnsTaken;
    }

    public void ShowVictory()
    {
        victoryText.gameObject.SetActive(true);
        victoryText.text = "Victory! You Won!";
    }

    /// <summary>
    /// Called when the back button is pressed.
    /// Hides in-game UI elements and activates the main menu panel.
    /// </summary>
    public void BackToMainMenu()
    {
        // Hide any in-game UI elements as needed (e.g., victory text)
        victoryText.gameObject.SetActive(false);
        // Activate the main menu panel using MenuManager.
        MenuManager.Instance.ShowMainMenu();
    }
}
