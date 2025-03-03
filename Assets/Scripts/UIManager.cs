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
}
