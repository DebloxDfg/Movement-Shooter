using Unity.Mathematics;
using Random = UnityEngine.Random;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class GunSystem : MonoBehaviour
{
   [Header("Gun stats")]
   public int damage;
   public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
   public int magazineSize, bulletsPerTap;
   public bool allowButtonHold;
   
   private int bulletsLeft, bulletsShot;

   //bools
   private bool shooting, readyToShoot, realoading;

   [Header("Reference")]
   public Camera fpsCamera;
   public Transform attackPoint;
   public RaycastHit rayHit;
   public LayerMask whatIsEnemy;
   public GameObject player;

   [Header("Graphics")]
   public GameObject muzzleFlash;
   public GameObject bulletHoleGraphic;
   public Animation gunAnimation;
   [HideInInspector]
   public CameraShake cameraShake;
   public float cameraShakeMaginitude, cameraShakeDuration;
   public TextMeshProUGUI bulletText;

   [Header("Sounds")]
    public AudioClip gunShotSoundEffect;
    public AudioClip reloadSoundEffect;

   private AudioSource source;

    void Start()
    {
        gunAnimation.GetComponent<Animation>();
        cameraShake = new CameraShake();
        source = player.GetComponent<AudioSource>();

        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    void Update()
    {
        MyInput();
    }
    private void MyInput()
    {
        if(allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);
        
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !realoading)
        {
            Reload();
        }
        
        //graphics
        if(realoading)
            bulletText.SetText($"... / {magazineSize}");
        else
            bulletText.SetText($"{bulletsLeft} / {magazineSize}");
        

        //Shoot
        if(readyToShoot && shooting && !realoading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        //play sound
        source.PlayOneShot(gunShotSoundEffect);

        //spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        //Calculate Direction with spread
        Vector3 direction = fpsCamera.transform.position + new Vector3(x, y, 0);

        //RayCast
        GameObject flash = Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity, attackPoint);
        Destroy(flash, 0.1f);

        if(Physics.Raycast(fpsCamera.transform.position, direction, out rayHit, range))
        {
            if (((1 << rayHit.collider.gameObject.layer) & whatIsEnemy) != 0)
            {
                Debug.Log(rayHit.collider.name);

                // if (rayHit.collider.CompareTag("Enemy"))
                // rayHit.collider.GetComponent<ShootingAI>().TakeDamage(damage);
            }
            else
            {
                Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.LookRotation(rayHit.normal));
            }
        }

        bulletsLeft--;
        bulletsShot--;

        Invoke("ResetShot", timeBetweenShooting);

        if(bulletsShot > 0 && bulletsLeft > 0)
            Invoke("shoot", timeBetweenShots);
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    private void Reload()
    {
        realoading = true;

        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        //reload sound effect
        source.PlayOneShot(reloadSoundEffect);
        
        bulletsLeft = magazineSize;
        realoading = false;
    }
}