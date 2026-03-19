using UnityEngine;

[CreateAssetMenu(fileName = "New Song", menuName = "Game/Song Data")]
public class SongData : ScriptableObject
{
    public string title;
    public string artist;
    public int year;
    public AudioClip audioClip;
    public Sprite cardSprite; // Image de la carte
}
