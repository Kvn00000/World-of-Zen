using UnityEngine;
using System.IO;
using System.Linq;  


public class LireFichierTexte : MonoBehaviour
{

    private float derniereLecture = 0f;
    private float intervalleLecture = 1f; // Lire toutes les 1 secondes (ou ajustez à votre besoin)

    string cheminFichier;
    void Start()
    {
        // Spécifiez le chemin du dossier contenant le fichier .txt
        string dossier = "/Users/claire/Documents/OpenSignals (r)evolution/temp"; 
        // Lire le seul fichier .txt dans ce dossier
        // cheminFichier = TrouverFichierTexte(dossier);
    }

    void Update()
    {
        if (Time.time - derniereLecture > intervalleLecture)
        {
            derniereLecture = Time.time;
            // LireDerniereLigne(cheminFichier);
        }
    }


    string TrouverFichierTexte(string dossier)
    {
        // Récupérer tous les fichiers .txt dans le dossier spécifié
        string[] fichiers = Directory.GetFiles(dossier, "*.txt");

        if (fichiers.Length > 0)
        {
            // Trier les fichiers par date de modification (du plus récent au plus ancien)
            string fichierRecente = fichiers
                .OrderByDescending(f => new FileInfo(f).LastWriteTime)  // Utiliser LINQ pour trier
                .First();  // Récupérer le premier fichier (le plus récent)

            // Afficher le fichier le plus récent dans la console
            Debug.Log("Fichier le plus récent : " + fichierRecente);

            return fichierRecente; // Retourner le fichier le plus récent
        }
        else
        {
            // Si aucun fichier n'est trouvé, retourner null
            Debug.LogError("Aucun fichier .txt trouvé dans le dossier.");
            return null;
        }
    }


    void LireDerniereLigne(string cheminFichier)
    {
        string[] lignes = File.ReadAllLines(cheminFichier); // Lire toutes les lignes du fichier

        if (lignes.Length > 0)
        {
            string derniereLigne = lignes[lignes.Length - 1]; // Récupérer la dernière ligne

            // Analyser la ligne et extraire les valeurs des colonnes A1 et A2
            string[] valeurs = derniereLigne.Split('\t');

            if (valeurs.Length >= 11)
            {
                // Extraire les valeurs de A1 et A2 (en indexant correctement)
                string bpm = valeurs[5]; // Colonne A1 (Index 5)
                string respiration = valeurs[6]; // Colonne A2 (Index 6)

                // Afficher les valeurs dans la console
                Debug.Log("BPM (A1): " + bpm);
                Debug.Log("Respiration (A2): " + respiration);
            }
            else
            {//Sinon récupérer l'avant derniere ligne
                derniereLigne = lignes[lignes.Length - 2];

                // Analyser la ligne et extraire les valeurs des colonnes A1 et A2
                valeurs = derniereLigne.Split('\t');

                // Extraire les valeurs de A1 et A2 (en indexant correctement)
                string bpm = valeurs[5]; // Colonne A1 (Index 5)
                string respiration = valeurs[6]; // Colonne A2 (Index 6)

                // Afficher les valeurs dans la console
                Debug.Log("BPM (A1): " + bpm);
                Debug.Log("Respiration (A2): " + respiration);

            }
        }
        else
        {
            Debug.LogError("Le fichier est vide.");
        }
    }
}

