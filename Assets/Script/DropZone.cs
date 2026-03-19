using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int playerIndex; // 0-4 pour Player1Timeline, 5-9 pour Player2Timeline
    private Image image;
    private bool isOccupied = false;
    private Color originalColor;

    private static int _firstCardYearPlayer1 = -1;
    private static int _firstCardYearPlayer2 = -1;

    public static int FirstCardYearPlayer1
    {
        get { return _firstCardYearPlayer1; }
        set { _firstCardYearPlayer1 = value; }
    }

    public static int FirstCardYearPlayer2
    {
        get { return _firstCardYearPlayer2; }
        set { _firstCardYearPlayer2 = value; }
    }

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
                    // Crée une nouvelle carte dans ce DropZone
                    GameObject cardGO = Instantiate(cardButton.gameObject, transform);
                    CardButton newCardButton = cardGO.GetComponent<CardButton>();
                    newCardButton.SetCard(cardButton.currentSong, false);

                    // Positionne correctement la carte dans le DropZone
                    RectTransform cardRectTransform = cardGO.GetComponent<RectTransform>();
                    cardRectTransform.anchorMin = new Vector2(0, 0);
                    cardRectTransform.anchorMax = new Vector2(1, 1);
                    cardRectTransform.offsetMin = Vector2.zero;
                    cardRectTransform.offsetMax = Vector2.zero;

                    // Vérifie si c'est la première carte du joueur
                    if (isPlayer1Card && _firstCardYearPlayer1 == -1)
                    {
                        _firstCardYearPlayer1 = cardButton.currentSong.year;
                        newCardButton.ShowAllText();
                    }
                    else if (!isPlayer1Card && _firstCardYearPlayer2 == -1)
                    {
                        _firstCardYearPlayer2 = cardButton.currentSong.year;
                        newCardButton.ShowAllText();
                    }

                    // Marque ce DropZone comme occupé
                    isOccupied = true;

                    // Passe au joueur suivant
                    GameManager.Instance.DrawCardForPlayer(1 - cardButton.playerIndex);

                    // Détruit la carte originale
                    Destroy(eventData.pointerDrag);
                }
                else
                {
                    // Retourne la carte à sa position initiale
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

    public static void ResetFirstCards()
    {
        _firstCardYearPlayer1 = -1;
        _firstCardYearPlayer2 = -1;
    }

    public static void ShowAllTexts()
    {
        CardButton[] allCards = FindObjectsOfType<CardButton>();
        foreach (CardButton card in allCards)
        {
            card.ShowAllText();
        }
    }
}
