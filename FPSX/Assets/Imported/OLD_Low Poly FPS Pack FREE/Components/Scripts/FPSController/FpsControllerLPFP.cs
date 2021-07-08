using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using FPSModes;
using System.Collections.Generic;

namespace FPSControllerLPFP
{
    /// Manages a first person character
    //[RequireComponent(typeof(Rigidbody))]
    //[RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(AudioSource))]
    public class FpsControllerLPFP : MonoBehaviour
    {
        public CharacterController controller;

        public float accel = 32f;

        public float maxWalkSpeed = 18f;
        public float speed = 18f;//not used currently
        public float airDashSpeed = 50f;
        public float groundDashSpeed = 20f;
        public float skidSpeed;
        public float postAirDashSpeed = 27f;
        public float postAirDashJumpSpeed = 30f;
        public float runSpeed = 27f;
        public float airSpeed = 1f;
        public float slideSpeed = 35f;
        public float normalGravity = -20f;
        public float gravity = -20f;//-14.72f;//-9.81f;
        public float descentSpeed = 20f;
        public float glideDeltaVel = 0f;
        public float maxGlideSpeed = 50f;
        public float grindSpeed = 30f;
        //speed multiplier when entering grind
        public float grindSpeedMultiplier = 1.6f;
        //minimum grindSpeed
        public float minGrindSpeed = 5f;
        public float maxGrindSpeed;
        //grind speed from last grind - used to maintain speed between grinds
        public float lastGrindSpeed;
        public float finalGrindSpeed;
        //grinds started since last grounded state
        public int grindCount = 0;
        public float grindSlope;
        public bool grindStarting = false;
        public bool grindIsUphill = false;
        //sideways jump speed from grindable
        public float sideJumpSpeed = 16f;
        public float groundPoundMultiplier = 5f;
        public float wallSlideGravity;
        public float wallSlideGravMultiplier = 0.2f;
        public float jumpHeight = 3f;
        public float jumpVelocity = 20f;
        public float shortJumpHeight = 1.25f;
        public float slideLerpDuration = 0.5f;
        public float skidLerpDuration = 1f;
        public float grindLerpDuration = 0.25f;
        private (List<Waypoint> waypoints, int wpIndex, int indexDelta, float grindSpeed) grindData;
        private float timeElapsed = 0f;

        //scale to gravity
        public float fastFallScale = 2;
        public float gpSuperJumpInterval = 0.25f;
        public float tripleJumpInterval = 0.1f;
        public float superJumpMultiplier = 2f;
        public float tripleJumpMultiplier = 2.6f;

        public Transform groundCheck;
        public float groundDistance = 0.3f;
        public LayerMask groundMask;

        Vector3 velocity;
        bool isGrounded;
        bool wasGrounded = false;
        bool groundPoundStart = false;
        public bool isGroundPound = false;
        bool groundPounded = false;
        bool isAirDashing = false;
        //makes sure sprint is released after jumping before air dash is available
        bool sprintReleased = false;
        bool airDashReleased = true;
        bool wallJumped = false;
        bool isCollidingWithWall = false;
        bool isHoldingJump = false;
        bool isRotating = false;
        bool rotateFinished = false;
        bool jumpHoldAllowed = true;
        private Vector3 worldRotationAxis;
        bool isSliding = false;
        bool isGliding = false;
        bool isDescending = false;
        public bool isGrinding = false;
        public bool isGroundDash = false;
        public bool isSkidding = false;
        public bool isGrindLerp = false;
        bool jumped = false;

        //the wall the character jumped off of
        private int wallJumpedOffOf;
        public Vector3 preWallJumpVelocity;

        Vector3 launchVelocity;
        Vector3 airDashDirection;
        Vector3 groundDashDirection;
        Vector3 grindLerpVector;
        float grindLerpDistance = 0f;

        private int airDashesUsed = 0;
        private int airDashesAllowed = 2;//1;
        private float gpJumpStartTime;
        private float tripleJumpStartTime;
        private float degreesRotated = 0f;
        private float newRotationDelta = 0f;
        private Vector3 groundNormal = Vector3.zero;
        private Vector3 wallNormal = Vector3.zero;

        private GameObject currentRail;
        private List<Transform> railPoints = new List<Transform>();
        private int railPointIndex;
        private int wpIndexDelta;

        private int tripleJumpContacts = 0;


#pragma warning disable 649

        [Header("Arms")]
        [Tooltip("The transform component that holds the gun camera."), SerializeField]
        private Transform arms;

        [Tooltip("The position of the arms and gun camera relative to the fps controller GameObject."), SerializeField]
        private Vector3 armPosition;

        [Tooltip("The transform of the main camera"), SerializeField]
        private Transform mainCamera;

        [Tooltip("The gun camera"), SerializeField]
        private Camera gunCamera;

        [Header("Gun Controller")]
        [Tooltip("The gun controller"), SerializeField]
        private HandgunScriptLPFP gunController;

        [Tooltip("The camera effects controller"), SerializeField]
        public CameraFXController cameraFXController;

        [Header("Audio Clips")]
        [Tooltip("The audio clip that is played while walking."), SerializeField]
        private AudioClip walkingSound;

        [Tooltip("The audio clip that is played while running."), SerializeField]
        private AudioClip runningSound;

        [Tooltip("The audio clip that is played while grounded dashing."), SerializeField]
        private AudioClip groundDashSound;

        [Tooltip("The audio clip that is played when starting ground pound."), SerializeField]
        private AudioClip gpSound;

        [Tooltip("The audio clip that is played when landing from ground pound."), SerializeField]
        private AudioClip gpLandSound;

        [Tooltip("The audio clip that is played when starting an airdash"), SerializeField]
        private AudioClip airDashSound;

        [Tooltip("The audio clip that is played when jumping"), SerializeField]
        private AudioClip jumpSound;

        [Tooltip("The audio clip that is played when triple jumping"), SerializeField]
        private AudioClip tripleJumpSound;

        [Tooltip("The audio clip that is played when holding jump"), SerializeField]
        private AudioClip jumpHoldSound;

        [Tooltip("The audio clip that is played when sliding"), SerializeField]
        private AudioClip slideSound;

        [Tooltip("The audio clip that is played when sliding"), SerializeField]
        private AudioClip landSound;

        [Tooltip("The audio clip that is played when gliding"), SerializeField]
        private AudioClip glideSound;

        [Tooltip("The audio clip that is played when skidding"), SerializeField]
        private AudioClip skidSound;

        [Tooltip("The audio clip that is played when flipping"), SerializeField]
        private AudioClip flipSound;

        [Tooltip("The audio clip that is played when grinding"), SerializeField]
        private AudioClip grindSound;

        [Tooltip("The audio clip that is played when jumping from a grind"), SerializeField]
        private AudioClip grindJumpSound;

        [Header("Movement Settings")]
        [Tooltip("How fast the player moves while walking and strafing."), SerializeField]
        private float walkingSpeed = 5f;

