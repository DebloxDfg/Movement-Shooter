using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Slide : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;

    [Header("Siiding")]
    public float maxSlideTime;
    public float slideForce;
    public float slideYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    


    private float slideTimer;
    private float startYScale;
    private float horizontalInput;
    private float verticalInput;

    private Rigidbody rb;
    private PlayerMovement3 playerMovement;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement3>();

        startYScale = playerObj.localScale.y;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0) && playerMovement.isGrounded)
            StartSlide();
        
        if(Input.GetKeyUp(slideKey) && playerMovement.isGrounded)
            StopSlide();
    }

    void FixedUpdate()
    {
        if(playerMovement.isGrounded)
            SlididngMovement();
    }

    private void StartSlide()
    {
        playerMovement.isGrounded = true;
        playerMovement.isSliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.z, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlididngMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //sliding normal
        if(!playerMovement.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }
       
        //sliding down a slope
        else
        {
            rb.AddForce(playerMovement.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }
        if(slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        playerMovement.isGrounded = false;
        playerMovement.isSliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.z, startYScale, playerObj.localScale.z);
    }

}
