using UnityEngine;

public enum MusicGenre { Pop, Rock, Rap, Jazz, Electro, All }

[CreateAssetMenu(fileName = "New Song", menuName = "Game/Song Data")]
public class SongData : ScriptableObject
{
    public string title;
    public string artist;
    public int year;
    public AudioClip audioClip;
    public Sprite cardSprite;
    public MusicGenre genre; // Nouveau : permet de choisir le genre dans l'inspecteur
}