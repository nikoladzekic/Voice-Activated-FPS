using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    public static Player instance;


    public int id;
    public string username;
    //Wallrunning
    public LayerMask whatIsWall;
    public float wallrunForce, maxWallrunTime, maxWallSpeed;
    bool isWallRight, isWallLeft;
    bool isWallRunning;
    public float maxWallRunCameraTilt, wallRunCameraTilt;

    //Other
    private Rigidbody rb;

    //Shooting
    public Transform shootOrigin;
    public float health;
    public float maxHealth = 100f;

    public GameObject bullet;

    public float shootForce, upwardForce;

    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;


    //Recoil
    public float recoilForce;
    public bool allowInvoke = true;

    //Movement
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    private float startMaxSpeed;
    public bool grounded;
    public LayerMask whatIsGround;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;
    public float crouchGravityMultiplier;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;

    public int startDoubleJumps = 1;
    int doubleJumpsLeft;

    //Input
    public float x, y;
    bool jumping, crouching;


    //Sliding
    private Vector3 normalVector = Vector3.up;

    public float bulletDamage;

    private bool[] inputs;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        rb = GetComponent<Rigidbody>();
        startMaxSpeed = maxSpeed;
        health = maxHealth;
        inputs = new bool[6];
    }

    private void WallRunInput() //make sure to call in void Update
    {
        //Wallrun
        if (inputs[3] && isWallRight) StartWallrun(); 
        if (inputs[2] && isWallLeft) StartWallrun();
    }
    private void StartWallrun()
    {
        rb.useGravity = false;
        isWallRunning = true;

        if (rb.velocity.magnitude <= maxWallSpeed)
        {
            rb.AddForce(gameObject.transform.forward * wallrunForce * Time.deltaTime);
            rb.AddForce(-gameObject.transform.up * (wallrunForce / 2) * Time.deltaTime);

            //Make sure char sticks to wall
            if (isWallRight)
                rb.AddForce(gameObject.transform.right * wallrunForce / 5 * Time.deltaTime);
            else
                rb.AddForce(-gameObject.transform.right * wallrunForce / 5 * Time.deltaTime);

        }
    }
    private void StopWallRun()
    {
        isWallRunning = false;
        rb.useGravity = true;
    }
    private void CheckForWall() //make sure to call in void Update
    {
        isWallRight = Physics.Raycast(transform.position, gameObject.transform.right, 1f, whatIsWall);
        isWallLeft = Physics.Raycast(transform.position, -gameObject.transform.right, 1f, whatIsWall);

        //leave wall run
        if (!isWallLeft && !isWallRight) StopWallRun();
        //reset double jump (if you have one :D)
        if (isWallLeft || isWallRight) doubleJumpsLeft = startDoubleJumps;
    }



    private void FixedUpdate()
    {
        if (health <= 0f) { return; }
        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }
        jumping = inputs[4];
        crouching = inputs[5];

        if (crouching)
            StartCrouch(_inputDirection);
        if(!crouching)
            transform.localScale = new Vector3(1, 1, 1);

        //Double Jumping
        if (jumping && !grounded && doubleJumpsLeft >= 1)
        {
            Jump();
            doubleJumpsLeft--;
        }

        CheckForWall();
        WallRunInput();
        Movement(_inputDirection);

    }
    private void StartCrouch(Vector2 _inputDirection)
    {
        
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        transform.localScale = new Vector3(transform.localScale.x, 0.7f, transform.localScale.z);
        if (rb.velocity.magnitude > 0.5f)
        {
            if (grounded)
            {
                rb.AddForce(gameObject.transform.forward * slideForce* _inputDirection.y);
            }
        }

        ServerSend.PlayerPosition(this);
    }

    private void Movement(Vector2 _inputDirection)
    {
        x = _inputDirection.x;
        y = _inputDirection.y;
        //Extra gravity
        //Needed that the Ground Check works better!
        float gravityMultiplier = 1f;

        if (crouching) gravityMultiplier = crouchGravityMultiplier;

        rb.AddForce(Vector3.down * Time.deltaTime * gravityMultiplier);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping && grounded) Jump();

        //ResetStuff when touching ground
        if (grounded)
        {
            doubleJumpsLeft = startDoubleJumps;
        }

        //Set max speed
        float maxSpeed = this.maxSpeed;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (grounded && crouching) multiplierV = 0.5f;

        //Apply forces to move player
        rb.AddForce(gameObject.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(gameObject.transform.right * x * moveSpeed * Time.deltaTime * multiplier);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    private void Jump()
    {
        if (grounded && jumping)
        {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if (!grounded && jumping)
        {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(gameObject.transform.forward * jumpForce * 1f);
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //Reset Velocity
            rb.velocity = Vector3.zero;

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //Walljump
        if (isWallRunning)
        {
            readyToJump = false;

            //normal jump
            if (isWallLeft && !inputs[2] || isWallRight && !inputs[3])
            {
                rb.AddForce(Vector2.up * jumpForce * 1.5f);
                rb.AddForce(normalVector * jumpForce * 0.5f);
            }

            //sidewards wallhop
            if (isWallRight || isWallLeft && inputs[3] ||inputs[2]) rb.AddForce(-gameObject.transform.up * jumpForce * 3f);
            if (isWallRight && inputs[3]) rb.AddForce(-gameObject.transform.right * jumpForce * 3.2f);
            if (isWallLeft && inputs[2]) rb.AddForce(gameObject.transform.right * jumpForce * 3.2f);

            //Always add forward force
            rb.AddForce(gameObject.transform.forward * jumpForce * 1f);


            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

  
    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        //Slow down sliding
        if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * gameObject.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * gameObject.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = gameObject.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    /// <summary>
    /// //Shooting
    /// </summary>
    /// <param name="_inputs"></param>
    /// <param name="_rotation"></param>
    public void Shoot(Vector3 _viewDirection)
    {
        if(health <= 0f)
        {
            return;
        }

        //NetworkManager.instance.InstantiateProjectile(shootOrigin).Initialize(_viewDirection, shootForce, id);
        if(Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, 100f))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                _hit.collider.GetComponent<Player>().TakeDamage(bulletDamage);
            }
        }

    }

    public void TakeDamage(float _damage)
    {
        if(health <= 0f)
        {
            return;
        }
        health -= _damage;
        if(health <= 0f)
        {
            transform.position = new Vector3(UnityEngine.Random.Range(-5,40), 3f, UnityEngine.Random.Range(-5, 15));
            ServerSend.PlayerPosition(this);
            rb.isKinematic = true;
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(1);
        health = maxHealth;
        rb.isKinematic = false;
        ServerSend.PlayerRespawned(this);
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

}