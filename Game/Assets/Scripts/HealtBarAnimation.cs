using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealtBarAnimation : MonoBehaviour
{
    private Animator heart;
    private float playerHealth;
    public AudioSource heartSound;

    private void Start()
    {
        heart = GameManager.instance.Heart.GetComponent<Animator>();
    }
    void Update()
    {
        playerHealth = gameObject.GetComponent<PlayerManager>().health;

        if (playerHealth > 0)
        {
            GameManager.instance.healthText.text = playerHealth.ToString();
            heart.speed = (101 - playerHealth) / 9;
            if (playerHealth < 40)
            {
                GameManager.instance.deathScreen.SetActive(true);
                heartSound.pitch = 1.5f;
            }
            else if(playerHealth <80){
                heartSound.pitch = 1.2f;
            }
            else
            {
                heartSound.pitch = 0.9f;
                GameManager.instance.deathScreen.SetActive(false);
            }
        }
    
    }


}
