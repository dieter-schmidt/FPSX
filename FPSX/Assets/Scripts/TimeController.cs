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
    public CameraFXController cameraFXController;

    //TODO - state variables for start transition and end transition to apply gradual changes
    //private bool isStarting = false;
    //private bool isStopping = false;
    private bool isDecreasing = false;
    private bool isIncreasing = false;

    public void Update()
    {
        //unscaled - not effected by timescale like regular deltatime is

        //Debug.Log(lowFactor + ", " + highFactor);

        if (isDecreasing)
        {
            Time.timeScale -= (1f / startTransitionInterval) * Time.unscaledDeltaTime;
            //float currentScale = Mathf.Clamp(Time.timeScale, slowdownFactor, 1f);
            float currentScale = Mathf.Clamp(Time.timeScale, lowFactor, highFactor);
            //if starting transition is complete, reset to normal time
            if (currentScale == lowFactor)
            {
                isDecreasing = false;
                //Time.timeScale = slowdownFactor;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
            //Time.timeScale = Mathf.Clamp(Time.timeScale, slowdownFactor, 1f);
        }
        else if (isIncreasing)
        {
            Time.timeScale += (1f / stopTransitionInterval) * Time.unscaledDeltaTime;
            //float currentScale = Mathf.Clamp(Time.timeScale, slowdownFactor, 1f);
            float currentScale = Mathf.Clamp(Time.timeScale, lowFactor, highFactor);
            //if stopping transition is complete, reset to normal time
            if (currentScale == highFactor)
            {
                isIncreasing = false;
                //Time.timeScale = 1f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
        }
        
    }

    public void DecreaseMotion(TimeState startTime, TimeState endTime)
    {
        //Time.timeScale = slowdownFactor;

        //default is 50fps
        //Time.fixedDeltaTime = Time.timeScale * 0.02f;

        //if (timeState == TimeState.Normal)
        //{
        //    highFactor = normalFactor;
        //    lowFactor = slowdownFactor;
        //}
        //else if (timeState == TimeState.Fast)
        //{
        //    highFactor = speedupFactor;
        //    lowFactor = slowdownFactor;
        //}

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
        //Time.timeScale = 1f;

        //default is 50fps
        //Time.fixedDeltaTime = Time.timeScale * 0.02f;

        //if (timeState == TimeState.Normal)
        //{
        //    lowFactor = normalFactor;
        //    highFactor = speedupFactor;
        //}
        //else if (timeState == TimeState.Slow)
        //{
        //    lowFactor = slowdownFactor;
        //    highFactor = speedupFactor;
        //}

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
}
