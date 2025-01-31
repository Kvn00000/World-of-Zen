using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour
{
    public float sensX;
    public float sensY;
    public Transform orientation;
    
    public bool playerRotation;
    private float xRotation;
    private float yRotation;

    private void Awake() {
        playerRotation = true;
    }

    private void Start(){
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = true;
    }


    void Update(){
        if(playerRotation){
            float mouseX = Input.GetAxisRaw("Mouse X") * sensX * Time.deltaTime;
            float mouseY = Input.GetAxisRaw("Mouse Y") * sensY * Time.deltaTime;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }
}
