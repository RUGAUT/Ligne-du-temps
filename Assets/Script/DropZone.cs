using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int playerIndex;
    public int orderIndex;
    public bool isOccupied = false;

    // Rendu public pour que le GameManager puisse lire la date nécessaire
    public int targetYear;

    [Header("UI")]
    public TMPro.TextMeshProUGUI dateText;
    public GameObject defaultDateText;

    [Header("Feedback")]
    public GameObject vfxObject;
    public GameObject errorVfxObject;

    private Image image;
    private Color originalColor;

    void Awake()
    {
        image = GetComponent<Image>();
        originalColor = image.color;
    }

    public void Initialize(int year, int pIdx, int oIdx)
    {
        targetYear = year;
        playerIndex = pIdx;
        orderIndex = oIdx;
        isOccupied = false;
        if (dateText != null) dateText.text = year.ToString();
        if (defaultDateText != null) defaultDateText.SetActive(true);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        CardButton card = eventData.pointerDrag.GetComponent<CardButton>();

        if (card != null && card.playerIndex == playerIndex && !isOccupied)
        {
            if (card.currentSong.year == targetYear)
            {
                // REUSSITE : On instancie la carte et on affiche ses infos
                GameObject cardGO = Instantiate(card.gameObject, transform);
                CardButton newCard = cardGO.GetComponent<CardButton>();
                newCard.SetCard(card.currentSong, true, true);

                RectTransform rt = cardGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

                isOccupied = true;
                if (defaultDateText != null) defaultDateText.SetActive(false);
                GameManager.Instance.HandleCorrectPlacement(playerIndex);
                if (vfxObject != null) { vfxObject.SetActive(true); Invoke("HideVFX", 2.0f); }
            }
            else
            {
                // ECHEC
                GameManager.Instance.HandleWrongPlacement();
                StartCoroutine(ShakeDropZone());
                if (errorVfxObject != null)
                {
                    GameObject evfx = Instantiate(errorVfxObject, transform.position, Quaternion.identity, transform);
                    Destroy(evfx, 1.5f);
                }
            }

            // Passage au tour suivant et destruction de la carte piochee
            GameManager.Instance.DrawCardForPlayer(1 - playerIndex);
            Destroy(eventData.pointerDrag);
        }
        image.color = originalColor;
    }

    private IEnumerator ShakeDropZone()
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * 8f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }

    private void HideVFX() => vfxObject.SetActive(false);

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            CardButton cb = eventData.pointerDrag.GetComponent<CardButton>();
            image.color = (cb && cb.playerIndex == playerIndex && !isOccupied) ? Color.green : Color.red;
        }
    }

    public void OnPointerExit(PointerEventData eventData) => image.color = originalColor;
}