        [Tooltip("How fast the player moves while running."), SerializeField]
        private float runningSpeed = 9f;

        [Tooltip("Approximately the amount of time it will take for the player to reach maximum running or walking speed."), SerializeField]
        private float movementSmoothness = 0.125f;

        [Tooltip("Amount of force applied to the player when jumping."), SerializeField]
        private float jumpForce = 35f;

		[Header("Look Settings")]
        [Tooltip("Rotation speed of the fps controller."), SerializeField]
        private float mouseSensitivity = 7f;

        [Tooltip("Approximately the amount of time it will take for the fps controller to reach maximum rotation speed."), SerializeField]
        private float rotationSmoothness = 0.05f;

        [Tooltip("Minimum rotation of the arms and camera on the x axis."),
         SerializeField]
        private float minVerticalAngle = -90f;

        [Tooltip("Maximum rotation of the arms and camera on the axis."),
         SerializeField]
        private float maxVerticalAngle = 90f;

        //[Tooltip("The names of the axes and buttons for Unity's Input Manager."), SerializeField]
        //private FpsInput input;
#pragma warning restore 649

        private Rigidbody _rigidbody;
        private CapsuleCollider _collider;
        private AudioSource _audioSource;
        public AudioController jumpAudioController;
        private SmoothRotation _rotationX;
        private SmoothRotation _rotationY;
        private SmoothVelocity _velocityX;
        private SmoothVelocity _velocityZ;
        private bool _isGrounded;

        private readonly RaycastHit[] _groundCastResults = new RaycastHit[8];
        private readonly RaycastHit[] _wallCastResults = new RaycastHit[8];

        private Vector3 lastPos;
        private Vector3 moved;
        private Vector3 playerVel;
        private int counter = 0;
        private Vector3 wallJumpDirection;
        private bool wallCollisionStarted = false;
        private float wallCollisionDot;

