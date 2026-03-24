using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void SelectGenreAndStart(int genreIndex)
    {
        // On convertit l'index du Dropdown en Enum
        GameSettings.SelectedGenre = (MusicGenre)genreIndex;

        // Charger la scène de jeu
        SceneManager.LoadScene("NomDeTaSceneDeJeu");
    }
}