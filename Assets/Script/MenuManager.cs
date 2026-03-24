using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Cette fonction sera visible dans ton bouton Unity
    // Elle demande un chiffre (le genre) et un texte (le nom de la scène)
    public void ChoisirGenreEtLancer(int genreIndex)
    {
        // 1. On enregistre le genre dans le "pont" statique
        GameSettings.SelectedGenre = (MusicGenre)genreIndex;

        // 2. On récupère le nom de la scène active pour la recharger 
        // OU on peut utiliser une variable si tu veux charger une scène précise.
        // Pour faire simple, on va charger la scène suivante dans le Build
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // VERSION AVEC NOM DE SCENE MANUEL (La plus sûre pour toi)
    public void LancerSceneParNom(string nomDeLaScene)
    {
        SceneManager.LoadScene(nomDeLaScene);
    }
}