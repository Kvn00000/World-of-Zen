using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;

public class BitalinoScript : MonoBehaviour
{
    // Class Variables
    private PluxDeviceManager pluxDevManager;

    // Class constants (CAN BE EDITED BY IN ACCORDANCE TO THE DESIRED DEVICE CONFIGURATIONS)
    [System.NonSerialized]
    public List<string> domains = new List<string>() { "BTH" };
    public string deviceMacAddress = "BTH20:19:07:00:80:B3"; // changer adresse mac
    public int samplingRate = 100;
    public int resolution = 10;
    //public PlayerModel playerModel;
    private int Hybrid8PID = 517;
    private int BiosignalspluxPID = 513;
    private int BitalinoPID = 1538;
    private int MuscleBanPID = 1282;
    private int MuscleBanNewPID = 2049;
    private int CardioBanPID = 2050;
    private int BiosignalspluxSoloPID = 532;
    private int MaxLedIntensity = 255;

    private bool isScanFinished = false;
    private bool isScanning = false;
    private bool isConnectionDone = false;
    private bool isConnecting = false;
    private bool isAcquisitionStarted = false;


    private float start =0f;
    // Lecture de fichier texte
    private float derniereLecture = 0f;
    private float intervalleLecture = 0.001f; // Lire toutes les 1 secondes (ou ajustez à votre besoin)

    // Respiration
    private float respirationInitTime = 3f;
    private bool resp = false;



    string filePath = "./Assets/Data/data.txt";

    List<int> toPlot = new List<int>();
    private List<GameObject> circles;
    private List<GameObject> lines;



    private float BPMMinitTime = 60f;
    private bool bpm = false;
    private List<int> BPMvalues = new List<int>(3000);

    public TextMeshProUGUI bpmInput; 
    private string bpmText;




    // Start is called before the first frame update
    private void Start()
    {
        // Initialise object
        pluxDevManager = new PluxDeviceManager(ScanResults, ConnectionDone, AcquisitionStarted, OnDataReceived, OnEventDetected, OnExceptionRaised);

        // Important call for UnityEngine.Debug purposes by creating a log file in the root directory of the project.
        pluxDevManager.WelcomeFunctionUnity();
    }

    // Update function, being constantly invoked by Unity.
    private void Update()
    { 
        if (isScanning || isConnecting || isAcquisitionStarted)
        {
            return;
        }

        if (!isScanFinished)
        {
            // Search for PLUX devices
            pluxDevManager.GetDetectableDevicesUnity(domains);
            isScanning = true;
            UnityEngine.Debug.Log("Scanning for devices...");
            return;
        }


        if (!isConnectionDone)
        {
            // Connect to the device selected in the Dropdown list.
            pluxDevManager.PluxDev(deviceMacAddress);
            UnityEngine.Debug.Log("Connecting to device " + deviceMacAddress);
            isConnecting = true;
            return;
        }

        if (!isAcquisitionStarted)
        {
            // Start the acquisition
            pluxDevManager.StartAcquisitionUnity(samplingRate, new List<int> {1,2}, resolution);
            return;
        }

    }

    // Method invoked when the application was closed.
    private void OnApplicationQuit()
    {

        // Disconnect from device.
        if (pluxDevManager != null)
        {
            pluxDevManager.DisconnectPluxDev();
            UnityEngine.Debug.Log("Application ending after " + Time.time + " seconds");
        }

    }

    /**
     * =================================================================================
     * ============================= GUI Events ========================================
     * =================================================================================
     */

    /**
     * =================================================================================
     * ============================= Callbacks =========================================
     * =================================================================================
     */

    // Callback that receives the list of PLUX devices found during the Bluetooth scan.
    public void ScanResults(List<string> listDevices)
    {

        if (listDevices.Count > 0)
        {

            isScanFinished = true;
            isScanning = false;
            // Show an informative message about the number of detected devices.
            UnityEngine.Debug.Log("Bluetooth device scan found: " + listDevices[0]);
            // deviceMacAddress = listDevices[0];
        }
        else
        {
            // Show an informative message stating the none devices were found.
            UnityEngine.Debug.Log("No devices were found. Please make sure the device is turned on and in range.");
            isScanning = false;
        }
    }

