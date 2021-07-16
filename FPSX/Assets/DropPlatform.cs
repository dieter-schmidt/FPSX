using System.Collections;
using System.Collections.Generic;
using FPSControllerLPFP;
using UnityEngine;

public class DropPlatform : MonoBehaviour
{
    //player
    public FpsControllerLPFP player;

    public bool isCollidingWithPlayer = false;
    //child collider (convex, not trigger)
    public GameObject platformCollider;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Handgun_01_FPSController").GetComponent<FpsControllerLPFP>();
        platformCollider = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(LayerMask.LayerToName(platformCollider.layer));
        Debug.Log(platformCollider.layer);
    }

    //changes platform layer to allow/disallow player drop through
    public void updateLayer()
    {
        Debug.Log("update layer");
        if (LayerMask.LayerToName(platformCollider.layer) == "DropPlatform")
        {
            platformCollider.layer = LayerMask.NameToLayer("Ground");
        }
        else
        {
            platformCollider.layer = LayerMask.NameToLayer("DropPlatform");
            isCollidingWithPlayer = false;
            //disable collider

        }
        Debug.Log(LayerMask.LayerToName(platformCollider.layer));

    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    Debug.Log("oncollisionexit");
    //    Debug.Log(LayerMask.LayerToName(collision.collider.gameObject.layer));
    //    if (LayerMask.LayerToName(collision.collider.gameObject.layer) == "Player")
    //    {
    //        isCollidingWithPlayer = true;
    //        player.setDropPlatform(this);
    //    }
    //}

    //private void OnCollisionExit(Collision collision)
    //{
    //    Debug.Log("oncollisionexit");
    //    //if (LayerMask.LayerToName(platformCollider.layer) == "DropPlatform")
    //    //{
    //    //    isCollidingWithPlayer = false;
    //    //    platformCollider.layer = LayerMask.NameToLayer("Ground");
    //    //}
    //}

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("ontriggerenter");
        Debug.Log(LayerMask.LayerToName(other.gameObject.layer));
        if (LayerMask.LayerToName(other.gameObject.layer) == "Player" && !isCollidingWithPlayer)
        {
            Debug.Log("ontriggerenter");
            isCollidingWithPlayer = true;
            player.setDropPlatform(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (LayerMask.LayerToName(platformCollider.layer) == "DropPlatform")
        {
            isCollidingWithPlayer = false;
            Debug.Log("ontriggerexit");
            platformCollider.layer = LayerMask.NameToLayer("Ground");
        }
    }

    public bool getIsCollidingWithPlayer()
    {
        return isCollidingWithPlayer;
    }
}
