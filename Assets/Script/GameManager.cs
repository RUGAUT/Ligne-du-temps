using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System; // Nécessaire pour string.IsNullOrEmpty

// --- IMPORTATION MAESTRO ---
using MidiPlayerTK;
// ---------------------------

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuration")]
    public List<SongData> allSongs;
    public Transform cardDeckParent;
    public GameObject cardButtonPrefab;
    public List<DropZone> player1DropZones;
    public List<DropZone> player2DropZones;
    public int cardsPerPlayer = 5;

    [Header("Audio Setup")]
    public AudioSource fxSource;
    public AudioSource musicSource;

    [Header("Maestro Integration")]
    [Tooltip("Glisse ici le prefab 'MidiFilePlayer' présent dans ta scène")]
    public MidiFilePlayer midiFilePlayer;

    private SongData currentlyPlayingSong;

    [Header("UI & Feedback")]
    public bool isDebugMode = true;
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public TextMeshProUGUI turnIndicatorText;
    public AudioClip validationSound;
    public AudioClip errorSound;
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryText;

    [Header("Tokens")]
    public int startingTokens = 2;
    private int player1Tokens;
    private int player2Tokens;
    public TextMeshProUGUI player1TokenText;
    public TextMeshProUGUI player2TokenText;

    private List<SongData> player1Deck = new List<SongData>();
    private List<SongData> player2Deck = new List<SongData>();

    private List<SongData> player1DrawPile = new List<SongData>();
    private List<SongData> player2DrawPile = new List<SongData>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);

        // Sécurité : si on n'a pas assigné le prefab dans l'inspecteur, on essaie de le trouver automatiquement
        if (midiFilePlayer == null)
        {
            midiFilePlayer = FindObjectOfType<MidiFilePlayer>();
            if (midiFilePlayer == null) Debug.LogWarning("Maestro MidiFilePlayer introuvable dans la scène !");
        }

        player1Tokens = player2Tokens = startingTokens;
        UpdateTokenUI();
        DivideSongsBetweenPlayers();
        InitializeDropZones();
        UpdateScoreUI();
        DrawCardForPlayer(0);
    }

    private List<SongData> GetFilteredSongs()
    {
        if (GameSettings.SelectedGenre == MusicGenre.All) return allSongs;

        List<SongData> filtered = allSongs
            .Where(s => s.genres != null && s.genres.Contains(GameSettings.SelectedGenre))
            .ToList();

        if (filtered.Count == 0)
        {
            Debug.LogWarning($"Aucune chanson trouvée pour le genre {GameSettings.SelectedGenre}. Toutes les chansons seront utilisées.");
            return allSongs;
        }
        return filtered;
    }

    // --- MÉTHODES AUDIO INTÉGRÉES AVEC MAESTRO (MidiPlayerTK) ---

    public void PlaySong(SongData song)
    {
        if (song == null) return;

        // Toggle : Si on reclique sur la même chanson, on arrête
        if (currentlyPlayingSong == song)
        {
            StopAllMusic();
            return;
        }

        // 1. Arrêter tout ce qui joue actuellement
        StopAllMusic();
        currentlyPlayingSong = song;

        // 2. PRIORITÉ MIDI
        if (!string.IsNullOrEmpty(song.midiFileName))
        {
            Debug.Log($"[Maestro] Lecture MIDI : {song.midiFileName}");

            if (midiFilePlayer != null)
            {
                midiFilePlayer.MPTK_MidiName = song.midiFileName;
                midiFilePlayer.MPTK_Play();
            }
            else
            {
                Debug.LogError("Maestro MidiFilePlayer est null ! Vérifie le prefab dans la scène.");
            }

            return;
        }

        // 3. FALLBACK MP3
        if (song.previewAudio != null && musicSource != null)
        {
            musicSource.clip = song.previewAudio;
            musicSource.loop = true;
            musicSource.Play();
            Debug.Log($"[Audio] Lecture MP3 : {song.title}");
        }
    }

    public void StopAllMusic()
    {
        // Arrêter MP3
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }

        // Arrêter MIDI via Maestro
        if (midiFilePlayer != null)
        {
            midiFilePlayer.MPTK_Stop();
        }

        currentlyPlayingSong = null;
    }
    // --------------------------------------------------------------


    private void DivideSongsBetweenPlayers()
    {
        List<SongData> songsToUse = GetFilteredSongs();
        // CORRECTION ICI : UnityEngine.Random pour éviter l'ambiguïté
        List<SongData> rnd = songsToUse.OrderBy(x => UnityEngine.Random.value).ToList();

        if (rnd.Count < cardsPerPlayer * 2)
            cardsPerPlayer = Mathf.Max(1, rnd.Count / 2);

        player1Deck = rnd.GetRange(0, cardsPerPlayer);
        player2Deck = rnd.GetRange(cardsPerPlayer, cardsPerPlayer);
    }

    private void InitializeDropZones()
    {
        var s1 = player1Deck.OrderBy(s => s.year).ToList();
        var s2 = player2Deck.OrderBy(s => s.year).ToList();

        for (int i = 0; i < player1DropZones.Count; i++)
        {
            if (i < s1.Count)
            {
                player1DropZones[i].Initialize(s1[i].year, 0, i);
                player1DropZones[i].gameObject.SetActive(true);
            }
            else
            {
                player1DropZones[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < player2DropZones.Count; i++)
        {
            if (i < s2.Count)
            {
                player2DropZones[i].Initialize(s2[i].year, 1, i);
                player2DropZones[i].gameObject.SetActive(true);
            }
            else
            {
                player2DropZones[i].gameObject.SetActive(false);
            }
        }

        player1DrawPile.Clear();
        player2DrawPile.Clear();

        List<SongData> availableSongsForPiles = new List<SongData>(GetFilteredSongs());

        foreach (var zone in player1DropZones)
        {
            if (zone.gameObject.activeSelf)
            {
                SongData match = availableSongsForPiles.FirstOrDefault(s => s.year == zone.targetYear);
                if (match != null)
                {
                    player1DrawPile.Add(match);
                    availableSongsForPiles.Remove(match);
                }
            }
        }

        foreach (var zone in player2DropZones)
        {
            if (zone.gameObject.activeSelf)
            {
                SongData match = availableSongsForPiles.FirstOrDefault(s => s.year == zone.targetYear);
                if (match != null)
                {
                    player2DrawPile.Add(match);
                    availableSongsForPiles.Remove(match);
                }
            }
        }

        player1Deck.Clear();
        player2Deck.Clear();
    }

    public void DrawCardForPlayer(int playerIndex)
    {
        UpdateTurnUI(playerIndex);
        List<SongData> currentPile = (playerIndex == 0) ? player1DrawPile : player2DrawPile;

        if (currentPile.Count == 0) return;

        // CORRECTION ICI : UnityEngine.Random pour éviter l'ambiguïté
        int randomIndex = UnityEngine.Random.Range(0, currentPile.Count);
        SongData songToDraw = currentPile[randomIndex];
        currentPile.RemoveAt(randomIndex);

        GameObject cardGO = Instantiate(cardButtonPrefab, cardDeckParent);
        var cb = cardGO.GetComponent<CardButton>();
        cb.SetCard(songToDraw, true, false);
        cb.SetPlayerIndex(playerIndex);
    }

    public void ReturnCardToPile(SongData song, int playerIndex)
    {
        if (playerIndex == 0) player1DrawPile.Add(song);
        else player2DrawPile.Add(song);
    }

    public void HandleCorrectPlacement(int pIdx)
    {
        StopAllMusic();
        if (validationSound) fxSource.PlayOneShot(validationSound);
        UpdateScoreUI();
        CheckVictory();
    }

    public void HandleWrongPlacement()
    {
        StopAllMusic();
        if (errorSound) fxSource.PlayOneShot(errorSound);
    }

    private void UpdateScoreUI()
    {
        int p1 = player1DropZones.Count(z => z.isOccupied);
        int p2 = player2DropZones.Count(z => z.isOccupied);
        if (player1ScoreText) player1ScoreText.text = $"J1: {p1}/{cardsPerPlayer}";
        if (player2ScoreText) player2ScoreText.text = $"J2: {p2}/{cardsPerPlayer}";
    }

    private void UpdateTokenUI()
    {
        if (player1TokenText) player1TokenText.text = $"Jokers J1: {player1Tokens}";
        if (player2TokenText) player2TokenText.text = $"Jokers J2: {player2Tokens}";
    }

    private void UpdateTurnUI(int pIdx)
    {
        if (turnIndicatorText) turnIndicatorText.text = $"TOUR DU JOUEUR {pIdx + 1}";
    }

    private void CheckVictory()
    {
        if (player1DropZones.Count(z => z.isOccupied) >= cardsPerPlayer) TriggerVictory(0);
        else if (player2DropZones.Count(z => z.isOccupied) >= cardsPerPlayer) TriggerVictory(1);
    }

    public void TriggerVictory(int winner)
    {
        if (victoryPanel) victoryPanel.SetActive(true);
        if (victoryText) victoryText.text = $"VICTOIRE J{winner + 1}";
    }

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    void Update() { if (isDebugMode && Keyboard.current.rKey.wasPressedThisFrame) RestartGame(); }
}