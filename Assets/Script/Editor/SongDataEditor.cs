using UnityEngine;
using UnityEditor;
using System.IO;

// Ce script ajoute des fonctionnalits  l'inspecteur de SongData
[CustomEditor(typeof(SongData))]
public class SongDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Rcupre l'objet SongData que tu es en train de modifier
        SongData songData = (SongData)target;

        // Affiche les champs normaux (Titre, Artiste, Annee...)
        DrawDefaultInspector();

        EditorGUILayout.Space(); // Un peu d'espace
        EditorGUILayout.LabelField("Configuration Audio MIDI", EditorStyles.boldLabel);

        // BOUTON MAGIQUE
        if (GUILayout.Button("Slectionner fichier MIDI (.mid)"))
        {
            // Ouvre la fentre de recherche de fichier de Windows/Mac
            string path = EditorUtility.OpenFilePanel("Choisir un fichier MIDI", "", "mid");

            if (!string.IsNullOrEmpty(path))
            {
                // Rcupre uniquement le nom du fichier (ex: "musique")
                string fileName = Path.GetFileNameWithoutExtension(path);

                // crit le nom automatiquement dans ton SongData
                songData.midiFileName = fileName;

                // Sauvegarde les changements
                EditorUtility.SetDirty(songData);

                Debug.Log($"[SongData] Fichier MIDI slectionn : {fileName}");
            }
        }

        // Affichage d'une aide
        EditorGUILayout.HelpBox("Assure-toi que tes fichiers .mid sont dans le dossier 'MidiPlayer/Resources/MidiDB' et qu'ils ont t ajouts dans 'Midi File Setup' (Menu MPTK).", MessageType.Info);
    }
}