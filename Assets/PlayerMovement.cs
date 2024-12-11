using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header('Movement')]
    public float moveSpeed;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    RigidBody rb;

    private Start(){
        rb = GetComponent<RigidBody>();
        rb.freezeRotation = true;
    }

    private void MyInput(){
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer(){
        moveDirection = orientation.forward*verticalInput + orientation.right * horizontalInput;
    }
}
