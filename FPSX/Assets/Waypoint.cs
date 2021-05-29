using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPSControllerLPFP;

public class Waypoint : MonoBehaviour
{
    public FpsControllerLPFP player;
    bool isColliding = false;
    private Grindable grindable;

    // Start is called before the first frame update
    void Start()
    {
        //assign parent rail
        grindable = GetComponentInParent<Grindable>();

        //assign player
        player = GameObject.Find("Handgun_01_FPSController").GetComponent<FpsControllerLPFP>();

        //Debug.Log(gameObject.name);
        //Debug.Log(transform.Find("Waypoints").gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        //call grind logic for player
        if (isColliding)
        {
            if (!player.isGrinding && Input.GetKeyDown(KeyCode.V))
            {
                //Debug.Log("GRIND START");

                //old
                //player.initiateGrind(gameObject);
                //new
                Debug.Log("INITIATE GRIND");
                grindable.initiateGrind(this);

                //Debug.Log(gameObject.name);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("RAIL TRIGGER ENTER");
        //detect player collision
        if (other.gameObject.tag == "Player")
        {
            isColliding = true;
            //player.initiateGrind();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isColliding = false;
        //Debug.Log("RAIL TRIGGER EXIT");
    }
}
