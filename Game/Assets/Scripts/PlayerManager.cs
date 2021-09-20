using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth;
    public MeshRenderer model;
    public GameObject gunHolder;
    public GameObject textHolder;
    private GameObject textDmg;
    bool equipped;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (textHolder && health>0 && health!=maxHealth)
        {
            ShowDamage();
        }

        if(health <= 0f)
        {
            Die();
        }
    }
    public void SetWeapon()
    {
        if (gunHolder.transform.childCount > 0 && gunHolder!=null)
        { 
          Transform child = gunHolder.transform.GetChild(0);
            Destroy(child.gameObject);
        }
    }

    void ShowDamage()
    {
        textDmg = Instantiate(textHolder, transform.position, Quaternion.identity, transform);
        textDmg.GetComponent<TMPro.TextMeshPro>().text = health.ToString();
    }

    public void Die()
    {
        SetWeapon();
        model.enabled = false;
        if (gameObject.GetComponent<SpeechRecognition>() != null)
            SpeechRecognition.instance.equipped = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }

}
