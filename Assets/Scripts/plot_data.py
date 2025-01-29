import pandas as pd
import matplotlib.pyplot as plt

# Load the data
data = pd.read_csv('data.csv')

# Plot the data
plt.plot(data['x'], data['y'])
plt.xlabel('Temps')
plt.ylabel('Valeur donnée par le capteur')
plt.title('Data')

plot_path = "plot.png"
plt.savefig(plot_path)