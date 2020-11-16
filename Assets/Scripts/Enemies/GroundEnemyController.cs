﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundEnemyController : MonoBehaviour
{
    public Transform lineEndpoint;
    public LayerMask layerMask;

    private Rigidbody2D rigidbody2d;
    public EnemySight enemySight;
    public GameObject attackObject;

    private Attack attackComponent;
    public bool isTherePlatform;
    public float speed;
    public float direction;
    public bool isInAir = false;
    public float attackRange;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        direction = 1;
        attackComponent = attackObject.GetComponent<Attack>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        LookForPlatformEdge();
        Move();
        SearchForPlayer();
    }

    private void LookForPlatformEdge()
    {
        isTherePlatform = Physics2D.Linecast(transform.position, lineEndpoint.position, layerMask);

        Debug.DrawLine(transform.position, lineEndpoint.position, Color.green);
    }

    private void Move()
    {
        if (!isInAir)
        {
            if (isTherePlatform)
            {
                rigidbody2d.AddForce(Vector2.right * speed * Time.deltaTime * direction);
            }
            else
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                direction *= -1;
            }

            rigidbody2d.velocity *= 0.90f;
        }
    }

    private void SearchForPlayer()
    {
        if (enemySight.seesPlayer)
        {
            if (enemySight.distanceToPlayer <= attackRange && !attackObject.activeInHierarchy)
            {
                attackComponent.attack();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isInAir = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isInAir = false;
        }
    }
}
