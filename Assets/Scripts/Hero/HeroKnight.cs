using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class HeroKnight : MonoBehaviour
{
    [SerializeField] float m_speed = 4.2f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;
    [SerializeField] int m_hp = 100;
    private int currentHP;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private bool m_grounded = false;
    private bool m_isBlocking = false;
    public bool isDead = false;
    private bool m_inMove = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    public Transform m_attackPoint;
    public float m_attackRange = 1.0f;
    public LayerMask enemyLayers;
    int attackDamage = 15;


    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        currentHP = m_hp;
    }

    // Update is called once per frame
    void Update()
    {
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");

        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }

        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // Move
        if (!m_isBlocking)
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        //Attack
        if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && ((inputX <= 0.5 && inputX >= 0) || (inputX >= -0.5 && inputX <= 0)))
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;

             
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(m_attackPoint.position, m_attackRange, enemyLayers);

            foreach (Collider2D enemy in hitEnemies)
            {
                // Attempt to get the SamuraiController component
                SamuraiController samuraiController = enemy.GetComponent<SamuraiController>();

                // Attempt to get the Bandit component
                Bandit bandit = enemy.GetComponent<Bandit>();

                // Attempt to get the Skeleton component
                Skeleton skeleton = enemy.GetComponent<Skeleton>();

                // Attempt to get the Skeleton component
                Wizard wizard = enemy.GetComponent<Wizard>();

                // Check if it's a Samurai
                if (samuraiController != null)
                {
                    samuraiController.TakeDamage(attackDamage);
                    Debug.Log("We hit a Samurai: " + enemy.name);
                }
                // Check if it's a Bandit
                else if (bandit != null)
                {
                    bandit.TakeDamage(attackDamage);
                    Debug.Log("We hit a Bandit: " + enemy.name);
                }
                // Check if it's a Skeleton
                else if (skeleton != null)
                {
                    skeleton.TakeDamage(attackDamage);
                    Debug.Log("We hit a Bandit: " + skeleton.name);
                }
                // Check if it's a Wizard
                else if (wizard != null)
                {
                    wizard.TakeDamage(attackDamage);
                    Debug.Log("We hit a Bandit: " + wizard.name);
                }
                // Log a message if it's neither a Samurai nor a Bandit
                else
                {
                    Debug.Log("Unknown enemy type: " + enemy.name);
                }
            }
        }

        // Block
        else if (Input.GetMouseButtonDown(1) && ((inputX <= 0.5 && inputX >= 0) || (inputX >= -0.5 && inputX <= 0)))
        {
            m_isBlocking = true;
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", m_isBlocking);
        }

        else if (Input.GetMouseButtonUp(1))
        {
            m_isBlocking = false;
            m_animator.SetBool("IdleBlock", m_isBlocking);
        }

        //Jump
        else if (Input.GetKeyDown("space") && m_grounded)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
            m_inMove = true;
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
            m_inMove = false;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }
    }

    public void TakeDamage(int damage)
    {
        if(!m_isBlocking)
        {
            currentHP -= damage;

            m_animator.SetTrigger("Hurt");

            if (currentHP <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        m_animator.SetBool("noBlood", m_noBlood);
        m_animator.SetTrigger("Death");
        isDead = true;
        Destroy(this);
        this.enabled = false;
    }
}