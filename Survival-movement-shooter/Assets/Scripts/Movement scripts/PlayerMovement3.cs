using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement3 : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;

    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;

    public float airMultiplier;
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;


    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;

    [Header("Slope handling")]
    public float maxSlopeAngle;
    public RaycastHit slopeHit;

    [Header("Keybinds")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;


    [Header("Ground check")]
    public float playerHeight;
    public float groundDistance;
    public LayerMask whatIsGround;
    public Transform groundCheck;

    [Header("References")]
    public Transform orientation;
    public Transform playerObj;

    [Header("Sounds")]
    public List<AudioClip> movingAudioClips;
    public AudioClip jumpAudioClip;
    

    private float startYScale;
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private float horizontalInput;
    private float verticalInput;
    private float lastSoundTime;

    [HideInInspector]
    public bool isGrounded;
    [HideInInspector]
    public bool isSliding;
    private bool canJump;
    private bool exitingSlope;

    [HideInInspector]
    public Vector3 moveDirection;

    private Rigidbody rb;

    [HideInInspector]
    public MovementState movementState;

    [HideInInspector]
    public AudioSource source;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        air
    }

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;
        canJump = true;

        startYScale = transform.localPosition.y;
        lastSoundTime = Time.time;
    }

    void Update()
    {
        //ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);

        Debug.Log(isGrounded);
        
        PlayerInput();

        transform.localScale = playerObj.localScale;

        SpeedController();
        StateHandler();

        //handle drag
        if(isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //when to jump
        if(Input.GetKey(jumpKey) && isGrounded && canJump)
        {
            canJump = false;

            PlayerJump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //crouch start
        if(Input.GetKeyDown(crouchKey))
        {
            playerObj.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        //chrouch end
        if(Input.GetKeyUp(crouchKey))
        {
            playerObj.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {   
        //Mode - sliding
        if(isSliding)
        {
            movementState = MovementState.sliding;

            if(OnSlope() && rb.velocity.y < .1f)
                desiredMoveSpeed = slideSpeed;

            else
                desiredMoveSpeed = sprintSpeed;
        }
        //Mode - chrouching
        else if(Input.GetKey(crouchKey))
        {
            movementState = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;    
        }
        //Mode - sprinting
        else if(isGrounded && Input.GetKey(sprintKey))
        {
            movementState = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        //Mode - walking
        else if(isGrounded)
        {
            movementState = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        //Mode - air
        else
        {
            movementState = MovementState.air;
        }

        //check if desiredMoveSpeed has changed drastically
        if(Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        //smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while(time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if(OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        //calculates movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //sound effect
        PlayWalkSound();
    
        //is on slope
        if(OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if(rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        //is on ground
        if(isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // is in air
        else if(!isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

            //turn gravity off while on slope
            rb.useGravity = !OnSlope();
        
    }

    private void SpeedController()
    {
        //limiting speed on slope
        if(OnSlope())
        {
            if(rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        //limiting speed on ground or in air
        else
        {
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f ,rb.velocity.z);

            //limits velocity if needed
            if(flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }
    }

    private void PlayerJump()
    {   
        //sound effect
        PlayJumpSound();

        exitingSlope = true;

        //reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * .5f + .3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void PlayWalkSound()
    {
        int i = Random.Range(0, 5);

        if(movementState == MovementState.walking && isGrounded && rb.velocity.x > .1f || rb.velocity.z > .1f)
        {
            if (Time.time - lastSoundTime > .4f)
            {
                lastSoundTime = Time.time;
                source.PlayOneShot(movingAudioClips[i]);
            }
        }
        else if(movementState == MovementState.sprinting && isGrounded && rb.velocity.x > .1f || rb.velocity.z > .1f)
        {
            if (Time.time - lastSoundTime > .3f)
            {
                lastSoundTime = Time.time;
                source.PlayOneShot(movingAudioClips[i]);
            }
        }
        else if(movementState == MovementState.crouching && isGrounded && rb.velocity.x > .1f || rb.velocity.z > .1f)
        {
           if (Time.time - lastSoundTime > .7f)
            {
                lastSoundTime = Time.time;
                source.PlayOneShot(movingAudioClips[i]);
            }
        }
    }

    public void PlayJumpSound()
    {
        source.PlayOneShot(jumpAudioClip);
    }
}