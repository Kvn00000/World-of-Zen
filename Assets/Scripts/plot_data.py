import argparse
import matplotlib.pyplot as plt
import os

# Fonction qui lit le fichier et trace les X dernières valeurs de la colonne A2
def lire_fichier_et_extraire_A2(nom_fichier, X, destination="mon_graphique.png"):
    # Si le dossier n'existe pas, le créer
    dossier = os.path.dirname(destination)
    if dossier and not os.path.exists(dossier):
        os.makedirs(dossier)

    with open(nom_fichier, "r") as f:
        lignes = f.readlines()

    # Trouver la fin de l'en-tête
    for i, ligne in enumerate(lignes):
        if ligne.strip() == "# EndOfHeader":
            debut_donnees = i + 1
            break

    # Lire les données numériques
    donnees = [list(map(int, ligne.split())) for ligne in lignes[debut_donnees:] if ligne.strip()]
    
    # Extraire la colonne A2 (indice 6)
    colonne_A2 = [ligne[6] for ligne in donnees]

    # Prendre les X dernières valeurs
    X = min(X, len(colonne_A2))  # S'assurer que X ne dépasse pas la taille des données
    dernieres_A2 = colonne_A2[-X:]

    # Tracer le graphique
    plt.figure(figsize=(8, 5))
    plt.plot(dernieres_A2, marker='o', linestyle='-')
    # plt.xlabel("Derniers X points")
    # plt.ylabel("Valeurs de A2")
    # plt.title(f"Les {X} dernières valeurs de A2")
    plt.grid(True)

    # Sauvegarde du graphique dans la destination spécifiée
    plt.savefig("new/oui", dpi=300, bbox_inches='tight')  # Enregistrer le plot


# Fonction principale pour gérer les arguments de la ligne de commande
def main():
    parser = argparse.ArgumentParser(description="Tracé des dernières valeurs d'A2 depuis un fichier texte")
    parser.add_argument("nom_fichier", help="Nom du fichier contenant les données à analyser")
    parser.add_argument("nombre_lignes", type=int, help="Nombre de lignes à afficher dans le graphique")

    args = parser.parse_args()

    # Appel de la fonction avec les paramètres
    lire_fichier_et_extraire_A2(args.nom_fichier, args.nombre_lignes)

if __name__ == "__main__":
    main()
