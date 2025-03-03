using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Only if you want it to persist across scenes.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        // Example save data
        PlayerPrefs.SetInt("CurrentMatches", GameManager.Instance.CurrentMatches);
        PlayerPrefs.SetInt("TotalMatches", GameManager.Instance.TotalMatches);
        PlayerPrefs.SetInt("TurnsTaken", GameManager.Instance.TurnsTaken);
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("CurrentMatches"))
        {
            GameManager.Instance.CurrentMatches = PlayerPrefs.GetInt("CurrentMatches");
            GameManager.Instance.TotalMatches = PlayerPrefs.GetInt("TotalMatches");
            GameManager.Instance.TurnsTaken = PlayerPrefs.GetInt("TurnsTaken");
            // Continue loading other game states
        }
        else
        {
            Debug.Log("No save game found.");
        }
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
