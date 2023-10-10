using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerMovementController : MonoBehaviour
{
    public Joystick Joystick
    {
        get => _joystick;
        set => _joystick = value;
    }

    public FixedTouchField FixedTouchField
    {
        get => _fixedTouchField;
        set => _fixedTouchField = value;
    }

    [SerializeField] private Joystick _joystick;
    [SerializeField] private FixedTouchField _fixedTouchField;

    private RigidbodyFirstPersonController _rigidbodyFirstPersonController;

    private Animator _animator;

    private void Start()
    {
        _rigidbodyFirstPersonController = GetComponent<RigidbodyFirstPersonController>();
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        _rigidbodyFirstPersonController.joystickInputAxis.x = _joystick.Horizontal;
        _rigidbodyFirstPersonController.joystickInputAxis.y = _joystick.Vertical;
        _rigidbodyFirstPersonController.mouseLook.lookInputAxis = _fixedTouchField.TouchDist;

        _animator.SetFloat("horizontal", _joystick.Horizontal);
        _animator.SetFloat("vertical", _joystick.Vertical);

        if (Mathf.Abs(_joystick.Horizontal) > 0.9 || Mathf.Abs(_joystick.Vertical) > 0.9)
        {
            _animator.SetBool("isRunning", true);
            _rigidbodyFirstPersonController.movementSettings.ForwardSpeed = 10;
        }
        else
        {
            _animator.SetBool("isRunning", false);
            _rigidbodyFirstPersonController.movementSettings.ForwardSpeed = 5;
        }
    }
}
