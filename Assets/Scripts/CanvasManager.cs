using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;


public class CanvasManager : MonoBehaviour
{
    List<float> devoilementRates = new List<float>
    {
        0.8f, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f,
        0.75f, 0.75f, 0.75f, 0.75f, 0.75f, 0.75f, 0.75f, 0.75f, 0.75f, 0.75f,
        0.7f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f,
        0.65f, 0.65f, 0.65f, 0.65f, 0.65f, 0.65f, 0.65f, 0.65f, 0.65f, 0.65f,
        0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f,
        0.55f, 0.55f, 0.55f, 0.55f, 0.55f, 0.55f, 0.55f, 0.55f, 0.55f, 0.55f,
        0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f,
        0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f,
        0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f,
        0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f
    };
    
        List<float> revertRates = new List<float>
        {
            0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f,
            0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f,
            0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f,
            0.01f, 0.01f, 0.02f, 0.02f, 0.03f, 0.03f, 0.03f, 0.04f, 0.04f, 0.05f,
            0.05f, 0.06f, 0.06f, 0.06f, 0.07f, 0.07f, 0.08f, 0.08f, 0.08f, 0.09f,
            0.09f, 0.10f, 0.10f, 0.11f, 0.11f, 0.11f, 0.12f, 0.12f, 0.13f, 0.13f,
            0.13f, 0.14f, 0.14f, 0.15f, 0.15f, 0.15f, 0.16f, 0.16f, 0.17f, 0.17f,
            0.18f, 0.18f, 0.18f, 0.19f, 0.19f, 0.20f, 0.20f, 0.20f, 0.21f, 0.21f,
            0.22f, 0.22f, 0.23f, 0.23f, 0.23f, 0.24f, 0.24f, 0.25f, 0.25f, 0.25f,
            0.26f, 0.26f, 0.27f, 0.27f, 0.28f, 0.28f, 0.28f, 0.29f, 0.29f, 0.30f,
            0.30f
        };
    
    private float accelerationRate;

    [Header("Layers")]
    public LayerMask doorLayer;
    public LayerMask paintLayer;

    [Header("Canvas")]
    public GameObject interiorCanvas;
    public GameObject exteriorCanvas;
    public GameObject doorCanvas; 
    public GameObject paintCanvas;
    public GameObject PictureMenu;
    public GameObject ExerciceCanvas;
    public GameObject MainCamera;
    private MouseControl mouseControl;

    [Header("Transform Position")]
    public Transform insidePos;
    public Transform outsidePos;
    public Transform mainCam; // The camera position to cast the ray from
    

    
    public GameObject player;
    public float pointerDistance = 3f; // Raycast distance to detect doors


    private bool doorDetection; // Whether the raycast hits the door
    private bool paintDetection; // Whether the raycast hits the paint

    private PlayerMovement playerMovement; // Référence au script PlayerMovement


    private bool firstTime = true;
    private bool inside = false;

    public Slider pictureSlider;
    public TextMeshProUGUI sliderValue;


    
    private string configPath = "Assets/UserConfig/config.json"; // JSON 配置文件路径
    private string pythonPath; // 存储 Python 解释器路径

    private string resultsFilePath = "Assets/Data/transformed/breathing_success_data.txt";
    private FileSystemWatcher fileWatcher;
    public TextMeshProUGUI resultsText;
    public TextMeshProUGUI exerciceStep;
    private Process breathingProcess;

    private bool isNewResultAvailable = false; // 标志是否有新结果

    private string latestResultText;  // 变量存储最新的结果

    private string exerciceStepText;

    private int difficulte = 20;

    private float devoilement_rate_tableau = 0;

    private bool inGame = false;
    private int lastNumber = 0; // 记录上一次的 number 值
    private float devoilementRate;
    private float revertRate;
    private float respSuccessRate = 0;
    private bool acceleration_mode = false;
    private int accumulated_success_cycle = 0;

    private int quantile = 0;
    public AudioSource exerciceMusic;

