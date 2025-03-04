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
    private int _score = 5000;

    [SerializeField] private int baseScore = 5000;
    [SerializeField] private int turnPenalty = 50;
    public RectTransform rightPanelRect;


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

    public int Score
    {
        get { return _score; }
        set
        {
            _score = value;
            UIManager.Instance.UpdateScoreText(_score);
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

    public void SetGameParameters(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        _currentMatches = 0;
        _turnsTaken = 0;
        TotalMatches = (rows * columns) / 2;

        UIManager.Instance.UpdateMatchText(_currentMatches, _totalMatches);
        UIManager.Instance.UpdateTurnText(_turnsTaken);
        UIManager.Instance.UpdateScoreText(Score);

    }

    public void GenerateCards(int rows, int cols, List<CardState> savedCardStates = null)
    {
        cardContainer.DestroyChildren();
        List<int> cardIDs = new List<int>();

        if (savedCardStates == null)
        {
            _currentMatches = 0;
            _turnsTaken = 0;
            UIManager.Instance.UpdateMatchText(_currentMatches, _totalMatches);
            UIManager.Instance.UpdateTurnText(_turnsTaken);

            for (int i = 0; i < (rows * cols) / 2; i++)
            {
                cardIDs.Add(i);
                cardIDs.Add(i);
            }
            Shuffle(cardIDs);
        }
        else
        {
            foreach (CardState cs in savedCardStates)
            {
                cardIDs.Add(cs.cardID);
            }
        }

        CreateAndPlaceCards(cardIDs, rows, cols, savedCardStates);
    }

    private void CreateAndPlaceCards(List<int> cardIDs, int rows, int cols, List<CardState> savedCardStates = null)
    {
        // Get the right panel's width in canvas (screen) pixels
        float panelPixelWidth = rightPanelRect.rect.width;

        // Convert that pixel width to world units. 
        // This conversion is based on the camera's orthographic size and aspect ratio.
        float panelWidthWorld = panelPixelWidth / Screen.width * (2f * Camera.main.orthographicSize * Camera.main.aspect);

        float topPanelHeight = 70f; // in pixels (for top UI, similar conversion applies)
        float topPanelHeightWorld = topPanelHeight / Screen.height * (2f * Camera.main.orthographicSize);

        // Now compute available game width using the panel's world unit width.
        float cameraWidth = 2f * Camera.main.orthographicSize * Camera.main.aspect - panelWidthWorld;
        float cameraHeight = 2f * Camera.main.orthographicSize - topPanelHeightWorld;

        // Get the native sprite size from the card prefab.
        SpriteRenderer spriteRenderer = cardPrefab.GetComponent<SpriteRenderer>();
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        float minPadding = 0.1f;
        float scaleFactorWidth = (cameraWidth - (cols + 1) * minPadding) / (cols * spriteSize.x);
        float scaleFactorHeight = (cameraHeight - (rows + 1) * minPadding) / (rows * spriteSize.y);
        float scaleFactor = Mathf.Min(scaleFactorWidth, scaleFactorHeight);

        float cardWidth = spriteSize.x * scaleFactor;
        float cardHeight = spriteSize.y * scaleFactor;
        float cardSpacingX = (cameraWidth - (cardWidth * cols)) / (cols + 1);
        float cardSpacingY = (cameraHeight - (cardHeight * rows)) / (rows + 1);

        // Compute the starting position for the card grid.
        Vector3 startPos = new Vector3(
            -Camera.main.orthographicSize * Camera.main.aspect + panelWidthWorld + cardSpacingX + cardWidth / 2,
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
            cardScript.SetCard(cardIDs[i], cardData.cardSprites[cardIDs[i]]);

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

    public void CardFlipped(Card card)
    {
        if (flipQueue.Contains(card)) return;

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
                    AudioManager.Instance.PlayGameOverSound();
                    break;
                }
                AudioManager.Instance.PlayMatchSound();

            }
            else
            {
                AudioManager.Instance.PlayMismatchSound();

                card1.FlipCard();
                card2.FlipCard();
            }
        }
        CalculateScore();
        isChecking = false;
    }

    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    private void CalculateScore()
    {
        Score = Mathf.Max(0, baseScore - (_turnsTaken * turnPenalty));
    }

    public void SaveGameState()
    {
        PlayerPrefs.SetInt("Rows", rows);
        PlayerPrefs.SetInt("Columns", columns);
        PlayerPrefs.SetInt("CurrentMatches", _currentMatches);
        PlayerPrefs.SetInt("TotalMatches", _totalMatches);
        PlayerPrefs.SetInt("TurnsTaken", _turnsTaken);
        PlayerPrefs.SetInt("Score", Score);


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

    public void LoadGameState()
    {
        if (PlayerPrefs.HasKey("Rows"))
        {
            int savedRows = PlayerPrefs.GetInt("Rows");
            int savedColumns = PlayerPrefs.GetInt("Columns");
            SetGameParameters(savedRows, savedColumns);

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
            Score = PlayerPrefs.GetInt("Score");


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
