using UnityEngine;
using System.Collections.Generic; // NOUVEAU : Nécessaire pour utiliser les Listes

public enum MusicGenre { Pop, Rock, Rap, Jazz, Electro, All }

[CreateAssetMenu(fileName = "New Song", menuName = "Game/Song Data")]
public class SongData : ScriptableObject
{
    public string title;
    public string artist;
    public int year;
    public AudioClip audioClip;
    public Sprite cardSprite;

    // NOUVEAU : Une liste pour cocher plusieurs genres dans l'inspecteur
    public List<MusicGenre> genres;
}