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
directory = config["mock_opensignal_data_folder"] if test_mode else config["opensignal_data_folder"]
output_file = "Assets/Data/transformed/transformed_data.txt"  # 处理后的数据存储文件


def find_opensignals_file(directory):
    """ 在指定目录下寻找唯一的 OpenSignals 数据文件 """
    files = [f for f in os.listdir(directory) if f.endswith(".txt")]
    if len(files) == 1:
        print(f"✅ 找到 OpenSignals 数据文件: {files[0]}")
        return os.path.join(directory, files[0])
    elif len(files) > 1:
        print("❌ 目录中有多个 .txt 文件，请手动指定！")
        return None
    else:
        print("❌ 目录中没有找到 .txt 文件！")
        return None


def count_lines(file_path):
    """ 计算文件的行数 """
    with open(file_path, "r") as file:
        return sum(1 for _ in file)


def monitor_and_transform_opensignals(directory, output_file, sampling_rate=10):
    """ 监听 OpenSignals 生成的文件，并转换格式后保存到 output_file """
    file_path = find_opensignals_file(directory)
    if not file_path:
        print("❌ 没有找到 OpenSignals 数据文件，退出监听")
        return

    # **计算已有行数**
    last_line_number = count_lines(file_path)

    # **时间步长**
    time_step = 1 / sampling_rate

    # **确保输出文件存在**
    first_write = not os.path.exists(output_file)  # 如果文件不存在，则是第一次写入
    if first_write:
        print(f"📝 {output_file} 不存在，创建并写入表头...")
        with open(output_file, "w") as out:
            out.write("time\tbpm\trespiration\n")

    print(f"🔄 监听 {file_path}，从第 {last_line_number + 1} 行开始处理数据...")

    first_data_written = False  # 标记是否已经写入第一行数据

    while True:
        time.sleep(0.1)  # **0.1秒检测一次，避免CPU过载**

        # **读取整个文件**
        with open(file_path, "r") as file:
            lines = file.readlines()

        # **找到新数据**
        new_data_lines = lines[last_line_number:]  # 只获取新增的部分
        if not new_data_lines:
            continue  # **如果没有新数据，跳过本次循环**

        # **更新 last_line_number**
        last_line_number = len(lines)

        print(f"📥 读取到 {len(new_data_lines)} 行新数据...")

        # **写入新数据**
        with open(output_file, "a") as out:
            for i, line in enumerate(new_data_lines):
                values = line.strip().split("\t")
                if len(values) < 11:
                    print(f"⚠️ 跳过不完整的数据: {line.strip()}")
                    continue

                try:
                    bpm_value = int(values[5])  # A1 对应 bpm 曲线
                    respiration_value = int(values[6])  # A2 对应 respiration 曲线
                except ValueError:
                    print(f"🚨 无法解析数据: {values}")
                    continue  # 遇到解析错误，跳过

                # **时间从 0.0 开始，每行递增**
                if not first_data_written:
                    current_time = 0.0  # **第一行写入时，从 `0.0` 开始**
                    first_data_written = True
                else:
                    current_time += time_step  # 之后的行正常累加时间

                formatted_line = f"{current_time:.1f}\t{bpm_value}\t{respiration_value}\n"
                out.write(formatted_line)
                print(f"✅ 记录: {formatted_line.strip()}")  # Debug 输出

            out.flush()
            os.fsync(out.fileno())  # **确保数据写入磁盘**

# 运行函数
monitor_and_transform_opensignals(directory, output_file)
