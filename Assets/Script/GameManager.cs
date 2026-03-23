using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuration")]
    public List<SongData> allSongs;
    public AudioSource audioSource;
    public Transform cardDeckParent;
    public GameObject cardButtonPrefab;
    public Button validateButton;
    public List<DropZone> player1DropZones;
    public List<DropZone> player2DropZones;
    public int cardsPerPlayer = 5;

    [Header("UI & Feedback")]
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public AudioClip validationSound;

    private int player1Score = 0;
    private int player2Score = 0;
    private List<SongData> player1Deck = new List<SongData>();
    private List<SongData> player2Deck = new List<SongData>();
    private List<(SongData song, int dropZoneIndex, int targetYear)> player1Cards = new List<(SongData, int, int)>();
    private List<(SongData song, int dropZoneIndex, int targetYear)> player2Cards = new List<(SongData, int, int)>();
    private bool firstCardPlayer1 = true;
    private bool firstCardPlayer2 = true;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        UpdateScoreUI();
        DivideSongsBetweenPlayers();
        InitializeDropZones();
        DrawCardForPlayer(0);
        validateButton.onClick.AddListener(OnValidateButtonClick);
        validateButton.gameObject.SetActive(false);
    }

    public void HandleCorrectPlacement(int playerIndex)
    {
        // 1. Son
        if (validationSound != null && audioSource != null)
            audioSource.PlayOneShot(validationSound);

        // 2. Score
        if (playerIndex == 0) player1Score++;
        else player2Score++;

        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (player1ScoreText != null) player1ScoreText.text = "Score J1: " + player1Score;
        if (player2ScoreText != null) player2ScoreText.text = "Score J2: " + player2Score;
    }

    private void DivideSongsBetweenPlayers()
    {
        List<SongData> shuffledSongs = allSongs.OrderBy(x => Random.value).ToList();
        player1Deck = shuffledSongs.GetRange(0, cardsPerPlayer);
        player2Deck = shuffledSongs.GetRange(cardsPerPlayer, cardsPerPlayer);
    }

    private void InitializeDropZones()
    {
        var sorted1 = player1Deck.OrderBy(s => s.year).ToList();
        var sorted2 = player2Deck.OrderBy(s => s.year).ToList();

        for (int i = 0; i < player1DropZones.Count && i < sorted1.Count; i++)
        {
            player1DropZones[i].Initialize(sorted1[i].year);
            player1DropZones[i].playerIndex = 0;
            player1DropZones[i].orderIndex = i;
        }
        for (int i = 0; i < player2DropZones.Count && i < sorted2.Count; i++)
        {
            player2DropZones[i].Initialize(sorted2[i].year);
            player2DropZones[i].playerIndex = 1;
            player2DropZones[i].orderIndex = i;
        }
    }

    public void DrawCardForPlayer(int playerIndex)
    {
        List<SongData> currentDeck = (playerIndex == 0) ? player1Deck : player2Deck;

        if (currentDeck.Count == 0)
        {
            if (playerIndex == 0 && player2Deck.Count > 0) DrawCardForPlayer(1);
            else if (playerIndex == 1 && player1Deck.Count > 0) DrawCardForPlayer(0);
            else validateButton.gameObject.SetActive(true);
            return;
        }

        SongData card = currentDeck[0];
        currentDeck.RemoveAt(0);

        GameObject cardGO = Instantiate(cardButtonPrefab, cardDeckParent);
        CardButton cardButton = cardGO.GetComponent<CardButton>();
        bool isFirst = (playerIndex == 0 && firstCardPlayer1) || (playerIndex == 1 && firstCardPlayer2);

        cardButton.SetCard(card, true, isFirst);
        if (playerIndex == 0) firstCardPlayer1 = false; else firstCardPlayer2 = false;
        cardButton.SetPlayerIndex(playerIndex);
    }

    public void AddCardToPlayer(SongData song, int pIdx, int dIdx)
    {
        DropZone dz = (pIdx == 0) ? player1DropZones[dIdx] : player2DropZones[dIdx];
        if (pIdx == 0) player1Cards.Add((song, dIdx, dz.GetTargetYear()));
        else player2Cards.Add((song, dIdx, dz.GetTargetYear()));
    }

    public void PlaySong(SongData song)
    {
        if (song?.audioClip != null) { audioSource.clip = song.audioClip; audioSource.Play(); }
    }

    private void OnValidateButtonClick() { CalculateWinner(); }

    public void CalculateWinner()
    {
        CardButton[] allCards = FindObjectsOfType<CardButton>();
        foreach (CardButton card in allCards) card.ShowAllText();
        Debug.Log($"Final: J1:{player1Score} - J2:{player2Score}");
    }
}