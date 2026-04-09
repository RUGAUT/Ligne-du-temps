using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using FluidSynthUnity;
using FluidSynth;

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
    public MidiSynthBehavior synthBehavior;
    public AudioSource fxSource;
    private SongData currentlyPlayingSong;
    private MPTKEvent currentMidiEvent;

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

    // Pioches spécifiques à chaque joueur (ne contient QUE les musiques qui correspondent aux dropzones)
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
        player1Tokens = player2Tokens = startingTokens;
        UpdateTokenUI();
        DivideSongsBetweenPlayers();
        InitializeDropZones();
        UpdateScoreUI();
        DrawCardForPlayer(0);
    }

    // --- FONCTION : Filtrer les musiques selon le choix du menu ---
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

    public void PlaySong(SongData song)
    {
        if (song == null) return;
        if (currentlyPlayingSong == song) { StopAllMusic(); return; }

        StopAllMusic();
        currentlyPlayingSong = song;

        if (synthBehavior != null && !string.IsNullOrEmpty(song.midiFileName))
        {
            currentMidiEvent = synthBehavior.PlayNote((Tone)60, (0, 0), duration: -1, velocity: 100);
        }
        else if (song.previewAudio != null && fxSource != null)
        {
            fxSource.clip = song.previewAudio;
            fxSource.loop = true;
            fxSource.Play();
        }
    }

    public void StopAllMusic()
    {
        if (synthBehavior != null && currentMidiEvent != null)
        {
            synthBehavior.StopEvent(currentMidiEvent);
            currentMidiEvent = null;
        }
        if (fxSource != null && currentlyPlayingSong != null && fxSource.clip == currentlyPlayingSong.previewAudio)
        {
            fxSource.Stop();
            fxSource.clip = null;
        }
        currentlyPlayingSong = null;
    }

    private void DivideSongsBetweenPlayers()
    {
        List<SongData> songsToUse = GetFilteredSongs();
        List<SongData> rnd = songsToUse.OrderBy(x => Random.value).ToList();

        if (rnd.Count < cardsPerPlayer * 2)
            cardsPerPlayer = Mathf.Max(1, rnd.Count / 2);

        player1Deck = rnd.GetRange(0, cardsPerPlayer);
        player2Deck = rnd.GetRange(cardsPerPlayer, cardsPerPlayer);
    }

    private void InitializeDropZones()
    {
        var s1 = player1Deck.OrderBy(s => s.year).ToList();
        var s2 = player2Deck.OrderBy(s => s.year).ToList();

        // --- INITIALISATION JOUEUR 1 ---
        for (int i = 0; i < player1DropZones.Count; i++)
        {
            if (i < s1.Count)
            {
                player1DropZones[i].Initialize(s1[i].year, 0, i);
                player1DropZones[i].gameObject.SetActive(true);
            }
            else
            {
                player1DropZones[i].gameObject.SetActive(false); // Désactive les zones en trop
            }
        }

        // --- INITIALISATION JOUEUR 2 ---
        for (int i = 0; i < player2DropZones.Count; i++)
        {
            if (i < s2.Count)
            {
                player2DropZones[i].Initialize(s2[i].year, 1, i);
                player2DropZones[i].gameObject.SetActive(true);
            }
            else
            {
                player2DropZones[i].gameObject.SetActive(false); // Désactive les zones en trop
            }
        }

        // --- CREATION DES PIOCHES CORRESPONDANTES AUX DATES ---
        player1DrawPile.Clear();
        player2DrawPile.Clear();

        // On crée une liste temporaire pour ne pas donner la même chanson aux deux joueurs
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
                else
                {
                    Debug.LogError($"Il manque une chanson pour l'année {zone.targetYear} pour le Joueur 1 !");
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
                else
                {
                    Debug.LogError($"Il manque une chanson pour l'année {zone.targetYear} pour le Joueur 2 !");
                }
            }
        }

        player1Deck.Clear();
        player2Deck.Clear();
    }

    // Pioche une carte STRICTEMENT parmi celles correspondant aux dropzones du joueur
    public void DrawCardForPlayer(int playerIndex)
    {
        UpdateTurnUI(playerIndex);

        List<SongData> currentPile = (playerIndex == 0) ? player1DrawPile : player2DrawPile;

        if (currentPile.Count == 0)
        {
            Debug.LogWarning("La pioche est vide !");
            return;
        }

        int randomIndex = Random.Range(0, currentPile.Count);
        SongData songToDraw = currentPile[randomIndex];
        currentPile.RemoveAt(randomIndex); // Retire la carte pour ne pas la redonner

        GameObject cardGO = Instantiate(cardButtonPrefab, cardDeckParent);
        var cb = cardGO.GetComponent<CardButton>();
        cb.SetCard(songToDraw, true, false);
        cb.SetPlayerIndex(playerIndex);
    }

    // Remet une carte échouée dans la pioche du joueur
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