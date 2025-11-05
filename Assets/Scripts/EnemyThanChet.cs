using System.Collections;
using UnityEngine;

public class EnemyThanChet : MonoBehaviour
{
    [Header("Components")]
    private Animator animator;
    private Rigidbody2D rb;

    [Header("References")]
    public BossHealthBar healthBar; // Giả định có


    [Header("Stats")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float detectRange = 6f;
    public float attackDamage = 10f;

    [Header("Health")]
    [Tooltip("Sát thương Player gây ra trong 1 cú đấm. (Nên là 2f)")]
    [SerializeField] private float playerPunchDamage = 2f;
    [Tooltip("Số lần Player phải đấm để Enemy chết. (Cần là 2)")]
    [SerializeField] private int requiredPunchesToKill = 2; // Yêu cầu 2 đấm
    private float maxHealth;
    private float currentHealth;

    [Header("Animation Timings")]
    [Tooltip("Thời gian hoạt ảnh tấn công (để đồng bộ với Coroutine)")]
    public float attackAnimationDuration = 1.0f;
    [Tooltip("Thời điểm gây sát thương trong hoạt ảnh (thường dùng trong Animation Event)")]
    public float damageFrameTime = 0.3f; // Giữ lại nếu bạn muốn dùng Coroutine/Event

    // Trạng thái nội bộ
    private Transform player;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isFacingRight = true;
    private float lastAttackTime = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Khởi tạo Máu
        maxHealth = requiredPunchesToKill * playerPunchDamage;
        currentHealth = maxHealth;
        lastAttackTime = Time.time - attackCooldown; // Cho phép tấn công ngay lập tức
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Nếu đang tấn công, khóa mọi hành động khác
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            // Trong tầm tấn công: Dừng, Quay mặt, Tấn công (nếu sẵn sàng)
            StopMoving();
            FlipTowardsPlayer();

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(Attack());
            }
        }
        else if (distance <= detectRange)
        {
            // Trong tầm phát hiện: Di chuyển
            MoveTowardsPlayer();
        }
        else
        {
            // Ngoài tầm phát hiện: Đứng yên
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // ================== MOVEMENT ==================

    private void MoveTowardsPlayer()
    {

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // ================== COMBAT ==================

    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetTrigger("TgAttack"); // Kích hoạt animation tấn công
        rb.linearVelocity = Vector2.zero; // Đóng băng vị trí khi tấn công

        // Chờ đến Frame gây sát thương (Tốt nhất là dùng Animation Event)
        // yield return new WaitForSeconds(damageFrameTime); 

        // **KHUYẾN NGHỊ:** Sử dụng Animation Event để gọi hàm ApplyDamageToPlayer() chính xác hơn!

        // Dùng Coroutine đơn giản: Chờ hết hoạt ảnh (chứa logic gây sát thương)
        yield return new WaitForSeconds(attackAnimationDuration);

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    // Hàm này NÊN được gọi bởi Animation Event tại frame gây sát thương
    public void ApplyDamageToPlayer()
    {
        if (player == null || isDead) return; // Không cần check isAttacking nếu dùng Event

        // 1. Check khoảng cách
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange + 0.1f) return; // Cho phép một chút sai số

        // 2. Check hướng nhìn (Player có ở phía trước không?)
        float directionToPlayer = player.position.x - transform.position.x;
        bool isPlayerInFront = (directionToPlayer > 0 && isFacingRight) ||
                               (directionToPlayer < 0 && !isFacingRight);

        if (isPlayerInFront)
        {
            // 3. Gây sát thương
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage);
                //  
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        // Ngừng mọi hành động tấn công/di chuyển khi bị đánh
        isAttacking = false;
        StopMoving();

        currentHealth -= dmg;
        Debug.Log(gameObject.name + " bị nhận " + dmg + " sát thương. Máu còn: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger("TgDeath");

        // Vô hiệu hóa vật lý và collider
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Tắt simulation để trọng lực không kéo Dai xuống
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Tắt script và hủy đối tượng sau 2 giây (hoặc thời gian của Death Anim)
        this.enabled = false;
        Destroy(gameObject, 2f);
    }

    // ================== FACING & FLIP ==================

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void FlipTowardsPlayer()
    {
        if (player == null) return;
        float directionX = player.position.x - transform.position.x;

        bool shouldBeFacingRight = directionX > 0;
        if (shouldBeFacingRight != isFacingRight) Flip();
    }

    // ================== GIZMOS ==================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}