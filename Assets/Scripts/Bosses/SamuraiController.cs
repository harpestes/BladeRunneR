using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class SamuraiController : MonoBehaviour
{
    private Rigidbody2D m_body2d;
    public Transform player;
    public Animator animator;
    public float m_speed = 3f;
    public float aggressiveDistance;
    public int maxHP = 100;
    private int currentHP;
    public int attackCounter = 0;
    public Transform m_attackPoint;
    public float m_attackRange = 2.0f;
    public LayerMask heroLayers;
    int attackDamage = 15;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_body2d.constraints = RigidbodyConstraints2D.FreezeRotation;
        currentHP = maxHP;
    }

    // Update is called once per frame
    private bool isAttacking = false;
    private float attackCooldown = 0.7f;
    private float currentAttackCooldown = 0f;

    void Update()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer < aggressiveDistance && distToPlayer > 2)
        {
            StartChase();
            isAttacking = false;
        }
        else if (distToPlayer <= 2 && !isAttacking)
        {
            Attack();
        }
        else
        {
            animator.SetInteger("AnimState", 0);
        }

        // Обновляем таймер задержки между атаками 
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

            if (attackCounter % 2 == 0)
            {
                animator.SetInteger("AnimState", 2);
                attackCounter++;
            }
            else
            {
                animator.SetInteger("AnimState", 3);
                attackCounter++;
            }

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
        yield return new WaitForSeconds(0.7f);
        
        isAttacking = false;
    }

    void Run(int direction)
    {
        m_body2d.velocity = new Vector2(direction * m_speed, player.position.y);
        animator.SetInteger("AnimState", 1);
    }

    void StartChase()
    {
        if(animator.GetInteger("AnimState") >= 2)
        {
            return;
        }
        if(player.position.x < transform.position.x)
        {
            Run(-1);
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else if(player.position.x > transform.position.x)
        {
            Run(1);
            GetComponent<SpriteRenderer>().flipX = false;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        animator.SetTrigger("Hurt");

        if(currentHP <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        animator.SetBool("isDead", true);
        m_body2d.velocity = new Vector2(0, 0);
        m_body2d.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }
}
