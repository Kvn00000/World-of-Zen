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
output_file = "Assets/Data/transformed/transformed_data.txt"  # Processed data file


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


def count_lines(file_path):
    """ Count the number of lines in a file """
    with open(file_path, "r") as file:
        return sum(1 for _ in file)


def monitor_and_transform_opensignals(directory, output_file, sampling_rate=10):
    """ Monitor OpenSignals data file and process it into output_file """
    file_path = find_opensignals_file(directory)
    if not file_path:
        print("❌ No OpenSignals data file found, exiting.")
        return

    # **Clear output file if it exists**
    if os.path.exists(output_file):
        print(f"🗑️ Clearing existing output file: {output_file}")
        os.remove(output_file)

    # **Ensure the output file exists and write header**
    print(f"📝 Creating new output file: {output_file}")
    with open(output_file, "w") as out:
        out.write("time\tbpm\trespiration\n")

    # **Initialize counters**
    last_line_number = count_lines(file_path)
    time_step = 1 / sampling_rate
    current_time = 0.0  # **Start time at 0.0**

    print(f"🔄 Monitoring {file_path}, starting from line {last_line_number + 1}...")

    while True:
        time.sleep(0.1)  # **Avoid CPU overload, check every 0.1s**

        # **Read the entire file**
        with open(file_path, "r") as file:
            lines = file.readlines()

        # **Extract new lines**
        new_data_lines = lines[last_line_number:]  # Read only the new data
        if not new_data_lines:
            continue  # **Skip if no new data is found**

        last_line_number = len(lines)  # **Update line counter**

        print(f"📥 Found {len(new_data_lines)} new lines...")

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

                current_time += time_step  # **Increment time by step size**

            out.flush()
            os.fsync(out.fileno())  # **Force write to disk**

# Run the function
monitor_and_transform_opensignals(directory, output_file)
