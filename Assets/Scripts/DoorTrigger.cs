using UnityEngine;
using UnityEngine.SceneManagement; // For scene loading

public class DoorTrigger : MonoBehaviour
{
    public string interiorSceneName; // Name of the interior scene
    public string exteriorSceneName; // Name of the exterior scene
    public LayerMask doorLayer; // Layer mask to filter out only the door or trigger objects

    private bool firstTime = true;
    public bool inside = false;
    public GameObject player;
    public GameObject insidePos;
    public GameObject outsidePos;
    public Transform cameraPos; // The camera position to cast the ray from
    public float pointerDistance = 3f; // Raycast distance to detect doors
    public GameObject interactionCanvas; // Reference to the Canvas that will appear when the player detects the door
    public GameObject interiorCanva;
    private bool frontDetection; // Whether the raycast hits the door
    private bool paintDetection; // Whether the raycast hits the door

    public LayerMask paintLayer; // Layer mask to filter out only the door or trigger objects

    public GameObject paintCanva;

    void Update()
    {
        // Perform a raycast from the camera position in the forward direction
        frontDetection = Physics.Raycast(cameraPos.position, cameraPos.forward, pointerDistance, doorLayer);
        paintDetection = Physics.Raycast(cameraPos.position, cameraPos.forward, pointerDistance, paintLayer);

        // Visualize the raycast in the scene for debugging
        Debug.DrawRay(cameraPos.position, cameraPos.forward * pointerDistance, frontDetection ? Color.green : Color.red);

        if(interiorCanva.activeSelf == true && Input.GetKeyDown(KeyCode.E)){
            interiorCanva.SetActive(false);
        }

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

        if (paintDetection)
        {
            // Show the interaction Canvas
            if (!paintCanva.activeSelf)
            {
                paintCanva.SetActive(true);
            }

            // Check if the player presses 'E' to change the scene
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Player has triggered the door with Raycast and pressed 'E' to switch scenes!");
                ToggleScene();
                paintCanva.SetActive(false); // Hide the Canvas once the scene is changed
            }
        }
        else
        {
            // Hide the interaction Canvas when the player is not in front of the door
            if (paintCanva.activeSelf)
            {
                paintCanva.SetActive(false);
            }
        }
    }


    private void ToggleScene()
    {

        if (inside == false)
        {
            Debug.Log("Switching to interior : " + interiorSceneName);
            player.transform.position = insidePos.transform.position;
            inside = true;
            if(firstTime == true){
                interiorCanva.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Switching to exterior : " + exteriorSceneName);
            player.transform.position = outsidePos.transform.position;
            inside = false;
        }
    }
}
