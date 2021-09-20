using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jumpad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        collision.rigidbody.AddExplosionForce(4000f, transform.position, 2, 100);
    }
}
