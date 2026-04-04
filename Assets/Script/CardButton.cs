using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI artistText;
    public TextMeshProUGUI yearText;
    private SongData _currentSong;
    public SongData currentSong => _currentSong;
    public int playerIndex;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    public Vector2 initialPosition { get; private set; }
    private Image cardImage;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        cardImage = GetComponent<Image>();
    }

    public void SetCard(SongData song, bool faceUp, bool showInfo = false)
    {
        _currentSong = song;
        if (titleText != null) { titleText.text = song.title; titleText.gameObject.SetActive(faceUp && showInfo); }
        if (artistText != null) { artistText.text = song.artist; artistText.gameObject.SetActive(faceUp && showInfo); }
        if (yearText != null) { yearText.text = song.year.ToString(); yearText.gameObject.SetActive(faceUp && showInfo); }

        if (cardImage != null && song.cardSprite != null)
        {
            cardImage.sprite = song.cardSprite;
            cardImage.color = Color.white;
        }
    }

    public void SetPlayerIndex(int index) => playerIndex = index;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Envoie l'ordre au GameManager de jouer ou d'arrêter cette chanson spécifique
        if (GameManager.Instance != null && _currentSong != null)
            GameManager.Instance.PlaySong(_currentSong);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (transform.parent != GameManager.Instance.cardDeckParent) { eventData.pointerDrag = null; return; }
        initialPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData) => rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // On récupère la DropZone sous la souris (s'il y en a une)
        DropZone dropZone = eventData.pointerEnter != null ? eventData.pointerEnter.GetComponent<DropZone>() : null;

        // La carte DOIT retourner à sa place si :
        // 1. On n'est pas au-dessus d'une DropZone
        // 2. OU la DropZone appartient à l'adversaire
        // 3. OU la DropZone est déjà occupée par une autre carte
        if (dropZone == null || dropZone.playerIndex != this.playerIndex || dropZone.isOccupied)
        {
            rectTransform.anchoredPosition = initialPosition;
        }
    }
}