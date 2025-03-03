[System.Serializable]
public class CardState
{
    public int cardID;  // Unique identifier for the card
    public bool isMatched;  // Indicates if the card has been matched

    // Constructor to set initial values
    public CardState(int id, bool matched)
    {
        cardID = id;
        isMatched = matched;
    }
}
