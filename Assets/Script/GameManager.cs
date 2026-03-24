using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem; // NOUVEAU : Indispensable pour le New Input System

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

    [Header("Debug & Dev Access")]
    public bool isDebugMode = true;
    public TextMeshProUGUI debugInfoText;

    [Header("UI & Feedback")]
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public AudioClip validationSound;

    private int player1Score = 0;
    private int player2Score = 0;
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
        if (isDebugMode && GameSettings.SelectedGenre == MusicGenre.All)
        {
            Debug.Log("<color=cyan>DEBUG : Aucun genre détecté (Lancement direct), mode ALL.</color>");
        }

        UpdateScoreUI();
        DivideSongsBetweenPlayers();
        InitializeDropZones();
        DrawCardForPlayer(0);
        validateButton.onClick.AddListener(OnValidateButtonClick);
        validateButton.gameObject.SetActive(false);

        if (debugInfoText != null)
            debugInfoText.text = "Genre : " + GameSettings.SelectedGenre.ToString();
    }

    private void DivideSongsBetweenPlayers()
    {
        List<SongData> filteredSongs;

        if (GameSettings.SelectedGenre == MusicGenre.All)
        {
            filteredSongs = allSongs.OrderBy(x => Random.value).ToList();
        }
        else
        {
            filteredSongs = allSongs
                .Where(s => s.genre == GameSettings.SelectedGenre)
                .OrderBy(x => Random.value)
                .ToList();
        }

        if (isDebugMode)
        {
            Debug.Log($"<color=yellow>GAME SETUP : Genre [{GameSettings.SelectedGenre}] | Musiques : {filteredSongs.Count}</color>");
        }

        if (filteredSongs.Count < cardsPerPlayer * 2)
        {
            if (filteredSongs.Count == 0)
            {
                Debug.LogError("ERREUR : Aucune musique pour " + GameSettings.SelectedGenre);
                filteredSongs = allSongs.OrderBy(x => Random.value).ToList();
            }
            else
            {
                cardsPerPlayer = filteredSongs.Count / 2;
                if (isDebugMode) Debug.Log("<color=orange>DEBUG : Cartes par joueur -> " + cardsPerPlayer + "</color>");
            }
        }

        player1Deck = filteredSongs.GetRange(0, cardsPerPlayer);
        player2Deck = filteredSongs.GetRange(cardsPerPlayer, cardsPerPlayer);
    }

    // --- MISE À JOUR POUR LE NEW INPUT SYSTEM ---
    void Update()
    {
        if (isDebugMode)
        {
            // Vérifie si la touche 'R' est pressée avec le New Input System
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
    }

    private void InitializeDropZones()
    {
        var sorted1 = player1Deck.OrderBy(s => s.year).ToList();
        var sorted2 = player2Deck.OrderBy(s => s.year).ToList();

        for (int i = 0; i < player1DropZones.Count; i++)
        {
            if (i < sorted1.Count)
            {
                player1DropZones[i].gameObject.SetActive(true);
                player1DropZones[i].Initialize(sorted1[i].year);
                player1DropZones[i].playerIndex = 0;
                player1DropZones[i].orderIndex = i;
            }
            else player1DropZones[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < player2DropZones.Count; i++)
        {
            if (i < sorted2.Count)
            {
                player2DropZones[i].gameObject.SetActive(true);
                player2DropZones[i].Initialize(sorted2[i].year);
                player2DropZones[i].playerIndex = 1;
                player2DropZones[i].orderIndex = i;
            }
            else player2DropZones[i].gameObject.SetActive(false);
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

    public void HandleCorrectPlacement(int playerIndex)
    {
        if (validationSound != null && audioSource != null)
            audioSource.PlayOneShot(validationSound);

        if (playerIndex == 0) player1Score++;
        else player2Score++;

        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (player1ScoreText != null) player1ScoreText.text = "Score J1: " + player1Score;
        if (player2ScoreText != null) player2ScoreText.text = "Score J2: " + player2Score;
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
    }

    public void AddCardToPlayer(SongData song, int pIdx, int dIdx) { }
}