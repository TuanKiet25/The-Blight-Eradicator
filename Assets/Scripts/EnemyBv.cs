using System.Collections;
using UnityEngine;

public class RangedEnemyController : MonoBehaviour
{
    // --- STATS ---
    [Header("Stats")]
    public float moveSpeed = 1.5f;   // Tốc độ di chuyển khi đuổi theo
    public float attackRange = 8f;   // Phạm vi tấn công (Raycast)
    public float stopRange = 5f;     // Khoảng cách dừng lại để bắn
    public float attackCooldown = 3.0f;
    public float detectRange = 10f;  // Phạm vi phát hiện Player
    public float attackDamage = 5f;

    // --- HEALTH & SÁT THƯƠNG TỪ PLAYER ---
    [Header("Health")]
    [Tooltip("Sát thương Player gây ra trong 1 cú đấm.")]
    [SerializeField] private float playerPunchDamage = 2f;
    [Tooltip("Số lần Player phải đấm để Enemy chết.")]
    [SerializeField] private int requiredPunchesToKill = 3;
    private float maxHealth;
    private float currentHealth;

    // --- ANIMATION & REFERENCES ---
    [Header("Animation Timings")]
    public float attackAnimationDuration = 1.0f;

    [Header("References")]
    public LayerMask playerLayer;
    public Transform firePoint; // Vị trí Enemy kiểm tra/gây sát thương (Dùng cho Raycast)

    // --- TRẠNG THÁI NỘI BỘ ---
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
        maxHealth = requiredPunchesToKill * playerPunchDamage;
        currentHealth = maxHealth;

        // Bắt đầu ở trạng thái Idle
        animator.SetBool("isWalking", false);
    }

    void Update()
    {
        if (isDead || player == null) return;
        if (isAttacking)
        {
            // Dừng di chuyển khi đang tấn công
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= stopRange && distance <= attackRange) // 1. TRONG TẦM DỪNG VÀ BẮN
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            FlipTowardsPlayer(); // Luôn nhìn về Player

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(Attack());
            }
        }
        else if (distance <= detectRange) // 2. ĐÃ PHÁT HIỆN, DI CHUYỂN VÀO TẦM BẮN
        {
            MoveTowardsPlayer(); // Đuổi theo
        }
        else // 3. NGOÀI TẦM PHÁT HIỆN -> ĐỨNG YÊN HOÀN TOÀN
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
        }
    }

    // --- HÀM DI CHUYỂN & LẬT ---
    private void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
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
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // --- HÀM TẤN CÔNG ---
    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking");
        rb.linearVelocity = Vector2.zero;

        // Chờ hết hoạt ảnh tấn công (Animation Event sẽ gây sát thương ở giữa)
        yield return new WaitForSeconds(attackAnimationDuration);

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    // 🔥 HÀM GÂY SÁT THƯƠNG TẦM XA (Được gọi từ Animation Event)
    public void ApplyDamageToPlayer()
    {
        // Sử dụng firePoint.position cho điểm xuất phát của Raycast
        if (player == null || isDead || !isAttacking || firePoint == null) return;

        // 1. Xác định hướng nhìn
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;

        // 2. Thực hiện Raycast (Kiểm tra tia)
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, attackRange, playerLayer);

        // 3. Xử lý kết quả Raycast
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                var playerController = hit.collider.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    // Gây sát thương TẦM XA trực tiếp (Tức thì)
                    playerController.TakeDamage(attackDamage);
                }
            }
        }
    }

    // --- HÀM BỊ THƯƠNG & CHẾT (Giữ nguyên) ---
    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        isAttacking = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        currentHealth -= dmg;
        Debug.Log(gameObject.name + " bị nhận " + dmg + " sát thương. Máu còn: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        if (currentHealth <= 0)
        {
            // 🔑 BẮT BUỘC: Vô hiệu hóa Collider trước khi gọi Die()
            // Điều này ngăn Player (hoặc một vật thể khác) gây sát thương thêm.
            GetComponent<Collider2D>().enabled = false;

            Die();
        }
        else
        {
            animator.SetTrigger("isHurt");
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        animator.SetTrigger("isDeath");

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        Destroy(gameObject, 2f);
    }

    // --- GIZMOS ---
    private void OnDrawGizmosSelected()
    {
        // Vùng dừng lại và bắn
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopRange);

        // Vùng tấn công (tầm bắn)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Vùng phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Visual Raycast
        if (firePoint != null)
        {
            Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(firePoint.position, (Vector2)firePoint.position + direction * attackRange);
        }
    }
}