        /// Initializes the FpsController on start.
        private void Start()
        {
            wallSlideGravity = gravity;
            lastPos = transform.position;
            _audioSource = GetComponent<AudioSource>();
            _audioSource.clip = walkingSound;
            _audioSource.loop = true;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void FixedUpdate()
        {

        }
        private void LateUpdate()
        {
            if (rotateFinished)
            {
                isRotating = false;
                rotateFinished = false;
            }
        }

        /// Moves the camera to the character, processes jumping and plays sounds every frame.
        private void Update()
        {
            //Debug.Log("COUNT: "+grindCount);
            //Lerp slide camera
            //LerpSlideCamera();

            //Debug.Log("RPIndex = "+railPointIndex+", WPIndexDelta = "+wpIndexDelta);

            //wallTriggered = false;
            moved = controller.transform.position - lastPos;
            lastPos = controller.transform.position;
            playerVel = moved / Time.deltaTime;

            //turn on wall slide gravity if falling while contacting wall
            if (playerVel.y < 0 && wallCollisionStarted) 
            {
                wallSlideGravity = gravity * wallSlideGravMultiplier;
            }
            else
            {
                wallSlideGravity = gravity;
            }

            _audioSource.pitch = Time.timeScale;
            //PlayFootstepSounds();

            //check previous grounded state from last call
            wasGrounded = isGrounded;
            //creates sphere with specified radius, and check if it collides with ground
            //added grinding check - 5/2
            if (!isGrinding)
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            }
            else
            {
                GrindMove();
                ////grind logic

                //Vector3 grindMove;

                ////old logic - only handles two waypoints
                ////if (railPointIndex == 0)
                ////{
                ////    grindMove = (railPoints.ElementAt<Transform>(1).position - controller.transform.position).normalized * grindSpeed;
                ////}
                ////else
                ////{
                ////    grindMove = (railPoints.ElementAt<Transform>(0).position - controller.transform.position).normalized * grindSpeed;
                ////}

                ////new logic - handles many waypoints - 5/26
                ////if (playerVel.z > 0f)
                //if (transform.forward.z > 0f)
                //{
                //    Debug.Log("POSITIVE Z");
                //    if (railPointIndex > 0)
                //    {
                //        grindMove = (railPoints.ElementAt<Transform>(railPointIndex - 1).position - controller.transform.position).normalized * grindSpeed;

                //        // if player moves passes new waypoint, increment index
                //        if (transform.position.z + grindMove.z * Time.deltaTime >= railPoints.ElementAt<Transform>(railPointIndex - 1).position.z)
                //        {
                //            railPointIndex--;
                //        }
                //    }
                //    else
                //    {
                //        grindMove = Vector3.zero;
                //    }
                //}
                ////else if (playerVel.z < 0f)
                //else if (transform.forward.z < 0f)
                //{
                //    Debug.Log("NEGATIVE Z");
                //    Debug.Log("RPCOUNT = " + railPoints.Count);
                //    if (railPointIndex < railPoints.Count - 1)
                //    {
                //        grindMove = (railPoints.ElementAt<Transform>(railPointIndex + 1).position - controller.transform.position).normalized * grindSpeed;

                //        // if player moves passes new waypoint, increment index
                //        if (transform.position.z + grindMove.z * Time.deltaTime <= railPoints.ElementAt<Transform>(railPointIndex + 1).position.z && railPointIndex < railPoints.Count - 1)
                //        {
                //            railPointIndex++;
                //        }
                //    }
                //    else
                //    {
                //        grindMove = Vector3.zero;
                //    }
                //}
                //else
                //{
                //    grindMove = Vector3.zero;
                //}

                //launchVelocity = playerVel;
                //controller.Move(grindMove * Time.deltaTime);

                //Debug.Log(railPointIndex);
            }

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -1f;//-2f;
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            Vector3 footPosition = new Vector3(transform.position.x, transform.position.y - ((controller.height / 2) - controller.skinWidth), transform.position.z);
            //TODO - add code to check for ground normal and project onto plane
            if (isGrounded)
            {
                grindCount = 0;
                //Vector3 footPosition = new Vector3(transform.position.x, transform.position.y - ((controller.height / 2) - controller.skinWidth), transform.position.z);
                //check when grounded and slightly leaving the ground (running down a slope for example)
                RaycastHit hit;
                if (Physics.Raycast(footPosition, -transform.up, out hit, 0.2f))// 5f))
                {
                    groundNormal = hit.normal;

                    //slide logic
                    //if (Input.GetKey(KeyCode.Q) && (Math.Abs(groundNormal.z) < 0.10f || Math.Abs(groundNormal.x) < 0.10f))
                    if (Input.GetKey(KeyCode.Q) && Math.Abs(groundNormal.y) < 0.95f)
                    {
                        Vector3 left = Vector3.Cross(hit.normal, Vector3.up);
                        move = Vector3.Cross(hit.normal, left).normalized;
                        //Debug.Log(move);
                        if (!isSliding)
                        {
                            timeElapsed = 0f;
                            PlaySlideSound();
                            //setSlideCamera(true);
                        }
                        isSliding = true;
                    }
                    else// if (groundNormal.z < 0.01f)
                    {
                        move = Vector3.ProjectOnPlane(move, groundNormal);
                        if (isSliding)
                        {
                            timeElapsed = 0f;                            
                            //setSlideCamera(false);
                        }
                        isSliding = false;
                    }
                }
                else
                {

                }
            }
            //check for grindrail - 5/2
            else if (!isGrinding && Input.GetKeyDown(KeyCode.V))
            {
                //RaycastHit hit;
                //if (Physics.Raycast(footPosition, -transform.up, out hit, 10f))
                //{
                //    if (hit.transform.gameObject.tag == "Grindable")
                //    {
                //        currentRail = GameObject.Find("Waypoints");
                //        railPoints.Clear();
                //        //transform.position = GameObject.Find("wp2").transform.position;

                //        float distanceToWaypoint = 100f;
                //        Transform startingWayPoint = GameObject.Find("Waypoints").transform.GetChild(0);

                //        foreach (Transform wayPoint in GameObject.Find("Waypoints").transform)
                //        {
                //            railPoints.Add(wayPoint);

                //            //find closest waypoint
                //            float newDistance = Vector3.Distance(transform.position, wayPoint.position);
                //            if (newDistance < distanceToWaypoint && transform.position.y > wayPoint.position.y)
                //            {
                //                distanceToWaypoint = newDistance;
                //                startingWayPoint = wayPoint;
                //                railPointIndex = railPoints.IndexOf(startingWayPoint);
                //            }
                //        }

                //        counter++;
                //        Debug.Log("GRIND HIT " + counter);

                //        //transform.position = startingWayPoint.position;
                //        controller.enabled = false;
                //        controller.transform.position = startingWayPoint.position;
                //        controller.enabled = true;
                //        isGrinding = true;
                //        //GameObject.Find("SparksEffect").SetActive(true);
                //        PlayGrindSound();
                //        tripleJumpContacts = 0;
                //        isGrounded = false;
                //        jumped = false;
                //        launchVelocity = Vector3.zero;
                //    }
                //    else
                //    {
                //        //Debug.Log("NO GRIND");
                //    }
                //}
                ////Debug.Log(transform.position);
            }

            PlayFootstepSounds(x,z);

            if (isGrounded)
            {
                sprintReleased = false;
                float run = Input.GetAxis("Fire3");
                if (Input.GetKey(KeyCode.E))
                {
                    if (!isGroundDash)
                    {
                        groundDashDirection = transform.forward;
                        isGroundDash = true;
                        PlayGroundDashSound();
                        cameraFXController.startFX("grounddash");
                    }
                }
                else
                {
                    //transition to skid state - 4/14
                    if (isGroundDash)
                    {
                        isSkidding = true;
                        timeElapsed = 0f;
                        PlaySkidSound();
                        StartCoroutine(SkidCoroutine());
                    }
                    isGroundDash = false;
                    cameraFXController.stopFX("grounddash");
                }

                if (isSliding)
                {
                    controller.Move(move * slideSpeed * Time.deltaTime);
                }
                else if (isGroundDash)
                {
                    controller.Move(groundDashDirection * groundDashSpeed * Time.deltaTime);
                }
                else if (isSkidding)
                {
                    float newSkidSpeed;
                    if (timeElapsed < skidLerpDuration)
                    {
                        newSkidSpeed = Mathf.Lerp(skidSpeed, 0f, timeElapsed/skidLerpDuration);
                        controller.Move(groundDashDirection * newSkidSpeed * Time.deltaTime);
                        timeElapsed += Time.deltaTime;
                    }
                }
                else
                {
                    //only sprint if going forward or diagonal forward - 4/13
                    if (run > 0 && z > 0.4f)
                    {
                        controller.Move(move * runSpeed * Time.deltaTime);
                    }
                    else
                    {
                        controller.Move(move * speed * Time.deltaTime);
                    }
                }

                if (wasGrounded == false)
                {
                    //reset gliding status
                    glideDeltaVel = 0f;
                    //velocity.y = launchVelocity.y;
                    //Debug.Log(velocity.y);
                    isGliding = false;
                    //temporary solution for regaining airdash after colliding with wall but not jumping
                    wallCollisionStarted = false;
                    //reset wall slide gravity if player hits wall but never jumped
                    wallSlideGravity = gravity;

                    jumped = false;

                    if (isGroundPound == true)
                    {
                        PlayGPLandSound();
                        cameraFXController.timedFX("gpland", 0.07f);
                        isGroundPound = false;
                    }
                    else
                    {
                        PlayLandSound();
                    }

                    airDashesUsed = 0;
                    wallJumped = false;
                    velocity.x = 0;
                    velocity.z = 0;
                    gravity = normalGravity;

                    //GP super jump logic - check link timing
                    if (Time.time - gpJumpStartTime <= gpSuperJumpInterval)
                    {
                        //prevent air jump after super jump
                        jumped = true;

                        //super jump
                        jumpAudioController.Play(true);
                        velocity.y = Mathf.Sqrt(jumpHeight * superJumpMultiplier * -2f * gravity);
                        launchVelocity = velocity;
                        jumpHoldAllowed = false;

                        //flip
                        if (Input.GetKey(KeyCode.Mouse2))
                        {
                            isRotating = true;
                            PlayFlipSound();
                        }

                        tripleJumpContacts = 0;
                        groundPounded = false;
                    }
                    else if (!groundPounded)
                    {
                        //triple jump
                        if (Time.time - tripleJumpStartTime <= tripleJumpInterval)
                        {
                            tripleJumpContacts++;
                            if (tripleJumpContacts == 3)
                            {
                                velocity.y = Mathf.Sqrt(jumpHeight *  tripleJumpMultiplier * -2f * gravity);
                                //launchVelocity = controller.velocity;
                                launchVelocity = playerVel;
                                tripleJumpContacts = 0;
                                jumpHoldAllowed = false;
                                PlayTripleJumpSound(1f);
                            }
                            else
                            {
                                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                                //4/27
                                tripleJumpStartTime = Time.time;
                                PlayJumpSound(1f);
                            }

                            //flip - added 4/28
                            if (Input.GetKey(KeyCode.Mouse2))
                            {
                                isRotating = true;
                                PlayFlipSound();
                            }

                            //this will maintain standard airdash momentum during/after triple jump sequence
                            jumped = true;
                        }
                        else
                        {
                            //reset triple jump count
                            tripleJumpContacts = 0;
                        }
                    }
                }
                else
                {
                    groundPounded = false;
                    tripleJumpContacts = 0;
                    jumpHoldAllowed = true;
                    //retrieve velocity on last frame before jump
                    launchVelocity = controller.velocity;// velocity;
                }
            }
            else
            {
                //grind lerp - 6/30/21
                if (isGrindLerp)
                {
                    //teleport instead of lerp if entering grind from gp
                    if (isGroundPound)
                    {
                        isGroundPound = false;
                        controller.transform.position = grindData.waypoints[grindData.wpIndex].transform.position;
                        isGrindLerp = false;
                        grindLerpDistance = 0f;

                        //TODO - get player velocity before ground pound and assign to grindspeed OR allow for direction input with fixed initial grind speed

                        startGrind();
                    }
                    else
                    {
                        //float newSkidSpeed;
                        //if (timeElapsed < skidLerpDuration)
                        //{
                        //    newSkidSpeed = Mathf.Lerp(skidSpeed, 0f, timeElapsed / skidLerpDuration);
                        //    controller.Move(groundDashDirection * newSkidSpeed * Time.deltaTime);
                        //    timeElapsed += Time.deltaTime;
                        //}
                        //move character in direction of first waypoint
                        //Vector3 motion = Mathf.Lerp(0f, grindLerpVector.magnitude, timeElapsed / grindLerpDuration) * grindLerpVector.normalized;
                        //Vector3 motion = grindLerpVector.normalized * grindLerpVector.magnitude * Time.deltaTime/grindLerpDuration;
                        //Vector3 motion = grindLerpVector.normalized * grindLerpVector.magnitude * Time.deltaTime * 0.01f;
                        //speed = d/t * deltaT
                        Vector3 motion = (grindLerpVector.magnitude / grindLerpDuration) * grindLerpVector.normalized * Time.deltaTime;
                        controller.Move(motion);
                        grindLerpDistance += motion.magnitude;
                        //Debug.Log("Lerp Dist: " + grindLerpDistance);
                        //Debug.Log("Lerp Vector mag: " + grindLerpVector.magnitude);
                        //Debug.Log("Difference: " + (grindLerpDistance - grindLerpVector.magnitude));
                        if (Mathf.Abs(grindLerpDistance - grindLerpVector.magnitude) <= 1f)
                        {
                            isGrindLerp = false;
                            grindLerpDistance = 0f;
                            //start grind
                            Debug.Log("START GRIND");
                            startGrind();

                        }
                    }
                }

                ////triple jump timing
                if (Input.GetButtonDown("Jump"))
                {
                    tripleJumpStartTime = Time.time;
                }

                if (Input.GetAxis("Fire3") == 0)
                {
                    sprintReleased = true;
                }

                if (wasGrounded)
                {
                    if (isSliding)
                    {
                        isSliding = false;
                        timeElapsed = 0f;
                        //setSlideCamera(false);
                    }
                    if (_audioSource.clip == walkingSound || _audioSource.clip == runningSound)
                    {
                        _audioSource.Pause();
                    }
                    velocity = new Vector3(launchVelocity.x, velocity.y, launchVelocity.z);
                }
                else
                {
                    //glide logic - 4/10
                    if (Input.GetKey(KeyCode.X))
                    {

                        if (!isGroundPound && !groundPoundStart)
                        {
                            if (!isGliding)
                            {
                                cameraFXController.startFX("glide");
                                PlayGlideSound();
                                glideDeltaVel = 0f;
                            }

                            float groundDot = Vector3.Dot(Vector3.down, gunCamera.transform.forward);
                            isGliding = true;

                            //looking below world horizontal
                            if (groundDot > 0f)
                            {
                                //descend
                                isDescending = true;
                                glideDeltaVel += groundDot * descentSpeed;
                                controller.Move(Mathf.Min(glideDeltaVel, maxGlideSpeed) * gunCamera.transform.forward * Time.deltaTime);
                            }
                            else
                            {
                                //ascend - reflect velocity.y when switching from ascend to descend
                                isDescending = false;
                                glideDeltaVel += groundDot * descentSpeed;//1f+groundDot
                                controller.Move(Mathf.Min(glideDeltaVel, maxGlideSpeed) * gunCamera.transform.forward * Time.deltaTime);
                            }

                            launchVelocity = Mathf.Min(glideDeltaVel, maxGlideSpeed) * gunCamera.transform.forward;
                        }
                    }
                    else
                    {
                        glideDeltaVel = 0f;
                        isGliding = false;
                        cameraFXController.stopFX("glide");
                    }

                    //Air Dash Logic
                    if (Input.GetAxis("Fire3") > 0)
                    {
                        if (!isAirDashing && airDashesUsed < airDashesAllowed && sprintReleased && !isGroundPound && !groundPoundStart && airDashReleased)
                        {
                            airDashReleased = false;
                            isGrinding = false;
                            //GameObject.Find("SparksEffect").SetActive(false);
                            StartCoroutine(AirDashCoroutine());
                        }
                    }
                    else
                    {
                        //force air dash button release before repress - 4/14
                        airDashReleased = true;
                    }
                    if (isAirDashing && !wallCollisionStarted)
                    {
                        controller.Move(airDashDirection * airDashSpeed * Time.deltaTime);
                    }

                    //Ground Pound
                    if (Input.GetKeyDown(KeyCode.Q) && !isGrinding && !isGrindLerp)
                    {
                        StartCoroutine(GroundPoundCoroutine());
                    }

                    //Ground Pound Timed Super Jump
                    //record time of jump input during GP
                    if (isGroundPound)
                    {
                        if (Input.GetButtonDown("Jump"))
                        {
                            gpJumpStartTime = Time.time;
                        }
                    }

                    //Only allow air control for sideways and backwards movement post jump
                    //TODO - add forward air control if launchvelocity is less than airspeed
                    if (!groundPoundStart && !isGroundPound && !isAirDashing && !isGliding && !isGrinding && !isGrindLerp)// && !isCollidingWithWall)
                    {
                        velocity = new Vector3(launchVelocity.x, velocity.y, launchVelocity.z);

                        float dot = Vector3.Dot(new Vector3(move.x, 0f, move.z).normalized, new Vector3(launchVelocity.x, 0f, launchVelocity.z).normalized);

                        if (dot <= 0.1)
                        {
                            //Debug.Log("BEHIND OR SIDE");
                            //only air control to the sides or back from initial jump orientation
                            //full airspeed directly lateral (1 - Mathf.Abs(dot), floor is airspeed fraction
                            //Debug.Log("AIR CONTROL");
                            controller.Move(move * airSpeed * ((1 - Mathf.Abs(dot))+0.75f) * Time.deltaTime);
                        }
                        else
                        {
                            //Debug.Log("FORWARD");
                        }

                        bool fastFallPressed = Input.GetButtonDown("Jump");// Input.GetAxis("Jump");
                        if (fastFallPressed && velocity.y <= 0 && !wallCollisionStarted && jumped)
                        {
                            gravity *= fastFallScale;
                        }

                        //hold jump logic - added check that disallows jump hold after triple jump, etc.
                        if (Input.GetButton("Jump") && velocity.y > 0 && jumpHoldAllowed)// && !wallCollisionStarted)
                        {
                            velocity.y += jumpVelocity * Time.deltaTime;
                        }
                        else
                        {
                            //Debug.Log("JUMP NOT HELD");
                        }
                    }
                }
            }

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                if (!isHoldingJump)
                {
                    isHoldingJump = true;
                    jumpAudioController.Play(false);
                }
            }

