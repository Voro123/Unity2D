using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyForward : MonoBehaviour
{
    public float flyspeed = 10;
    private Rigidbody2D rgb;

    // Start is called before the first frame update
    void Start()
    {
        rgb = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rgb.velocity = transform.up * flyspeed;
    }
}
