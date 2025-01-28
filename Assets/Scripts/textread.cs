using UnityEngine;
using System.IO;
using System.Linq;  
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using TMPro;
using System.Linq;

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
    private List<GameObject> circles = new List<GameObject>();
    private List<GameObject> lines = new List<GameObject>();


    // BPM
    private float BPMMinitTime = 60f;
    private bool bpm = false;
    private List<int> BPMvalues = new List<int>();

    public TMP_InputField bpmInput; 
    private string bpmText;


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

            if (valeurs.Length >= 11)
            {
                // Extraire les valeurs de A1 et A2 (en indexant correctement)
                string bpmData = valeurs[5]; // Colonne A1 (Index 5)
                string respiration = valeurs[6]; // Colonne A2 (Index 6)
                
                BPMvalues.Add(int.Parse(bpmData));
                if(bpm){
                    BPMvalues.RemoveAt(0);
                    int maxValue = BPMvalues.Max();
                    int count = BPMvalues.Count(n => n >= (maxValue - 50));
                    bpmText = (count/60).ToString();
                }

                //Respiration
                // int randomValue = Random.Range(0, 1001);
                // toPlot.Add(randomValue);
                
                toPlot.Add(int.Parse(valeurs[6]));
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
                string bpmData = valeurs[5]; // Colonne A1 (Index 5)
                string respiration = valeurs[6]; // Colonne A2 (Index 6)


                BPMvalues.Add(int.Parse(bpmData));
                if(bpm){
                    BPMvalues.RemoveAt(0);
                    int maxValue = BPMvalues.Max();
                    int count = BPMvalues.Count(n => n >= (maxValue - 50));
                    bpmText = (count/60).ToString();
                }


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
    

    private GameObject CreateCircle(Vector2 anchoredPosition){
        GameObject gameObj = new GameObject("circle", typeof(Image));
        circles.Add(gameObj);
        gameObj.transform.SetParent(graphContainer, false);
        gameObj.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(5, 5);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObj;
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

        GameObject lastCircleGameObject = null;
        for(int i = 0; i < valueList.Count; i++){
            float xPosition = (i+1)*11 ;
            float yPosition = (valueList[i] / yMaximum) * graphHeight;
            GameObject cGO = CreateCircle(new Vector2(xPosition, yPosition));
            if(lastCircleGameObject != null){
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, cGO.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObject = cGO;

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
        rectTransform.sizeDelta = new Vector2(distance, 3f);
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

