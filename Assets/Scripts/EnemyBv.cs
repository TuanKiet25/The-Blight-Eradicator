using System.Collections;
using UnityEngine;

public class RangedEnemyController : MonoBehaviour
{
    // --- STATS ---
    [Header("Stats")]
    public float moveSpeed = 1.5f;    // Tốc độ di chuyển khi đuổi theo
    public float attackRange = 8f;    // Phạm vi tấn công (Raycast)
    public float stopRange = 5f;      // Khoảng cách dừng lại để bắn
    public float attackCooldown = 3.0f;
    public float detectRange = 10f;   // Phạm vi phát hiện Player
    public float attackDamage = 5f;

    // 🔥 --- PHẦN HEALTH ĐÃ SỬA ---
    [Header("Health")]
    [Tooltip("Tổng lượng máu của quái. Player (punchDamage) đang gây 15f damage mỗi cú đấm.")]
    public float maxHealth = 45f; // Mặc định 45f (chịu được 3 cú đấm 15f)
    private float currentHealth;
    // ---------------------------------

    // 🔥 --- THÊM LOGIC RỚT VÀNG ---
    [Header("Loot")]
    [Tooltip("Số lượng vàng rớt ra khi quái chết.")]
    public int goldDropAmount = 10; // Tùy chỉnh số vàng rớt ra
    // ---------------------------------

    // --- SOUNDS ---
    [Header("Sound")]
    [SerializeField] private AudioClip shootSound; // Âm thanh khi bắn
    [SerializeField] private AudioClip hurtSound;  // Âm thanh khi bị đánh
    [SerializeField] private AudioClip deathSound; // Âm thanh khi chết
    private AudioSource audioSource;            // Component phát âm thanh

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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        lastAttackTime = Time.time;

        // 🔥 Gán máu trực tiếp từ biến maxHealth
        currentHealth = maxHealth;

        animator.SetBool("isWalking", false);
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

        if (distance <= stopRange && distance <= attackRange) // 1. TRONG TẦM DỪNG VÀ BẮN
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            FlipTowardsPlayer();

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(Attack());
            }
        }
        else if (distance <= detectRange) // 2. ĐÃ PHÁT HIỆN, DI CHUYỂN VÀO TẦM BẮN
        {
            MoveTowardsPlayer();
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

        yield return new WaitForSeconds(attackAnimationDuration);

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    public void ApplyDamageToPlayer()
    {
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        if (player == null || isDead || firePoint == null) return;

        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, attackRange, playerLayer);



        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                var playerController = hit.collider.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(attackDamage);
                    Debug.Log("Enemy bắn trúng Player gây " + attackDamage + " sát thương!");
                }
            }
        }
    }

    // --- HÀM BỊ THƯƠNG & CHẾT ---
    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        isAttacking = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        currentHealth -= dmg;
        Debug.Log(gameObject.name + " bị nhận " + dmg + " sát thương. Máu còn: " + currentHealth);

        if (currentHealth <= 0)
        {
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

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // 🔥 --- BẮT ĐẦU LOGIC RỚT VÀNG ---
        // Chúng ta đã có biến 'player' (Transform) từ hàm Start()
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // 2. Gọi hàm AddGold() của Player
                playerController.AddGold(goldDropAmount);
                Debug.Log(gameObject.name + " rớt ra " + goldDropAmount + " vàng.");
            }
            else
            {
                Debug.LogError("Lỗi: Player object thiếu script PlayerController!");
            }
        }
        else
        {
            Debug.LogError("Lỗi: Không tìm thấy Player trong Scene (Kiểm tra Tag 'Player')!");
        }
        // 🔥 --- KẾT THÚC LOGIC RỚT VÀNG ---

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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        if (firePoint != null)
        {
            Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(firePoint.position, (Vector2)firePoint.position + direction * attackRange);
        }
    }
}