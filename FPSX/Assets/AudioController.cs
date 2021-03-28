using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{

    public AudioSource audioSource;
    public float defaultVolume = 0.25f;
    public float defaultPitch = 0.50f;

    [SerializeField]
    private AudioClip jumpHoldClip;
    [SerializeField]
    private AudioClip superJumpClip;
    


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //float newVolume = audioSource.volume + (Time.timeScale * (3f / Time.deltaTime));
        float newPitch = audioSource.pitch + (Time.timeScale * (0.25f / Time.deltaTime));
        //Mathf.Clamp(audioSource.volume, newVolume, 1f);
        Mathf.Clamp(audioSource.pitch, newPitch, 1f);
        audioSource.pitch = Time.timeScale;
        audioSource.pitch = Time.timeScale;
    }

    public void Play(bool superJump)
    {
        //Debug.Log("JUMP HOLD PLAY");
        if (superJump)
        {
            audioSource.clip = superJumpClip;
        }
        else
        {
            audioSource.clip = jumpHoldClip;
        }
        audioSource.Play();
    }

    public void Stop()
    {
        //Debug.Log("JUMP HOLD STOP");
        audioSource.Stop();
        audioSource.volume = defaultVolume;
        audioSource.pitch = defaultPitch;
    }
}
