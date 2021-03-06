using FPSControllerLPFP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderController : MonoBehaviour
{

    public FpsControllerLPFP playerController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Trigger Collided with " + other.gameObject.tag);
        playerController.OnTriggerColliderEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        playerController.OnTriggerColliderExit(other);
    }
}
