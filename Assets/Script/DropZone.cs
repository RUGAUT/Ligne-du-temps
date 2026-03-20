using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int playerIndex; // 0 pour Player1, 1 pour Player2
    public int orderIndex;  // Index de la DropZone (0, 1, 2, ...)
    public TextMeshProUGUI dateText;
    private Image image;
    private bool isOccupied = false;
    private Color originalColor;
    private int targetYear;

    void Awake()
    {
        image = GetComponent<Image>();
        originalColor = image.color;
    }

    public void Initialize(int year)
    {
        targetYear = year;
        if (dateText != null)
        {
            dateText.text = year.ToString();
        }
    }

    public int GetTargetYear()
    {
        return targetYear;
    }

    public void ResetOccupied()
    {
        isOccupied = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            CardButton cardButton = eventData.pointerDrag.GetComponent<CardButton>();
            if (cardButton != null)
            {
                bool isPlayer1Card = cardButton.playerIndex == 0;
                bool isPlayer1DropZone = playerIndex == 0;

                if (isPlayer1Card == isPlayer1DropZone && !isOccupied)
                {
                    GameObject cardGO = Instantiate(cardButton.gameObject, transform);
                    CardButton newCardButton = cardGO.GetComponent<CardButton>();
                    newCardButton.SetCard(cardButton.currentSong, true);

                    RectTransform cardRectTransform = cardGO.GetComponent<RectTransform>();
                    cardRectTransform.anchorMin = new Vector2(0, 0);
                    cardRectTransform.anchorMax = new Vector2(1, 1);
                    cardRectTransform.offsetMin = Vector2.zero;
                    cardRectTransform.offsetMax = Vector2.zero;

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
                bool isPlayer1DropZone = playerIndex == 0;

                // Vérifier que la carte appartient au bon joueur ET que la DropZone n'est pas occupée
                if (isPlayer1Card == isPlayer1DropZone && !isOccupied)
                {
                    image.color = Color.green;
                }
                else
                {
                    image.color = Color.red;
                }
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
