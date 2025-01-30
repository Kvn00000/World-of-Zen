using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasManager : MonoBehaviour
{



    [Header("Layers")]
    public LayerMask doorLayer;
    public LayerMask paintLayer;

    [Header("Canvas")]
    public GameObject interiorCanvas;
    public GameObject exteriorCanvas;
    public GameObject doorCanvas; 
    public GameObject paintCanvas;
    public GameObject PictureMenu;
    public GameObject ExerciceCanvas;

    [Header("Transform Position")]
    public Transform insidePos;
    public Transform outsidePos;
    public Transform mainCam; // The camera position to cast the ray from
    

    
    public GameObject player;
    public float pointerDistance = 3f; // Raycast distance to detect doors


    private bool doorDetection; // Whether the raycast hits the door
    private bool paintDetection; // Whether the raycast hits the paint

    private PlayerMovement playerMovement; // Référence au script PlayerMovement


    private bool firstTime = true;
    private bool inside = false;

    public Slider pictureSlider;
    public TextMeshProUGUI sliderValue;



    void Start()
    {
        // Afficher le Canvas et désactiver le mouvement du joueur au début
        exteriorCanvas.SetActive(true);
        playerMovement = GetComponent<PlayerMovement>();
        playerMovement.canMove = false;
        pictureSlider.onValueChanged.AddListener(UpdateSliderValue);
    }

    void UpdateSliderValue(float value)
    {
        sliderValue.text = value.ToString("0");
    }

    void Update()
    {
        
        doorDetection = Physics.Raycast(mainCam.position, mainCam.forward, pointerDistance, doorLayer);
        paintDetection = Physics.Raycast(mainCam.position, mainCam.forward, pointerDistance, paintLayer);

        Debug.DrawRay(mainCam.position, mainCam.forward * pointerDistance, doorDetection ? Color.green : Color.red);


        // Vérifier si la touche Entrée est pressée
        if (exteriorCanvas.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            CloseCanvas(exteriorCanvas);
        }

        if (interiorCanvas.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            CloseCanvas(interiorCanvas);
        }



        // Display the Canvas when the door is detected and press 'E' to change the scene
        if (doorDetection)
        {
            // Show the interaction Canvas
            if (!doorCanvas.activeSelf)
            {
                doorCanvas.SetActive(true);
            }

            // Check if the player presses 'E' to change the scene
            if (Input.GetKeyDown(KeyCode.E) && doorCanvas.activeSelf)
            {
                if(inside){
                    teleport(outsidePos);
                    inside = false;
                }else{
                    teleport(insidePos);
                    inside = true;
                }
                doorCanvas.SetActive(false); // Hide the Canvas once the scene is changed
            }
        }
        else
        {
            // Hide the interaction Canvas when the player is not in front of the door
            if (doorCanvas.activeSelf)
            {
                doorCanvas.SetActive(false);
            }
        }



        if (paintDetection)
        {
            // Show the interaction Canvas
            if (!paintCanvas.activeSelf)
            {
                paintCanvas.SetActive(true);
            }

            // Check if the player presses 'E' to change the scene
            if (Input.GetKeyDown(KeyCode.E) && paintCanvas.activeSelf)
            {

                playerMovement.canMove = false;
                PictureMenu.SetActive(true);
                //Turn on the respiration game
                paintCanvas.SetActive(false); // Hide the Canvas once the scene is changed
            }

            // if(quad.activeSelf &&Input.GetKeyDown(KeyCode.Return)){
            //     quad.SetActive(false);
            // }
        }
        else
        {
            // Hide the interaction Canvas when the player is not in front of the door
            if (paintCanvas.activeSelf)
            {
                paintCanvas.SetActive(false);
            }
        }

    }

    public void CloseCanvas(GameObject canv)
    {
        // Désactiver le Canvas et activer le mouvement du joueur
        canv.SetActive(false);
        playerMovement.canMove = true;
    }



    private void teleport(Transform pos)
    {
        player.transform.position = pos.position;
        if(firstTime == true){
            interiorCanvas.SetActive(true);
            playerMovement.canMove = false;

            firstTime = false;
        }
    }


    public void StartGame()
    {
        PictureMenu.SetActive(false);
        ExerciceCanvas.SetActive(true);
    }

    public void CloseGame()
    {
        ExerciceCanvas.SetActive(false);
        playerMovement.canMove = true;
    }


}
