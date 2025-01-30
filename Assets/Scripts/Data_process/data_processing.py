import os
import time
import json

# **加载 config.json**
config_path = "Assets/UserConfig/config.json"
if not os.path.exists(config_path):
    raise FileNotFoundError(f"❌ 配置文件 {config_path} 不存在，请创建 config.json")

with open(config_path, "r") as file:
    config = json.load(file)

test_mode = config["test_mode"]
if test_mode:
    directory = config["mock_opensignal_data_folder"]
else:
    directory = config["opensignal_data_folder"]

output_file = "Assets/Data/transformed/transformed_data.txt"  # 处理后的数据存储文件


def find_opensignals_file(directory):
    """
    在指定目录下寻找唯一的 OpenSignals 数据文件。
    :param directory: 目标文件夹 (例如 "Assets/Data")
    :return: 文件路径（如果找到），否则返回 None
    """
    files = [f for f in os.listdir(directory) if f.endswith(".txt")]
    if len(files) == 1:
        return os.path.join(directory, files[0])
    elif len(files) > 1:
        print("❌ 目录中有多个 .txt 文件，请手动指定！")
        return None
    else:
        print("❌ 目录中没有找到 .txt 文件！")
        return None

def monitor_and_transform_opensignals(directory, output_file, sampling_rate=10):
    """
    监听 OpenSignals 生成的文件，并转换格式后保存到 output_file。
    :param directory: 目标目录，自动查找 OpenSignals 文件
    :param output_file: 处理后的数据存储文件
    :param sampling_rate: 采样率 (Hz)，默认 10Hz
    """
    file_path = find_opensignals_file(directory)
    if not file_path:
        return  # 找不到文件就退出

    # print(f"✅ 发现数据文件：{file_path}")
    # print(f"📡 监听中，正在转换数据...")

    last_position = 0  # 记录上次读取的文件位置
    time_step = 1 / sampling_rate
    current_time = 0

    # 先清空文件，写入表头
    with open(output_file, "w") as out:
        out.write("time\tbpm\trespiration\n")

    while True:
        with open(file_path, "r") as file:
            file.seek(last_position)  # 从上次读取的位置继续
            new_lines = file.readlines()

            if new_lines:
                with open(output_file, "a") as out:  # 以追加模式写入
                    for line in new_lines:
                        values = line.strip().split("\t")
                        if len(values) < 11:
                            continue  # 跳过不完整的数据

                        bpm_value = int(values[5])   # A1 对应 bpm 曲线
                        respiration_value = int(values[6])  # A2 对应 respiration 曲线

                        formatted_line = f"{current_time:.1f}\t{bpm_value}\t{respiration_value}\n"
                        # print(formatted_line.strip())  # 显示输出
                        out.write(formatted_line)  # 写入文件

                        current_time += time_step

                last_position = file.tell()  # 记录新位置
        
        time.sleep(0.1)  # 休眠 100ms，避免 CPU 过载



monitor_and_transform_opensignals(directory, output_file)
