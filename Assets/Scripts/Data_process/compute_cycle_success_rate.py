import pandas as pd
import time
import os

# Define phase types
PHASE_TYPES = ["inspire", "no", "expire"]
NO_PHASE_VARIATION_THRESHOLD = 15  # Maximum allowed variation in "no" phase

def calculate_success_rate(df, start_time, duration, phase_type):
    """
    Calculate the success rate for a specific breathing phase:
    - Inspire: BPM should increase.
    - No: BPM should remain stable within a small variation range.
    - Expire: BPM should decrease.
    """
    end_time = start_time + duration
    phase_data = df[(df["time"] >= start_time) & (df["time"] < end_time)]
    bpm_values = phase_data["bpm"].values

    if len(bpm_values) < 2:
        return 0.0  # Not enough data points

    if phase_type == "inspire":  # Rising trend
        success_count = sum(bpm_values[i] < bpm_values[i+1] for i in range(len(bpm_values)-1))
    elif phase_type == "no":  # Stable phase
        success_count = sum(abs(bpm_values[i] - bpm_values[i+1]) <= NO_PHASE_VARIATION_THRESHOLD for i in range(len(bpm_values)-1))
        
        # Alternative method: Use standard deviation
        std_dev = phase_data["bpm"].std()
        if std_dev <= NO_PHASE_VARIATION_THRESHOLD:
            return 100.0  
    else:  # Expire: Decreasing trend
        success_count = sum(bpm_values[i] > bpm_values[i+1] for i in range(len(bpm_values)-1))

    success_rate = success_count / (len(bpm_values) - 1) if len(bpm_values) > 1 else 0
    return round(success_rate * 100, 2)

def get_last_number(output_file):
    """
    Get the latest 'number' value from breathing_success_rates.txt.
    If the file does not exist, start from 1.
    """
    if not os.path.exists(output_file) or os.stat(output_file).st_size == 0:
        return 1  # Start from 1 if file does not exist

    try:
        df = pd.read_csv(output_file, sep="\t")
        return df["number"].max() + 1  # Get last number and increment
    except Exception:
        return 1  # Default to 1 if read fails

def monitor_and_evaluate(file_path, phases, output_file, timeout=50):
    """
    Monitor the data file and compute success rates for each breathing phase.
    Automatically progresses to the next phase after the set duration.
    """
    last_mod_time = os.path.getmtime(file_path)
    last_data_size = 0
    current_cycle = 1
    start_time = 0  # Track phase start time
    phase_index = 0  # Track current phase index
    next_number = 0  # Get next sequence number
    time_without_updates = 0  # Track timeout

    # Ensure output directory exists
    output_dir = os.path.dirname(output_file)
    os.makedirs(output_dir, exist_ok=True)

    # Create file with headers if it does not exist
    with open(output_file, "w") as f:
        f.write("number\tcycle\tlong\ttype\tsuccessrate\n")

    print(f" Monitoring {file_path} for breathing patterns: {phases}...")

    while True:
        time.sleep(1)  # Check every second

        # Check if file has been updated
        new_mod_time = os.path.getmtime(file_path)
        if new_mod_time == last_mod_time:
            time_without_updates += 1
            if time_without_updates >= timeout:
                print(" No new data detected for a long time. Exiting...")
                break
            continue
        last_mod_time = new_mod_time
        time_without_updates = 0  # Reset timeout

        # Read latest data
        df = pd.read_csv(file_path, sep=" ")
        if df.shape[0] == last_data_size:
            continue  # No new rows, continue waiting
        new_rows = df.shape[0] - last_data_size
        last_data_size = df.shape[0]

        print(f" Detected {new_rows} new data rows.")

        # Get latest time
        latest_time = df["time"].max()

        # Check if the current phase duration is completed
        phase_duration = phases[phase_index]
        if latest_time >= start_time + phase_duration:
            phase_type = PHASE_TYPES[phase_index]
            success_rate = calculate_success_rate(df, start_time, phase_duration, phase_type)

            # Log result
            with open(output_file, "a") as f:
                f.write(f"{next_number}\t{current_cycle}\t{phase_duration}\t{phase_type}\t{success_rate}\n")

            print(f" Phase: {phase_type.upper()} ({start_time}s - {start_time+phase_duration}s) Success Rate: {success_rate}%")

            # Move to the next phase
            next_number += 1
            start_time += phase_duration
            phase_index = (phase_index + 1) % len(phases)  # Loop through phases
            if phase_index == 0:
                current_cycle += 1  # Increment cycle number

# Run the script
file_path = "Assets/Data/data.txt"
output_file = "Assets/Data/transformed/breathing_success_data.txt"
phases = [4, 7, 8]  # Breathing cycle durations
monitor_and_evaluate(file_path, phases, output_file)
