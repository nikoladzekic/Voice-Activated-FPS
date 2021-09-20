using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public static PlayerController instance;

    public Transform camTransform;
    public SpeechRecognition speech;
    private ProjectileGun gun;
    public GameObject gunContainer;
    bool _input;

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    private void Update()
    {
        if (speech.equipped)
        {
            if (speech.word == "gun")
            {
                if(Input.GetKeyDown(KeyCode.Mouse0))
                    ClientSend.PlayerShoot(camTransform.forward);
            }
            else
            {
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    gun = gunContainer.GetComponentInChildren<ProjectileGun>();
                    if (gun != null)
                        Shoot();
                }
            }
        }
    }
    private void SendInputToServer()
    {
        bool[] _inputs = new bool[] {
        Input.GetKey(KeyCode.W),
        Input.GetKey(KeyCode.S),
        Input.GetKey(KeyCode.A),
        Input.GetKey(KeyCode.D),
        Input.GetKey(KeyCode.Space),
        Input.GetKey(KeyCode.LeftShift),
    };

        ClientSend.PlayerMovement(_inputs);

    }

    private void Shoot()
    {
        if(gun.readyToShoot && !gun.reloading && gun.bulletsLeft > 0)
            ClientSend.PlayerShoot(camTransform.forward);
    }
}
