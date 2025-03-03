using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject cardPrefab;
    public Transform cardContainer;
    public CardData cardData;

    private Queue<Card> flipQueue = new Queue<Card>(); // Stores flipped cards  
    private int totalMatches;
    private int currentMatches = 0;
    private int turnsTaken = 0;
    private bool isChecking = false; // Tracks if a match check is in progress

    private int rows;
    private int columns;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        //GenerateCards(5, 6);
        //totalMatches = (5 * 6) / 2;
        //UIManager.Instance.UpdateMatchText(currentMatches, totalMatches);
        //UIManager.Instance.UpdateTurnText(turnsTaken);
    }

    public void GenerateCards(int rows, int cols)
    {
        List<int> cardIDs = new List<int>();

        for (int i = 0; i < (rows * cols) / 2; i++)
        {
            cardIDs.Add(i);
            cardIDs.Add(i);
        }

        Shuffle(cardIDs);

        // Space taken by UI panels
        float leftPanelWidth = 200; // Example width in pixels for left panel
        float topPanelHeight = 70; // Example height in pixels for top panel

        // Convert panel dimensions from pixels to world units
        float leftPanelWidthWorld = leftPanelWidth / Camera.main.pixelWidth * Camera.main.orthographicSize * Camera.main.aspect * 2;
        float topPanelHeightWorld = topPanelHeight / Camera.main.pixelHeight * Camera.main.orthographicSize * 2;

        float cameraHeight = 2f * Camera.main.orthographicSize - topPanelHeightWorld;
        float cameraWidth = 2f * Camera.main.orthographicSize * Camera.main.aspect - leftPanelWidthWorld;

        // Retrieve the native sprite size
        SpriteRenderer spriteRenderer = cardPrefab.GetComponent<SpriteRenderer>();
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        // Define minimum padding
        float minPadding = 0.1f; // World units

        // Calculate the scaling factor that allows the card to fit the panel correctly with padding
        float scaleFactorWidth = (cameraWidth - (cols + 1) * minPadding) / (cols * spriteSize.x);
        float scaleFactorHeight = (cameraHeight - (rows + 1) * minPadding) / (rows * spriteSize.y);
        float scaleFactor = Mathf.Min(scaleFactorWidth, scaleFactorHeight);

        float cardWidth = spriteSize.x * scaleFactor;
        float cardHeight = spriteSize.y * scaleFactor;
        float cardSpacingX = (cameraWidth - (cardWidth * cols)) / (cols + 1);
        float cardSpacingY = (cameraHeight - (cardHeight * rows)) / (rows + 1);

        Vector3 startPos = new Vector3((-Camera.main.orthographicSize * Camera.main.aspect + leftPanelWidthWorld + cardSpacingX + cardWidth / 2),
                                       (Camera.main.orthographicSize - topPanelHeightWorld - cardSpacingY - cardHeight / 2), 0);

        for (int i = 0; i < cardIDs.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;

            Vector3 cardPosition = startPos + new Vector3(col * (cardWidth + cardSpacingX), -row * (cardHeight + cardSpacingY), 0);
            GameObject newCard = Instantiate(cardPrefab, cardPosition, Quaternion.identity, cardContainer);
            newCard.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1); // Apply uniform scaling

            Card cardScript = newCard.GetComponent<Card>();
            cardScript.SetCard(cardIDs[i], cardData.cardSprites[cardIDs[i]]);
        }
    }

    public void SetGameParameters(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        totalMatches = (rows * columns) / 2;
        UIManager.Instance.UpdateMatchText(currentMatches, totalMatches);
        UIManager.Instance.UpdateTurnText(turnsTaken);
    }
    public void CardFlipped(Card card)
    {
        if (flipQueue.Contains(card)) return; // Prevent flipping the same card twice

        flipQueue.Enqueue(card); // Add the flipped card to the queue

        if (flipQueue.Count >= 2 && !isChecking) // If there are at least 2 cards and no ongoing check
        {
            turnsTaken++;
            UIManager.Instance.UpdateTurnText(turnsTaken);
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        isChecking = true;

        while (flipQueue.Count >= 2) // Process pairs in the queue
        {
            Card card1 = flipQueue.Dequeue();
            Card card2 = flipQueue.Dequeue();

            yield return new WaitForSeconds(0.5f); // Wait before checking match

            if (card1.cardID == card2.cardID)
            {
                card1.SetMatched();
                card2.SetMatched();
                currentMatches++;

                UIManager.Instance.UpdateMatchText(currentMatches, totalMatches);

                if (currentMatches == totalMatches)
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

        isChecking = false; // Allow new match checking
    }

    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}
