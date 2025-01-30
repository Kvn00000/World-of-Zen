import os
import time

def stream_opensignals_data(input_file, output_folder, sampling_rate=10):
    """
    逐行读取 OpenSignals 数据文件，并以指定频率写入到 output_folder。

    :param input_file: OpenSignals 数据文件路径
    :param output_folder: 目标文件夹（写入 original_data.txt）
    :param sampling_rate: 采样率（Hz），默认 10Hz（每 0.1 秒写入一行）
    """
    output_file = os.path.join(output_folder, "original_data.txt")

    # 确保目标文件夹存在
    os.makedirs(output_folder, exist_ok=True)

    # 如果文件不存在，则创建新文件
    if not os.path.exists(output_file):
        open(output_file, "w").close()

    # print(f"📂 正在读取文件: {input_file}")
    # print(f"💾 数据将写入: {output_file}")
    # print(f"📡 每 {1/sampling_rate:.1f} 秒写入一行数据...")

    with open(input_file, "r") as infile, open(output_file, "w") as outfile:
        header_ended = False  # 标记是否跳过了头部
        time_counter = 0.0  # 记录写入时间
        
        for line in infile:
            # 跳过文件头
            if not header_ended:
                if line.strip() == "# EndOfHeader":
                    header_ended = True
                    outfile.write(line)  # 写入 EndOfHeader
                    # print("✅ 头部信息已跳过，开始写入数据...")
                else:
                    outfile.write(line)  # 复制头部信息
                continue

            # 处理数据行
            outfile.write(line)  # 逐行写入
            outfile.flush()  # 立即写入硬盘
            # print(f"⏳ {time_counter:.1f}s: {line.strip()}")  # 显示当前写入的数据

            time_counter += 1 / sampling_rate
            time.sleep(1 / sampling_rate)  # 等待 0.1 秒

    print("✅ 数据写入完成！")

# 设置输入文件和输出文件夹
input_file = "Assets/Scripts/Data_process/opensignals_2019070080B3_2025-01-30_14-59-05.txt"  # 你的 OpenSignals 数据文件
output_folder = "Assets/Data/original"  # 目标存储路径

# 运行脚本
stream_opensignals_data(input_file, output_folder)
