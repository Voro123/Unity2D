using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destory : MonoBehaviour
{
    public float delay = 2;
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Destroy(gameObject, delay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
