using UnityEngine;
using System.IO;
using System.Linq;  
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class LireFichierTexte : MonoBehaviour
{
    private float start =0f;
    // Lecture de fichier texte
    private float derniereLecture = 0f;
    private float intervalleLecture = 0.01f; // Lire toutes les 1 secondes (ou ajustez à votre besoin)
    private float deltaX = 1f;
    private float xInit = 165f;
    // Respiration
    private float respirationInitTime = 3f;
    private bool resp = false;
    List<int> toPlot = new List<int>();
    private List<GameObject> circles;
    private List<GameObject> lines;


    // BPM
    private float BPMMinitTime = 20f;
    private bool bpm = false;
    private List<int> BPMvalues = new List<int>(3000);

    public TextMeshProUGUI bpmInput; 
    private string bpmText;


    string cheminFichier;

    private void Awake()
    {
        circles = new List<GameObject>(3000);
        lines = new List<GameObject>(3000);
        
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
        // string dossier = "/Users/claire/Documents/OpenSignals (r)evolution/temp"; 
        string dossier = "./Assets/Scripts"; // Dossier du projet Unity
        // Lire le seul fichier .txt dans ce dossier
        cheminFichier = TrouverFichierTexte(dossier);
    }

    void Update()
    {
        if (Time.time - derniereLecture > intervalleLecture)
        {
            derniereLecture = Time.time;
            LireDerniereLigne(cheminFichier);
            if(bpm){
                bpmInput.text = bpmText;
            }
        }

        if (Time.time - start > respirationInitTime){
            resp = true;
        }

        if(Time.time - start > BPMMinitTime){
            bpm = true;
        }

        if(toPlot.Count > 0){
                ShowGraph(toPlot);
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

            if (valeurs.Length < 11){
                derniereLigne = lignes[lignes.Length - 2];
                valeurs = derniereLigne.Split('\t');
            }
                string respiration = valeurs[6]; // Colonne A2 (Index 6) => respiration
                
                if(bpm){
                    var data = File.ReadLines(cheminFichier)
                            .Where(line => !line.StartsWith("#")) // Ignore l'en-tête
                            .ToList();

                    // Sélectionner les 125*15 dernières lignes
                    var BPMdernieresLignes = data.Skip(Math.Max(0, data.Count - (125*15))).ToList();

                    List<int> signalCardiaque = BPMdernieresLignes
                        .Select(line => line.Split('\t')) // Séparer les colonnes
                        .Where(parts => parts.Length > 5) // Vérifier qu'on a bien assez de colonnes
                        .Select(parts => int.Parse(parts[5])) // Récupérer la valeur de la colonne A1
                        .ToList();

                    // Détecter les pics du signal cardiaque
                    int battements = 0;
                    for (int i = 1; i < signalCardiaque.Count - 1; i++)
                    {
                        if (signalCardiaque[i] >= signalCardiaque[i - 1] && signalCardiaque[i] > signalCardiaque[i + 1])
                        {
                            battements++; // Compter le pic
                        }
                    }

                    // Calcul du BPM (fréquence cardiaque)
                    int bpmvalue = battements;
                    bpmText = bpmvalue.ToString();
                }


                //Respiration
                int randomValue = UnityEngine.Random.Range(0, 1001);
                toPlot.Add(randomValue);

                // toPlot.Add(int.Parse(valeurs[6]));
                if(resp){
                    toPlot.RemoveAt(0);
                }

                // Afficher les valeurs dans la console
                // Debug.Log("BPM (A1): " + bpm);
                // Debug.Log("Respiration (A2): " + respiration);
            
        }
        else
        {
            Debug.LogError("Le fichier est vide.");
        }
    }



    [SerializeField] private Sprite circleSprite;
    private RectTransform graphContainer;
    

    private GameObject CreateCircle(Vector2 anchoredPosition){
        GameObject gameObj = new GameObject("circle", typeof(Image));
        circles.Add(gameObj);
        gameObj.transform.SetParent(graphContainer, false);
        gameObj.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(2, 2);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObj;
    }

    private void shiftPoints(List<GameObject> points){
        for(int i = 0; i < points.Count; i++){
            points[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(points[i].GetComponent<RectTransform>().anchoredPosition.x - deltaX, points[i].GetComponent<RectTransform>().anchoredPosition.y);
        }
    }


    private void ShowGraph(List<int> valueList){
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = 1000f;

        for(int i = 0; i < lines.Count; i++){
            Destroy(lines[i]);
        }
        lines.Clear();

        for(int i = 0; i < circles.Count; i++){
            Destroy(circles[i]);
        }
        circles.Clear();

        // shiftPoints(circles);


        GameObject lastCircleGameObject = null;
        for(int i = 0; i < valueList.Count; i++){
            
                float xPosition = (i+1)*deltaX ;
                float yPosition = (valueList[i] / yMaximum) * graphHeight;
                GameObject cGO = CreateCircle(new Vector2(xPosition, yPosition));
                if(lastCircleGameObject != null){
                    CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, cGO.GetComponent<RectTransform>().anchoredPosition);
                }
                lastCircleGameObject = cGO;
            

        }
    }


    private void ShowGraph2(int value){
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = 1000f;

        // for(int i = 0; i < lines.Count; i++){
        //     Destroy(lines[i]);
        // }
        // lines.Clear();

        // for(int i = 0; i < circles.Count; i++){
        //     Destroy(circles[i]);
        // }
        // circles.Clear();
        if(circles.Count > 15){
            GameObject toDestroy = circles[0];
            circles.RemoveAt(0);
            Destroy(toDestroy);
        }
        shiftPoints(circles);


        if(circles.Count == 0){
            float xPosition = xInit;
            float yPosition = (value / yMaximum) * graphHeight;
            GameObject cGO = CreateCircle(new Vector2(xPosition, yPosition));
        }else{

            GameObject lastCircleGameObject = circles[circles.Count - 1];


            float xPosition = lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition.x + deltaX;
            float yPosition = (value / yMaximum) * graphHeight;
            GameObject cGO = CreateCircle(new Vector2(xPosition, yPosition));

            CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, cGO.GetComponent<RectTransform>().anchoredPosition);
        }

        
    }

    private void CreateDotConnection(Vector2 dotPosA, Vector2 dotPosB){
        GameObject gameObj = new GameObject("dotConnection", typeof(Image));
        lines.Add(gameObj);
        gameObj.transform.SetParent(graphContainer, false);
        gameObj.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        RectTransform rectTransform = gameObj.GetComponent<RectTransform>();
        Vector2 dir = (dotPosB - dotPosA).normalized;
        float distance = Vector2.Distance(dotPosA, dotPosB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 1f);
        rectTransform.anchoredPosition = dotPosA + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(dir));
    }

    private float GetAngleFromVectorFloat(Vector2 dir){
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if(n < 0){
            n += 360;
        }
        return n;
    }


    
}

