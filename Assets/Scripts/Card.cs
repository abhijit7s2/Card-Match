using System.Collections;
using UnityEngine;

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

        isFlipped = true;
        StartCoroutine(FlipAnimation());

        GameManager.Instance.CardFlipped(this); // Notify GameManager
    }

    IEnumerator FlipAnimation()
    {
        float duration = 0.2f;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(0, startScale.y, startScale.z);

        // Flip animation (scale to 0, swap images, scale back)
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(startScale, endScale, t / duration);
            yield return null;
        }

        frontImage.gameObject.SetActive(isFlipped);
        backImage.gameObject.SetActive(!isFlipped);
        transform.localScale = startScale;
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
}
