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
    [Tooltip("Thời gian chặn Enemy di chuyển trong khi Attack 1")]
    public float attackAnimationDuration = 1.0f;
    [Tooltip("Thời gian chặn Enemy di chuyển trong khi Attack 2")]
    public float attack2AnimationDuration = 1.2f; // Ví dụ: Attack 2 lâu hơn
    
    // public float damageFrameTime = 0.3f; // Giữ lại nếu dùng Animation Event

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

        // KHỞI TẠO MÁU
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
            rb.linearVelocity = Vector2.zero; // Luôn đóng băng khi tấn công
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
                // Gọi hàm tấn công ngẫu nhiên
                StartCoroutine(RandomAttack()); 
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
    // RANDOM ATTACK - Chọn ngẫu nhiên Attack 1 hoặc Attack 2
    // ============================================
    private IEnumerator RandomAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        // 🔑 LOGIC CHỌN NGẪU NHIÊN: 0 hoặc 1
        int attackChoice = Random.Range(0, 2); 
        float duration;

        if (attackChoice == 0)
        {
            animator.SetTrigger("TgAttack");
            duration = attackAnimationDuration;
            Debug.Log("⚔️ Bắt đầu Attack 1 (TgAttack)");
        }
        else
        {
            animator.SetTrigger("TgAttack2");
            duration = attack2AnimationDuration;
            Debug.Log("⚔️ Bắt đầu Attack 2 (TgAttack2)");
        }
        
        // Chờ hết thời gian hoạt ảnh (hoặc đợi Animation Event gọi ApplyDamageToPlayer)
        // Lưu ý: Nếu dùng Animation Event để gây damage, bạn vẫn cần Coroutine này để chặn isAttacking
        yield return new WaitForSeconds(duration); 

        isAttacking = false;
        lastAttackTime = Time.time;
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
    // APPLY DAMAGE TO PLAYER - Gây damage (NÊN gọi từ Animation Event)
    // ============================================
    public void ApplyDamageToPlayer()
    {
        if (player == null || isDead || !isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange + 0.1f) return; // Thêm tolerance nhỏ

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
                // 
            }
        }
    }

    // ============================================
    // TAKE DAMAGE - Nhận sát thương
    // ============================================
    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        // Dừng tấn công và di chuyển khi bị đánh
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
            // Sử dụng Trigger isHurt
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