using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Vector3 playerPosition; // Position du joueur
    public bool positionSaved = false; // Si une position a été sauvegardée

    private void Awake()
    {
        // Assurez-vous que le GameManager est persistant entre les scènes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            // Debug.LogWarning("GameManager already exists in the scene. Deleting duplicate.");
        }
    }
}
