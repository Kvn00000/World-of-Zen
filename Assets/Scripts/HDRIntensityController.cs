using UnityEngine;

public class HDRIntensityController : MonoBehaviour
{
    public Material targetMaterial; // Matériau dont on veut modifier l'intensité HDR
    public float hdrIntensity = -10f; // Intensité HDR de base (limitée entre -10 et 10)
    private Color hdrBaseColor = Color.white; // Couleur HDR de base (fixée à blanc)

    void Start()
    {
        if (targetMaterial != null)
        {
            UpdateHDRIntensity();
        }
        else
        {
            Debug.LogError("Aucun matériau assigné au script !");
        }
    }

    void Update()
    {
        // Ajustement interactif de l'intensité avec les touches "I" et "J"
        if (Input.GetKeyDown(KeyCode.I))
        {
            hdrIntensity = Mathf.Clamp(hdrIntensity + 1f, -10f, 10f); // Augmente l'intensité (limité à 10)
            UpdateHDRIntensity();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            hdrIntensity = Mathf.Clamp(hdrIntensity - 1f, -10f, 10f); // Diminue l'intensité (limité à -10)
            UpdateHDRIntensity();
        }
    }

    void UpdateHDRIntensity()
    {
        // Vérifie que le matériau possède une propriété "_EmissionColor"
        if (targetMaterial.HasProperty("_EmissionColor"))
        {
            // Applique la couleur blanche et ajuste l'intensité HDR
            Color hdrColor = hdrBaseColor * Mathf.Pow(2.0f, hdrIntensity);
            targetMaterial.SetColor("_EmissionColor", hdrColor);

            // Active l'émission dans le rendu si ce n'est pas déjà fait
            DynamicGI.SetEmissive(GetComponent<Renderer>(), hdrColor);
            Debug.Log($"Intensité HDR mise à jour : {hdrIntensity}, Couleur : {hdrBaseColor}");
        }
        else
        {
            Debug.LogError("Le matériau ne contient pas de propriété '_EmissionColor' !");
        }
    }
}