    public AudioSource quatre_s;
    public AudioSource sept_s;
    public AudioSource huit_s;

    void Start()
    {
        // Afficher le Canvas et désactiver le mouvement du joueur au début
        exteriorCanvas.SetActive(true);
        playerMovement = GetComponent<PlayerMovement>();
        playerMovement.canMove = false;
        pictureSlider.onValueChanged.AddListener(UpdateSliderValue);
        mouseControl = MainCamera.GetComponent<MouseControl>();
    }

    void LoadConfig()
    {
        if (File.Exists(configPath))
        {
            string jsonText = File.ReadAllText(configPath);
            ConfigData config = JsonUtility.FromJson<ConfigData>(jsonText);
            pythonPath = config.python_path;
        }
        else
        {
            UnityEngine.Debug.LogError("配置文件 " + configPath + " 不存在，请检查！");
        }
    }

    void ChangeDifficulte()
{
    devoilementRate = devoilementRates[difficulte];
    revertRate = revertRates[difficulte];
    
    UnityEngine.Debug.Log($" 更新难度到 {difficulte}，新的 acceleration_rate: {accelerationRate}");
    UnityEngine.Debug.Log($" 新的 devoilementRate: {devoilementRate}");
    UnityEngine.Debug.Log($" 新的 revertRate: {revertRate}");
}


    void UpdateSliderValue(float value)
    {
        sliderValue.text = value.ToString("0");
    }

    void Update()
    {
        
        doorDetection = Physics.Raycast(mainCam.position, mainCam.forward, pointerDistance, doorLayer);
        paintDetection = Physics.Raycast(mainCam.position, mainCam.forward, pointerDistance, paintLayer);

        UnityEngine.Debug.DrawRay(mainCam.position, mainCam.forward * pointerDistance, doorDetection ? Color.green : Color.red);


        // Vérifier si la touche Entrée est pressée
        if (exteriorCanvas.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            CloseCanvas(exteriorCanvas);
        }

        if (interiorCanvas.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            CloseCanvas(interiorCanvas);
        }

        if (inGame && isNewResultAvailable) // 如果有新数据
        {
            resultsText.text = latestResultText; // 更新 UI
            exerciceStep.text = exerciceStepText;
            
            isNewResultAvailable = false; // 重置标记
            if (devoilement_rate_tableau >= 1)
            {
                inGame = false;
                StopExercice();
                resultsText.text = "Bravo ! Vous avez terminé l'exercice !"; 
            }
            AdjustOpacity();
        }


        // Display the Canvas when the door is detected and press 'E' to change the scene
        if (doorDetection)
        {
            // Show the interaction Canvas
            if (!doorCanvas.activeSelf)
            {
                doorCanvas.SetActive(true);
            }

            // Check if the player presses 'E' to change the scene
            if (Input.GetKeyDown(KeyCode.E) && doorCanvas.activeSelf)
            {
                if(inside){
                    teleport(outsidePos);
                    inside = false;
                }else{
                    teleport(insidePos);
                    inside = true;
                }
                doorCanvas.SetActive(false); // Hide the Canvas once the scene is changed
            }
        }
        else
        {
            // Hide the interaction Canvas when the player is not in front of the door
            if (doorCanvas.activeSelf)
            {
                doorCanvas.SetActive(false);
            }
        }



        if (paintDetection)
        {
            // Show the interaction Canvas
            if (!paintCanvas.activeSelf && playerMovement.canMove)
            {
                paintCanvas.SetActive(true);
            }

            // Check if the player presses 'E' to change the scene
            if (Input.GetKeyDown(KeyCode.E) && paintCanvas.activeSelf)
            {
                mouseControl.playerRotation = false;
                playerMovement.canMove = false;
                
                PictureMenu.SetActive(true);
                //Turn on the respiration game
                paintCanvas.SetActive(false); // Hide the Canvas once the scene is changed
            }
        }
        else
        {
            // Hide the interaction Canvas when the player is not in front of the door
            if (paintCanvas.activeSelf)
            {
                paintCanvas.SetActive(false);
            }
        }


    }
    
