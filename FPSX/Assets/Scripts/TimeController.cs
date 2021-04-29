using UnityEngine;
using FPSModes;

public class TimeController : MonoBehaviour
{

    //fixedDeltaTime - changes FixedUpdate step (too big will make slow motion less smooth)
    public float slowdownFactor = 0.5f;
    public float speedupFactor = 1.3f;
    public float normalFactor = 1f;

    //situational upper and lower time bound factors
    public float highFactor = 1f;
    public float lowFactor = 1f;

    //seconds it takes to increase timescale by 1
    public float startTransitionInterval = 1f;
    public float stopTransitionInterval = 1f;

    //camera FX
    public float fxStartTransitionInterval = 1f;
    public float fxStopTransitionInterval = 1f;
    public CameraFXController cameraFXController;

    private bool isDecreasing = false;
    private bool isIncreasing = false;
    private bool isStable = true;

    private void Start()
    {

    }

    public void Update()
    {
        isStable = !isDecreasing && !isIncreasing;
        //unscaled - not effected by timescale like regular deltatime is

        if (isDecreasing)
        {
            Time.timeScale -= (1f / startTransitionInterval) * Time.unscaledDeltaTime;
            float currentScale = Mathf.Clamp(Time.timeScale, lowFactor, highFactor);

            //if starting transition is complete, reset to normal time
            if (currentScale == lowFactor)
            {
                isDecreasing = false;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
        }
        else if (isIncreasing)
        {
            Time.timeScale += (1f / stopTransitionInterval) * Time.unscaledDeltaTime;
            float currentScale = Mathf.Clamp(Time.timeScale, lowFactor, highFactor);

            //if stopping transition is complete, reset to normal time
            if (currentScale == highFactor)
            {
                isIncreasing = false;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
        }
        
    }

    public void DecreaseMotion(TimeState startTime, TimeState endTime)
    {
        if (startTime == TimeState.Normal)
        {
            lowFactor = normalFactor;
            highFactor = speedupFactor;
        }
        else
        {
            highFactor = speedupFactor;
            if (startTime == TimeState.Normal)
            {
                lowFactor = normalFactor;
            }
            else
            {
                lowFactor = slowdownFactor;
            }
        }

        //apply transition state
        isDecreasing = true;
    }

    public void IncreaseMotion(TimeState startTime, TimeState endTime)
    {
        if (startTime == TimeState.Normal)
        {
            lowFactor = normalFactor;
            highFactor = speedupFactor;
        }
        else
        {
            lowFactor = slowdownFactor;
            if (endTime == TimeState.Normal)
            {
                highFactor = normalFactor;
            }
            else
            {
                highFactor = speedupFactor;
            }
        }

        //apply transition state
        isIncreasing = true;
    }

    public bool getIsStable()
    {
        return isStable;
    }
}
