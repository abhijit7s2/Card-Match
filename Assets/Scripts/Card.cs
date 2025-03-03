using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour
{
    public int cardID;  // Unique ID for matching
    public SpriteRenderer frontImage; // Assigned dynamically
    public SpriteRenderer backImage;

    private bool isFlipped = false;
    private bool isMatched = false;

    private void OnMouseDown()
    {
        if (isMatched || isFlipped) return;  // Ignore if already matched

        Debug.Log("Clicked Card");
        FlipCard();  // Call FlipCard() instead of handling everything in OnMouseDown
        GameManager.Instance.CardFlipped(this); // Notify GameManager
    }

    public void FlipCard()
    {
        if (isMatched) return; // Ignore if already matched

        isFlipped = !isFlipped; // Toggle state
        StartCoroutine(FlipAnimation());
    }

    IEnumerator FlipAnimation()
    {
        float duration = 0.2f;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(0, startScale.y, startScale.z);

        // Shrink before swapping images
        for (float t = 0; t < duration / 2; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(startScale, endScale, t / (duration / 2));
            yield return null;
        }

        // Swap images
        frontImage.gameObject.SetActive(isFlipped);
        backImage.gameObject.SetActive(!isFlipped);

        // Expand back
        for (float t = 0; t < duration / 2; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(endScale, startScale, t / (duration / 2));
            yield return null;
        }
    }

    public void SetCard(int id, Sprite sprite)
    {
        cardID = id;
        frontImage.sprite = sprite;
    }

    public void SetMatched()
    {
        isMatched = true;
    }
    public bool IsMatched
    {
        get { return isMatched; }
    }

}
