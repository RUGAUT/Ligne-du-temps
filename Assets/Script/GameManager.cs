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

    [Header("UI & Feedback")]
    public bool isDebugMode = true;
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public TextMeshProUGUI turnIndicatorText;
    public AudioClip validationSound;
    public AudioClip errorSound;

    [Header("Victory UI")]
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryText;

    [Header("Jetons (Tokens)")]
    public int startingTokens = 2;
    private int player1Tokens;
    private int player2Tokens;
    public TextMeshProUGUI player1TokenText;
    public TextMeshProUGUI player2TokenText;

    private List<SongData> player1Deck = new List<SongData>();
    private List<SongData> player2Deck = new List<SongData>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);

        player1Tokens = startingTokens;
        player2Tokens = startingTokens;
        UpdateTokenUI();

        DivideSongsBetweenPlayers();
        InitializeDropZones();
        UpdateScoreUI();

        // J1 commence, mais sans jouer de musique automatiquement
        DrawCardForPlayer(0);
    }

    private void DivideSongsBetweenPlayers()
    {
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

        player1Deck.Clear();
        player2Deck.Clear();
    }

    public void DrawCardForPlayer(int playerIndex)
    {
        UpdateTurnUI(playerIndex);

        List<SongData> currentDeck = (playerIndex == 0) ? player1Deck : player2Deck;
        List<DropZone> currentZones = (playerIndex == 0) ? player1DropZones : player2DropZones;

        // PIOCHE INTELLIGENTE
        if (currentDeck.Count == 0)
        {
            List<int> neededYears = currentZones.Where(z => !z.isOccupied).Select(z => z.targetYear).ToList();
            var validSongs = allSongs
                .Where(s => neededYears.Contains(s.year) && (GameSettings.SelectedGenre == MusicGenre.All || s.genres.Contains(GameSettings.SelectedGenre)))
                .OrderBy(x => Random.value).Take(3).ToList();

            if (validSongs.Count > 0) currentDeck.AddRange(validSongs);
            else currentDeck.AddRange(allSongs.OrderBy(x => Random.value).Take(3));
        }

        SongData card = currentDeck[0];
        currentDeck.RemoveAt(0);

        GameObject cardGO = Instantiate(cardButtonPrefab, cardDeckParent);
        CardButton cb = cardGO.GetComponent<CardButton>();
        cb.SetCard(card, true, false); // Infos masquées au début
        cb.SetPlayerIndex(playerIndex);

        // NOTE : On ne joue PAS la musique ici, on attend le clic du joueur.
    }

    private void UpdateTurnUI(int playerIndex)
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = $"TOUR DU JOUEUR {playerIndex + 1}";
            turnIndicatorText.color = (playerIndex == 0) ? new Color(0.2f, 0.6f, 1f) : new Color(1f, 0.4f, 0.4f);
        }
    }

    public void PlaySong(SongData song)
    {
        if (song?.audioClip != null && audioSource != null)
        {
            // TOGGLE : Si c'est la même musique, on l'arrête. Sinon on joue la nouvelle.
            if (audioSource.clip == song.audioClip && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            else
            {
                audioSource.clip = song.audioClip;
                audioSource.Play();
            }
        }
    }

    public void UseToken(int playerIndex)
    {
        if (playerIndex == 0 && player1Tokens <= 0) return;
        if (playerIndex == 1 && player2Tokens <= 0) return;

        CardButton currentCard = cardDeckParent.GetComponentsInChildren<CardButton>().FirstOrDefault(c => c.playerIndex == playerIndex);
        if (currentCard == null) return;

        SongData replacement = allSongs
            .Where(s => s.year == currentCard.currentSong.year && s != currentCard.currentSong && (GameSettings.SelectedGenre == MusicGenre.All || s.genres.Contains(GameSettings.SelectedGenre)))
            .OrderBy(x => Random.value).FirstOrDefault();

        if (replacement != null)
        {
            if (playerIndex == 0) player1Tokens--; else player2Tokens--;
            currentCard.SetCard(replacement, true, false);
            UpdateTokenUI();
            audioSource.Stop(); // On arrête la musique de l'ancienne carte
        }
    }

    private void UpdateTokenUI()
    {
        if (player1TokenText != null) player1TokenText.text = $"Jokers J1: {player1Tokens}";
        if (player2TokenText != null) player2TokenText.text = $"Jokers J2: {player2Tokens}";
    }

    public void HandleCorrectPlacement(int playerIndex)
    {
        audioSource.Stop(); // Arrête la musique dès qu'on a fini le tour
        if (validationSound != null) audioSource.PlayOneShot(validationSound);
        UpdateScoreUI();
        CheckVictory();
    }

    public void HandleWrongPlacement()
    {
        audioSource.Stop();
        if (errorSound != null) audioSource.PlayOneShot(errorSound);
    }

    private void UpdateScoreUI()
    {
        int p1 = player1DropZones.Count(z => z.isOccupied);
        int p2 = player2DropZones.Count(z => z.isOccupied);
        if (player1ScoreText != null) player1ScoreText.text = $"J1: {p1}/{cardsPerPlayer}";
        if (player2ScoreText != null) player2ScoreText.text = $"J2: {p2}/{cardsPerPlayer}";
    }

    private void CheckVictory()
    {
        if (player1DropZones.Count(z => z.isOccupied) >= cardsPerPlayer) TriggerVictory(0);
        else if (player2DropZones.Count(z => z.isOccupied) >= cardsPerPlayer) TriggerVictory(1);
    }

    public void TriggerVictory(int winnerIndex)
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
        if (turnIndicatorText != null) turnIndicatorText.gameObject.SetActive(false);
        if (victoryText != null) victoryText.text = $"VICTOIRE DU JOUEUR {winnerIndex + 1} !";
    }

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    void Update()
    {
        if (isDebugMode && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) RestartGame();
    }
}