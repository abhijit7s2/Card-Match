using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject cardPrefab;
    public Transform cardContainer;
    public CardData cardData;

    private List<Card> flippedCards = new List<Card>();
    private List<Card> allCards = new List<Card>();

    private int totalMatches;
    private int currentMatches = 0;
    private int turnsTaken = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        GenerateCards(3, 4);
        totalMatches = (3 * 4) / 2;
        UIManager.Instance.UpdateMatchText(currentMatches, totalMatches);
        UIManager.Instance.UpdateTurnText(turnsTaken);
    }


    void GenerateCards(int rows, int cols)
    {
        List<int> cardIDs = new List<int>();

        for (int i = 0; i < (rows * cols) / 2; i++)
        {
            cardIDs.Add(i);
            cardIDs.Add(i);
        }

        Shuffle(cardIDs);

        float spacingX = 2.0f, spacingY = 2.5f; // Adjust based on sprite size
        Vector3 startPos = new Vector3(-cols / 2.0f, rows / 2.0f, 0); // Center grid

        for (int i = 0; i < cardIDs.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;

            Vector3 cardPosition = startPos + new Vector3(col * spacingX, -row * spacingY, 0);
            GameObject newCard = Instantiate(cardPrefab, cardPosition, Quaternion.identity, cardContainer);

            Card cardScript = newCard.GetComponent<Card>();
            cardScript.SetCard(cardIDs[i], cardData.cardSprites[cardIDs[i]]);
        }
    }


    public void CardFlipped(Card card)
    {
        if (flippedCards.Contains(card)) return; // Prevent double flip
        flippedCards.Add(card);

        if (flippedCards.Count == 2)
        {
            turnsTaken++;
            StartCoroutine(CheckMatch());
            UIManager.Instance.UpdateTurnText(turnsTaken);
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.5f);

        if (flippedCards[0].cardID == flippedCards[1].cardID)
        {
            flippedCards[0].SetMatched();
            flippedCards[1].SetMatched();
            currentMatches++;

            UIManager.Instance.UpdateMatchText(currentMatches, totalMatches);

            if (currentMatches == totalMatches)
            {
                UIManager.Instance.ShowVictory();
            }
        }
        else
        {
            flippedCards[0].FlipCard(); // Add FlipCard() in Card.cs
            flippedCards[1].FlipCard();
        }

        flippedCards.Clear();
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
