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

    // CHÚ Ý: ĐIỀU CHỈNH GIÁ TRỊ NÀY TRONG INSPECTOR
    [Header("Animation Timings")]
    [Tooltip("Tổng thời gian (giây) của hoạt ảnh 'enemy_attacl'.")]
    public float attackAnimationDuration = 1.0f; // <-- ĐIỀN CHÍNH XÁC THỜI GIAN HOẠT ẢNH
    [Tooltip("Thời gian (giây) từ đầu hoạt ảnh đến frame gây sát thương (0:30 là 0.3s).")]
    public float damageFrameTime = 0.3f; // <-- THỜI ĐIỂM BẮT ĐẦU TÍNH SÁT THƯƠNG

    [Header("References")]
    public LayerMask playerLayer;

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    private bool isDead = false;
    private bool isAttacking = false;
    private bool isFacingRight = true;
    private float lastAttackTime = 0f; // Khởi tạo an toàn

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Ngăn tấn công ngay lập tức khi spawn
        lastAttackTime = Time.time;

        // Đảm bảo thời gian chờ còn lại không bị âm
        if (damageFrameTime >= attackAnimationDuration)
        {
            Debug.LogError("Damage Frame Time phải nhỏ hơn Attack Animation Duration!");
            damageFrameTime = attackAnimationDuration * 0.5f;
        }
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
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        // Lật hướng nếu cần
        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking");
        rb.linearVelocity = Vector2.zero;

        // 1. Chờ đến Frame Gây Sát Thương (ví dụ: 0.3s)
        yield return new WaitForSeconds(damageFrameTime);

        // 2. Gây Sát Thương (Chỉ xảy ra một lần tại frame này)
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage);
            }
        }

        // 3. Chờ Hoạt ảnh Kết thúc
        float waitTimeRemaining = attackAnimationDuration - damageFrameTime;

        // Đảm bảo không chờ nếu thời gian còn lại là 0 hoặc âm (dù đã kiểm tra trong Start)
        if (waitTimeRemaining > 0)
        {
            yield return new WaitForSeconds(waitTimeRemaining);
        }

        // 4. Kết thúc Tấn công và Bắt đầu tính Cooldown
        isAttacking = false;
        // Cập nhật lastAttackTime ngay sau khi hoạt ảnh kết thúc
        lastAttackTime = Time.time;
    }

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