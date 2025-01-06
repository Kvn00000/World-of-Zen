using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public GameObject popupWindow; // Référence à la fenêtre popup
    public PlayerMovement playerMovement; // Référence au script PlayerMovement

    void Start()
    {
        // Afficher la popup et désactiver le mouvement du joueur au début
        popupWindow.SetActive(true);
        playerMovement.canMove = false;
    }

    void Update()
    {
        // Vérifier si la touche Entrée est pressée
        if (popupWindow.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            ClosePopup();
        }
    }

    public void ClosePopup()
    {
        // Désactiver la popup et activer le mouvement du joueur
        popupWindow.SetActive(false);
        playerMovement.canMove = true;
    }
}
