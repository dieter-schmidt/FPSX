using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFXController : MonoBehaviour
{
    CameraFilterPack_Drawing_Manga_Flash flash;
    CameraFilterPack_Blur_Radial radialBlur;
    CameraFilterPack_Color_GrayScale grayScale;
    CameraFilterPack_Blur_GaussianBlur gaussianBlur;
    // Start is called before the first frame update
    void Start()
    {
        //initialize FX variables
        flash = GetComponent<CameraFilterPack_Drawing_Manga_Flash>();
        radialBlur = GetComponent<CameraFilterPack_Blur_Radial>();
        grayScale = GetComponent<CameraFilterPack_Color_GrayScale>();
        gaussianBlur = GetComponent<CameraFilterPack_Blur_GaussianBlur>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void timedFX(string action, float length)
    {
        StartCoroutine(TimedFXCoroutine(action, length));
    }

    IEnumerator TimedFXCoroutine(string action, float length)
    {
        switch(action)
        {
            case "airdash":
                flash.enabled = true;
                radialBlur.enabled = true;
                break;
            case "gpland":
                gaussianBlur.enabled = true;
                break;
            default:
                break;
        }
        
        //play effect for set length
        yield return new WaitForSeconds(length);

        switch (action)
        {
            case "airdash":
                flash.enabled = false;
                radialBlur.enabled = false;
                break;
            case "gpland":
                gaussianBlur.enabled = false;
                break;
            default:
                break;
        }
    }

    public void startFX(string effect)
    {
        switch (effect)
        {
            case "grayscale":
                grayScale.enabled = true;
                break;
            default:
                break;
        }
    }

    public void stopFX(string effect)
    {
        switch (effect)
        {
            case "grayscale":
                grayScale.enabled = false;
                break;
            default:
                break;
        }
    }
}
