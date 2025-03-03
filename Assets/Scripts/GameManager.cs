using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject cardPrefab;
    public Transform cardContainer;
    public CardData cardData; // ScriptableObject that holds the card sprites

    private Queue<Card> flipQueue = new Queue<Card>(); // Stores flipped cards  
    private bool isChecking = false; // Tracks if a match check is in progress

    private int _totalMatches;
    private int _currentMatches = 0;
    private int _turnsTaken = 0;

    private int rows;
    private int columns;

    // Public properties for UI updating
    public int CurrentMatches
    {
        get { return _currentMatches; }
        set
        {
            _currentMatches = value;
            UIManager.Instance.UpdateMatchText(_currentMatches, TotalMatches);
        }
    }

    public int TotalMatches
    {
        get { return _totalMatches; }
        set
        {
            _totalMatches = value;
            UIManager.Instance.UpdateMatchText(CurrentMatches, _totalMatches);
        }
    }

    public int TurnsTaken
    {
        get { return _turnsTaken; }
        set
        {
            _turnsTaken = value;
            UIManager.Instance.UpdateTurnText(_turnsTaken);
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    /// <summary>
    /// Set the basic game parameters.
    /// </summary>
    public void SetGameParameters(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        TotalMatches = (rows * columns) / 2;
        // Reset counters when starting a new game
        _currentMatches = 0;
        _turnsTaken = 0;
        UIManager.Instance.UpdateMatchText(_currentMatches, _totalMatches);
        UIManager.Instance.UpdateTurnText(_turnsTaken);
    }

    /// <summary>
    /// Generates the cards. If savedCardStates is provided, it uses that state,
    /// otherwise it creates a new shuffled set.
    /// </summary>
    public void GenerateCards(int rows, int cols, List<CardState> savedCardStates = null)
    {
        // Clear any existing cards
        cardContainer.DestroyChildren();
        List<int> cardIDs = new List<int>();

        if (savedCardStates == null)
        {
            // Create new card IDs for a new game
            for (int i = 0; i < (rows * cols) / 2; i++)
            {
                cardIDs.Add(i);
                cardIDs.Add(i);
            }
            Shuffle(cardIDs);
        }
        else
        {
            // When loading, use the saved card states (which preserves order)
            foreach (CardState cs in savedCardStates)
            {
                cardIDs.Add(cs.cardID);
            }
        }

        // Create and position cards on screen
        CreateAndPlaceCards(cardIDs, rows, cols, savedCardStates);
    }

    /// <summary>
    /// Handles the instantiation, scaling, and positioning of cards.
    /// If savedCardStates is provided, each card's state is restored.
    /// </summary>
    private void CreateAndPlaceCards(List<int> cardIDs, int rows, int cols, List<CardState> savedCardStates = null)
    {
        // Space taken by UI panels (example values)
        float leftPanelWidth = 200f; // in pixels
        float topPanelHeight = 70f;  // in pixels

        // Convert panel dimensions from pixels to world units using Camera settings
        float leftPanelWidthWorld = leftPanelWidth / Camera.main.pixelWidth * Camera.main.orthographicSize * Camera.main.aspect * 2;
        float topPanelHeightWorld = topPanelHeight / Camera.main.pixelHeight * Camera.main.orthographicSize * 2;

        float cameraHeight = 2f * Camera.main.orthographicSize - topPanelHeightWorld;
        float cameraWidth = 2f * Camera.main.orthographicSize * Camera.main.aspect - leftPanelWidthWorld;

        // Get the native sprite size from the card prefab
        SpriteRenderer spriteRenderer = cardPrefab.GetComponent<SpriteRenderer>();
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        // Define a minimum padding between cards (in world units)
        float minPadding = 0.1f;

        // Calculate scaling factor so that cards (plus padding) fit within available space
        float scaleFactorWidth = (cameraWidth - (cols + 1) * minPadding) / (cols * spriteSize.x);
        float scaleFactorHeight = (cameraHeight - (rows + 1) * minPadding) / (rows * spriteSize.y);
        float scaleFactor = Mathf.Min(scaleFactorWidth, scaleFactorHeight);

        float cardWidth = spriteSize.x * scaleFactor;
        float cardHeight = spriteSize.y * scaleFactor;
        float cardSpacingX = (cameraWidth - (cardWidth * cols)) / (cols + 1);
        float cardSpacingY = (cameraHeight - (cardHeight * rows)) / (rows + 1);

        Vector3 startPos = new Vector3(
            -Camera.main.orthographicSize * Camera.main.aspect + leftPanelWidthWorld + cardSpacingX + cardWidth / 2,
            Camera.main.orthographicSize - topPanelHeightWorld - cardSpacingY - cardHeight / 2,
            0);

        for (int i = 0; i < cardIDs.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;

            Vector3 cardPosition = startPos + new Vector3(col * (cardWidth + cardSpacingX), -row * (cardHeight + cardSpacingY), 0);
            GameObject newCard = Instantiate(cardPrefab, cardPosition, Quaternion.identity, cardContainer);
            newCard.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

            Card cardScript = newCard.GetComponent<Card>();
            // Set the card sprite and id from CardData
            cardScript.SetCard(cardIDs[i], cardData.cardSprites[cardIDs[i]]);

            // If we're loading a saved state, restore each card's specific state
            if (savedCardStates != null)
            {
                CardState state = savedCardStates[i];
                if (state.isMatched)
                {
                    cardScript.FlipCard();
                    cardScript.SetMatched();
                }
            }
        }
    }

    /// <summary>
    /// Called by individual cards when they are flipped.
    /// </summary>
    public void CardFlipped(Card card)
    {
        if (flipQueue.Contains(card)) return; // Prevent flipping the same card twice

        flipQueue.Enqueue(card);

        if (flipQueue.Count >= 2 && !isChecking)
        {
            _turnsTaken++;
            UIManager.Instance.UpdateTurnText(_turnsTaken);
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        isChecking = true;
        yield return new WaitForSeconds(0.5f);

        while (flipQueue.Count >= 2)
        {
            Card card1 = flipQueue.Dequeue();
            Card card2 = flipQueue.Dequeue();

            if (card1.cardID == card2.cardID)
            {
                card1.SetMatched();
                card2.SetMatched();
                _currentMatches++;
                UIManager.Instance.UpdateMatchText(_currentMatches, _totalMatches);

                if (_currentMatches == _totalMatches)
                {
                    UIManager.Instance.ShowVictory();
                }
            }
            else
            {
                card1.FlipCard();
                card2.FlipCard();
            }
        }
        isChecking = false;
    }

    /// <summary>
    /// Shuffles a list of integers using the Fisher–Yates algorithm.
    /// </summary>
    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    /// <summary>
    /// Saves the game state, including parameters and each card's state.
    /// </summary>
    public void SaveGameState()
    {
        PlayerPrefs.SetInt("Rows", rows);
        PlayerPrefs.SetInt("Columns", columns);
        PlayerPrefs.SetInt("CurrentMatches", _currentMatches);
        PlayerPrefs.SetInt("TotalMatches", _totalMatches);
        PlayerPrefs.SetInt("TurnsTaken", _turnsTaken);

        int cardCount = cardContainer.childCount;
        PlayerPrefs.SetInt("CardCount", cardCount);
        for (int i = 0; i < cardCount; i++)
        {
            Card card = cardContainer.GetChild(i).GetComponent<Card>();
            if (card != null)
            {
                PlayerPrefs.SetInt("CardID_" + i, card.cardID);
                PlayerPrefs.SetInt("CardMatched_" + i, card.IsMatched ? 1 : 0);
            }
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the game state, reconstructing the card order and each card's state.
    /// </summary>
    public void LoadGameState()
    {
        if (PlayerPrefs.HasKey("Rows"))
        {
            int savedRows = PlayerPrefs.GetInt("Rows");
            int savedColumns = PlayerPrefs.GetInt("Columns");
            SetGameParameters(savedRows, savedColumns);

            // Build saved card states from PlayerPrefs
            int cardCount = PlayerPrefs.GetInt("CardCount");
            List<CardState> savedStates = new List<CardState>();
            for (int i = 0; i < cardCount; i++)
            {
                int id = PlayerPrefs.GetInt("CardID_" + i);
                bool matched = PlayerPrefs.GetInt("CardMatched_" + i) == 1;
                savedStates.Add(new CardState(id, matched));
            }

            GenerateCards(savedRows, savedColumns, savedStates);

            _currentMatches = PlayerPrefs.GetInt("CurrentMatches");
            _totalMatches = PlayerPrefs.GetInt("TotalMatches");
            _turnsTaken = PlayerPrefs.GetInt("TurnsTaken");

            UIManager.Instance.UpdateMatchText(_currentMatches, _totalMatches);
            UIManager.Instance.UpdateTurnText(_turnsTaken);
            MenuManager.Instance.HideMainMenu();

        }
        else
        {
            Debug.Log("No saved game to load.");
            MenuManager.Instance.DisableLoadButton();

        }
    }
}