    public void AdjustOpacity()
{
    float devoilement_rate_to_use = devoilementRate;
    UnityEngine.Debug.Log("Initial devoilementRate: " + devoilementRate);
    if (devoilement_rate_tableau < 25)
    {
        quantile = 1;
    }
    else if (devoilement_rate_tableau < 50)
    {
        quantile = 2;
        devoilement_rate_to_use = Math.Max(0.05f, devoilementRate - 0.02f);
    }
    else if (devoilement_rate_tableau < 75)
    {
        quantile = 3;
        devoilement_rate_to_use = Math.Max(0.05f, devoilementRate - 0.05f);
    }
    else
    {
        quantile = 4;
        devoilement_rate_to_use = Math.Max(0.05f, devoilementRate - 0.10f);
    }
    
    UnityEngine.Debug.Log("Quantile set to: " + quantile);
    UnityEngine.Debug.Log("Devoilement rate to use: " + devoilement_rate_to_use);
    
    if (respSuccessRate > 50)
    {
        float percentage_to_devoile = (respSuccessRate - 50) * 2 * 0.01f;
        float rate_to_devoile = percentage_to_devoile * devoilement_rate_to_use;
        devoilement_rate_tableau += rate_to_devoile;
        
        UnityEngine.Debug.Log("Response success rate > 50, increasing devoilement_rate_tableau by: " + rate_to_devoile);
        UnityEngine.Debug.Log("Updated devoilement_rate_tableau: " + devoilement_rate_tableau);
        
        accumulated_success_cycle += 1;
        UnityEngine.Debug.Log("Accumulated success cycle: " + accumulated_success_cycle);
        
        if (accumulated_success_cycle == 3)
        {
            acceleration_mode = true;
            devoilementRate = Math.Min(1, devoilementRate * 2);
            UnityEngine.Debug.Log("Acceleration mode activated! devoilementRate doubled: " + devoilementRate);
            
            GameObject panel = ExerciceCanvas.transform.Find("Panel").gameObject;
            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(1f, 0.84f, 0f, 1f);
            UnityEngine.Debug.Log("Panel color changed to yellow");
        }
    }
    else
    {
        float percentage_to_revert = (50 - respSuccessRate) * 2 * 0.01f;
        float rate_to_revert = percentage_to_revert * revertRate;
        devoilement_rate_tableau = Math.Max(0, devoilement_rate_tableau - rate_to_revert);
        
        UnityEngine.Debug.Log("Response success rate <= 50, decreasing devoilement_rate_tableau by: " + rate_to_revert);
        UnityEngine.Debug.Log("Updated devoilement_rate_tableau: " + devoilement_rate_tableau);
        
        accumulated_success_cycle = 0;
        acceleration_mode = false;
        devoilementRate = devoilementRates[difficulte];
        UnityEngine.Debug.Log("Acceleration mode deactivated. Resetting devoilementRate to difficulty setting: " + devoilementRate);
        
        GameObject panel = ExerciceCanvas.transform.Find("Panel").gameObject;
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(1f, 1f, 1f, 1f);
        UnityEngine.Debug.Log("Panel color reset to white");
    }
    
    UnityEngine.Debug.Log("Setting opacity to: " + devoilement_rate_tableau);
    setOpacity(devoilement_rate_tableau);
}

    public void CloseCanvas(GameObject canv)
    {
        // Désactiver le Canvas et activer le mouvement du joueur
        canv.SetActive(false);
        playerMovement.canMove = true;
        mouseControl.playerRotation = true;
        CloseGame();
    }

