using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int playerIndex;
    public int orderIndex;
    public bool isOccupied = false;

    [Header("UI")]
    public TMPro.TextMeshProUGUI dateText;
    public GameObject defaultDateText;

    [Header("Feedback")]
    public GameObject vfxObject; // Succès
    public GameObject errorVfxObject; // Échec (ex: croix rouge)

    private Image image;
    private Color originalColor;
    private int targetYear;

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
                // RÉUSSITE
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
                // ÉCHEC
                GameManager.Instance.HandleWrongPlacement();
                StartCoroutine(ShakeDropZone());
                if (errorVfxObject != null)
                {
                    GameObject evfx = Instantiate(errorVfxObject, transform.position, Quaternion.identity, transform);
                    Destroy(evfx, 1.5f);
                }
            }

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
            float x = Random.Range(-8f, 8f);
            float y = Random.Range(-8f, 8f);
            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }

    private void HideVFX() { if (vfxObject != null) vfxObject.SetActive(false); }
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