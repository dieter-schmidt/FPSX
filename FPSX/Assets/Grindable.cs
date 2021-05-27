using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPSControllerLPFP;

public class Grindable : MonoBehaviour
{

    public FpsControllerLPFP player;
    bool isColliding = false;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(gameObject.name);
        Debug.Log(transform.Find("Waypoints").gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        //call grind logic for player
        if (isColliding)
        {
            if (!player.isGrinding && Input.GetKeyDown(KeyCode.V))
            {
                Debug.Log("GRIND START");
                player.initiateGrind(gameObject);
                Debug.Log(gameObject.name);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("RAIL TRIGGER ENTER");
        //detect player collision
        if (other.gameObject.tag == "Player")
        {
            isColliding = true;
            //player.initiateGrind();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("RAIL TRIGGER EXIT");
    }
}
