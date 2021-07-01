using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPSControllerLPFP;

public class Grindable : MonoBehaviour
{
    public FpsControllerLPFP player;
    public List<Waypoint> waypoints = new List<Waypoint>();

    private void Start()
    {
        //add all waypoints to list
        Waypoint[] childWaypoints = GetComponentsInChildren<Waypoint>();
        foreach(Waypoint waypoint in childWaypoints)
        {
            waypoints.Add(waypoint);
        }
    }

    public void initiateGrind(Waypoint waypoint)
    {
        //TODO - this indexDelta code is fucked up

        int wpIndex = waypoints.IndexOf(waypoint);
        //Debug.Log(wpIndex);
        Vector3 initialVel = player.getVelocity();
        int newWPIndex = -1;
        //direction of grind - used in player script
        int indexDelta;
        //initial grind speed
        float grindSpeed;
        float dot;
        float dotNormalized;

        //if player position is "past" the waypoint in direction of rail relative to player velocity, change starting waypoint by 1
        //do not check if at and end waypoint
        //set potential new waypoint
        if ((waypoints[wpIndex - 1].transform.position - waypoints[wpIndex].transform.position).magnitude >
            (waypoints[wpIndex + 1].transform.position - waypoints[wpIndex].transform.position).magnitude)
        {
            indexDelta = -1;
        }
        else
        {
            indexDelta = 1;
        }
        if (wpIndex > 0 && wpIndex < waypoints.Count - 1)
        {
            newWPIndex = wpIndex + indexDelta;
        }

        //check if player velocity is going towards new waypoint, else use the current waypoint
        if (newWPIndex != -1)
        {
            //get player velocity in relation to grind direction
            dot = Vector3.Dot(initialVel, waypoints[newWPIndex].transform.position - waypoints[wpIndex].transform.position);
            dotNormalized = Vector3.Dot(initialVel.normalized, (waypoints[newWPIndex].transform.position - waypoints[wpIndex].transform.position).normalized);
            //grindSpeed = dotNormalized * initialVel.magnitude;
            grindSpeed = Mathf.Abs(dotNormalized * initialVel.magnitude);
            //Debug.Log(dotNormalized);

            if (dot > 0f)
            {
                player.initiateGrind(this.waypoints, newWPIndex, indexDelta, grindSpeed);// indexDelta);
            }
            else
            {
                player.initiateGrind(this.waypoints, wpIndex, -indexDelta, grindSpeed);// indexDelta * -1) ;
            }
        }
        else
        {
            //get player velocity in relation to grind direction
            dot = Vector3.Dot(initialVel, waypoints[wpIndex - 1].transform.position - waypoints[wpIndex].transform.position);
            dotNormalized = Vector3.Dot(initialVel.normalized, (waypoints[wpIndex - 1].transform.position - waypoints[wpIndex].transform.position).normalized);
            //grindSpeed = dotNormalized * initialVel.magnitude;
            grindSpeed = Mathf.Abs(dotNormalized * initialVel.magnitude);
            Debug.Log(dotNormalized);

            if (dot > 0f)
            {
                indexDelta = -1;
                player.initiateGrind(this.waypoints, newWPIndex, indexDelta, grindSpeed);// indexDelta);
            }
            else
            {
                indexDelta = 1;
                player.initiateGrind(this.waypoints, wpIndex, indexDelta, grindSpeed);// indexDelta * -1) ;
            }
        }

        //Debug.Log(dot);
        //player.initiateGrind();
    }

    //public FpsControllerLPFP player;
    //bool isColliding = false;

    //// Start is called before the first frame update
    //void Start()
    //{
    //    //Debug.Log(gameObject.name);
    //    Debug.Log(transform.Find("Waypoints").gameObject.name);
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    //call grind logic for player
    //    if (isColliding)
    //    {
    //        if (!player.isGrinding && Input.GetKeyDown(KeyCode.V))
    //        {
    //            Debug.Log("GRIND START");
    //            player.initiateGrind(gameObject);
    //            Debug.Log(gameObject.name);
    //        }
    //    }
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("RAIL TRIGGER ENTER");
    //    //detect player collision
    //    if (other.gameObject.tag == "Player")
    //    {
    //        isColliding = true;
    //        //player.initiateGrind();
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    isColliding = false;
    //    Debug.Log("RAIL TRIGGER EXIT");
    //}
}
