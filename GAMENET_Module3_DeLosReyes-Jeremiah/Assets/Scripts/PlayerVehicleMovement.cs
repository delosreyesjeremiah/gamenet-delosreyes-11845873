using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVehicleMovement : MonoBehaviour
{
    public float Speed = 20f;
    public float RotationSpeed = 200f;
    public float CurrentSpeed = 0f;

    public bool IsControlEnabled;

    private void Start()
    {
        IsControlEnabled = false;
    }

    private void LateUpdate()
    {
        if (IsControlEnabled)
        {
            float translation = Input.GetAxis("Vertical") * Speed * Time.deltaTime;
            float rotation = Input.GetAxis("Horizontal") * RotationSpeed * Time.deltaTime;

            transform.Translate(0, 0, translation);
            CurrentSpeed = translation;

            transform.Rotate(0, rotation, 0);
        }
    }
}
