using System;
using System.Linq;
using System.Collections;
using UnityEngine;

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
        public float groundPoundMultiplier = 5f;
        public float wallSlideGravity;
        public float wallSlideGravMultiplier = 0.2f;
        public float jumpHeight = 3f;
        public float jumpVelocity = 20f;
        public float shortJumpHeight = 1.25f;
        public float slideLerpDuration = 0.5f;
        public float skidLerpDuration = 1f;
        private float timeElapsed = 0f;

        //scale to gravity
        public float fastFallScale = 2;
        public float gpSuperJumpInterval = 0.25f;
        public float tripleJumpInterval = 0.1f;
        public float superJumpMultiplier = 2f;

        public Transform groundCheck;
        public float groundDistance = 0.3f;
        public LayerMask groundMask;

        Vector3 velocity;
        bool isGrounded;
        bool wasGrounded = false;
        bool groundPoundStart = false;
        bool isGroundPound = false;
        bool isAirDashing = false;
        //makes sure sprint is released after jumping before air dash is available
        bool sprintReleased = false;
        bool airDashReleased = true;
        bool wallJumped = false;
        bool isCollidingWithWall = false;
        bool isHoldingJump = false;
        bool isRotating = false;
        bool isSliding = false;
        bool isGliding = false;
        bool isDescending = false;
        public bool isGroundDash = false;
        public bool isSkidding = false;
        bool jumped = false;

        //the wall the character jumped off of
        private int wallJumpedOffOf;
        public Vector3 preWallJumpVelocity;

        Vector3 launchVelocity;
        Vector3 airDashDirection;
        Vector3 groundDashDirection;

        private int airDashesUsed = 0;
        private int airDashesAllowed = 2;//1;
        private float gpJumpStartTime;
        private float tripleJumpStartTime;
        private float degreesRotated = 0;
        private Vector3 groundNormal = Vector3.zero;
        private Vector3 wallNormal = Vector3.zero;

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
            //skidSpeed = runSpeed;
            lastPos = transform.position;
            //Debug.Log("BEHIND OR SIDE");
            //_rigidbody = GetComponent<Rigidbody>();
            //_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            //_collider = GetComponent<CapsuleCollider>();
            _audioSource = GetComponent<AudioSource>();
			//arms = AssignCharactersCamera();
            _audioSource.clip = walkingSound;
            _audioSource.loop = true;
            //_rotationX = new SmoothRotation(RotationXRaw);
            //_rotationY = new SmoothRotation(RotationYRaw);
            //_velocityX = new SmoothVelocity();
            //_velocityZ = new SmoothVelocity();
            Cursor.lockState = CursorLockMode.Locked;
            //ValidateRotationRestriction();
        }
			
        //private Transform AssignCharactersCamera()
        //{
   //         var t = transform;
			//arms.SetPositionAndRotation(t.position, t.rotation);
			//return arms;
        //}
        
        /// Clamps <see cref="minVerticalAngle"/> and <see cref="maxVerticalAngle"/> to valid values and
        /// ensures that <see cref="minVerticalAngle"/> is less than <see cref="maxVerticalAngle"/>.
        //private void ValidateRotationRestriction()
        //{
        //    minVerticalAngle = ClampRotationRestriction(minVerticalAngle, -90, 90);
        //    maxVerticalAngle = ClampRotationRestriction(maxVerticalAngle, -90, 90);
        //    if (maxVerticalAngle >= minVerticalAngle) return;
        //    Debug.LogWarning("maxVerticalAngle should be greater than minVerticalAngle.");
        //    var min = minVerticalAngle;
        //    minVerticalAngle = maxVerticalAngle;
        //    maxVerticalAngle = min;
        //}

        //private static float ClampRotationRestriction(float rotationRestriction, float min, float max)
        //{
        //    if (rotationRestriction >= min && rotationRestriction <= max) return rotationRestriction;
        //    var message = string.Format("Rotation restrictions should be between {0} and {1} degrees.", min, max);
        //    Debug.LogWarning(message);
        //    return Mathf.Clamp(rotationRestriction, min, max);
        //}
			
        /// Checks if the character is on the ground.
        //private void OnCollisionStay()
        //{
        //    var bounds = _collider.bounds;
        //    var extents = bounds.extents;
        //    var radius = extents.x - 0.01f;
        //    Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
        //        _groundCastResults, extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);
        //    if (!_groundCastResults.Any(hit => hit.collider != null && hit.collider != _collider)) return;
        //    for (var i = 0; i < _groundCastResults.Length; i++)
        //    {
        //        _groundCastResults[i] = new RaycastHit();
        //    }

        //    _isGrounded = true;
        //}
			
        /// Processes the character movement and the camera rotation every fixed framerate frame.
        private void FixedUpdate()
        {

            //if (!isCollidingWithWall && !wallJumped)
            //{
            //    preWallJumpVelocity = controller.velocity;
            //}
            // FixedUpdate is used instead of Update because this code is dealing with physics and smoothing.
            //RotateCameraAndCharacter();
            //MoveCharacter();
            //_isGrounded = false;
        }

        /// Moves the camera to the character, processes jumping and plays sounds every frame.
        private void Update()
        {
            //Lerp slide camera
            //LerpSlideCamera();

            Debug.Log(isRotating);

            //rotation testing
            //mainCamera.Rotate(540f * Time.deltaTime, 0f, 0f, Space.Self);

            //testing this logic with mouseinput - 4/17
            //if (isRotating)
            //{
            //    float newRotation = Mathf.Clamp(540f * Time.deltaTime, 0f, 360f - degreesRotated);
            //    mainCamera.Rotate(-newRotation, 0f, 0f, Space.Self);
            //    degreesRotated += newRotation;

            //    if (degreesRotated == 360f)
            //    {
            //        isRotating = false;
            //        degreesRotated = 0;
            //    }
            //}

            if (isRotating)
            {
                float newRotation = Mathf.Clamp(540f * Time.deltaTime, 0f, 360f - degreesRotated);
                //mainCamera.Rotate(-newRotation, 0f, 0f, Space.Self);
                degreesRotated += newRotation;

                if (degreesRotated == 360f)
                {
                    isRotating = false;
                    degreesRotated = 0;
                }
            }


            //Debug.Log(velocity.y);


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
            //Debug.Log(wallCollisionStarted);


            //if (!isCollidingWithWall && !wallJumped)
            //{
            //    preWallJumpVelocity = controller.velocity;
            //}
            //if (!isCollidingWithWall)
            //{
            //    preWallJumpVelocity = controller.velocity;
            //}

            //Debug.Log(wallJumped);
            //arms.position = transform.position + transform.TransformVector(armPosition);
            //Jump();
            //if (Input.GetButtonDown("Slomo"))
            //{
            //_audioSource.pitch = Time.timeScale;
            //if (isHoldingJump)
            //{
            //    //_audioSource.pitch = Time.timeScale;// + (0.1f / Time.deltaTime);
            //    //float newVolume = _audioSource.volume + (Time.timeScale * (0.01f / Time.deltaTime));
            //    //Mathf.Clamp(_audioSource.volume, newVolume, 1f);
            //    //_audioSource.pitch = Time.timeScale;
            //}
            //else
            //{
                //_audioSource.Play();
                _audioSource.pitch = Time.timeScale;
            //}
            //}

            //PlayFootstepSounds();

            //check previous grounded state from last call
            wasGrounded = isGrounded;
            //creates sphere with specified radius, and check if it collides with ground
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -1f;//-2f;
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            //TODO - add code to check for ground normal and project onto plane
            if (isGrounded)
            {
                    //Debug.Log(transform.position.y - ((controller.height / 2) + controller.skinWidth) - 0.5f);
                Vector3 footPosition = new Vector3(transform.position.x, transform.position.y - ((controller.height / 2) - controller.skinWidth), transform.position.z);
                //check when grounded and slightly leaving the ground (running down a slope for example)
                RaycastHit hit;
                //Vector3 groundNormal;
                if (Physics.Raycast(footPosition, -transform.up, out hit, 0.2f))// 5f))
                {
                    //counter++;
                    //Debug.Log(hit.distance);
                    //Debug.Log(footPosition.y - 0.5f);
                    //Debug.Log("CAST HIT");
                    groundNormal = hit.normal;

                    //Debug.Log(groundNormal);
                    //Debug.Log(transform.position.y - ((controller.height / 2) + controller.skinWidth) - 0.5f);

                    //move = Vector3.ProjectOnPlane(move, groundNormal);

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
                            //Debug.Log("TEST " + counter);
                            //counter++;
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
                        //if (_audioSource.isPlaying)
                        //{
                        //    _audioSource.Stop();
                        //}
                        isSliding = false;
                    }
                }
                else
                {
                    //Debug.Log("CAST MISS");
                }
            }

        //Vector3 move = transform.right * x + transform.forward * z;


        PlayFootstepSounds(x,z);

            if (isGrounded)
            {

                

                sprintReleased = false;
                //if (x == 0)
                //{
                //    velocity.x = 0;
                //}
                //if (z == 0)
                //{
                //    velocity.z = 0;
                //}
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

                    //if (_audioSource.isPlaying)
                    //{
                    //    _audioSource.Stop();
                    //}
                    isGroundDash = false;
                    cameraFXController.stopFX("grounddash");


                }

                //Vector3 move = transform.right * x + transform.forward * z;
                if (isSliding)
                {
                    controller.Move(move * slideSpeed * Time.deltaTime);
                }
                else if (isGroundDash)
                {
                    //controller.Move(move * groundDashSpeed * Time.deltaTime);
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

                        //velocity.x += Mathf.Min(move.x * accel, maxRunSpeed - velocity.x);
                        //velocity.z += Mathf.Min(move.z * accel, maxRunSpeed - velocity.z);
                        //controller.Move(velocity * Time.deltaTime * Time.deltaTime);
                        //velocity += (move * Mathf.Min(maxSpeed, new Vector3(move * accel * Time.deltaTime).x, 0f, ));
                        //velocity += Mathf.Min(maxSpeed, controller.velocity)
                        //velocity = Mathf.Min(move * maxSpeed, move * accel * Time.deltaTime);
                        controller.Move(move * runSpeed * Time.deltaTime);
                    }
                    else
                    {
                        //velocity.x += Mathf.Min(move.x * accel, maxWalkSpeed - velocity.x);
                        //velocity.z += Mathf.Min(move.z * accel, maxWalkSpeed - velocity.z);
                        //controller.Move(velocity * Time.deltaTime);
                        //velocity = (move * Mathf.Min(maxSpeed, controller.velocity.magnitude + (accel * Time.deltaTime)));
                        controller.Move(move * speed * Time.deltaTime);
                    }
                }
                //if (run > 0)
                //{

                //    //velocity.x += Mathf.Min(move.x * accel, maxRunSpeed - velocity.x);
                //    //velocity.z += Mathf.Min(move.z * accel, maxRunSpeed - velocity.z);
                //    //controller.Move(velocity * Time.deltaTime * Time.deltaTime);
                //    //velocity += (move * Mathf.Min(maxSpeed, new Vector3(move * accel * Time.deltaTime).x, 0f, ));
                //    //velocity += Mathf.Min(maxSpeed, controller.velocity)
                //    //velocity = Mathf.Min(move * maxSpeed, move * accel * Time.deltaTime);
                //    controller.Move(move * runSpeed * Time.deltaTime);
                //}
                //else
                //{
                //    //velocity.x += Mathf.Min(move.x * accel, maxWalkSpeed - velocity.x);
                //    //velocity.z += Mathf.Min(move.z * accel, maxWalkSpeed - velocity.z);
                //    //controller.Move(velocity * Time.deltaTime);
                //    //velocity = (move * Mathf.Min(maxSpeed, controller.velocity.magnitude + (accel * Time.deltaTime)));
                //    controller.Move(move * speed * Time.deltaTime);
                //}

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
                    //isGroundPound = false;
                    velocity.x = 0;
                    velocity.z = 0;
                    gravity = normalGravity;

                    //GP super jump logic - check link timing
                    if (Time.time - gpJumpStartTime <= gpSuperJumpInterval)
                    {

                        //prevent air jump after super jump
                        jumped = true;

                        //Debug.Log("SUPER JUMP");
                        jumpAudioController.Play(true);
                        velocity.y = Mathf.Sqrt(jumpHeight * superJumpMultiplier * -2f * gravity);
                        launchVelocity = velocity;

                        //rotate testing
                        if (Input.GetKey(KeyCode.Mouse2))
                        {
                            isRotating = true;
                        }
                    }
                    //else
                    //{
                    //    //triple jump logic
                    //    if (Time.time - tripleJumpStartTime <= tripleJumpInterval)
                    //    {
                    //        tripleJumpContacts++;
                    //        if (tripleJumpContacts == 3)
                    //        {
                    //            velocity.y = Mathf.Sqrt(jumpHeight * 2 * superJumpMultiplier * -2f * gravity);
                    //            launchVelocity = controller.velocity;
                    //            tripleJumpContacts = 0;
                    //        }
                    //        else
                    //        {
                    //            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        //reset
                    //        tripleJumpContacts = 0;
                    //    }
                    //}

                    
                }
                else
                {
                    //retrieve velocity on last frame before jump
                    //launchVelocity = move * speed;
                    //launchVelocity = playerVel;
                    launchVelocity = controller.velocity;// velocity;
                }
            }
            else
            {
                ////triple jump logic
                //if (Input.GetButtonDown("Jump"))
                //{
                //    tripleJumpStartTime = Time.time;
                //}

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

                    ////rotate testing
                    //isRotating = true;
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

                            //Vector3 deltaV;
                            //float deltaV;

                            //looking below world horizontal
                            if (groundDot > 0f)
                            {
                                //descend
                                isDescending = true;
                                glideDeltaVel += groundDot * descentSpeed;
                                //velocity += deltaV;
                                //velocity.y += deltaV.y;
                                //Debug.Log(deltaV);
                                //velocity.y = deltaV.y;
                                //velocity += gunCamera.transform.forward * deltaV;
                                controller.Move(Mathf.Min(glideDeltaVel, maxGlideSpeed) * gunCamera.transform.forward * Time.deltaTime);

                                //velocity = deltaV;
                            }
                            else
                            {
                                //ascend - reflect velocity.y when switching from ascend to descend
                                isDescending = false;
                                //if (isDescending)
                                //{
                                //    isDescending = false;
                                //    //velocity.y *= -1f;
                                //    //isGliding = false;
                                //    //velocity.y = 0;
                                //}
                                //else
                                //{
                                glideDeltaVel += groundDot * descentSpeed;//1f+groundDot
                                //velocity.y += deltaV.y + 2*gravity*Time.deltaTime;
                                //velocity.y = deltaV.y;
                                //velocity -= gunCamera.transform.forward * deltaV;
                                controller.Move(Mathf.Min(glideDeltaVel, maxGlideSpeed) * gunCamera.transform.forward * Time.deltaTime);
                                //controller.Move(Mathf.Min(glideDeltaVel, maxGlideSpeed) * gunCamera.transform.forward * Time.deltaTime);
                                //Debug.Log(glideDeltaVel);

                                //velocity = deltaV;
                                //}

                            }
                            //Debug.Log(gunCamera.transform.forward);
                            
                            //velocity += gunCamera.transform.forward * descentSpeed;
                            launchVelocity = Mathf.Min(glideDeltaVel, maxGlideSpeed) * gunCamera.transform.forward;
                        }
                    }
                    else
                    {
                        glideDeltaVel = 0f;
                        //velocity.y = launchVelocity.y;
                        //Debug.Log(velocity.y);
                        isGliding = false;
                        cameraFXController.stopFX("glide");
                    }

                    //Air Dash Logic
                    if (Input.GetAxis("Fire3") > 0)
                    {
                        if (!isAirDashing && airDashesUsed < airDashesAllowed && sprintReleased && !isGroundPound && !groundPoundStart && airDashReleased)
                        {
                            airDashReleased = false;
                            StartCoroutine(AirDashCoroutine());
                        }
                        //else
                        //{
                        //    controller.Move(airDashDirection * airDashSpeed * Time.deltaTime);
                        //}
                        
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


                    //Ground Pound Logic
                    //if (Input.GetButton("Ground Pound"))
                    //if (Input.GetButton("Jump"))
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        StartCoroutine(GroundPoundCoroutine());
                        //Debug.Log("GROUND POUND");
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


                    //change air control only for side and back movement
                    //velocity = new Vector3(launchVelocity.x, velocity.y, launchVelocity.z);


                    //velocity.x = move.x * airSpeed;// * Time.deltaTime;
                    //velocity.z = move.z * airSpeed;
                    //if (move.x > 1)
                    //{
                    //launchvelocity should be consistent
                    //velocity += launchVelocity * Time.deltaTime;
                    //controller.Move(new Vector3(launchVelocity.x, 0f, launchVelocity.z));
                    //Mathf.Min(Math.Abs(move.x * airSpeed * Time.deltaTime), launchVelocity.magnitude - Mathf.Abs(move.x * airSpeed * Time.deltaTime));
                    //move or transform.forward?

                    //Only allow air control for sideways and backwards movement post jump
                    //TODO - add forward air control if launchvelocity is less than airspeed
                    if (!groundPoundStart && !isGroundPound && !isAirDashing && !isGliding)// && !isCollidingWithWall)
                    {

                        velocity = new Vector3(launchVelocity.x, velocity.y, launchVelocity.z);

                        float dot = Vector3.Dot(new Vector3(move.x, 0f, move.z).normalized, new Vector3(launchVelocity.x, 0f, launchVelocity.z).normalized);

                        if (dot <= 0.1)
                        {
                            //Debug.Log("BEHIND OR SIDE");
                            //only air control to the sides or back from initial jump orientation
                            //full airspeed directly lateral (1 - Mathf.Abs(dot), floor is airspeed fraction
                            controller.Move(move * airSpeed * ((1 - Mathf.Abs(dot))+0.75f) * Time.deltaTime);
                        }
                        else
                        {

                            //Debug.Log("FORWARD");
                        }


                        bool fastFallPressed = Input.GetButtonDown("Jump");// Input.GetAxis("Jump");
                        if (fastFallPressed && velocity.y <= 0 && !wallCollisionStarted && jumped)
                        {
                            //velocity.y += gravity * fastFallScale * Time.deltaTime;
                            gravity *= fastFallScale;
                        }

                        //hold fast fall logic
                        //float fastFall = Input.GetAxis("Jump");
                        //bool fastFall = Input.GetKey(KeyCode.CapsLock);
                        //if (fastFall > 0 && velocity.y < 0)
                        //{
                        //    velocity.y += gravity * fastFallScale * Time.deltaTime;
                        //}

                        //hold jump logic

                        if (Input.GetButton("Jump") && velocity.y > 0)// && !wallCollisionStarted)
                        {
                            //Debug.Log("JUMP HOLD");
                            velocity.y += jumpVelocity * Time.deltaTime;
                            //mainCamera.Rotate(540f * Time.deltaTime, 0f, 0f, Space.Self);
                        }
                        else
                        {
                            //Debug.Log("JUMP NOT HELD");
                        }



                    }


                    //controller.Move(move * airSpeed * Time.deltaTime);
                    //velocity.x += Mathf.Min(Math.Abs(move.x * airSpeed * Time.deltaTime), launchVelocity.magnitude - Mathf.Abs(move.x * airSpeed * Time.deltaTime));
                    //velocity.z += Mathf.Min(Math.Abs(move.z * airSpeed * Time.deltaTime), launchVelocity.magnitude - Mathf.Abs(move.z * airSpeed * Time.deltaTime));

                    //}

                    //velocity = move * airSpeed * Time.deltaTime;
                    //controller.Move(new Vector3(velocity.x, 0f, velocity.z));// * Time.deltaTime);
                    //Vector3 airSpeedDelta = move * airSpeed * Time.deltaTime;
                    //Vector3 newAirSpeed = velocity 
                    //velocity += Mathf.Min(airSpeedDelta, )
                    //float newAirSpeed = Mathf.Min(launchVelocity.magnitude, (controller.velocity + (move * airSpeed * Time.deltaTime)).magnitude);
                    //velocity += move * airSpeed * Time.deltaTime;
                    //controller.Move(move * Mathf.Max(airSpeed, launchVelocity.magnitude) * Time.deltaTime);

                }
                //velocity = new Vector3(launchVelocity.x, velocity.y, launchVelocity.z);
                //float fastFall = Input.GetAxis("Fire3");
                //if (fastFall > 0 && velocity.y < 0)
                //{
                //    velocity.y += gravity * fastFallScale * Time.deltaTime;
                //}
            }

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                //Debug.Log("TESTDS");
                if (!isHoldingJump)
                {
                    isHoldingJump = true;
                    jumpAudioController.Play(false);
                }
            }
            //else
            //{
            //    velocity += launchVelocity;
            //}

            if (Input.GetButtonDown("Jump") && !jumped)//isGrounded)
            {
                ////triple jump counter
                //if (tripleJumpContacts == 0)
                //{
                //    tripleJumpContacts++;
                //}

                //DKC roll-midair jump logic
                jumped = true;

                PlayJumpSound();
                //v = -2gh
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                //velocity.x = move.x;
                //velocity.z = move.z;
                //velocity = new Vector3(launchVelocity.x, velocity.y, launchVelocity.z);

                //rotate testing
                if (Input.GetKey(KeyCode.Mouse2))
                {
                    isRotating = true;
                }
            }
            
            else if (Input.GetKeyDown(KeyCode.CapsLock) && isGrounded)
            {
                //v = -2gh
                velocity.y = Mathf.Sqrt(shortJumpHeight * -2f * gravity);

            }
            //added wallcollisionstarted check - 4/8
            if (!groundPoundStart && !isGroundPound &&!isAirDashing && !wallCollisionStarted && !isGliding)
            {
                //if (!isGrounded)
                //{
                velocity.y += gravity * Time.deltaTime;
                //}
                //delta y = 1/2gt2
                controller.Move(velocity * Time.deltaTime);
            }
            if (isGroundPound)
            {
                velocity.y += gravity * groundPoundMultiplier * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime);
            }
            //reduce gravity while sliding down wall - 4/8
            else if (wallCollisionStarted)
            {
                velocity.y += wallSlideGravity * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime);
            }

            if ((velocity.y <= 0 || Input.GetButtonUp("Jump")) && isHoldingJump)
            {
                isHoldingJump = false;
                jumpAudioController.Stop();
                //_audioSource.volume = 1f;
                //_audioSource.Pause();

            }




            //Debug.Log("IsGrounded: " + isGrounded);
            //Debug.Log("Velocity: " + velocity);
            //Debug.Log("Fire3: "+Input.GetAxis("Fire3"));
            //Debug.Log(velocity * Time.deltaTime);
            //Debug.Log(isGrounded);
        }

        IEnumerator GroundPoundCoroutine()
        {
            //Print the time of when the function is first called.
            //Debug.Log("Started Coroutine at timestamp : " + Time.time);
            groundPoundStart = true;
            velocity.x = 0;
            velocity.z = 0;
            PlayGPStartSound();
            //velocity.y = 0;

            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(0.25f);

            //After we have waited 5 seconds print the time again.
            //Debug.Log("Finished Coroutine at timestamp : " + Time.time);

            //old
            //groundPoundStart = false;
            //isGroundPound = true;

            //new 3/26
            groundPoundStart = false;
            if (!isGrounded)
            {
                isGroundPound = true;
            }
            

            //velocity.y
        }

        IEnumerator AirDashCoroutine()
        {
            //Print the time of when the function is first called.
            //Debug.Log("Started Coroutine at timestamp : " + Time.time);

            //airDashDirection = transform.forward;

            //start camera effect
            cameraFXController.timedFX("airdash", 0.1f);

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            airDashDirection = (transform.right * x + transform.forward * z).normalized;

            //limits air control after dash finishes
            //allow momentum conservation if jumping mid-airdash - 4/13
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
            //velocity.y = 0;

            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(0.12f);

            //After we have waited 5 seconds print the time again.
            //Debug.Log("Finished Coroutine at timestamp : " + Time.time);

            //velocity = airDashDirection * postAirDashSpeed;
             
            //velocity.x = 0;
            //velocity.z = 0;
            isAirDashing = false;
            //velocity.y
        }

        IEnumerator WallJumpCoroutine()
        {
            //4/8
            wallSlideGravity = gravity;
            yield return new WaitForSeconds(0.2f);
            wallCollisionStarted = false;
        }

        IEnumerator SkidCoroutine()
        {
            yield return new WaitForSeconds(skidLerpDuration);
            isSkidding = false;
        }

        private void RotateCameraAndCharacter()
        {
   //         var rotationX = _rotationX.Update(RotationXRaw, rotationSmoothness);
   //         var rotationY = _rotationY.Update(RotationYRaw, rotationSmoothness);
   //         var clampedY = RestrictVerticalRotation(rotationY);
   //         _rotationY.Current = clampedY;
			//var worldUp = arms.InverseTransformDirection(Vector3.up);
			//var rotation = arms.rotation *
   //                        Quaternion.AngleAxis(rotationX, worldUp) *
   //                        Quaternion.AngleAxis(clampedY, Vector3.left);
   //         transform.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
			//arms.rotation = rotation;
        }
			
        /// Returns the target rotation of the camera around the y axis with no smoothing.
        //private float RotationXRaw
        //{
        //    get { return input.RotateX * mouseSensitivity; }
        //}
			
        ///// Returns the target rotation of the camera around the x axis with no smoothing.
        //private float RotationYRaw
        //{
        //    get { return input.RotateY * mouseSensitivity; }
        //}
			
        /// Clamps the rotation of the camera around the x axis
        /// between the <see cref="minVerticalAngle"/> and <see cref="maxVerticalAngle"/> values.
   //     private float RestrictVerticalRotation(float mouseY)
   //     {
			//var currentAngle = NormalizeAngle(arms.eulerAngles.x);
   //         var minY = minVerticalAngle + currentAngle;
   //         var maxY = maxVerticalAngle + currentAngle;
   //         return Mathf.Clamp(mouseY, minY + 0.01f, maxY - 0.01f);
   //     }
			
        /// Normalize an angle between -180 and 180 degrees.
        /// <param name="angleDegrees">angle to normalize</param>
        /// <returns>normalized angle</returns>
        //private static float NormalizeAngle(float angleDegrees)
        //{
        //    while (angleDegrees > 180f)
        //    {
        //        angleDegrees -= 360f;
        //    }

        //    while (angleDegrees <= -180f)
        //    {
        //        angleDegrees += 360f;
        //    }

        //    return angleDegrees;
        //}

        private void MoveCharacter()
        {
            //var direction = new Vector3(input.Move, 0f, input.Strafe).normalized;
            //var worldDirection = transform.TransformDirection(direction);
            //var velocity = worldDirection * (input.Run ? runningSpeed : walkingSpeed);
            ////Checks for collisions so that the character does not stuck when jumping against walls.
            //var intersectsWall = CheckCollisionsWithWalls(velocity);
            //if (intersectsWall)
            //{
            //    _velocityX.Current = _velocityZ.Current = 0f;
            //    return;
            //}

            //var smoothX = _velocityX.Update(velocity.x, movementSmoothness);
            //var smoothZ = _velocityZ.Update(velocity.z, movementSmoothness);
            //var rigidbodyVelocity = _rigidbody.velocity;
            //var force = new Vector3(smoothX - rigidbodyVelocity.x, 0f, smoothZ - rigidbodyVelocity.z);
            //_rigidbody.AddForce(force, ForceMode.VelocityChange);
        }

        //private bool CheckCollisionsWithWalls(Vector3 velocity)
        //{
        //    if (_isGrounded) return false;
        //    var bounds = _collider.bounds;
        //    var radius = _collider.radius;
        //    var halfHeight = _collider.height * 0.5f - radius * 1.0f;
        //    var point1 = bounds.center;
        //    point1.y += halfHeight;
        //    var point2 = bounds.center;
        //    point2.y -= halfHeight;
        //    Physics.CapsuleCastNonAlloc(point1, point2, radius, velocity.normalized, _wallCastResults,
        //        radius * 0.04f, ~0, QueryTriggerInteraction.Ignore);
        //    var collides = _wallCastResults.Any(hit => hit.collider != null && hit.collider != _collider);
        //    if (!collides) return false;
        //    for (var i = 0; i < _wallCastResults.Length; i++)
        //    {
        //        _wallCastResults[i] = new RaycastHit();
        //    }

        //    return true;
        //}

        private void Jump()
        {
            //if (!_isGrounded || !input.Jump) return;
            //_isGrounded = false;
            //_rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        private void PlayFootstepSounds_old()
        {
            //if (_isGrounded && _rigidbody.velocity.sqrMagnitude > 0.1f)
            //{
            //    _audioSource.clip = input.Run ? runningSound : walkingSound;
            //    if (!_audioSource.isPlaying)
            //    {
            //        _audioSource.Play();
            //    }
            //}
            //else
            //{
            //    if (_audioSource.isPlaying)
            //    {
            //        _audioSource.Pause();
            //    }
            //}
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
            //else
            //{
            //    if (_audioSource.isPlaying)
            //    {
            //        _audioSource.Pause();
            //    }
            //}
        }

        private void PlayGPStartSound()
        {
            //Debug.Log("GP START SOUND");
            _audioSource.clip = gpSound;
            //if (!_audioSource.isPlaying)
            //{
                _audioSource.Play();
            //}
        }

        private void PlayGPLandSound()
        {
            _audioSource.clip = gpLandSound;
            _audioSource.loop = false;
            //if (!_audioSource.isPlaying)
            //{
            _audioSource.Play();
            //}
        }

        private void PlayGroundDashSound()
        {
            _audioSource.clip = groundDashSound;
            _audioSource.loop = true;
            //if (!_audioSource.isPlaying)
            //{
            _audioSource.Play();
            //}
        }

        private void PlayAirDashSound()
        {
            _audioSource.clip = airDashSound;
            _audioSource.loop = false;
            //if (!_audioSource.isPlaying)
            //{
            _audioSource.Play();
            //}
        }

        private void PlayGlideSound()
        {
            _audioSource.clip = glideSound;
            _audioSource.loop = false;
            //if (!_audioSource.isPlaying)
            //{
            _audioSource.Play();
            //}
        }

        private void PlayLandSound()
        {
            _audioSource.clip = landSound;
            _audioSource.loop = false;
            //if (!_audioSource.isPlaying)
            //{
            _audioSource.Play();
            //}
        }

        private void PlayJumpSound()
        {
            _audioSource.clip = jumpSound;
            _audioSource.loop = false;
            //if (!_audioSource.isPlaying)
            //{
            _audioSource.Play();
            //}
        }

        private void PlayJumpHoldSound()
        {
            _audioSource.clip = jumpHoldSound;
            _audioSource.loop = false;
            _audioSource.volume = 1f;// 0.05f;
            //if (!_audioSource.isPlaying)
            //{
            _audioSource.Play();
            //}
        }

        private void PlaySlideSound()
        {
            //Debug.Log("GP START SOUND");
            _audioSource.clip = slideSound;
            _audioSource.loop = false;
            //if (!_audioSource.isPlaying)
            //{
            _audioSource.Play();
            //}
        }

        private void PlaySkidSound()
        {
            Debug.Log("SKID SOUND");
            _audioSource.clip = skidSound;
            _audioSource.loop = false;
            //if (!_audioSource.isPlaying)
            //{
            _audioSource.Play();
            //}
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

        //void OnControllerColliderHit(ControllerColliderHit hit)
        //{
        //    //wall jump logic
        //    //TODO - only do this code on first frame wall jump is pressed
        //    //TODO - store character velocity before collision (otherwise it will be near zero after first frame of collision)
        //    if (!isGrounded && !isGroundPound )//&& !wallJumped)// && isCollidingWithWall) // && !isAirDashing
        //    {
        //        //only set wall jump trajectory on first frame of collision
        //        if (!wallCollisionStarted)
        //        {
        //            //wallJumpDirection = Vector3.Reflect(playerVel, hit.normal).normalized;
        //            wallJumpDirection = hit.normal;
        //            //Debug.Log(wallJumpDirection);
        //            //counter++;
        //            //wallCollisionDot = Vector3.Dot(hit.normal, new Vector3(playerVel.x, 0f, playerVel.z).normalized);
        //            wallCollisionStarted = true;
        //        }
        //        //collide with wall
        //        if (hit.normal.y < 0.05)
        //        {
        //            float dot = Vector3.Dot(hit.normal, new Vector3(playerVel.x, 0f, playerVel.z).normalized);
        //            //if 45 degrees to wall or less, reflect motion angle over wall normal.  Wall jump vertical angle set to 45 degrees
        //            if (dot != 100f)//(dot <= -0.75f || Mathf.Abs(dot)==0.001)//!= 100
        //            {
        //                if (Input.GetButtonDown("Jump"))
        //                {
        //                    PlayJumpSound();
        //                    //Debug.Log("WALLJUMPED");
        //                    wallJumped = true;
        //                    wallCollisionStarted = false;
        //                    launchVelocity = wallJumpDirection * 18f;
        //                    velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
        //                }
        //            }

        //        }
        //    }

        //}

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
                            }

                            //allow airdash post-wall jump
                            airDashesUsed = 0;

                            PlayJumpSound();
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

        public bool getIsGrounded()
        {
            return this.isGrounded;
        }

        public bool getIsGroundDash()
        {
            return this.isGroundDash;
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

        public float getDegreesRotated()
        {
            return this.degreesRotated;
        }
    }

    //mainCamera.transform.localPosition = new Vector3(mainCamera.transform.localPosition.x, (mainCamera.transform.localPosition.y - controller.height / 4f), mainCamera.transform.localPosition.z);
}