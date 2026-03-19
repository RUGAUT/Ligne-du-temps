using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<SongData> allSongs;
    public AudioSource audioSource;
    public Transform cardDeckParent;
    public GameObject cardButtonPrefab;
    public Button validateButton;

    private List<SongData> deck;
    private List<(SongData song, int dropZoneIndex)> player1Cards = new List<(SongData, int)>();
    private List<(SongData song, int dropZoneIndex)> player2Cards = new List<(SongData, int)>();
    private int currentPlayerIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        ShuffleDeck();
        DrawCardForPlayer(currentPlayerIndex);
        validateButton.onClick.AddListener(CalculateWinner);
        validateButton.gameObject.SetActive(false);
    }

    void ShuffleDeck()
    {
        deck = new List<SongData>(allSongs);
        deck = deck.OrderBy(x => Random.value).ToList();
    }

    public void DrawCardForPlayer(int playerIndex)
    {
        currentPlayerIndex = playerIndex;
        if (deck.Count == 0)
        {
            validateButton.gameObject.SetActive(true);
            return;
        }

        SongData card = deck[0];
        deck.RemoveAt(0);

        GameObject cardGO = Instantiate(cardButtonPrefab, cardDeckParent);
        CardButton cardButton = cardGO.GetComponent<CardButton>();
        cardButton.SetCard(card, true);
        cardButton.SetPlayerIndex(playerIndex);
    }

    public void AddCardToPlayer(SongData song, int playerIndex, int dropZoneIndex)
    {
        if (playerIndex == 0) player1Cards.Add((song, dropZoneIndex));
        else player2Cards.Add((song, dropZoneIndex));
    }

    public void PlaySong(SongData song)
    {
        if (song?.audioClip != null)
        {
            audioSource.clip = song.audioClip;
            audioSource.Play();
        }
    }

    public void CalculateWinner()
    {
        var sortedPlayer1 = player1Cards.OrderBy(c => c.dropZoneIndex).ToList();
        var sortedPlayer2 = player2Cards.OrderBy(c => c.dropZoneIndex).ToList();

        int scorePlayer1 = 0, scorePlayer2 = 0;

        for (int i = 0; i < sortedPlayer1.Count - 1; i++)
            if (sortedPlayer1[i].song.year <= sortedPlayer1[i + 1].song.year) scorePlayer1++;

        for (int i = 0; i < sortedPlayer2.Count - 1; i++)
            if (sortedPlayer2[i].song.year <= sortedPlayer2[i + 1].song.year) scorePlayer2++;

        Debug.Log($"Player 1: {scorePlayer1} cartes dans l'ordre");
        Debug.Log($"Player 2: {scorePlayer2} cartes dans l'ordre");

        if (scorePlayer1 > scorePlayer2) Debug.Log("Player 1 gagne !");
        else if (scorePlayer2 > scorePlayer1) Debug.Log("Player 2 gagne !");
        else Debug.Log("Égalité !");
    }
}
