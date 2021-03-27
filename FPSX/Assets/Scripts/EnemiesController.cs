using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesController : MonoBehaviour
{

    public AudioSource audioSource;
    public AudioSource snapAudioSource;
    public AudioSource humanAudioSource;
    private List<(Enemy, Vector3)> markedEnemies = new List<(Enemy, Vector3)>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(markedEnemies);   
    }

    public void snap()
    {
        foreach((Enemy, Vector3) e in markedEnemies){
            //playSnapAudioAtPosition(e.transform.position);
            //destroy enemy object
            e.Item1.SnapDie(e.Item2);
        }
        this.markedEnemies.Clear();
    }

    public void addMarkedEnemy((Enemy, Vector3) markedEnemy)
    {
        this.markedEnemies.Add(markedEnemy);
        //TODO - delete enemy
    }

    public void playAudioAtPosition(Vector3 position)
    {
        audioSource.transform.position = position;
        //Debug.Log("PLAY");
        audioSource.Play();

    }

    public void playSnapAudioAtPosition(Vector3 position)
    {
        snapAudioSource.transform.position = position;
        //Debug.Log("PLAY");
        snapAudioSource.Play();

    }

    public void playHumanAudioAtPosition(Vector3 position)
    {
        humanAudioSource.transform.position = position;
        //Debug.Log("PLAY");
        humanAudioSource.Play();

    }

}
