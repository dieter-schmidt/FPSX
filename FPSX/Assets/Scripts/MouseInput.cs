using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInput : MonoBehaviour
{
    public float mouseSensitivity = 75f;//100f;

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
        //float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        //float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        //normalize sensitivity when timescale changes
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * (1/Time.timeScale) * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (1/Time.timeScale) * Time.deltaTime;

        //Debug.Log(mouseY);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //this is used instead of rotate due to adding clamping
        transform.parent.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //Debug.Log(transform.localRotation.x);
        //Debug.Log(xRotation);

        //rotate around Y axis
        playerBody.Rotate(Vector3.up * mouseX);
        //transform.Rotate(transform.up * mouseX);
    }
}
