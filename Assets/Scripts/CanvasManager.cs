using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public GameObject canvas; // Référence au Canvas
    public PlayerMovement playerMovement; // Référence au script PlayerMovement

    void Start()
    {
        if(GameManager.Instance.positionSaved){
            canvas.SetActive(false);
            playerMovement.canMove = true;
        }
        else
        {
        // Afficher le Canvas et désactiver le mouvement du joueur au début
        canvas.SetActive(true);
        playerMovement.canMove = false;
        }
    }

    void Update()
    {
        // Vérifier si la touche Entrée est pressée
        if (canvas.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            CloseCanvas();
        }
    }

    public void CloseCanvas()
    {
        // Désactiver le Canvas et activer le mouvement du joueur
        canvas.SetActive(false);
        playerMovement.canMove = true;
    }
}
