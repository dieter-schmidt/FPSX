using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;
    public float gravity = -14.72f;//-9.81f;
    public float jumpHeight = 3f;

    //scale to gravity
    public float fastFallScale = 2;

    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;
    bool wasGrounded = false;

    Vector3 launchVelocity;

    // Update is called once per frame
    void Update()
    {
        //check previous grounded state from last call
        wasGrounded = isGrounded;
        //creates sphere with specified radius, and check if it collides with ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (isGrounded)
        {
            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * speed * Time.deltaTime);

            if (wasGrounded == false)
            {
                velocity.x = 0;
                velocity.z = 0;
            }
            else
            {
                //retrieve velocity on last frame before jump
                launchVelocity = move * speed;
            }
        }
        else
        {
            velocity = new Vector3(launchVelocity.x, velocity.y, launchVelocity.z);
            float fastFall = Input.GetAxis("Fire3");
            if (fastFall > 0)
            {
                velocity.y += gravity * fastFallScale * Time.deltaTime;
            }
        }
        //else
        //{
        //    velocity += launchVelocity;
        //}

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            //v = -2gh
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            //velocity.x = move.x;
            //velocity.z = move.z;
            velocity = new Vector3(launchVelocity.x, velocity.y, launchVelocity.z);
        }

        velocity.y += gravity * Time.deltaTime;

        //delta y = 1/2gt2
        controller.Move(velocity * Time.deltaTime);

        //Debug.Log("IsGrounded: " + isGrounded);
        //Debug.Log("Velocity: " + velocity);
        //Debug.Log("Fire3: "+Input.GetAxis("Fire3"));
    }
}
