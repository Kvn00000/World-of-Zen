import pandas as pd
import time
import os

# 定义阶段类型
PHASE_TYPES = ["inspire", "no", "expire"]
NO_PHASE_VARIATION_THRESHOLD = 15  # 允许的最大波动范围

def calculate_success_rate(df, start_time, duration, phase_type):
    """
    计算单个阶段的 success rate：
    - inspire（吸气）: bpm 应该上升
    - no（屏息）: bpm 允许小幅波动
    - expire（呼气）: bpm 应该下降
    """
    end_time = start_time + duration
    phase_data = df[(df["time"] >= start_time) & (df["time"] < end_time)]
    bpm_values = phase_data["bpm"].values

    if len(bpm_values) < 2:
        return 0.0  # 数据量不足，返回 0

    if phase_type == "inspire":  # 上升趋势
        success_count = sum(bpm_values[i] < bpm_values[i+1] for i in range(len(bpm_values)-1))
    elif phase_type == "no":  # **屏息判定**
        success_count = sum(abs(bpm_values[i] - bpm_values[i+1]) <= NO_PHASE_VARIATION_THRESHOLD for i in range(len(bpm_values)-1))
        
        # **标准差方案（如果整体波动小，则 100% 成功）**
        std_dev = phase_data["bpm"].std()
        if std_dev <= NO_PHASE_VARIATION_THRESHOLD:
            return 100.0  
    else:  # expire 下降趋势
        success_count = sum(bpm_values[i] > bpm_values[i+1] for i in range(len(bpm_values)-1))

    success_rate = success_count / (len(bpm_values) - 1) if len(bpm_values) > 1 else 0
    return round(success_rate * 100, 2)

def get_last_number(output_file):
    """
    获取 breathing_success_rates.txt 里最新的 number，如果文件不存在则返回 1。
    """
    if not os.path.exists(output_file) or os.stat(output_file).st_size == 0:
        return 1  # 如果文件不存在或为空，则从 1 开始

    try:
        df = pd.read_csv(output_file, sep="\t")
        return df["number"].max() + 1  # 获取最后的 number + 1
    except Exception:
        return 1  # 读取失败时，从 1 开始

def monitor_and_evaluate(file_path, phases, output_file, timeout=5):
    """
    监听数据文件，并按阶段计算 success rate，每个阶段结束后切换到下一个。
    """
    last_mod_time = os.path.getmtime(file_path)
    last_data_size = 0
    current_cycle = 1
    start_time = 0  # 记录当前阶段开始时间
    phase_index = 0  # 当前进行的阶段索引
    next_number = get_last_number(output_file)  # 获取下一个编号

    # **确保输出文件夹存在**
    output_dir = os.path.dirname(output_file)
    os.makedirs(output_dir, exist_ok=True)

    # **如果文件不存在，则创建并写入表头**
    if not os.path.exists(output_file):
        with open(output_file, "w") as f:
            f.write("number\tcycle\tlong\ttype\tsuccessrate\n")

    print(f"📡 正在监听 {file_path}，检测呼吸模式 {phases}...")

    while True:
        time.sleep(1)  # 每秒检查一次文件

        # 检查文件是否有更新
        new_mod_time = os.path.getmtime(file_path)
        if new_mod_time == last_mod_time:
            timeout -= 1
            if timeout <= 0:
                print("⏳ 长时间无新数据，脚本自动退出。")
                break
            continue
        last_mod_time = new_mod_time

        # 读取最新数据
        df = pd.read_csv(file_path, sep="\t")
        if df.shape[0] == last_data_size:
            continue  # 数据没有新增，继续等待
        last_data_size = df.shape[0]

        # 获取最新的时间
        latest_time = df["time"].max()

        # 检查当前阶段是否结束
        phase_duration = phases[phase_index]  # 当前阶段的持续时间
        if latest_time >= start_time + phase_duration:
            phase_type = PHASE_TYPES[phase_index]  # 获取当前阶段类型
            success_rate = calculate_success_rate(df, start_time, phase_duration, phase_type)

            # **记录结果**
            with open(output_file, "a") as f:
                f.write(f"{next_number}\t{current_cycle}\t{phase_duration}\t{phase_type}\t{success_rate}\n")

            print(f"✅ {phase_type.upper()} 阶段 ({start_time}s - {start_time+phase_duration}s) 成功率: {success_rate}%")

            # 进入下一个阶段
            next_number += 1  # 递增编号
            start_time += phase_duration
            phase_index = (phase_index + 1) % len(phases)  # 循环阶段
            if phase_index == 0:
                current_cycle += 1  # 每完成一个完整 cycle，增加 cycle 计数

# 运行脚本
file_path = "Assets/Data/transformed/transformed_data.txt"  # 你的数据文件
output_file = "Assets/Data/transformed/breathing_success_rates.txt"  # 结果存储文件
phases = [4, 7, 8]  # 设定的呼吸模式
monitor_and_evaluate(file_path, phases, output_file)
