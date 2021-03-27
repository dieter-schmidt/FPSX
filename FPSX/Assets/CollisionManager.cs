using FPSControllerLPFP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{

    public FpsControllerLPFP playerController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Collider: " + collision.gameObject.name);
    }

}