            //added triple jump check for dkc jump (prevents airjumping during triple jump string)
            if (Input.GetButtonDown("Jump") && !jumped && tripleJumpContacts == 0)//isGrounded)
            {
                //Debug.Log("JUMP TEST");

                //change grinding state
                if (isGrinding)
                {
                    PlayGrindJumpSound();
                    isGrinding = false;
                    //GameObject.Find("SparksEffect").SetActive(false);

                    //TODO - directional THPS jumping from grind - 5/30
                    //x - horiz input, z - vert input
                    Vector3 horizontalPlayerVel = new Vector3(playerVel.x, 0f, playerVel.z);
                    //if not holding straight back or neutral or forward, jump sideways
                    //Debug.Log(move);
                    if (x != 0 && Vector3.Dot(horizontalPlayerVel.normalized, move.normalized) > -1f)
                    {
                        //calculate the two vectors perpendicular to player velocity, parallel to ground
                        Vector3 p1 = Quaternion.AngleAxis(90f, Vector3.up) * horizontalPlayerVel;
                        Vector3 p2 = Quaternion.AngleAxis(-90f, Vector3.up) * horizontalPlayerVel;

                        //set horizontal grind jump direction
                        if (Vector3.Dot(p1, move) > Vector3.Dot(p2, move))
                        {
                            velocity = (4f * playerVel + p1).normalized * finalGrindSpeed;// grindSpeed;// playerVel.magnitude;
                            //velocity.x += p1.normalized.x * sideJumpSpeed;
                            //velocity.z += p1.normalized.z * sideJumpSpeed;
                            //launchVelocity = playerVel.normalized* grindSpeed;
                        }
                        else
                        {
                            velocity = (4f * playerVel + p2).normalized * finalGrindSpeed;// grindSpeed;// playerVel.magnitude;
                            //velocity.x += p2.normalized.x * sideJumpSpeed;
                            //velocity.z += p2.normalized.z * sideJumpSpeed;
                            //launchVelocity = playerVel.normalized * grindSpeed;
                        }
                        launchVelocity.x = velocity.x;
                        launchVelocity.z = velocity.z;
                        //controller.Move(new Vector3(velocity.x, 0f, velocity.z));

                    }
                }
                else
                {
                    PlayJumpSound(1f);
                }

                ////triple jump counter
                if (tripleJumpContacts == 0)
                {
                    tripleJumpContacts++;
                }

                //DKC roll-midair jump logic
                jumped = true;

                //PlayJumpSound(1f);
                //v = -2gh
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                //flip
                if (Input.GetKey(KeyCode.Mouse2))
                {
                    isRotating = true;
                    PlayFlipSound();
                    //set initial rotation direction
                    worldRotationAxis = transform.right;
                }
            }
            
