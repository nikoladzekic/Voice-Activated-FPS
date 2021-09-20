using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDestroy : MonoBehaviour
{
    public float LifeTime;

    private void Update()
    {
        Invoke(nameof(Delay), LifeTime);
    }

    private void Delay()
    {
        Destroy(gameObject);
    }
}
