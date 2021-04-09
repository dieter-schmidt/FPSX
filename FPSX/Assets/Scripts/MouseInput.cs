using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPSModes;
using FPSControllerLPFP;

public class MouseInput : MonoBehaviour
{
    public float mouseSensitivity = 75f;//100f;

    public Transform playerBody;
    public GameObject crossHairContainer;
    public GameObject crossHairContainerSecond;
    public GameObject crossHairOrigin;
    public GameObject canvasFixed;
    public GameObject canvasFree;
    //public Canvas canvas;
    public Camera mainCamera;
    public HandgunScriptLPFP gunController;
    public FpsControllerLPFP playerController;
    public FireMode previousFireMode;

    float xRotation;
    float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        //canvas.worldCamera = mainCamera;
        //Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        //Cursor.visible = false;
        //keep cursor centered on screen
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.Confined;
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

        //double reticle inputs in fixed mode
        //bool moveReticleUp = Input.GetKey(KeyCode.W);
        //bool moveReticleDown = Input.GetKey(KeyCode.S);
        //bool moveReticleLeft = Input.GetKey(KeyCode.A);
        //bool moveReticleRight = Input.GetKey(KeyCode.D);

        //float x = Input.GetAxis("Horizontal");
        //float y = Input.GetAxis("Vertical");
        //Vector3 move = transform.forward * x + transform.right * y;


        //keep fire mode logic here to avoid async - 4/5
        switch (gunController.fireMode)
        {
            case FireMode.Fixed:
                if (previousFireMode == FireMode.Free)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    canvasFixed.SetActive(true);
                    canvasFree.SetActive(false);
                }
                break;
            case FireMode.Free:
                if (previousFireMode == FireMode.Fixed)
                {
                    //reset origin position to center
                    Vector3 originPos = crossHairOrigin.transform.position;
                    crossHairOrigin.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, originPos.z);

                    Cursor.lockState = CursorLockMode.Locked;
                    canvasFixed.SetActive(false);
                    canvasFree.SetActive(true);
                }
                break;
        }

        previousFireMode = gunController.fireMode;

        //move mouse cursor with fixed camera - 4/2
        //crossHairContainer.position = Input.mousePosition;
        //crossHairContainer.position = gunCamera.ViewportToScreenPoint(Input.mousePosition);
        //Event currentEvent = Event.current;
        //Vector2 mousePos = new Vector2();
        //mousePos.x = currentEvent.mousePosition.x;
        //mousePos.y = mainCamera.pixelHeight - currentEvent.mousePosition.y;
        switch (gunController.fireMode)
        {
            case FireMode.Fixed:
                if (Input.GetButton("Fire2"))
                {
                    //crossHairOrigin.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, (mainCamera.nearClipPlane + 0.01f)));// mainCamera.nearClipPlane + 1f)); + 1f));
                    crossHairOrigin.transform.position = Input.mousePosition;
                }
                else
                {
                    //crossHairContainer.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, (mainCamera.nearClipPlane + 0.01f)));// mainCamera.nearClipPlane + 1f));
                    //crossHairContainerSecond.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width - Input.mousePosition.x, Screen.height - Input.mousePosition.y, (mainCamera.nearClipPlane + 0.01f)));// mainCamera.nearClipPlane + 1f));
                    //crossHairContainer.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, (mainCamera.nearClipPlane + 0.01f)));// mainCamera.nearClipPlane + 1f));
                    crossHairContainer.transform.position = Input.mousePosition;

                    float x = Input.mousePosition.x - crossHairOrigin.transform.position.x;
                    float y = Input.mousePosition.y - crossHairOrigin.transform.position.y;
                    //if (x > 0)
                    //{
                    //Vector3 originScreenPos = mainCamera.WorldToScreenPoint(crossHairOrigin.transform.position);
                    Vector3 originScreenPos = crossHairOrigin.transform.position;
                    //Debug.Log(originScreenPos.x + ", " + Input.mousePosition.x);
                    //crossHairContainerSecond.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(originScreenPos.x - (Input.mousePosition.x - originScreenPos.x), originScreenPos.y - (Input.mousePosition.y - originScreenPos.y), (mainCamera.nearClipPlane + 0.01f)));// mainCamera.nearClipPlane
                    crossHairContainerSecond.transform.position = originScreenPos - (Input.mousePosition - originScreenPos);
                    //}
                    //else
                    //{
                    //    crossHairContainerSecond.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(crossHairOrigin.transform.position.x + crossHairOrigin.transform.position.x - Input.mousePosition.x, Input.mousePosition.y - crossHairOrigin.transform.position.y, (mainCamera.nearClipPlane + 0.01f)));// mainCamera.nearClipPlane
                    //}
                }
                
                //rotate fixed camera with wsad
                //xRotation -= move.x * mouseSensitivity * (1 / Time.timeScale) * Time.deltaTime;
                //xRotation = Mathf.Clamp(xRotation, -90f, 90f);
                //transform.parent.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                //playerBody.Rotate(Vector3.up * move.y * mouseSensitivity * (1 / Time.timeScale) * Time.deltaTime);

                break;

            case FireMode.Free:
                
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);

                //used for ground dash only
                yRotation -= mouseX;
                yRotation = Mathf.Clamp(yRotation, -45f, 45f);

                //transform.parent.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                if (!playerController.isGroundDash)
                {
                    transform.parent.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                    playerBody.Rotate(Vector3.up * mouseX);
                }
                else
                {
                    transform.parent.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                    //mainCamera.transform.localRotation = Quaternion.Euler(xRotation, -yRotation, 0f);
                    //transform.parent.parent.Rotate(Vector3.up * yRotation);
                }
                //}
                break;
        }
        //crossHairContainer.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, (mainCamera.nearClipPlane + 1) * 1f));// mainCamera.nearClipPlane + 1f));
        ////Debug.Log(Input.mousePosition);
        ////crossHairContainer.position += new Vector3(mouseY, mouseX, 0f);
        ////crossHairContainer.position = canvas.
        //Debug.Log(mainCamera.ScreenToWorldPoint(Input.mousePosition));

        //Debug.Log(mouseY);

        //xRotation -= mouseY;
        //xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //this is used instead of rotate due to adding clamping
        //transform.parent.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //Debug.Log(transform.localRotation.x);
        //Debug.Log(xRotation);

        //rotate around Y axis
        //playerBody.Rotate(Vector3.up * mouseX);
        //transform.Rotate(transform.up * mouseX);
    }


    public void ApplyRecoil()
    {
        xRotation -= 2.5f;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.parent.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
