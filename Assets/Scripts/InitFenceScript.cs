using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitFenceScript : MonoBehaviour
{
    public GameObject fencePrefab;
    public float fenceSpacing = 5.7f; // Distance entre les clôtures
    public float areaSize = 300f;    // Taille de la zone (longueur d'un côté du carré)

    void Start()
    {
        // Ajuster areaSize pour qu'il soit un multiple exact de fenceSpacing
        int fencesPerSide = Mathf.RoundToInt(areaSize / fenceSpacing);
        areaSize = fencesPerSide * fenceSpacing;

        float halfArea = areaSize / 2;

        // Générer les clôtures pour chaque côté
        GenerateFenceSide(new Vector3(-halfArea, 0, -halfArea), Vector3.forward, fenceSpacing, fencesPerSide + 1); // Côté gauche
        GenerateFenceSide(new Vector3(halfArea, 0, -halfArea), Vector3.forward, fenceSpacing, fencesPerSide + 1); // Côté droit
        GenerateFenceSide(new Vector3(-halfArea, 0, -halfArea), Vector3.right, fenceSpacing, fencesPerSide + 1, Quaternion.Euler(0, 90, 0)); // Bas
        GenerateFenceSide(new Vector3(-halfArea, 0, halfArea), Vector3.right, fenceSpacing, fencesPerSide + 1, Quaternion.Euler(0, 90, 0)); // Haut
    }

    // Méthode générique pour générer une clôture sur un côté
    void GenerateFenceSide(Vector3 startPosition, Vector3 direction, float spacing, int count, Quaternion? rotation = null)
    {
        for (int i = 1; i < count + 1; i++) // Ajouter une clôture supplémentaire avec count + 1
        {
            Vector3 position = startPosition + direction * i * spacing;
            Quaternion rot = rotation ?? Quaternion.identity;
            InstantiateFence(position, rot);
        }
    }

    // Méthode pour instancier une clôture et définir son parent
    void InstantiateFence(Vector3 position, Quaternion rotation)
    {
        GameObject fence = Instantiate(fencePrefab, position, rotation);
        fence.transform.parent = this.transform;
    }
}
