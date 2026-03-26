using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuration")]
    public List<SongData> allSongs;
    public AudioSource audioSource;
    public Transform cardDeckParent;
    public GameObject cardButtonPrefab;
    public List<DropZone> player1DropZones;
    public List<DropZone> player2DropZones;
    public int cardsPerPlayer = 5;

    [Header("Debug & UI")]
    public bool isDebugMode = true;
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public AudioClip validationSound;
    public AudioClip errorSound;

    [Header("Victory UI")]
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryText;

    private List<SongData> player1Deck = new List<SongData>();
    private List<SongData> player2Deck = new List<SongData>();
    private bool firstCardPlayer1 = true;
    private bool firstCardPlayer2 = true;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        DivideSongsBetweenPlayers();
        InitializeDropZones();
        UpdateScoreUI();
        DrawCardForPlayer(0);
    }

    private void DivideSongsBetweenPlayers()
    {
        // CHANGEMENT ICI : On vérifie si la liste "genres" CONTIENT le genre sélectionné
        List<SongData> filteredSongs = allSongs
            .Where(s => GameSettings.SelectedGenre == MusicGenre.All || (s.genres != null && s.genres.Contains(GameSettings.SelectedGenre)))
            .OrderBy(x => Random.value).ToList();

        if (filteredSongs.Count < cardsPerPlayer * 2)
            cardsPerPlayer = filteredSongs.Count / 2;

        player1Deck = filteredSongs.GetRange(0, cardsPerPlayer);
        player2Deck = filteredSongs.GetRange(cardsPerPlayer, cardsPerPlayer);
    }

    private void InitializeDropZones()
    {
        var s1 = player1Deck.OrderBy(s => s.year).ToList();
        var s2 = player2Deck.OrderBy(s => s.year).ToList();

        for (int i = 0; i < player1DropZones.Count; i++)
        {
            if (i < s1.Count) player1DropZones[i].Initialize(s1[i].year, 0, i);
            else player1DropZones[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < player2DropZones.Count; i++)
        {
            if (i < s2.Count) player2DropZones[i].Initialize(s2[i].year, 1, i);
            else player2DropZones[i].gameObject.SetActive(false);
        }
    }

    public void DrawCardForPlayer(int playerIndex)
    {
        List<SongData> currentDeck = (playerIndex == 0) ? player1Deck : player2Deck;
        if (currentDeck.Count == 0)
        {
            currentDeck.AddRange(allSongs.OrderBy(x => Random.value).Take(3));
        }

        SongData card = currentDeck[0];
        currentDeck.RemoveAt(0);

        GameObject cardGO = Instantiate(cardButtonPrefab, cardDeckParent);
        CardButton cb = cardGO.GetComponent<CardButton>();
        bool isFirst = (playerIndex == 0 && firstCardPlayer1) || (playerIndex == 1 && firstCardPlayer2);

        cb.SetCard(card, true, isFirst);
        if (playerIndex == 0) firstCardPlayer1 = false; else firstCardPlayer2 = false;
        cb.SetPlayerIndex(playerIndex);
    }

    public void HandleCorrectPlacement(int playerIndex)
    {
        if (validationSound != null && audioSource != null)
            audioSource.PlayOneShot(validationSound);
        UpdateScoreUI();
        CheckVictory();
    }

    public void HandleWrongPlacement()
    {
        if (errorSound != null && audioSource != null)
            audioSource.PlayOneShot(errorSound);
    }

    private void UpdateScoreUI()
    {
        int p1Count = player1DropZones.Count(z => z.isOccupied);
        int p2Count = player2DropZones.Count(z => z.isOccupied);
        if (player1ScoreText != null) player1ScoreText.text = $"J1: {p1Count}/{cardsPerPlayer}";
        if (player2ScoreText != null) player2ScoreText.text = $"J2: {p2Count}/{cardsPerPlayer}";
    }

    private void CheckVictory()
    {
        int p1Count = player1DropZones.Count(z => z.isOccupied);
        int p2Count = player2DropZones.Count(z => z.isOccupied);
        if (p1Count >= cardsPerPlayer) TriggerVictory(0);
        else if (p2Count >= cardsPerPlayer) TriggerVictory(1);
    }

    public void TriggerVictory(int winnerIndex)
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
        if (victoryText != null) victoryText.text = $"JOUEUR {winnerIndex + 1} A GAGNÉ !";
    }

    public void PlaySong(SongData song) { if (song?.audioClip != null) { audioSource.clip = song.audioClip; audioSource.Play(); } }
    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    public void AddCardToPlayer(SongData s, int p, int d) { }

    void Update()
    {
        if (isDebugMode && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) RestartGame();
    }
}