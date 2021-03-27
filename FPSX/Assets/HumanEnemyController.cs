using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanEnemyController : MonoBehaviour
{

    public CharacterController controller;
    Animator anim;

    public float speed = 3f;
    public float gravity = -40f;
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    public bool isGrounded;
    public bool animChanged = false;

    public Vector3 velocity;

    private void Awake()
    {
        //Set the animator component
        anim = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        controller.detectCollisions = false;
        //Debug.Log(controller.detectCollisions);

        StartCoroutine(ChangeAnimCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (animChanged)
        {
            StartCoroutine(ChangeAnimCoroutine());
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        controller.Move(transform.forward * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        //delta y = 1/2gt2
        controller.Move(velocity * Time.deltaTime);
    }

    IEnumerator ChangeAnimCoroutine()
    {
        animChanged = false;

        if (gameObject.tag == "AnimatedEnemy")
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Sprint"))
            {
                yield return new WaitForSeconds(1f);
                anim.Play("Jump", 0, 0f);
                animChanged = true;
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
            {
                yield return new WaitForSeconds(1f);
                anim.Play("Idle01", 0, 0f);
                animChanged = true;
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle01"))
            {
                yield return new WaitForSeconds(0.25f);
                anim.Play("Sprint", 0, 0f);
                animChanged = true;
            }
        }

    }


}
