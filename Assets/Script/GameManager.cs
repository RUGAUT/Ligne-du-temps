using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public List<SongData> allSongs;
    public AudioSource audioSource;
    public Transform cardDeckParent;
    public GameObject cardButtonPrefab;
    public Button validateButton;

    private List<SongData> deck;
    private int _currentPlayerIndex = 0;
    public int currentPlayerIndex { get { return _currentPlayerIndex; } }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        DropZone.ResetFirstCards();
        ShuffleDeck();
        DrawCardForPlayer(_currentPlayerIndex);

        if (validateButton != null)
        {
            validateButton.onClick.AddListener(OnValidateButtonClick);
            validateButton.gameObject.SetActive(false);
        }
    }

    void ShuffleDeck()
    {
        deck = new List<SongData>(allSongs);
        deck = deck.OrderBy(x => Random.value).ToList();
    }

    public void DrawCardForPlayer(int playerIndex)
    {
        _currentPlayerIndex = playerIndex;
        if (deck.Count == 0)
        {
            if (validateButton != null)
            {
                validateButton.gameObject.SetActive(true);
            }
            return;
        }

        SongData card = deck[0];
        deck.RemoveAt(0);

        GameObject cardGO = Instantiate(cardButtonPrefab, cardDeckParent);
        CardButton cardButton = cardGO.GetComponent<CardButton>();
        bool isFirstCard = (playerIndex == 0 && DropZone.FirstCardYearPlayer1 == -1) ||
                           (playerIndex == 1 && DropZone.FirstCardYearPlayer2 == -1);
        cardButton.SetCard(card, false);
        if (isFirstCard)
        {
            cardButton.ShowAllText();
        }
        cardButton.SetPlayerIndex(playerIndex);
    }

    public void PlaySong(SongData song)
    {
        if (song != null && song.audioClip != null)
        {
            audioSource.clip = song.audioClip;
            audioSource.Play();
        }
    }

    private void OnValidateButtonClick()
    {
        DropZone.ShowAllTexts();
    }
}
