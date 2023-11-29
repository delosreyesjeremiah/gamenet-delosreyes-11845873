using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    public float MovementSpeed
    {
        get => movementSpeed;
        set => movementSpeed = value;
    }

    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed = 300.0f;
    [SerializeField] GameObject fpsCamera;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        ProcessMovementInput();
        ProcessRotationInput();
    }

    private void ProcessMovementInput()
    {
        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            movement += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement -= transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement -= transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement += transform.right;
        }

        movement = movement.normalized * movementSpeed * Time.fixedDeltaTime;
        Vector3 newPosition = rb.position + movement;

        rb.MovePosition(newPosition);
    }

    private void ProcessRotationInput()
    {
        float mouseXInput = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up, mouseXInput * rotationSpeed * Time.deltaTime);
    }
}
