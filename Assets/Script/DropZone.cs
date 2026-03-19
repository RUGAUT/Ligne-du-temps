using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int playerIndex; // 0-4 pour Player1, 5-9 pour Player2
    public int orderIndex;  // Index de la DropZone (0, 1, 2, ...)
    private Image image;
    private bool isOccupied = false;
    private Color originalColor;

    void Awake()
    {
        image = GetComponent<Image>();
        originalColor = image.color;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            CardButton cardButton = eventData.pointerDrag.GetComponent<CardButton>();
            if (cardButton != null)
            {
                bool isPlayer1Card = cardButton.playerIndex == 0;
                bool isPlayer1DropZone = playerIndex < 5;

                if (isPlayer1Card == isPlayer1DropZone && !isOccupied)
                {
                    // Instancie la carte dans le DropZone
                    GameObject cardGO = Instantiate(cardButton.gameObject, transform);
                    CardButton newCardButton = cardGO.GetComponent<CardButton>();
                    newCardButton.SetCard(cardButton.currentSong, true);

                    // Positionnement
                    RectTransform cardRectTransform = cardGO.GetComponent<RectTransform>();
                    cardRectTransform.anchorMin = new Vector2(0, 0);
                    cardRectTransform.anchorMax = new Vector2(1, 1);
                    cardRectTransform.offsetMin = Vector2.zero;
                    cardRectTransform.offsetMax = Vector2.zero;

                    // Ajoute la carte au joueur
                    GameManager.Instance.AddCardToPlayer(cardButton.currentSong, cardButton.playerIndex, orderIndex);
                    isOccupied = true;
                    GameManager.Instance.DrawCardForPlayer(1 - cardButton.playerIndex);
                    Destroy(eventData.pointerDrag);
                }
                else
                {
                    cardButton.GetComponent<RectTransform>().anchoredPosition = cardButton.initialPosition;
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            CardButton cardButton = eventData.pointerDrag.GetComponent<CardButton>();
            if (cardButton != null)
            {
                bool isPlayer1Card = cardButton.playerIndex == 0;
                bool isPlayer1DropZone = playerIndex < 5;
                image.color = (isPlayer1Card == isPlayer1DropZone && !isOccupied) ? Color.green : Color.red;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            image.color = originalColor;
        }
    }
}