    private void teleport(Transform pos)
    {
        player.transform.position = pos.position;
        if(firstTime == true){
            interiorCanvas.SetActive(true);
            playerMovement.canMove = false;

            firstTime = false;
        }
    }

public void StartGame()
    {
        PictureMenu.SetActive(false);
        ExerciceCanvas.SetActive(true);
        setOpacity(0);
        ChangeDifficulte();
        StartFileWatcher();
        StartBreathingGame();

        if (exerciceMusic != null)
        {
            exerciceMusic.Play(); // Lance la musique
        }
    }

private void setOpacity(float opacity)
{
    // 确保获取的是 `ExerciceCanvas` 下 `Panel/Image` 的 `Image` 组件
    Transform imageTransform = ExerciceCanvas.transform.Find("Panel/Image");

    if (imageTransform != null)
    {
        Image img = imageTransform.GetComponent<Image>();
        if (img != null)
        {
            Color color = img.color;
            color.a = opacity; // 设置透明度
            img.color = color;
            devoilement_rate_tableau = opacity;
            UnityEngine.Debug.Log($"成功修改 Image 透明度: {opacity}");
        }
        else
        {
            UnityEngine.Debug.LogWarning("⚠️ 找到了 `Panel/Image`，但没有 `Image` 组件！");
        }
    }
    else
    {
        UnityEngine.Debug.LogWarning("⚠️ `ExerciceCanvas` 下找不到 `Panel/Image`，请检查层级结构！");
    }
}

void StartFileWatcher()
    {
        if (!File.Exists(resultsFilePath))
        {
            File.WriteAllText(resultsFilePath, "number\tcycle\tlong\ttype\tsuccessrate\n");
        }

        fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(resultsFilePath))
        {
            Filter = Path.GetFileName(resultsFilePath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        fileWatcher.Changed += OnFileChanged;
        fileWatcher.EnableRaisingEvents = true;

        UnityEngine.Debug.Log("FileSystemWatcher started. Watching: " + resultsFilePath);
    }

private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        UnityEngine.Debug.Log($"File change detected: {e.FullPath}, ChangeType: {e.ChangeType}");
        ReadLastResult();
    }

    void ReadLastResult()
{
    UnityEngine.Debug.Log("Reading last line from file...");

    try
    {
        string[] lines = File.ReadAllLines(resultsFilePath);
        if (lines.Length < 2)
        {
            UnityEngine.Debug.Log(" No valid data found in the file.");
            return;
        }

        string lastResult = lines[lines.Length - 1]; // 读取最后一行
        UnityEngine.Debug.Log($" Last line read: {lastResult}");

        string[] resultData = lastResult.Split('\t');

        if (resultData.Length < 5)
        {
            UnityEngine.Debug.LogError(" Data format error: less than 5 columns.");
            return;
        }

        // **这里确保即使 `lastNumber` 还是 0，也能读取到第一条数据**
        if (!int.TryParse(resultData[0], out int newNumber))
        {
            UnityEngine.Debug.LogError("Failed to parse 'number' column.");
            return;
        }

        // **如果 lastNumber 还没有被初始化，直接读取**
        if (lastNumber == 0 || newNumber > lastNumber)
        {
            lastNumber = newNumber; // 更新 lastNumber
            string duration = resultData[2];
            string type = resultData[3];
            string successRate = resultData[4];
            respSuccessRate = float.Parse(successRate);


            UnityEngine.Debug.Log($"New data detected! Number: {newNumber}, Duration: {duration}, Type: {type}, SuccessRate: {successRate}%");

            // **存储到变量，不直接修改 UI**
            latestResultText = $"Last {duration} seconds {type} success rate is {successRate}%";
            isNewResultAvailable = true; // 标记有新数据
            if(duration == "7"){
                UnityEngine.Debug.Log("Expirez pendant 8 s");
                exerciceStepText = $"Expirez pendant 8 s";
                huit_s.Play();
            }

            else if(duration == "4"){
                UnityEngine.Debug.Log("Tenez votre respiration pendant 7 s");
                exerciceStepText = $"Tenez votre respiration pendant 7 s";
                sept_s.Play();
            }
            
            else //if(duration == "8" && inGame) 
            {
                UnityEngine.Debug.Log("Inspirez pendant 4 s");
                exerciceStepText = $"Inspirez pendant 4 s";
                quatre_s.Play();
            }
        }
        else
        {
            UnityEngine.Debug.Log("No new data detected.");
        }
    }
    catch (IOException ex)
    {
        UnityEngine.Debug.LogError("File read error: " + ex.Message);
    }
}


