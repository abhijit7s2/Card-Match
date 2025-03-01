using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject cardPrefab;
    public Transform cardContainer;
    public Sprite[] cardSprites; // Assign in Inspector

    private List<Card> flippedCards = new List<Card>();

    private void Awake()
    {
        Instance = this;
        GenerateCards(3, 4); // Example: 3x4 Grid
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

        for (int i = 0; i < cardIDs.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardContainer);
            Card cardScript = newCard.GetComponent<Card>();
            cardScript.SetCard(cardIDs[i], cardSprites[cardIDs[i]]);
        }
    }

    public void CardFlipped(Card card)
    {
        flippedCards.Add(card);

        if (flippedCards.Count == 2)
        {
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.5f);

        if (flippedCards[0].cardID == flippedCards[1].cardID)
        {
            flippedCards[0].SetMatched();
            flippedCards[1].SetMatched();
        }
        else
        {
            flippedCards[0].FlipCard();
            flippedCards[1].FlipCard();
        }

        flippedCards.Clear();
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int temp = list[i];
            int randomIndex = Random.Range(0, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
