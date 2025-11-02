using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 2f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float attackDamage = 15f;
    public int attacksPerPhase = 3;       // Mỗi đợt boss đánh bao nhiêu lần
    public float restTimeBetweenPhases = 6f; // Boss nghỉ giữa các đợt đánh

    [Header("Health")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Animation Timings")]
    public float attackAnimationDuration = 1.2f;
    public float damageFrameTime = 0.4f;

    [Header("References")]
    public LayerMask playerLayer;
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    // ⭐ THAM CHIẾU THANH MÁU MỚI ⭐
    public BossHealthBar healthBar; // Cần kéo Canvas/Slider vào đây trong Inspector

    private bool isDead = false;
    private bool isAttacking = false;
    private bool isResting = false;
    private bool isFacingRight = true;
    private float lastAttackTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        currentHealth = maxHealth;
        lastAttackTime = Time.time;

        // ⭐ KHỞI TẠO MÁU TỐI ĐA CHO THANH MÁU ⭐
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }

    void Update()
    {
        if (isDead || player == null || isResting) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (!isAttacking)
        {
            if (distance <= attackRange)
            {
                rb.linearVelocity = Vector2.zero;
                FlipTowardsPlayer();

                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    StartCoroutine(AttackPhase());
                }
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
    }

    private IEnumerator AttackPhase()
    {
        isAttacking = true;
        animator.SetBool("isWalking", false);

        for (int i = 0; i < attacksPerPhase; i++)
        {
            if (isDead) yield break;

            animator.SetTrigger("isAttacking");
            rb.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(attackAnimationDuration);

            ApplyDamageToPlayer();
            yield return new WaitForSeconds(attackCooldown);
        }

        isAttacking = false;
        StartCoroutine(RestPhase());
    }

    private IEnumerator RestPhase()
    {
        isResting = true;
        animator.SetBool("isWalking", false);
        yield return new WaitForSeconds(restTimeBetweenPhases);
        isResting = false;
    }

    public void ApplyDamageToPlayer()
    {
        if (player == null || isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange) return;

        bool isPlayerInFront = (player.position.x - transform.position.x > 0 && isFacingRight)
                            || (player.position.x - transform.position.x < 0 && !isFacingRight);

        if (isPlayerInFront)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Giả định PlayerController có hàm TakeDamage
                // playerController.TakeDamage(attackDamage); 
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        Debug.Log("Boss nhận " + dmg + " sát thương. Máu còn: " + currentHealth);

        // ⭐ CẬP NHẬT THANH MÁU ⭐
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("isHurt");
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger("isDeath");

        // ⭐ ẨN THANH MÁU KHI BOSS CHẾT ⭐
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        Destroy(gameObject, 2f);
    }

    private void FlipTowardsPlayer()
    {
        float directionX = player.position.x - transform.position.x;
        if (directionX > 0 && !isFacingRight) Flip();
        else if (directionX < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        // Giả sử SpriteRenderer nằm trực tiếp trên Boss,
        // nếu không, hãy áp dụng logic Flip của bạn
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}