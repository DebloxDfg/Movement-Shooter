using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Kick : MonoBehaviour
{
    [Header("Movement")]
    public float kickGroundForce = 2;
    public float kickWallForce = 3;
    public float kickEnemyFoce = 4;
    public float kickAirForce = 1;
    public float kickForceMultiplier = 5f;

    [Header("Settings")]
    public float kickReach = 2f;
    public float timeDependingOnAnimation = 1;

    [Header("References")]
    public GameObject leg;
    public Transform cameraPerspective;

    private Vector3 rememberedDirection;
    private RaycastHit rayCastHit;
    private Animation anim;

    private Rigidbody rb;
    private PlayerMovement3 playerMovement;

    void Start()
    {
        anim = leg.GetComponent<Animation>();
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement3>();
    }

    void Update()
    {
        if(Input.GetButtonDown("Kick") && !anim.isPlaying)
        {
            anim.Play("Kick animation");

            Invoke("RayHit", timeDependingOnAnimation);
        }
    }

    private void RayHit()
    {
        rememberedDirection = playerMovement.moveDirection;
        playerMovement.moveDirection = Vector3.zero;

        if(Physics.Raycast(cameraPerspective.position, cameraPerspective.forward, out rayCastHit, kickReach))
        {
            if(rayCastHit.collider.gameObject.tag == "Ground")
            {
                rb.AddForce((-cameraPerspective.forward + rememberedDirection) * kickGroundForce * kickForceMultiplier, ForceMode.Impulse);
            }
            else if(rayCastHit.collider.gameObject.tag == "Wall")
            {
                rb.AddForce((-cameraPerspective.forward + rememberedDirection) * kickWallForce * kickForceMultiplier, ForceMode.Impulse);
            }
            else if(rayCastHit.collider.gameObject.tag == "Enemy")
            {
                rb.AddForce((-cameraPerspective.forward + rememberedDirection) * kickEnemyFoce * kickForceMultiplier, ForceMode.Impulse);
            }
        }
        else
        {
            rb.AddForce((-cameraPerspective.forward + rememberedDirection) * kickAirForce * kickForceMultiplier, ForceMode.Impulse);
        } 
    }

    public void RememeberPlayerSpeed()
    {
        
    }

    public void SetPlayerSpeedZero()
    {
        
    }
}
