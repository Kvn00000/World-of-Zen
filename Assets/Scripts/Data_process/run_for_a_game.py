import subprocess
import time
import os
import glob
import json

# **加载 config.json**
config_path = "Assets/UserConfig/config.json"
if not os.path.exists(config_path):
    raise FileNotFoundError(f"❌ 配置文件 {config_path} 不存在，请创建 config.json")

with open(config_path, "r") as file:
    config = json.load(file)

# **从配置文件读取参数**
PYTHON_PATH = config["python_path"]
ORIGINAL_DATA_FOLDER = config["opensignal_data_folder"]  # OpenSignals 原始数据存放文件夹
MOCK_DATA_FOLDER = config["mock_opensignal_data_folder"]  # Mock 数据存放文件夹
TRANSFORMED_DATA_FOLDER = config["transformed_data_folder"]  # 处理后数据存放文件夹
TEST_MODE = config["test_mode"]  # JSON 里 true 是布尔值，不需要 `.lower() == "true"`

# **清空文件夹**
def clean_folder(folder_path):
    if os.path.exists(folder_path):
        files = glob.glob(os.path.join(folder_path, "*"))  # 获取所有文件
        for f in files:
            os.remove(f)  # 删除文件
        print(f"🧹 已清空 {folder_path} 目录！")
    else:
        os.makedirs(folder_path)  # 确保目录存在
        print(f"📁 目录不存在，已创建 {folder_path}！")

# **执行清理**
clean_folder(ORIGINAL_DATA_FOLDER if not TEST_MODE else MOCK_DATA_FOLDER)  # 选择清空 OpenSignals 数据文件夹 或 Mock 数据文件夹
clean_folder(TRANSFORMED_DATA_FOLDER)  # 清空转换后数据文件夹

# **如果在测试模式下，启动 mock_data_load.py**
mock_process = None
if TEST_MODE:
    mock_script = "Assets/Scripts/Data_process/mock_data_load.py"
    print(f"🚀 (TEST MODE) 启动 {mock_script}...")
    mock_process = subprocess.Popen([PYTHON_PATH, mock_script])
    time.sleep(0.5)  # 确保 mock 数据加载程序先启动

# **启动 data_processing.py**
data_processing_script = "Assets/Scripts/Data_process/data_processing.py"
print(f"🚀 启动 {data_processing_script}...")
data_process = subprocess.Popen([PYTHON_PATH, data_processing_script])
time.sleep(0.5)  # 确保 data_processing.py 也启动了

# **启动 compute_cycle_success_rate.py 进行 success rate 计算**
success_rate_script = "Assets/Scripts/Data_process/compute_cycle_success_rate.py"
print(f"🚀 启动 {success_rate_script}...")
monitor_process = subprocess.Popen([PYTHON_PATH, success_rate_script])

# **等待所有进程执行完毕**
if TEST_MODE and mock_process:
    mock_process.wait()
data_process.wait()
monitor_process.wait()

print("✅ 所有进程执行完毕！")
