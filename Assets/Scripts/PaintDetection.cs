using UnityEngine;
using UnityEngine.SceneManagement; // For scene loading
using UnityEngine.UI; // For UI elements like text

public class PaintDetection : MonoBehaviour
{
    public LayerMask paint; // Layer mask to filter out only paint objects
    public Transform cameraPos; // The camera position to cast the ray from
    public float pointerDistance = 3f; // Raycast distance to detect paint
    public GameObject interactionCanvas; // Reference to the Canvas that will show the interaction text

    private bool frontDetection; // Whether the raycast hits the paint

    void Update()
    {
        // Perform a raycast from the camera position in the forward direction
        frontDetection = Physics.Raycast(cameraPos.position, cameraPos.forward, pointerDistance, paint);
        
        // Visualize the raycast in the scene for debugging
        Debug.DrawRay(cameraPos.position, cameraPos.forward * pointerDistance, frontDetection ? Color.green : Color.red);

        // If the paint is detected, show the Canvas with the "Press E" message
        if (frontDetection)
        {
            // Show the interaction Canvas and the message to press 'E'
            if (!interactionCanvas.activeSelf)
            {
                interactionCanvas.SetActive(true);
            }

            // Update the text message
            //interactionText.text = "Appuyer sur la touche E to change scene";

            // Check if the player presses 'E' to change the scene
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Player has detected paint and pressed 'E' to switch scenes!");
                ToggleScene();
                interactionCanvas.SetActive(false); // Hide the Canvas once the scene is changed
            }
        }
        else
        {
            // Hide the interaction Canvas if the player is not facing the paint
            if (interactionCanvas.activeSelf)
            {
                interactionCanvas.SetActive(false);
            }
        }
    }

    private void ToggleScene()
    {
        // Get the current scene name
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Logic to switch between scenes
        if (currentSceneName == "InteriorScene")
        {
            Debug.Log("Switching to respiration scene...");
            SceneManager.LoadScene("SceneRespiration2D"); // Replace with your actual exterior scene name
        }
        else
        {
            Debug.LogError("Current scene name does not match known scenes!");
        }
    }
}
