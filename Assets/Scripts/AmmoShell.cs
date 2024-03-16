using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoShell : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float position_y, yInitVel, xInitVel, grav = -9.8f, height = 0.5f, velTime, rotationSpeed;
    [SerializeField] private bool _eject = false;

    void Start()
    {
        position_y = transform.position.y;
        rb.velocity = new Vector2(xInitVel, yInitVel);
    }

    public void setVel(float _xVel, float _yVel)
    {
        xInitVel = _xVel;
        yInitVel = _yVel;
    }

    public void eject()
    {
        _eject = true;
    }

    void FixedUpdate()
    {
        rb.simulated = _eject;
        if(_eject)
        {
            velTime += Time.fixedDeltaTime;

            if(rb.velocity.magnitude < 0.5f)
            {
                rb.velocity = Vector2.zero;
                Destroy(gameObject);    // to replace with async fading
            }

            transform.Rotate(0f, 0f, rotationSpeed * Time.fixedDeltaTime);

            if (transform.position.y <= position_y - height && rb.velocity.y < 0)
            {
                float yVelocity = -rb.velocity.y * 0.25f;
                float xVelocity = rb.velocity.x * 0.25f;
                rb.velocity = new Vector2(xVelocity, yVelocity);
                velTime = 0;
            }
            else
            {
                float yVelocity = rb.velocity.y + grav * velTime;
                rb.velocity = new Vector2(rb.velocity.x, yVelocity);
            }
        }
    }
}
