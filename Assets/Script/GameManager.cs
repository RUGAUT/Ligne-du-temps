using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<SongData> allSongs;
    public AudioSource audioSource;
    public Transform cardDeckParent;
    public GameObject cardButtonPrefab;
    public Button validateButton;
    public List<DropZone> player1DropZones;
    public List<DropZone> player2DropZones;
    public int cardsPerPlayer = 5;

    private List<SongData> player1Deck = new List<SongData>();
    private List<SongData> player2Deck = new List<SongData>();
    private List<(SongData song, int dropZoneIndex, int targetYear)> player1Cards = new List<(SongData, int, int)>();
    private List<(SongData song, int dropZoneIndex, int targetYear)> player2Cards = new List<(SongData, int, int)>();
    private int currentPlayerIndex = 0;
    private bool firstCardPlayer1 = true;
    private bool firstCardPlayer2 = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        DivideSongsBetweenPlayers();
        InitializeDropZones();
        DrawCardForPlayer(0);
        validateButton.onClick.AddListener(OnValidateButtonClick);
        validateButton.gameObject.SetActive(false);
    }

    private void DivideSongsBetweenPlayers()
    {
        List<SongData> shuffledSongs = new List<SongData>(allSongs);
        shuffledSongs = shuffledSongs.OrderBy(x => Random.value).ToList();

        player1Deck = shuffledSongs.GetRange(0, cardsPerPlayer);
        player2Deck = shuffledSongs.GetRange(cardsPerPlayer, cardsPerPlayer);
    }

    private void InitializeDropZones()
    {
        var sortedPlayer1Songs = player1Deck.OrderBy(s => s.year).ToList();
        var sortedPlayer2Songs = player2Deck.OrderBy(s => s.year).ToList();

        for (int i = 0; i < player1DropZones.Count && i < sortedPlayer1Songs.Count; i++)
        {
            player1DropZones[i].Initialize(sortedPlayer1Songs[i].year);
            player1DropZones[i].playerIndex = 0;
            player1DropZones[i].orderIndex = i;
        }

        for (int i = 0; i < player2DropZones.Count && i < sortedPlayer2Songs.Count; i++)
        {
            player2DropZones[i].Initialize(sortedPlayer2Songs[i].year);
            player2DropZones[i].playerIndex = 1;
            player2DropZones[i].orderIndex = i;
        }
    }

    public void DrawCardForPlayer(int playerIndex)
    {
        currentPlayerIndex = playerIndex;
        List<SongData> currentDeck = playerIndex == 0 ? player1Deck : player2Deck;

        if (currentDeck.Count == 0)
        {
            if (playerIndex == 0 && player2Deck.Count > 0)
            {
                DrawCardForPlayer(1);
            }
            else if (playerIndex == 1 && player1Deck.Count > 0)
            {
                DrawCardForPlayer(0);
            }
            else
            {
                validateButton.gameObject.SetActive(true);
            }
            return;
        }

        SongData card = currentDeck[0];
        currentDeck.RemoveAt(0);

        GameObject cardGO = Instantiate(cardButtonPrefab, cardDeckParent);
        CardButton cardButton = cardGO.GetComponent<CardButton>();
        bool isFirstCard = (playerIndex == 0 && firstCardPlayer1) || (playerIndex == 1 && firstCardPlayer2);
        cardButton.SetCard(card, true, isFirstCard);

        if (playerIndex == 0 && firstCardPlayer1) firstCardPlayer1 = false;
        if (playerIndex == 1 && firstCardPlayer2) firstCardPlayer2 = false;

        cardButton.SetPlayerIndex(playerIndex);
    }

    public void AddCardToPlayer(SongData song, int playerIndex, int dropZoneIndex)
    {
        DropZone dropZone = GetDropZoneForPlayer(playerIndex, dropZoneIndex);
        if (dropZone != null)
        {
            if (playerIndex == 0)
            {
                player1Cards.Add((song, dropZoneIndex, dropZone.GetTargetYear()));
            }
            else
            {
                player2Cards.Add((song, dropZoneIndex, dropZone.GetTargetYear()));
            }
        }
    }

    private DropZone GetDropZoneForPlayer(int playerIndex, int dropZoneIndex)
    {
        if (playerIndex == 0 && dropZoneIndex < player1DropZones.Count)
            return player1DropZones[dropZoneIndex];
        else if (playerIndex == 1 && dropZoneIndex < player2DropZones.Count)
            return player2DropZones[dropZoneIndex];
        return null;
    }

    public void PlaySong(SongData song)
    {
        if (song?.audioClip != null)
        {
            audioSource.clip = song.audioClip;
            audioSource.Play();
        }
    }

    public void ResetAllDropZones()
    {
        foreach (var dropZone in player1DropZones)
        {
            dropZone.ResetOccupied();
        }
        foreach (var dropZone in player2DropZones)
        {
            dropZone.ResetOccupied();
        }
    }

    public void CalculateWinner()
    {
        ShowAllCardsText();

        // Calculer le score pour chaque joueur
        int scorePlayer1 = CalculateScore(player1Cards);
        int scorePlayer2 = CalculateScore(player2Cards);

        Debug.Log("Résultat final :");
        Debug.Log($"Player 1 a {scorePlayer1} bonnes réponses.");
        Debug.Log($"Player 2 a {scorePlayer2} bonnes réponses.");

        if (scorePlayer1 > scorePlayer2)
        {
            Debug.Log("Player 1 gagne !");
        }
        else if (scorePlayer2 > scorePlayer1)
        {
            Debug.Log("Player 2 gagne !");
        }
        else
        {
            Debug.Log("Égalité !");
        }
    }

    private int CalculateScore(List<(SongData song, int dropZoneIndex, int targetYear)> playerCards)
    {
        int score = 0;

        foreach (var placedCard in playerCards)
        {
            // Vérifier que l'année de la chanson posée correspond à l'année de la DropZone
            if (placedCard.song.year == placedCard.targetYear)
            {
                score++;
            }
        }

        return score;
    }

    private void ShowAllCardsText()
    {
        CardButton[] allCards = FindObjectsOfType<CardButton>();
        foreach (CardButton card in allCards)
            card.ShowAllText();
    }

    private void OnValidateButtonClick()
    {
        CalculateWinner();
        ResetAllDropZones();
    }
}