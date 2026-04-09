using UnityEngine;
using System.Collections.Generic;

public enum MusicGenre { Pop, Rock, Rap, Jazz, Electro, All }

[CreateAssetMenu(fileName = "New Song", menuName = "Game/Song Data")]
public class SongData : ScriptableObject
{
    public string title;
    public string artist;
    public int year;

    [Header("Audio (Hybrid System)")]
    [Tooltip("Nom du fichier MIDI dans StreamingAssets (ex: hit.mid)")]
    public string midiFileName;

    [Tooltip("Fichier MP3/WAV à utiliser si le MIDI est absent")]
    public AudioClip previewAudio;

    [Header("Visuals")]
    public Sprite cardSprite;
    public List<MusicGenre> genres;
}