    private void OnDestroy()
    {
        UnityEngine.Debug.Log("Stopping FileSystemWatcher...");
        fileWatcher.EnableRaisingEvents = false;
        fileWatcher.Dispose();
    }

public void StartBreathingGame()
{
    inGame = true;
    LoadConfig(); // 确保在启动前加载配置

    ExerciceCanvas.SetActive(true);
    resultsText.text = "Breathing Test in Progress...";

    if (!File.Exists(resultsFilePath))
    {
        File.WriteAllText(resultsFilePath, "number\tcycle\tlong\ttype\tsuccessrate\n");
    }

    // **检查 Python 路径**
    if (string.IsNullOrEmpty(pythonPath))
    {
        UnityEngine.Debug.LogError("Python path is missing! Check config.json.");
        return;
    }

    // **检查 Python 可执行文件是否存在**
    string pythonExecutable = pythonPath;
    if (!File.Exists(pythonExecutable) && pythonExecutable != "python" && pythonExecutable != "python3")
    {
        UnityEngine.Debug.LogError($"Python executable not found at: {pythonExecutable}");
        return;
    }

    // **检查 Python 脚本路径**
    string scriptPath = Path.Combine(Application.dataPath, "Scripts/Data_process/run_for_a_game.py");
    if (!File.Exists(scriptPath))
    {
        UnityEngine.Debug.LogError($"Python script not found at: {scriptPath}");
        return;
    }

    UnityEngine.Debug.Log($"Running: {pythonExecutable} {scriptPath}");

    // **启动 Python 进程**
    breathingProcess = new Process();
    breathingProcess.StartInfo.FileName = pythonExecutable;
    breathingProcess.StartInfo.Arguments = $"\"{scriptPath}\"";
    breathingProcess.StartInfo.RedirectStandardOutput = true; // 读取 Python 输出
    breathingProcess.StartInfo.RedirectStandardError = true;  // 读取 Python 错误
    breathingProcess.StartInfo.UseShellExecute = false;
    breathingProcess.StartInfo.CreateNoWindow = true;

    // **捕获 Python 日志并在 Unity 显示**
    breathingProcess.OutputDataReceived += (sender, args) =>
    {
        if (!string.IsNullOrEmpty(args.Data))
        {
            UnityEngine.Debug.Log($"[Python]: {args.Data}");
        }
    };
    breathingProcess.ErrorDataReceived += (sender, args) =>
    {
        if (!string.IsNullOrEmpty(args.Data))
        {
            UnityEngine.Debug.LogError($"[Python Error]: {args.Data}");
        }
    };

    try
    {
        breathingProcess.Start();
        breathingProcess.BeginOutputReadLine(); // 开始异步读取标准输出
        breathingProcess.BeginErrorReadLine();  // 开始异步读取错误输出
    }
    catch (System.Exception e)
    {
        UnityEngine.Debug.LogError($"Failed to start process: {e.Message}");
        return;
    }
}

private void StopBreathingProcess()
    {
        if (breathingProcess != null && !breathingProcess.HasExited)
        {
            UnityEngine.Debug.Log("Stopping Python script...");
            breathingProcess.Kill();
            breathingProcess.Dispose();
        }
    }

    public void StopExercice(){
        StopBreathingProcess();
        OnDestroy();

        if (exerciceMusic != null)
        {
            exerciceMusic.Stop(); // Arrête la musique
        }
    }

    public void CloseGame()
    {
        ExerciceCanvas.SetActive(false);
        exerciceMusic.Stop();
        playerMovement.canMove = true;
    }

private void OnApplicationQuit()
    {
        StopBreathingProcess();
    }

[System.Serializable]  // 必须加这个，否则 JsonUtility 无法解析
    private class ConfigData
    {
        public string python_path;
    }

}
