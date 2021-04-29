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

    public float recoilLerpDuration = 0.20f;
    public float recenterLerpDuration = 0.30f;
    public float recoilRotation = 3f;
    public float recoilTimeElapsed = 0f;
    public float recenterTimeElapsed = 0f;

    private float newRecoilRotation = 0f;
    private float recoilRotationDelta = 0f;

    private float mouseDegreesRotated = 0f;
    private float degreesRotated = 0f;
    private float newRotationDelta = 0f;

    private bool isRecoiling = false;
    private bool isRecentering = false;



    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRecoiling)
        {
            UpdateRecoil();
        }
        else if (isRecentering)
        {
            UpdateRecenter();
        }

        //consistent across variable frame rate
        //float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        //float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        //normalize sensitivity when timescale changes
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * (1/Time.timeScale) * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (1/Time.timeScale) * Time.deltaTime;

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

        //get flip rotation from player script
        float flipRotationDelta = playerController.getDegreesRotated();
        Vector3 flipAxis = playerController.getWorldRotationAxis();
        float flipRotationIncrement = playerController.getNewRotationDelta();

        switch (gunController.fireMode)
        {

            case FireMode.Fixed:
                if (Input.GetButton("Fire2"))
                {
                    crossHairOrigin.transform.position = Input.mousePosition;
                }
                else
                {
                    crossHairContainer.transform.position = Input.mousePosition;

                    float x = Input.mousePosition.x - crossHairOrigin.transform.position.x;
                    float y = Input.mousePosition.y - crossHairOrigin.transform.position.y;
                    Vector3 originScreenPos = crossHairOrigin.transform.position;
                    crossHairContainerSecond.transform.position = originScreenPos - (Input.mousePosition - originScreenPos);
                }

                //flip rotation
                float currentRotationX = transform.parent.transform.localRotation.x;
                transform.parent.transform.localRotation = Quaternion.Euler(currentRotationX - flipRotationDelta, 0f, 0f);// Quaternion.Euler(xRotation - flipRotationDelta, 0f, 0f);

                break;

            case FireMode.Free:
                
                //used for ground dash only
                yRotation += mouseX;
                //yRotation = Mathf.Clamp(yRotation, -45f, 45f);

                if (!playerController.isGroundDash)
                {
                    if (playerController.getIsRotating() == true)
                    {
                        updateRotation();
                        Vector3 worldFlipRotation = new Vector3(flipAxis.x, 0f, flipAxis.z).normalized;
                        playerBody.Rotate(worldFlipRotation * -newRotationDelta, Space.World);
                        mouseDegreesRotated += newRotationDelta;
                        playerBody.Rotate(0f, mouseX, 0f, Space.Self);

                        //NEW BLOCK
                        xRotation -= mouseY;
                        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
                        transform.parent.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                    }
                    else
                    {
                        mouseDegreesRotated = 0f;

                        //NEW BLOCK
                        xRotation -= mouseY;
                        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
                        transform.parent.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                        playerBody.Rotate(Vector3.up * mouseX);
                    }
                }
                else
                {
                    if (isRecoiling || isRecentering)
                    {
                        xRotation -= recoilRotationDelta;
                        transform.parent.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                    }
                    else
                    {
                        transform.parent.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                    }
                }
                break;
        } 
    }


    public void ApplyRecoil()
    {
        if (isRecoiling)
        {
            //reset recoil vars and start new recoil
            //recoilRotationDelta = 0f;
            recoilTimeElapsed = 0f;
        }
        if (isRecentering)
        {
            //reset recenter vars
            recenterTimeElapsed = 0f;
            isRecentering = false;
        }
        recoilRotationDelta = 0f;
        isRecoiling = true;
        newRecoilRotation = 0f;
    }

    public void UpdateRecoil()
    {
        float oldRecoilRotation = newRecoilRotation;
        if (recoilTimeElapsed < recoilLerpDuration)
        {
            //upwards recoil
            newRecoilRotation = Mathf.Lerp(0f, recoilRotation, recoilTimeElapsed / recoilLerpDuration) * -1f;
            newRecoilRotation = Mathf.Clamp(newRecoilRotation, -90f, 90f);
            recoilRotationDelta = newRecoilRotation - oldRecoilRotation;
            recoilTimeElapsed += Time.deltaTime;
        }
        else
        {
            //end recoil state and reset vars
            isRecoiling = false;
            isRecentering = true;
            newRecoilRotation = 0f;
            recoilTimeElapsed = 0f;
        }
    }

    public void UpdateRecenter()
    {
        float oldRecoilRotation = newRecoilRotation;
        if (recenterTimeElapsed < recenterLerpDuration)
        {
            //recentering post-recoil
            newRecoilRotation = Mathf.Lerp(0f, recoilRotation, recenterTimeElapsed / recenterLerpDuration);
            newRecoilRotation = Mathf.Clamp(newRecoilRotation, -90f, 90f);
            recoilRotationDelta = newRecoilRotation - oldRecoilRotation;
            recenterTimeElapsed += Time.deltaTime;
        }
        else
        {
            //end recenter state and reset vars
            isRecentering = false;
            recenterTimeElapsed = 0f;
            newRecoilRotation = 0f;
        }
    }

    public void updateRotation()
    {
        float newRotation = Mathf.Clamp(540f * Time.deltaTime, 0f, 360f - degreesRotated);
        degreesRotated += newRotation;
        newRotationDelta = newRotation;

        if (degreesRotated == 360f)
        {
            playerController.setIsRotating(false);
            degreesRotated = 0f;
        }
    }
}
