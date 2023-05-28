using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translate2D : MonoBehaviour
{
    public Vector2 velocity;
    public Vector2 min;
    public Vector2 max;

    void Update()
    {

        if (transform.position.x < min.x || transform.position.x > max.x)
            velocity = new Vector2(-velocity.x, velocity.y);
        if (transform.position.y < min.y || transform.position.y > max.y)
            velocity = new Vector2(velocity.x, -velocity.y);

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, min.x, max.x),
                 Mathf.Clamp(transform.position.y, min.y, max.y), transform.position.z);

        transform.position += new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime;

    }
}