            else if (Input.GetKeyDown(KeyCode.CapsLock) && isGrounded)
            {
                //v = -2gh
                velocity.y = Mathf.Sqrt(shortJumpHeight * -2f * gravity);
            }

            if (!groundPoundStart && !isGroundPound &&!isAirDashing && !wallCollisionStarted && !isGliding && !isGrinding &&!isGrindLerp)
            {
                velocity.y += gravity * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime);
            }
            if (isGroundPound)
            {
                velocity.y += gravity * groundPoundMultiplier * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime);
            }
            //wall slide gravity change
            else if (wallCollisionStarted)
            {
                velocity.y += wallSlideGravity * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime);
            }

            if ((velocity.y <= 0 || Input.GetButtonUp("Jump")) && isHoldingJump)
            {
                isHoldingJump = false;
                jumpAudioController.Stop();
            }
        }

        private void GrindMove()
        {
            //grind logic

            Vector3 grindMove;
            //float finalGrindSpeed = finalGrindSpeed = Mathf.Min(Mathf.Max(grindSpeed * grindSpeedMultiplier, minGrindSpeed), maxGrindSpeed);
            if (grindStarting)
            {
                //initial speed upon entering grind
                finalGrindSpeed = Mathf.Min(Mathf.Max(grindSpeed * grindSpeedMultiplier, minGrindSpeed), maxGrindSpeed);
                grindStarting = false;
            }

            //change speed based on slope of grind
            float prevGrindSpeed = finalGrindSpeed;
            if (grindIsUphill)
            {
                finalGrindSpeed -= grindSlope * 150f * Time.deltaTime;
            }
            else
            {
                finalGrindSpeed += grindSlope * 150f * Time.deltaTime;
            }

            //change direction
            if (prevGrindSpeed > 0f && finalGrindSpeed <= 0f)
            {
                grindIsUphill = !grindIsUphill;
                wpIndexDelta *= -1;
            }

            Debug.Log("GSPEED: " + finalGrindSpeed);

            //ending waypoint checks
            if (!((railPointIndex == 0 && wpIndexDelta == -1) || (railPointIndex == railPoints.Count - 1 && wpIndexDelta == 1)))
            {
                //grindMove = (railPoints[railPointIndex].position - railPoints[railPointIndex + wpIndexDelta].position).normalized * grindSpeed;
                //finalGrindSpeed = Mathf.Min(Mathf.Max(grindSpeed * grindSpeedMultiplier, minGrindSpeed), maxGrindSpeed);
                grindMove = (railPoints[railPointIndex + wpIndexDelta].position - railPoints[railPointIndex].position).normalized * finalGrindSpeed;

                // if player moves passes new waypoint, update index ( - could use refactoring
                //added this if statement check because of null index error
                //old
                //if (!((railPointIndex <= 1 && wpIndexDelta == -1) || (railPointIndex >= railPoints.Count - 2 && wpIndexDelta == 1)))
                //{
                //    float distanceToNext = (transform.position + (grindMove * Time.deltaTime) - railPoints[railPointIndex + wpIndexDelta].position).magnitude;
                //    float distanceToSecond = (transform.position + (grindMove * Time.deltaTime) - railPoints[railPointIndex + wpIndexDelta * 2].position).magnitude;
                //    //if (distanceToSecond < distanceToNext)
                //    if (distanceToSecond < distanceToNext)
                //    {
                //        railPointIndex += wpIndexDelta;
                //    }
                //}

                //new
                float distanceToNext = (railPoints[railPointIndex + wpIndexDelta].position - (transform.position + (grindMove * Time.deltaTime))).magnitude;
                if (Mathf.Abs(distanceToNext) < 0.5f)
                {
                    railPointIndex += wpIndexDelta;
                }

            }
            else
            {
                grindMove = Vector3.zero;
                isGrinding = false;
            }

            launchVelocity = playerVel.normalized * finalGrindSpeed;
            //launchVelocity = playerVel.normalized * grindSpeed * grindSpeedMultiplier;
            //launchVelocity = playerVel.normalized * Mathf.Max(grindSpeed * grindSpeedMultiplier, minGrindSpeed);
            controller.Move(grindMove * Time.deltaTime);

            //old
            ////new logic - handles many waypoints - 5/26
            ////if (playerVel.z > 0f)
            //if (transform.forward.z > 0f)
            //{
            //    Debug.Log("POSITIVE Z");
            //    if (railPointIndex > 0)
            //    {
            //        grindMove = (railPoints.ElementAt<Transform>(railPointIndex - 1).position - controller.transform.position).normalized * grindSpeed;

            //        // if player moves passes new waypoint, increment index
            //        if (transform.position.z + grindMove.z * Time.deltaTime >= railPoints.ElementAt<Transform>(railPointIndex - 1).position.z)
            //        {
            //            railPointIndex--;
            //        }
            //    }
            //    else
            //    {
            //        grindMove = Vector3.zero;
            //    }
            //}
            ////else if (playerVel.z < 0f)
            //else if (transform.forward.z < 0f)
            //{
            //    Debug.Log("NEGATIVE Z");
            //    Debug.Log("RPCOUNT = " + railPoints.Count);
            //    if (railPointIndex < railPoints.Count - 1)
            //    {
            //        grindMove = (railPoints.ElementAt<Transform>(railPointIndex + 1).position - controller.transform.position).normalized * grindSpeed;

            //        // if player moves passes new waypoint, increment index
            //        if (transform.position.z + grindMove.z * Time.deltaTime <= railPoints.ElementAt<Transform>(railPointIndex + 1).position.z && railPointIndex < railPoints.Count - 1)
            //        {
            //            railPointIndex++;
            //        }
            //    }
            //    else
            //    {
            //        grindMove = Vector3.zero;
            //    }
            //}
            //else
            //{
            //    grindMove = Vector3.zero;
            //}

            //launchVelocity = playerVel;
            //controller.Move(grindMove * Time.deltaTime);

            //Debug.Log(railPointIndex);
        }

        IEnumerator GroundPoundCoroutine()
        {
            //Print the time of when the function is first called.
            //Debug.Log("Started Coroutine at timestamp : " + Time.time);
            groundPoundStart = true;
            velocity.x = 0;
            velocity.z = 0;
            PlayGPStartSound();

            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(0.25f);

            groundPoundStart = false;
            if (!isGrounded)
            {
                isGroundPound = true;
                groundPounded = true;
            }
        }

        IEnumerator AirDashCoroutine()
        {
            //start camera effect
            cameraFXController.timedFX("airdash", 0.1f);

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            airDashDirection = (transform.right * x + transform.forward * z).normalized;

            //limits air control after dash finishes
            //allow momentum conservation if jumping mid-airdash
            if (jumped)
            {
                launchVelocity = airDashDirection * postAirDashSpeed; //airSpeed;// launchVelocity.magnitude;
            }
            else
            {
                //mid-airdash jump speed (DKC jump)
                launchVelocity = airDashDirection * postAirDashJumpSpeed; //airSpeed;// launchVelocity.magnitude;
            }
            
            velocity.y = 0;
            isAirDashing = true;
            airDashesUsed = 1;
            PlayAirDashSound();

            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(0.12f);

            isAirDashing = false;
        }

        IEnumerator WallJumpCoroutine()
        {
            wallSlideGravity = gravity;
            yield return new WaitForSeconds(0.2f);
            wallCollisionStarted = false;
        }

        IEnumerator SkidCoroutine()
        {
            yield return new WaitForSeconds(skidLerpDuration);
            isSkidding = false;
        }

        IEnumerator GrindLerpCoroutine()
        {
            yield return new WaitForSeconds(grindLerpDuration);
            isGrindLerp = false;
        }

        private void PlayFootstepSounds(float x, float z)
        {
            if (isGrounded)
            {
                
                Vector3 groundVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
                //Debug.Log("Ground vel mag = "+groundVelocity.magnitude);
                //_audioSource.clip = Input.GetButton("Fire3") ? runningSound : walkingSound;
                if (!_audioSource.isPlaying)
                {
                    //Debug.Log(controller.velocity.magnitude);
                    //if (groundVelocity.magnitude > 0.1f)
                    if (x > 0.01 || z > 0.01)
                    {
                        Vector3 move = transform.right * x + transform.forward * z;
                        _audioSource.loop = true;
                        _audioSource.clip = Input.GetButton("Fire3") ? runningSound : walkingSound;
                        _audioSource.Play();
                    }
                }
                else
                {
                    //if ((_audioSource.clip == runningSound || _audioSource == walkingSound) && groundVelocity.magnitude < 0.1f)
                    if ((_audioSource.clip == runningSound || _audioSource.clip == walkingSound) && (x < 0.01 && z < 0.01))
                    {
                        _audioSource.Pause();
                    }
                }
            }
        }

        private void PlayGPStartSound()
        {
            _audioSource.pitch = 1f;
            _audioSource.clip = gpSound;
            _audioSource.Play();
        }

        private void PlayGPLandSound()
        {
            _audioSource.pitch = 1f;
            _audioSource.clip = gpLandSound;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        private void PlayGroundDashSound()
        {
            _audioSource.pitch = 1f;
            _audioSource.clip = groundDashSound;
            _audioSource.loop = true;
            _audioSource.Play();
        }

        private void PlayAirDashSound()
        {
            _audioSource.pitch = 1f;
            _audioSource.clip = airDashSound;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        private void PlayGlideSound()
        {
            _audioSource.pitch = 1f;
            _audioSource.clip = glideSound;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        private void PlayLandSound()
        {
            _audioSource.pitch = 1f;
            _audioSource.clip = landSound;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        private void PlayJumpSound(float pitch)
        {
            _audioSource.pitch = pitch;
            _audioSource.clip = jumpSound;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        private void PlayTripleJumpSound(float pitch)
        {
            _audioSource.pitch = pitch;
            _audioSource.clip = tripleJumpSound;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        private void PlayJumpHoldSound()
        {
            _audioSource.clip = jumpHoldSound;
            _audioSource.loop = false;
            _audioSource.volume = 1f;
            _audioSource.Play();
        }

        private void PlaySlideSound()
        {
            _audioSource.pitch = 1f;
            _audioSource.clip = slideSound;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        private void PlayGrindSound()
        {
            _audioSource.pitch = 1f;
            _audioSource.clip = grindSound;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        private void PlayGrindJumpSound()
        {
            _audioSource.pitch = 1f;
            _audioSource.clip = grindJumpSound;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        private void PlaySkidSound()
        {
            _audioSource.pitch = 1f;
            Debug.Log("SKID SOUND");
            _audioSource.clip = skidSound;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        private void PlayFlipSound()
        {
            _audioSource.pitch = 1f;
            _audioSource.clip = flipSound;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            //wall jump logic
            //TODO - only do this code on first frame wall jump is pressed
            //TODO - store character velocity before collision (otherwise it will be near zero after first frame of collision)
            if (!isGrounded && !isGroundPound)//&& !wallJumped)// && isCollidingWithWall) // && !isAirDashing
            {
                //only set wall jump trajectory on first frame of collision
                //if (!wallCollisionStarted)
                //{
                //    //wallJumpDirection = Vector3.Reflect(playerVel, hit.normal).normalized;
                //    wallJumpDirection = hit.normal;
                //    //Debug.Log(wallJumpDirection);
                //    //counter++;
                //    //wallCollisionDot = Vector3.Dot(hit.normal, new Vector3(playerVel.x, 0f, playerVel.z).normalized);
                //    wallCollisionStarted = true;
                //}
                //collide with wall
                if (hit.normal.y < 0.05f)
                {
                    //Debug.Log(hit.normal.y);
                    if (!wallCollisionStarted)
                    {
                        PlaySlideSound();
                        //reduce velocity while sliding on wall - 4/8
                        if (velocity.y < 0)
                        {
                            wallSlideGravity = gravity * wallSlideGravMultiplier;
                        }
                        else
                        {
                            wallSlideGravity = gravity;
                        }
                        //wallSlideGravity = gravity * wallSlideGravMultiplier;
                        //test - reduce wallslide velocity
                        launchVelocity = new Vector3(velocity.x*0.7f, velocity.y/2f, velocity.z*0.7f);

                        wallJumpDirection = hit.normal;
                        wallCollisionStarted = true;
                        //used for post wall contact air momentum (in direction of wall plane) - 4/13
                        wallNormal = hit.normal;
                    }
                    float dot = Vector3.Dot(hit.normal, new Vector3(playerVel.x, 0f, playerVel.z).normalized);
                    //if 45 degrees to wall or less, reflect motion angle over wall normal.  Wall jump vertical angle set to 45 degrees
                    if (dot != 100f)//(dot <= -0.75f || Mathf.Abs(dot)==0.001)//!= 100
                    {
                        if (Input.GetButtonDown("Jump"))
                        {
                            //rotate during wall jump - 4/19
                            if (Input.GetKey(KeyCode.Mouse2))
                            {
                                isRotating = true;
                                PlayFlipSound();
                                //set initial rotation direction - 4/21
                                worldRotationAxis = transform.right;
                            }

                            //allow airdash post-wall jump
                            airDashesUsed = 0;

                            tripleJumpContacts = 0;

                            PlayJumpSound(1f);
                            //Debug.Log("WALLJUMPED");
                            wallJumped = true;
                            //wallCollisionStarted = false;
                            //this coroutine waits a small amount of time before setting wallcollisionstarted to false
                            StartCoroutine(WallJumpCoroutine());
                            launchVelocity = wallJumpDirection * 18f;
                            velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
                        }
                    }

                }
            }

        }

        public void OnTriggerColliderEnter(Collider other)
        {
            //get normal of wall - used to reset air speed after collision stops
            RaycastHit hit;
            //Vector3 groundNormal;
            //Vector3 castPosition = new Vector3(transform.position.x - ((controller.radius / 2) - controller.skinWidth), transform.position.y, transform.position.z);
            //Vector3 castDirection = new Vector3(velocity.x, 0f, velocity.z).normalized;
            //if (Physics.Raycast(transform.position, castDirection, out hit, 3f))
            //{

            //}
        }

        public void OnTriggerColliderExit(Collider other)
        {
            //reset air speed if character contacts and leaves wall without jumping
            
            if (other.gameObject.tag == "Wall")
            {
                //Debug.Log("COLLISION EXIT");
                //launchVelocity = wallJumpDirection * 18f;
                //reset from wall slide
                //launchVelocity.y *= 2f;
                wallSlideGravity = gravity;
                if (!wallJumped)
                {
                    wallCollisionStarted = false;
                    launchVelocity = Vector3.ProjectOnPlane(velocity, wallNormal);
                    velocity = Vector3.ProjectOnPlane(velocity, wallNormal);
                }
                //Debug.Log(wallNormal);
                //project movement to wall plane after collision ends (instead of maintaining)
            }
        }

        public void LerpSlideCamera()
        {
            if (isSliding && timeElapsed < slideLerpDuration)
            {
                mainCamera.transform.localPosition = new Vector3(0f, Mathf.Lerp(0f, -controller.height / 2.5f, timeElapsed/slideLerpDuration), 0f);
                timeElapsed += Time.deltaTime;
            }
            else if (!isSliding && timeElapsed < slideLerpDuration)
            {
                mainCamera.transform.localPosition = new Vector3(0f, Mathf.Lerp(-controller.height / 2.5f, 0f, timeElapsed/slideLerpDuration), 0f);
                timeElapsed += Time.deltaTime;
            }
        }

        public void initiateGrind(List<Waypoint> waypoints, int wpIndex, int indexDelta, float grindSpeed)
        {
            //calculate slope
            float theta = Vector2.Angle(waypoints[wpIndex].transform.position, waypoints[wpIndex + indexDelta].transform.position);
            grindSlope = Mathf.Tan(theta);
            Debug.Log("SLOPE: "+grindSlope);

            //save parameters for post-lerp
            grindData = (waypoints, wpIndex, indexDelta, grindSpeed);
            if (waypoints[wpIndex].transform.position.y < waypoints[wpIndex + indexDelta].transform.position.y)
            {
                grindIsUphill = true;
            }
            else
            {
                grindIsUphill = false;
            }

            //grind lerp - 6/30/21
            isGrindLerp = true;
            timeElapsed = 0f;
            Transform startingWayPoint = waypoints[wpIndex].transform;
            grindLerpVector = startingWayPoint.position - controller.transform.position;
            //StartCoroutine(SkidCoroutine());

            //stop running animation
            if (isGroundDash)
            {
                isGroundDash = false;
            }
            //gunController.setIsRunning(false);

            //grindCount++;
            //Debug.Log("COUNT: "+grindCount);

            ////set initial grindSpeed (else maintain current grind speed if going from grind to grind
            //if (grindCount <= 1)
            //{
            //    Debug.Log("SPEED: " + grindSpeed);
            //    this.grindSpeed = grindSpeed;
            //}

            ////clears, but maintains list length
            //railPoints.Clear();
            ////restores to default capacity
            //railPoints.TrimExcess();

            ////Transform startingWayPoint = waypoints[wpIndex].transform;
            //railPointIndex = wpIndex;
            //wpIndexDelta = indexDelta;

            ////populate current grind waypoints
            //foreach (Waypoint waypoint in waypoints)
            //{
            //    railPoints.Add(waypoint.transform);
            //}

            //controller.enabled = false;
            ////controller.transform.position = startingWayPoint.position;
            //controller.enabled = true;
            //isGrinding = true;
            ////GameObject.Find("SparksEffect").SetActive(true);
            //PlayGrindSound();
            //tripleJumpContacts = 0;
            //isGrounded = false;
            //jumped = false;
            //launchVelocity = Vector3.zero;
        }

        public void startGrind()
        {
            grindCount++;
            //Debug.Log("COUNT: " + grindCount);
            Transform startingWayPoint = grindData.waypoints[grindData.wpIndex].transform;

            //set initial grindSpeed (else maintain current grind speed if going from grind to grind
            if (grindCount <= 1)
            {
                //Debug.Log("SPEED: " + grindSpeed);
                this.grindSpeed = grindData.grindSpeed;
            }

            //clears, but maintains list length
            railPoints.Clear();
            //restores to default capacity
            railPoints.TrimExcess();

            //Transform startingWayPoint = waypoints[wpIndex].transform;
            railPointIndex = grindData.wpIndex;
            wpIndexDelta = grindData.indexDelta;

            //populate current grind waypoints
            foreach (Waypoint waypoint in grindData.waypoints)
            {
                railPoints.Add(waypoint.transform);
            }

            controller.enabled = false;
            controller.transform.position = startingWayPoint.position;
            controller.enabled = true;
            isGrinding = true;
            grindStarting = true;
            //GameObject.Find("SparksEffect").SetActive(true);
            PlayGrindSound();
            tripleJumpContacts = 0;
            isGrounded = false;
            jumped = false;
            launchVelocity = Vector3.zero;
        }

        //initiate grind if rail detects new trigger collision
        //public void initiateGrind(GameObject grindRail)
        //{
        //    //updated to allow multiple objects with "Waypoints" children
        //    currentRail = grindRail.transform.Find("Waypoints").gameObject;
        //    Debug.Log(currentRail.name);
        //    railPoints.Clear();
        //    //transform.position = GameObject.Find("wp2").transform.position;

        //    float distanceToWaypoint = 100f;
        //    Transform startingWayPoint = grindRail.transform.Find("Waypoints").GetChild(0);// GameObject.Find("Waypoints").transform.GetChild(0);

        //    foreach (Transform wayPoint in grindRail.transform.Find("Waypoints"))//GameObject.Find("Waypoints").transform)
        //    {
        //        railPoints.Add(wayPoint);

        //        //find closest waypoint
        //        float newDistance = Vector3.Distance(transform.position, wayPoint.position);
        //        if (newDistance < distanceToWaypoint)// && transform.position.y > wayPoint.position.y)
        //        {
        //            distanceToWaypoint = newDistance;
        //            startingWayPoint = wayPoint;
        //            railPointIndex = railPoints.IndexOf(startingWayPoint);
        //        }
        //    }

        //    counter++;
        //    //Debug.Log("GRIND HIT " + counter);
        //    Debug.Log("rail index: "+railPointIndex);

        //    //transform.position = startingWayPoint.position;
        //    controller.enabled = false;
        //    controller.transform.position = startingWayPoint.position;
        //    controller.enabled = true;
        //    isGrinding = true;
        //    //GameObject.Find("SparksEffect").SetActive(true);
        //    PlayGrindSound();
        //    tripleJumpContacts = 0;
        //    isGrounded = false;
        //    jumped = false;
        //    launchVelocity = Vector3.zero;
        //}

        public bool getIsGrounded()
        {
            return this.isGrounded;
        }

        public bool getIsGroundDash()
        {
            return this.isGroundDash;
        }

        public float getDegreesRotated()
        {
            return this.degreesRotated;
        }

        public float getNewRotationDelta()
        {
            return this.newRotationDelta;
        }

        public bool getIsRotating()
        {
            return this.isRotating;
        }

        public Vector3 getVelocity()
        {
            return this.playerVel;
        }

        public void setIsRotating(bool rotationState)
        {
            this.isRotating = rotationState;
            if (_audioSource.clip = flipSound)
            {
                _audioSource.Stop();
            }
        }

        public Vector3 getWorldRotationAxis()
        {
            return this.worldRotationAxis;
        }

        /// A helper for assistance with smoothing the camera rotation.
        private class SmoothRotation
        {
            //private float _current;
            //private float _currentVelocity;

            //public SmoothRotation(float startAngle)
            //{
            //    _current = startAngle;
            //}

            ///// Returns the smoothed rotation.
            //public float Update(float target, float smoothTime)
            //{
            //    return _current = Mathf.SmoothDampAngle(_current, target, ref _currentVelocity, smoothTime);
            //}

            //public float Current
            //{
            //    set { _current = value; }
            //}
        }

        /// A helper for assistance with smoothing the movement.
        private class SmoothVelocity
        {
            //private float _current;
            //private float _currentVelocity;

            ///// Returns the smoothed velocity.
            //public float Update(float target, float smoothTime)
            //{
            //    return _current = Mathf.SmoothDamp(_current, target, ref _currentVelocity, smoothTime);
            //}

            //public float Current
            //{
            //    set { _current = value; }
            //}
        }

        /// Input mappings
        //[Serializable]
        //private class FpsInput
        //{
        //    [Tooltip("The name of the virtual axis mapped to rotate the camera around the y axis."),
        //     SerializeField]
        //    private string rotateX = "Mouse X";

        //    [Tooltip("The name of the virtual axis mapped to rotate the camera around the x axis."),
        //     SerializeField]
        //    private string rotateY = "Mouse Y";

        //    [Tooltip("The name of the virtual axis mapped to move the character back and forth."),
        //     SerializeField]
        //    private string move = "Horizontal";

        //    [Tooltip("The name of the virtual axis mapped to move the character left and right."),
        //     SerializeField]
        //    private string strafe = "Vertical";

        //    [Tooltip("The name of the virtual button mapped to run."),
        //     SerializeField]
        //    private string run = "Fire3";

        //    [Tooltip("The name of the virtual button mapped to jump."),
        //     SerializeField]
        //    private string jump = "Jump";

        //    /// Returns the value of the virtual axis mapped to rotate the camera around the y axis.
        //    public float RotateX
        //    {
        //        get { return Input.GetAxisRaw(rotateX); }
        //    }

        //    /// Returns the value of the virtual axis mapped to rotate the camera around the x axis.        
        //    public float RotateY
        //    {
        //        get { return Input.GetAxisRaw(rotateY); }
        //    }

        //    /// Returns the value of the virtual axis mapped to move the character back and forth.        
        //    public float Move
        //    {
        //        get { return Input.GetAxisRaw(move); }
        //    }

        //    /// Returns the value of the virtual axis mapped to move the character left and right.         
        //    public float Strafe
        //    {
        //        get { return Input.GetAxisRaw(strafe); }
        //    }

        //    /// Returns true while the virtual button mapped to run is held down.          
        //    public bool Run
        //    {
        //        get { return Input.GetButton(run); }
        //    }

        //    /// Returns true during the frame the user pressed down the virtual button mapped to jump.          
        //    public bool Jump
        //    {
        //        get { return Input.GetButtonDown(jump); }
        //    }
        //}


        private void OnTriggerExit(Collider other)
        {
            //Debug.Log("COLLISION EXIT");
            //if (other.gameObject.tag == "Wall")
            //{
            //    launchVelocity = wallJumpDirection * 18f;
            //}
        }

        private void OnCollisionExit(Collision collision)
        {

        }
    }

}