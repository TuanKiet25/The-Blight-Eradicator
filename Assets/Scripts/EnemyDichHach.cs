using System.Collections;
using UnityEngine;

public class EnemyDichHach : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float detectRange = 6f;
    public float attackDamage = 15f;

    [Header("Health")]
    [Tooltip("Sát thương Player gây ra trong 1 cú đấm. (Nên là 2f)")]
    [SerializeField] private float playerPunchDamage = 2f;
    [Tooltip("Số lần Player phải đấm để Enemy chết. (Cần là 5)")]
    [SerializeField] private int requiredPunchesToKill = 5;
    private float maxHealth;
    private float currentHealth;

    [Header("Animation Timings")]
    public float attackAnimationDuration = 1.0f;
    public float damageFrameTime = 0.3f;

    [Header("References")]
    public LayerMask playerLayer;

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    private bool isDead = false;
    private bool isAttacking = false;
    private bool isFacingRight = true;
    private float lastAttackTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        lastAttackTime = Time.time;

        // KHỞI TẠO MÁU: Máu = 5 * 2 = 10f
        maxHealth = requiredPunchesToKill * playerPunchDamage;
        currentHealth = maxHealth;

        if (player == null)
        {
            Debug.LogWarning("⚠️ Không tìm thấy Player! Hãy set Tag = 'Player' cho player GameObject");
        }

        Debug.Log($"✅ Enemy {gameObject.name} đã khởi tạo! HP: {currentHealth}/{maxHealth}");
    }

    void Update()
    {
        if (isDead || player == null) return;
        
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // === ATTACK RANGE ===
        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            FlipTowardsPlayer();

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(Attack());
            }
        }
        // === DETECT RANGE (CHASE) ===
        else if (distance <= detectRange)
        {
            MoveTowardsPlayer();
        }
        // === OUT OF RANGE ===
        else
        {
            animator.SetBool("isWalking", false);
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ============================================
    // MOVE TOWARDS PLAYER - Di chuyển về phía player
    // ============================================
    private void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
    }

    // ============================================
    // FLIP TOWARDS PLAYER - Quay về phía player
    // ============================================
    private void FlipTowardsPlayer()
    {
        float directionX = player.position.x - transform.position.x;
        if (directionX > 0 && !isFacingRight) Flip();
        else if (directionX < 0 && isFacingRight) Flip();
    }

    // ============================================
    // ATTACK - Tấn công player
    // ============================================
    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking");
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(attackAnimationDuration);

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    // ============================================
    // APPLY DAMAGE TO PLAYER - Gây damage (gọi từ Animation Event)
    // ============================================
    public void ApplyDamageToPlayer()
    {
        if (player == null || isDead || !isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange) return;

        // Check player có ở phía trước không
        float directionToPlayer = player.position.x - transform.position.x;
        bool isPlayerInFront = (directionToPlayer > 0 && isFacingRight) ||
                               (directionToPlayer < 0 && !isFacingRight);

        if (isPlayerInFront)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage);
                Debug.Log($"⚔️ {gameObject.name} tấn công Player! Damage: {attackDamage}");
            }
        }
    }

    // ============================================
    // TAKE DAMAGE - Nhận sát thương
    // ============================================
    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        isAttacking = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        currentHealth -= dmg;
        Debug.Log($"💥 {gameObject.name} bị nhận {dmg} sát thương. Máu còn: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("isHurt");
        }
    }

    // ============================================
    // DIE - Chết
    // ============================================
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. Kích hoạt animation chết
        animator.SetTrigger("isDeath");

        // 2. Dừng vật lý ngay lập tức
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            // QUAN TRỌNG: Tắt Simulation để vô hiệu hóa trọng lực
            rb.simulated = false;
        }

        // 3. Tắt Collider và script
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Debug.Log($"💀 {gameObject.name} đã chết!");

        // 4. Hủy đối tượng sau 2 giây (khớp với animation)
        Destroy(gameObject, 2f);

        // 5. Tắt script (đặt cuối cùng)
        this.enabled = false;
    }

    // ============================================
    // FLIP - Lật sprite
    // ============================================
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ============================================
    // GIZMOS - Vẽ phạm vi
    // ============================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}