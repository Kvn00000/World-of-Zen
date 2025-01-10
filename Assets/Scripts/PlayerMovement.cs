using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    private Rigidbody rb;

    [Header("Gameplay")]
    public bool canMove = true; // Nouvelle variable pour contrôler le mouvement

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Si une position est sauvegardée, appliquez-la
        if (GameManager.Instance.positionSaved)
        {
            transform.position = GameManager.Instance.playerPosition;
            GameManager.Instance.positionSaved = false;
        }
    }

    void Update()
    {
        // Bloquer les mouvements si canMove est false
        if (!canMove)
        {
            rb.velocity = Vector3.zero; // Stoppe le joueur complètement
            return;
        }

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        MyInput();
        SpeedControl();

        // Appliquer le drag si le joueur est au sol
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    void FixedUpdate()
    {
        // Ne pas appliquer de mouvement si canMove est false
        if (canMove)
        {
            MovePlayer();
        }
    }

    private void MyInput()
    {
        // Récupérer les entrées clavier
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        // Déplacer le joueur en fonction de l'orientation
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        // Limiter la vitesse du joueur
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}
