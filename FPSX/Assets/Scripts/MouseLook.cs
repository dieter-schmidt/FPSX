using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public float mouseSensitivity = 100f;

    public Transform playerBody;

    float xRotation;

    // Start is called before the first frame update
    void Start()
    {
        //keep cursor centered on screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //consistent across variable frame rate
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        //this is usedinstead of rotate due to adding clamping
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //rotate around Y axis
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
