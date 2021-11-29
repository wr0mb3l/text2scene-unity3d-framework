using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class MouseControll : MonoBehaviour
{
    private CharacterController controller;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        // Add Character controller
        controller = gameObject.AddComponent<CharacterController>();
    }

    public float mouseSensitivity = 1.0f;
    public float moveSpeed = 1.0f;
    public bool invertX = false;
    public bool invertY = true;
    private float gravityValue = -9.81f;

    // Update is called once per frame
    void Update()
    {
        //Moving the camera rotation with the Mouse delta
        Vector3 mousePos = Mouse.current.delta.ReadValue();
        float movementX = mousePos.x;
        float movementY = mousePos.y;
        movementX /= Screen.height;
        movementY /= Screen.width;
        movementX *= mouseSensitivity;
        movementY *= mouseSensitivity;
        if(invertX) {
            movementX = -movementX;
        }
        if(invertY) {
            movementY = -movementY;
        }

        transform.RotateAround(transform.position, Vector3.up, movementX);
        transform.RotateAround(transform.position, transform.right, movementY);
        
        //Moving the player with the Keyboard
        Keyboard keyboard = Keyboard.current;

        float forward = keyboard.wKey.ReadValue() - keyboard.sKey.ReadValue();
        float horizontal = keyboard.dKey.ReadValue() - keyboard.aKey.ReadValue();
        //forward vector with removed up down angle (y)
        Vector3 forwardVec = new Vector3(transform.forward.x, 0, transform.forward.z);
        forwardVec = Vector3.Normalize(forwardVec);

        Vector3 transformPosition = Time.deltaTime * moveSpeed * (forwardVec * forward + transform.right * horizontal);

        // Apply gravity to character
        transformPosition.y += gravityValue * Time.deltaTime;
        
        // Move the character
        controller.Move(transformPosition);
    }
}
