using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int playerIndex;
    public int orderIndex;

    [Header("Textes & UI")]
    public TextMeshProUGUI dateText; // Le texte géré par le code (si tu l'utilises)
    public GameObject defaultDateText; // NOUVEAU : Le texte fixe à désactiver (ton repère chronologique)

    [Header("Feedback Visuel")]
    public GameObject vfxObject;

    private Image image;
    private bool isOccupied = false;
    private Color originalColor;
    private int targetYear;

    void Awake()
    {
        image = GetComponent<Image>();
        originalColor = image.color;
        if (vfxObject != null) vfxObject.SetActive(false);

        // On s'assure que le texte de fond est bien allumé au départ
        if (defaultDateText != null) defaultDateText.SetActive(true);
    }

    public void Initialize(int year)
    {
        targetYear = year;
        if (dateText != null) dateText.text = year.ToString();
    }

    public int GetTargetYear() => targetYear;
    public void ResetOccupied() => isOccupied = false;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            CardButton cardButton = eventData.pointerDrag.GetComponent<CardButton>();
            if (cardButton != null)
            {
                if (cardButton.playerIndex == playerIndex && !isOccupied)
                {
                    // Placement de la carte
                    GameObject cardGO = Instantiate(cardButton.gameObject, transform);
                    CardButton newCardButton = cardGO.GetComponent<CardButton>();
                    newCardButton.SetCard(cardButton.currentSong, true);

                    RectTransform rt = cardGO.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

                    GameManager.Instance.AddCardToPlayer(cardButton.currentSong, playerIndex, orderIndex);
                    isOccupied = true;

                    // --- NOUVEAU : On désactive le texte fixe de la case ---
                    if (defaultDateText != null)
                    {
                        defaultDateText.SetActive(false);
                    }
                    // -------------------------------------------------------

                    // Vérification du score & VFX
                    if (cardButton.currentSong.year == targetYear)
                    {
                        newCardButton.ShowAllText();
                        GameManager.Instance.HandleCorrectPlacement(playerIndex);

                        if (vfxObject != null)
                        {
                            vfxObject.SetActive(true);
                            Invoke("HideVFX", 2.0f);
                        }
                    }

                    GameManager.Instance.DrawCardForPlayer(1 - playerIndex);
                    Destroy(eventData.pointerDrag);
                }
                else
                {
                    cardButton.GetComponent<RectTransform>().anchoredPosition = cardButton.initialPosition;
                }
            }
        }
        image.color = originalColor;
    }

    private void HideVFX() { if (vfxObject != null) vfxObject.SetActive(false); }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            CardButton cb = eventData.pointerDrag.GetComponent<CardButton>();
            image.color = (cb != null && cb.playerIndex == playerIndex && !isOccupied) ? Color.green : Color.red;
        }
    }

    public void OnPointerExit(PointerEventData eventData) => image.color = originalColor;
}