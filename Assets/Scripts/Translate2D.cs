using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translate2D : MonoBehaviour
{
    public Vector3 velocity;
    public Vector3 min;
    public Vector3 max;

    void Update()
    {

        if (transform.position.x < min.x || transform.position.x > max.x)
            velocity = new Vector3(-velocity.x, velocity.y, velocity.z);
        if (transform.position.y < min.y || transform.position.y > max.y)
            velocity = new Vector3(velocity.x, -velocity.y, velocity.z);
        if (transform.position.z < min.z || transform.position.z > max.z)
            velocity = new Vector3(velocity.x, velocity.y, -velocity.z);

        transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, min.x, max.x),
                Mathf.Clamp(transform.position.y, min.y, max.y),
                Mathf.Clamp(transform.position.z, min.z, max.z));

        transform.position += new Vector3(velocity.x, velocity.y, velocity.z) * Time.deltaTime;

    }
}
