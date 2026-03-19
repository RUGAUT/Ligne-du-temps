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
        initialPosition = rectTransform.anchoredPosition;
    }

    public void SetCard(SongData song, bool faceUp)
    {
        _currentSong = song;
        if (titleText != null) { titleText.text = song.title; titleText.gameObject.SetActive(faceUp); }
        if (artistText != null) { artistText.text = song.artist; artistText.gameObject.SetActive(faceUp); }
        if (yearText != null) { yearText.text = song.year.ToString(); yearText.gameObject.SetActive(faceUp); }
        if (cardImage != null && song.cardSprite != null) { cardImage.sprite = song.cardSprite; cardImage.color = Color.white; }
    }

    public void SetPlayerIndex(int index) => playerIndex = index;

    public void ShowAllText()
    {
        if (titleText != null) titleText.gameObject.SetActive(true);
        if (artistText != null) artistText.gameObject.SetActive(true);
        if (yearText != null) yearText.gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance != null && _currentSong != null)
            GameManager.Instance.PlaySong(_currentSong);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        initialPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData) =>
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<DropZone>() == null)
            rectTransform.anchoredPosition = initialPosition;
    }
}
