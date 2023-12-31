using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonoBehaviour
{
    [SerializeField] float m_speed = 3f;

    private Animator animator;
    private Rigidbody2D m_body2d;
    public Transform player;
    public float aggressiveDistance;
    public int maxHP = 30;
    private int currentHP;
    public Transform m_attackPoint;
    public float m_attackRange = 3.5f;
    public LayerMask heroLayers;
    int attackDamage = 10;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_body2d.constraints = RigidbodyConstraints2D.FreezeRotation;
        currentHP = maxHP;
    }

    private bool isAttacking = false;
    private float attackCooldown = 2f;
    private float currentAttackCooldown = 0f;

    // Update is called once per frame
    void Update()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer < aggressiveDistance && distToPlayer > 3.5)
        {
            StartChase();
            isAttacking = false;
        }
        else if (distToPlayer <= 3.5 && !isAttacking)
        {
            Attack();
        }
        else
        {
            animator.SetInteger("AnimState", 1);
        }

        if (currentAttackCooldown > 0)
        {
            currentAttackCooldown -= Time.deltaTime;
        }
    }

    void Attack()
    {
        if (currentAttackCooldown <= 0)
        {
            Collider2D[] hitHero = Physics2D.OverlapCircleAll(m_attackPoint.position, m_attackRange, heroLayers);
            if (hitHero[0].GetComponent<HeroKnight>().isDead)
            {
                return;
            }
            isAttacking = true;

            animator.SetTrigger("Attack");

            StartCoroutine(DealDamageAfterDelay(hitHero[0].GetComponent<HeroKnight>(), attackDamage, 0.3f));
            StartCoroutine(ResetAttackFlag());

            currentAttackCooldown = attackCooldown;
        }
    }

    IEnumerator DealDamageAfterDelay(HeroKnight target, int damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        target.TakeDamage(damage);
    }

    IEnumerator ResetAttackFlag()
    {
        yield return new WaitForSeconds(2f);

        isAttacking = false;
    }

    void Run(int direction)
    {
        m_body2d.velocity = new Vector2(direction * m_speed, player.position.y);
        animator.SetInteger("AnimState", 2);
    }

    void StartChase()
    {
        if (player.position.x < transform.position.x)
        {
            Run(-1);
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else if (player.position.x > transform.position.x)
        {
            Run(1);
            GetComponent<SpriteRenderer>().flipX = false;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        animator.SetTrigger("Hurt");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        animator.SetTrigger("Death");
        m_body2d.velocity = new Vector2(0, 0);
        m_body2d.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }
}
