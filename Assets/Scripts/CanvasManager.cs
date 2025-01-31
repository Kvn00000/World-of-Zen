using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Diagnostics;
using System.Collections;


public class CanvasManager : MonoBehaviour
{



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
    private Process breathingProcess;

    private bool isNewResultAvailable = false; // 标志是否有新结果

    private string latestResultText;  // 变量存储最新的结果

    private int difficulte = 0;

    private float devoilement_rate_tableau = 0;

    private bool inGame = false;
    private int lastNumber = 0; // 记录上一次的 number 值
    void Start()
    {
        // Afficher le Canvas et désactiver le mouvement du joueur au début
        exteriorCanvas.SetActive(true);
        playerMovement = GetComponent<PlayerMovement>();
        playerMovement.canMove = false;
        pictureSlider.onValueChanged.AddListener(UpdateSliderValue);
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
            isNewResultAvailable = false; // 重置标记
            if (devoilement_rate_tableau >= 1)
            {
                inGame = false;
                CloseGame();
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
            if (!paintCanvas.activeSelf)
            {
                paintCanvas.SetActive(true);
            }

            // Check if the player presses 'E' to change the scene
            if (Input.GetKeyDown(KeyCode.E) && paintCanvas.activeSelf)
            {

                playerMovement.canMove = false;
                PictureMenu.SetActive(true);
                //Turn on the respiration game
                paintCanvas.SetActive(false); // Hide the Canvas once the scene is changed
            }

            // if(quad.activeSelf &&Input.GetKeyDown(KeyCode.Return)){
            //     quad.SetActive(false);
            // }
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
    devoilement_rate_tableau += 0.5f; // 每次增加 1%
    setOpacity(devoilement_rate_tableau);
}




    public void CloseCanvas(GameObject canv)
    {
        // Désactiver le Canvas et activer le mouvement du joueur
        canv.SetActive(false);
        playerMovement.canMove = true;
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
        StartFileWatcher();
        StartBreathingGame();
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

            UnityEngine.Debug.Log($"New data detected! Number: {newNumber}, Duration: {duration}, Type: {type}, SuccessRate: {successRate}%");

            // **存储到变量，不直接修改 UI**
            latestResultText = $"Last {duration} seconds {type} success rate is {successRate}%";
            isNewResultAvailable = true; // 标记有新数据
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

    public void CloseGame()
    {
        ExerciceCanvas.SetActive(false);
        playerMovement.canMove = true;
        StopBreathingProcess();
        OnDestroy();
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
