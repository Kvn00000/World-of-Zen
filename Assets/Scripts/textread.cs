using UnityEngine;
using System.IO;
using System.Linq;  
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;


public class LireFichierTexte : MonoBehaviour
{
    private float start =0f;
    // Lecture de fichier texte
    private float derniereLecture = 0f;
    private float intervalleLecture = 0.2f; // Lire toutes les 1 secondes (ou ajustez à votre besoin)

    // Respiration
    private float respirationInitTime = 3f;
    private bool resp = false;
    List<int> toPlot = new List<int>();
    List<int> xAxis = new List<int>();
    List<GameObject> circles = new List<GameObject>();
    private int count = 0;
    string cheminFichier;

    private void Awake()
    {
        graphContainer = transform.Find("Container").GetComponent<RectTransform>();
        // for (int i = 0; i < 15; i++)
        // {
        //     xAxis.Add((i+1)*11);
        // }
    }

    void Start()
    {
        start = Time.time;
        // Spécifiez le chemin du dossier contenant le fichier .txt
        string dossier = "./Assets/test"; 
        // Lire le seul fichier .txt dans ce dossier
        cheminFichier = TrouverFichierTexte(dossier);
    }

    void Update()
    {
        if (Time.time - derniereLecture > intervalleLecture)
        {
            derniereLecture = Time.time;
            LireDerniereLigne(cheminFichier);
        }

        if (Time.time - start > respirationInitTime){
            resp = true;
        }
        if(toPlot.Count > 0){
            if (count > 15){

            }else{

                ShowGraph(toPlot);
            }
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
                toPlot.Add(int.Parse(valeurs[6]));
                count++;
                if(resp){
                    toPlot.RemoveAt(0);
                }


                // Afficher les valeurs dans la console
                // Debug.Log("BPM (A1): " + bpm);
                // Debug.Log("Respiration (A2): " + respiration);
            }
            else
            {//Sinon récupérer l'avant derniere ligne
                derniereLigne = lignes[lignes.Length - 2];

                // Analyser la ligne et extraire les valeurs des colonnes A1 et A2
                valeurs = derniereLigne.Split('\t');

                // Extraire les valeurs de A1 et A2 (en indexant correctement)
                string bpm = valeurs[5]; // Colonne A1 (Index 5)
                string respiration = valeurs[6]; // Colonne A2 (Index 6)

                toPlot.Add(int.Parse(valeurs[6]));
                if(resp){
                    toPlot.RemoveAt(0);
                    circles.RemoveAt(0);
                }


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



    [SerializeField] private Sprite circleSprite;
    private RectTransform graphContainer;
    

    private void CreateCircle(Vector2 anchoredPosition){
        GameObject gameObj = new GameObject("circle", typeof(Image));
        circles.Add(gameObj);
        gameObj.transform.SetParent(graphContainer, false);
        gameObj.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
    }

    private void ShowGraph(List<int> valueList){
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = 100f;
        for(int i = 0; i < circles.Count; i++){
            Destroy(circles[i]);
            circles.RemoveAt(i);
        }

        for(int i = 0; i < valueList.Count; i++){
            float xPosition = (i+1)*11 ;
            float yPosition = (valueList[i] / yMaximum) * graphHeight;
            CreateCircle(new Vector2(xPosition, yPosition));

        }
    }
    
}

