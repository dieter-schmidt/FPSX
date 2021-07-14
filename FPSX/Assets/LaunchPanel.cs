using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPanel : MonoBehaviour
{
    public enum Orientation
    {
        Up,
        Forward,
        Back,
        Right,
        Left
    }
    public Orientation orientation;
    private Vector3 direction;
    public float launchSpeed = 60f;

    // Start is called before the first frame update
    void Start()
    {
        //backwards (direction of normal) (and slightly upwards)
        //TODO - make sure panel pivot points make z-direction forward
        //direction = new Quaternion(0f, 0f, 0f, 180f) * transform.right;
        switch (orientation)
        {
            case Orientation.Back:
                direction = Quaternion.AngleAxis(10f, transform.forward) * transform.right;
                break;
            case Orientation.Up:
                direction = transform.up;
                break;
            case Orientation.Forward:
                direction = Quaternion.AngleAxis(-10f, transform.forward) * -transform.right;
                break;
            case Orientation.Right:
                direction = Quaternion.AngleAxis(-10f, transform.right) * transform.forward;
                break;
            case Orientation.Left:
                direction = Quaternion.AngleAxis(10f, transform.right) * -transform.forward;
                break;

        }
        //direction = Quaternion.AngleAxis(10f, transform.forward) * transform.right;
        //direction = transform.up;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 getDirection()
    {
        return this.direction;
    }

    public float getLaunchSpeed()
    {
        return this.launchSpeed;
    }
}
