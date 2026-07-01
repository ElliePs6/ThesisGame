using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotation : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, 45f * Time.deltaTime, 0f);
    }
}