    // Callback invoked once the connection with a PLUX device was established.
    // connectionStatus -> A boolean flag stating if the connection was established with success (true) or not (false).
    public void ConnectionDone(bool connectionStatus)
    {
        if (connectionStatus)
        {
            isConnectionDone = true;
            isConnecting = false;
            UnityEngine.Debug.Log("Connexion réussie à l'appareil BITalino");

        }
        else
        {
            UnityEngine.Debug.Log("Erreur lors de la connexion à l'appareil");
            isConnecting = false;
        }
    }

    // Callback invoked once the data streaming between the PLUX device and the computer is started.
    // acquisitionStatus -> A boolean flag stating if the acquisition was started with success (true) or not (false).
    // exceptionRaised -> A boolean flag that identifies if an exception was raised and should be presented in the GUI (true) or not (false).
    public void AcquisitionStarted(bool acquisitionStatus, bool exceptionRaised = false, string exceptionMessage = "")
    {
        if (acquisitionStatus)
        {
            isAcquisitionStarted = true;
            File.WriteAllText(filePath, string.Empty);
            File.AppendAllText(filePath, "time bpm respiration\n");
            UnityEngine.Debug.Log("Acquisition démarrée avec succès");

            start = 0;
        }
        else
        {
            UnityEngine.Debug.Log("Erreur lors du démarrage de l'acquisition: " + exceptionMessage);
        }
    }

    // Callback invoked every time an exception is raised in the PLUX API Plugin.
    // exceptionCode -> ID number of the exception to be raised.
    // exceptionDescription -> Descriptive message about the exception.
    public void OnExceptionRaised(int exceptionCode, string exceptionDescription)
    {
        if (pluxDevManager.IsAcquisitionInProgress())
        {
            UnityEngine.Debug.Log("Exception raised: " + exceptionDescription);
        }
    }

    // Callback that receives the data acquired from the PLUX devices that are streaming real-time data.
    // nSeq -> Number of sequence identifying the number of the current package of data.
    // data -> Package of data containing the RAW data samples collected from each active channel ([sample_first_active_channel, sample_second_active_channel,...]).
    public void OnDataReceived(int nSeq, int[] data)
    {
        // Show samples with a 0.1s interval.
        //if (nSeq % samplingRate == 0){
        
        // Show the current package of data.
        start += 0.1f;
        string outputString = (start).ToString();
        UnityEngine.Debug.Log(data.Length);

        for (int j = 0; j < data.Length; j++)
        {
            outputString += " " + data[j]  ;
        }
        outputString+= "\n";
        UnityEngine.Debug.Log(outputString);

        File.AppendAllText(filePath, outputString);
        
        // using (StreamWriter writer = new StreamWriter(filePath))
        // {
        //     writer.WriteLine(outputString);
        // }
        // }
    
    }

    // Callback that receives the events raised from the PLUX devices that are streaming real-time data.
    // pluxEvent -> Event object raised by the PLUX API.
    public void OnEventDetected(PluxDeviceManager.PluxEvent pluxEvent)
    {
        if (pluxEvent is PluxDeviceManager.PluxDisconnectEvent)
        {
            // Present an error message.
            UnityEngine.Debug.Log("The device was disconnected. Please make sure the device is turned on and in range.");

            // Securely stop the real-time acquisition.
            pluxDevManager.StopAcquisitionUnity(-1);
        }
        else if (pluxEvent is PluxDeviceManager.PluxDigInUpdateEvent)
        {
            // PluxDeviceManager.PluxDigInUpdateEvent digInEvent = (pluxEvent as PluxDeviceManager.PluxDigInUpdateEvent);
            // UnityEngine.Debug.Log("Digital Input Update Event Detected on channel " + digInEvent.channel + ". Current state: " + digInEvent.state);
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
            float xPosition = (i+1)*11f ;
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