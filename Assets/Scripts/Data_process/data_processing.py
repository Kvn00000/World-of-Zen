import os
import time
import json

# **加载 config.json**
config_path = "Assets/UserConfig/config.json"
if not os.path.exists(config_path):
    raise FileNotFoundError(f"❌ Config file {config_path} not found. Please create config.json")

with open(config_path, "r") as file:
    config = json.load(file)

test_mode = config["test_mode"]
directory = config["mock_opensignal_data_folder"] if test_mode else config["opensignal_data_folder"]
output_file = "Assets/Data/transformed/transformed_data.txt"
output_breathing_file = "Assets/Data/transformed/breathing_success_data.txt"

def find_opensignals_file(directory):
    """ Find the unique OpenSignals data file in the specified directory """
    files = [f for f in os.listdir(directory) if f.endswith(".txt")]
    if len(files) == 1:
        print(f"✅ Found OpenSignals data file: {files[0]}")
        return os.path.join(directory, files[0])
    elif len(files) > 1:
        print("❌ Multiple .txt files found in the directory. Please specify manually!")
        return None
    else:
        print("❌ No .txt file found in the directory!")
        return None


def monitor_and_transform_opensignals(directory, output_file, output_breathing_file, rythm=[4, 7, 8], sampling_rate=10):
    """ Monitor OpenSignals data file and process it incrementally """
    file_path = find_opensignals_file(directory)
    if not file_path:
        print("❌ No OpenSignals data file found, exiting.")
        return

    # **Clear the OpenSignals data file when first detected**
    print(f"🗑️ Clearing existing OpenSignals data file: {file_path}")
    open(file_path, "w").close()  # 清空 OpenSignals 文件

    # **Clear output file and write header**
    print(f"🗑️ Clearing existing output file: {output_file}")
    open(output_file, "w").write("time\tbpm\trespiration\n")  # 直接创建并写入表头

    # **Initialize variables**
    time_step = 1 / sampling_rate
    current_time = 0.0
    order_time = 0.0
    rythm_order = rythm
    number = 0
    cycle = 1
    type_map = {4: "inspire", 7: "retenir", 8: "expire"}
    processed_lines = 0  # **跟踪已处理的行数**

    print(f"🔄 Monitoring {file_path}...")

    while True:
        time.sleep(0.1)  # **Avoid CPU overload, check every 0.1s**

        # **Read new lines only**
        with open(file_path, "r") as file:
            lines = file.readlines()

        new_data_lines = lines[processed_lines:]  # **只处理新数据**
        if not new_data_lines:
            continue  # **没有新数据，跳过**

        processed_lines = len(lines)  # **更新已处理的行数**
        print(f"📥 Processing {len(new_data_lines)} new lines...")

        # **Write new data**
        with open(output_file, "a") as out:
            for line in new_data_lines:
                values = line.strip().split("\t")
                if len(values) < 11:
                    print(f"⚠️ Skipping incomplete data: {line.strip()}")
                    continue

                try:
                    bpm_value = int(values[5])  # A1 -> bpm
                    respiration_value = int(values[6])  # A2 -> respiration
                except ValueError:
                    print(f"🚨 Error parsing data: {values}")
                    continue

                formatted_line = f"{current_time:.1f}\t{bpm_value}\t{respiration_value}\n"
                out.write(formatted_line)
                print(f"✅ Recorded: {formatted_line.strip()}")

                # **Calculate success rate at rhythm change**
                if abs(order_time - rythm_order[0]) < 0.05:
                    with open(output_file, "r") as read_out:  # 重新打开以读取完整数据
                        all_lines = read_out.readlines()
                    data_to_evaluate = [int(line.strip().split("\t")[2]) for line in all_lines[1:][-rythm_order[0] * 10:]]
                    success_rate = compute_success_rate(data_to_evaluate, rythm_order)
                    print(f"Success Rate: {success_rate}%")

                    if not os.path.exists(output_breathing_file):
                        with open(output_breathing_file, "w") as f:
                            f.write("number\tcycle\tlong\ttype\tsuccessrate\n")

                    with open(output_breathing_file, "a") as f:
                        f.write(f"{number}\t{cycle}\t{rythm_order[0]}\t{type_map[rythm_order[0]]}\t{success_rate}\n")

                    rythm_order = rythm_order[1:] + rythm_order[:1]  # 轮换节奏顺序
                    number += 1
                    if rythm_order[0] == 8:
                        cycle += 1
                    order_time = 0.0

                current_time += time_step
                order_time += time_step

            out.flush()
            os.fsync(out.fileno())  # **Force write to disk**


def compute_success_rate(data_lines, rythm_order):
    """ Compute success rate based on the given data lines and rythm order """
    if not data_lines:
        return 0

    success_count = 0
    total_count = len(data_lines)
    rythm_type = rythm_order[0]

    if rythm_type == 4:  # inspire
        for i in range(1, total_count):
            if data_lines[i] > data_lines[i - 1]:
                success_count += 1
    elif rythm_type == 7:  # retenir
        for i in range(1, total_count):
            if abs(data_lines[i] - data_lines[i - 1]) <= 30:  # 允许小幅度波动
                success_count += 1
    elif rythm_type == 8:  # expire
        for i in range(1, total_count):
            if data_lines[i] < data_lines[i - 1]:
                success_count += 1

    success_rate = (success_count / total_count) * 100
    return success_rate


# Run the function
monitor_and_transform_opensignals(directory, output_file, output_breathing_file)
