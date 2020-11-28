﻿/* GroundEnemyController.cs
 * 
 * Samuel Ko
 * 101168049
 * Last Modified: 2020-11-28
 * 
 * AI for land-based enemies.
 * 
 * 2020-11-16: Added this script.
 * 2020-11-16: AI can detect edges of platforms and will not walk off.
 * 2020-11-16: AIs can be knocked back by the player's attack.
 * 2020-11-16: AIs will not freak out in the air.
 * 2020-11-16: AI sees the player and will attack when in range.
 * 2020-11-21: Added Health.
 * 2020-11-22: Adjusting attack spam.
 * 2020-11-22: Added reset.
 * 2020-11-23: Added collider reset for better platform detection.
 * 2020-11-28: Added animation.
 * 2020-11-28: Added random character customization.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GroundEnemyController : ICharacter
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

    public int defaultPoints = 100;
    public int soulPoints = 100;
    public float attackDelay = 0;
    public float maxAttackDelay = 1.0f;

    private Vector3 startingPosition;

    [SerializeField]
    private List<Animator> mandatoryCharacterAnimators;
    [SerializeField]
    private List<Animator> optionalCharacterAnimators;
    [SerializeField]
    private List<SwappablePart> swappableCharacterAnimators;

    [System.Serializable]
    public class SwappablePart
    {
        public List<Animator> options;
    }

    [SerializeField]
    private List<Animator> allAnimators;

    // Start is called before the first frame update
    void Awake()
    {
        startingPosition = transform.position;

        CharacterConfiguration();

        foreach (Animator bodyPart in allAnimators)
        {
            bodyPart.SetInteger("AnimState", (int)PlayerMovementState.RUN);
        }

        objType = EnumSpawnObjectType.AI;

        rigidbody2d = GetComponent<Rigidbody2D>();
        direction = 1;
        attackComponent = attackObject.GetComponent<Attack>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (attackDelay > 0)
            attackDelay -= Time.deltaTime;

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
                ChangeDirection();
            }

            rigidbody2d.velocity *= 0.90f;
        }
    }

    private void ChangeDirection()
    {
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        direction *= -1;
    }

    private void SearchForPlayer()
    {
        if (enemySight.seesPlayer)
        {
            if (enemySight.distanceToPlayer <= attackRange && !attackObject.activeInHierarchy)
            {
                if (attackDelay <= 0)
                {
                    attackComponent.attack();
                    attackDelay = maxAttackDelay;
                    
                    foreach (Animator bodyPart in allAnimators)
                    {
                        bodyPart.SetTrigger("Attack");
                    }
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isInAir = true;
        }

        ColliderReset();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isInAir = false;
        }
    }

    public override void UpdateHealth(int pointLoss, int heartGain, Vector2 knockbackForce)
    {
        // Turn around when hit from behind
        if (Mathf.Sign(knockbackForce.x) == Mathf.Sign(direction))
        {
            ChangeDirection();
        }

        rigidbody2d.AddForce(knockbackForce);
        soulPoints -= pointLoss;

        if (soulPoints <= 0)
        {
            GameManager.Instance.UpdateHealth(Random.Range(10, 50), 0);
            Despawn();
        }
    }

    public void CharacterConfiguration()
    {
        allAnimators.AddRange(mandatoryCharacterAnimators);

        foreach (Animator part in optionalCharacterAnimators)
        {
            if (Random.Range(0.0f, 1.0f) < 0.5f)
            {
                part.gameObject.SetActive(true);
                allAnimators.Add(part);
            }
            else
            {
                part.gameObject.SetActive(false);
            }
        }

        foreach (SwappablePart part in swappableCharacterAnimators)
        {
            foreach (Animator option in part.options)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    option.gameObject.SetActive(true);
                    allAnimators.Add(option);
                    return;
                }
            }

            // If no part is selected, default to one.
            part.options[0].gameObject.SetActive(true);
            allAnimators.Add(part.options[0]);
        }
    }

    // Refires trigger
    public void ColliderReset()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Collider2D>().enabled = true;
    }

    public override void Reset()
    {
        transform.position = startingPosition;
        soulPoints = defaultPoints;
        gameObject.SetActive(true);
    }
}
