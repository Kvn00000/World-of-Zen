using UnityEngine;
using UnityEngine.SceneManagement; // For scene loading

public class DoorTrigger : MonoBehaviour
{
    public string interiorSceneName; // Name of the interior scene
    public string exteriorSceneName; // Name of the exterior scene
    public LayerMask doorLayer; // Layer mask to filter out only the door or trigger objects
    public Transform cameraPos; // The camera position to cast the ray from
    public float pointerDistance = 3f; // Raycast distance to detect doors
    public GameObject interactionCanvas; // Reference to the Canvas that will appear when the player detects the door

    private bool frontDetection; // Whether the raycast hits the door

    void Update()
    {
        // Perform a raycast from the camera position in the forward direction
        frontDetection = Physics.Raycast(cameraPos.position, cameraPos.forward, pointerDistance, doorLayer);
        
        // Visualize the raycast in the scene for debugging
        Debug.DrawRay(cameraPos.position, cameraPos.forward * pointerDistance, frontDetection ? Color.green : Color.red);

        // Display the Canvas when the door is detected and press 'E' to change the scene
        if (frontDetection)
        {
            // Show the interaction Canvas
            if (!interactionCanvas.activeSelf)
            {
                interactionCanvas.SetActive(true);
            }

            // Check if the player presses 'E' to change the scene
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Player has triggered the door with Raycast and pressed 'E' to switch scenes!");
                ToggleScene();
                interactionCanvas.SetActive(false); // Hide the Canvas once the scene is changed
            }
        }
        else
        {
            // Hide the interaction Canvas when the player is not in front of the door
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

        if (currentSceneName == exteriorSceneName)
        {
            Debug.Log("Switching to interior scene: " + interiorSceneName);
            SceneManager.LoadScene(interiorSceneName);
        }
        else if (currentSceneName == interiorSceneName)
        {
            Debug.Log("Switching to exterior scene: " + exteriorSceneName);
            SceneManager.LoadScene(exteriorSceneName);
        }
        else
        {
            Debug.LogError("Current scene name does not match known scenes!");
        }
    }
}
