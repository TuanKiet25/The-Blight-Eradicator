using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f; // đánh chậm lại
    public float detectRange = 6f;
    public float attackDamage = 10f;

    // Các biến Animation Timings vẫn được giữ để tham chiếu thời gian
    [Header("Animation Timings")]
    [Tooltip("Tổng thời gian (giây) của hoạt ảnh 'enemy_attacl'.")]
    public float attackAnimationDuration = 1.0f; // Dùng để chờ hoạt ảnh kết thúc
    [Tooltip("Thời gian (giây) từ đầu hoạt ảnh đến frame gây sát thương (0:30 là 0.3s).")]
    public float damageFrameTime = 0.3f; // Chỉ là biến tham chiếu trong Inspector

    [Header("References")]
    public LayerMask playerLayer;
    // public Collider2D attackHitbox; // <--- KHÔNG DÙNG NỮA

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
        // Sử dụng GameObject.FindGameObjectWithTag an toàn hơn cho lần tìm kiếm đầu tiên
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Ngăn tấn công ngay lập tức khi spawn
        lastAttackTime = Time.time;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            // Dừng di chuyển ngay lập tức khi trong tầm đánh
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);

            // Đảm bảo kẻ địch quay mặt về phía người chơi trước khi tấn công
            FlipTowardsPlayer();

            if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(Attack());
            }
        }
        else if (distance <= detectRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    private void MoveTowardsPlayer()
    {
        // Ngăn di chuyển khi đang tấn công
        if (isAttacking) return;

        animator.SetBool("isWalking", true);

        Vector2 direction = (player.position - transform.position).normalized;

        // Chỉ di chuyển theo trục X
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        // Lật hướng nếu cần
        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
    }

    private void FlipTowardsPlayer()
    {
        float directionX = player.position.x - transform.position.x;
        if (directionX > 0 && !isFacingRight) Flip();
        else if (directionX < 0 && isFacingRight) Flip();
    }

    // ----------------------------------------------------------------------
    // PHẦN LOGIC TẤN CÔNG
    // ----------------------------------------------------------------------

    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking");
        rb.linearVelocity = Vector2.zero;

        // Coroutine chỉ chờ hoạt ảnh kết thúc (damageFrameTime không còn được dùng ở đây)
        // Logic gây sát thương được gọi bởi Animation Event.
        yield return new WaitForSeconds(attackAnimationDuration);

        // Kết thúc Tấn công và Bắt đầu tính Cooldown
        isAttacking = false;
        lastAttackTime = Time.time;
    }

    // HÀM GÂY SÁT THƯƠNG ĐƯỢC GỌI BỞI ANIMATION EVENT (Tại 0:30)
    public void ApplyDamageToPlayer()
    {
        if (player == null) return;

        // 1. Kiểm tra cự ly (Player có trong tầm đánh không?)
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange)
        {
            return;
        }

        // 2. Kiểm tra hướng mặt (Player có ở phía trước Enemy không?)
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
                Debug.Log("Sát thương thành công tại frame hoạt ảnh!");
            }
        }
    }

    // ----------------------------------------------------------------------
    // CÁC HÀM CÒN LẠI
    // ----------------------------------------------------------------------

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        animator.SetTrigger("isHurt");
        // Nếu có máu riêng cho enemy thì trừ ở đây
        // currentHealth -= dmg; 
        // if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        isDead = true;
        animator.SetTrigger("isDeath");
        rb.linearVelocity = Vector2.zero;
        // Xóa logic tắt Hitbox khỏi đây
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}