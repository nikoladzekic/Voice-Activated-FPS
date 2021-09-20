using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProjectileGun : MonoBehaviour
{
    public static ProjectileGun instance;

    public GameObject bullet;
    private Camera Maincamera;

    private GameObject fpsCam;
    private GameObject scope;

    public float shootForce, upwardForce;
    public float scopedFOV = 15f;
    private float normalFOV;

    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    public Animator animator;

    public int bulletsLeft, bulletsShot;

    //Recoil
    public float recoilForce;

     public bool shooting, readyToShoot, reloading;

    public Transform attackPoint;

    //Grafika
    public ParticleSystem muzzleFlash;
    private TextMeshProUGUI ammunitionDisplay;

    public bool allowInvoke = true;
    private bool isScoped=false;
    private bool isRunning = false;


    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
        scope = GameManager.instance.scope;

        ammunitionDisplay = GameManager.instance.ammo;


    }
    // Start is called before the first frame update
    void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        fpsCam = GameObject.FindWithTag("FPSCam");
        Maincamera = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        MyInput();

        //////////////ANIMACIJE//////////////
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            isScoped = !isScoped;
            if (isRunning) animator.SetBool("Running", false);

            animator.SetBool("Scoped", isScoped);
            Invoke("OnScoped", 0.25f);
        }
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S))
        {
            isRunning = true;
            animator.SetBool("Running", isRunning);
            
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
        {
            isRunning = false;
            animator.SetBool("Running", isRunning);
        }
        //////////////////////////////////////////

        //Prikazati ammo HUD ako postoji
        if(ammunitionDisplay != null)
        {
            ammunitionDisplay.SetText(""+bulletsLeft / bulletsPerTap);
        }

    }

    /// <summary>
    /// Animacija za scope sa malim delay-om
    /// </summary>
    private void OnScoped()
    {
        scope.SetActive(isScoped);
        fpsCam.SetActive(!isScoped);

        //Efekat zoom-a
        if (isScoped)
        {
            normalFOV = Maincamera.fieldOfView;
            Maincamera.fieldOfView = scopedFOV;
        }
        else Maincamera.fieldOfView = normalFOV;
    }


    private void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        //Reloadovanje
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();

        //Auto Reload ukoliko igrac pokusa da reloaduje kad je magazin prazan
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        //Auto reload ako pokusa da puca kad je skopovan
        if(readyToShoot && shooting && !reloading && bulletsLeft<=0 && isScoped)
        {
            Reload();
            animator.SetBool("Reloading", reloading);

        }
        //Pucanje
        if(readyToShoot && shooting && !reloading && bulletsLeft> 0)
        {
            bulletsShot = 0;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        Ray ray = Maincamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        //provera da li je uradio u nesto
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);

        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        //Novi pravac sa spread-om
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        //Instanciranje metka
        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);

        //Rotiranje metka da ide u pravcu pucnja
        currentBullet.transform.forward = directionWithSpread.normalized;

        //Dodavanje sile metku
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);

        //Instanciranje muzzleFlash-a
        if(muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;

        }
        bulletsShot++;
        //Ukoliko zelimo vise metkova odjednom, odnosno ako je bulletsPerTap > 1
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);
        bulletsLeft--;

    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        animator.SetBool("Reloading", true);
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        animator.SetBool("Reloading", false);
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
