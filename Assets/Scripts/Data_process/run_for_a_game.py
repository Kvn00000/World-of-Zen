import subprocess
import time
import os
import glob
import json
import signal
import sys

# **加载 config.json**
config_path = "Assets/UserConfig/config.json"
if not os.path.exists(config_path):
    raise FileNotFoundError(f"❌ Config file {config_path} not found. Please create config.json")

with open(config_path, "r") as file:
    config = json.load(file)

# **读取参数**
PYTHON_PATH = config["python_path"]
ORIGINAL_DATA_FOLDER = config["opensignal_data_folder"]
MOCK_DATA_FOLDER = config["mock_opensignal_data_folder"]
TRANSFORMED_DATA_FOLDER = config["transformed_data_folder"]
TEST_MODE = config["test_mode"]

# **存储所有进程**
processes = []

# **清空文件夹**
def clean_folder(folder_path):
    if os.path.exists(folder_path):
        files = glob.glob(os.path.join(folder_path, "*"))
        for f in files:
            os.remove(f)
        print(f"🧹 Cleared {folder_path}")
    else:
        os.makedirs(folder_path)
        print(f"📁 Created {folder_path}")

# **清理数据**
clean_folder(TRANSFORMED_DATA_FOLDER)

# **信号处理函数，确保所有子进程关闭**
def cleanup_processes():
    print("\n🛑 Terminating all subprocesses...")
    for process in processes:
        if process and process.poll() is None:  # Check if process is still running
            process.terminate()
            try:
                process.wait(timeout=5)  # Give it time to exit
            except subprocess.TimeoutExpired:
                print(f"⚠️ Force-killing process {process.pid}")
                process.kill()  # Force kill if it doesn't exit gracefully
    print("✅ All subprocesses terminated. Exiting.")

# **捕获 SIGINT (Ctrl+C) 和 SIGTERM (kill command)**
def signal_handler(sig, frame):
    cleanup_processes()
    sys.exit(0)

signal.signal(signal.SIGINT, signal_handler)
signal.signal(signal.SIGTERM, signal_handler)

# **启动 Mock Data（仅在测试模式下）**
mock_process = None
if TEST_MODE:
    mock_script = "Assets/Scripts/Data_process/mock_data_load.py"
    print(f"🚀 (TEST MODE) Launching {mock_script}...")
    mock_process = subprocess.Popen([PYTHON_PATH, mock_script])
    processes.append(mock_process)
    time.sleep(0.5)

# **启动 Data Processing**
data_processing_script = "Assets/Scripts/Data_process/data_processing.py"
print(f"🚀 Launching {data_processing_script}...")
data_process = subprocess.Popen([PYTHON_PATH, data_processing_script])
processes.append(data_process)
time.sleep(0.5)

# **启动 Compute Cycle Success Rate**
success_rate_script = "Assets/Scripts/Data_process/compute_cycle_success_rate.py"
print(f"🚀 Launching {success_rate_script}...")
monitor_process = subprocess.Popen([PYTHON_PATH, success_rate_script])
processes.append(monitor_process)

# **等待所有进程**
try:
    if TEST_MODE and mock_process:
        mock_process.wait()
    data_process.wait()
    monitor_process.wait()
except KeyboardInterrupt:
    print("\n🛑 KeyboardInterrupt detected. Stopping processes...")
finally:
    cleanup_processes()
    print("✅ Exiting cleanly.")
