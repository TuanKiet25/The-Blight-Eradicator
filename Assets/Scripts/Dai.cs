using UnityEngine;
using System.Collections;

<<<<<<< Updated upstream
// public class Dai : MonoBehaviour
// {
//     // ... (Các biến Stats, Health, Animation Timings giữ nguyên) ...
//     [Header("Stats")]
//     public float moveSpeed = 2f;
//     public float attackRange = 1.5f;
//     public float attackCooldown = 1.5f;
//     public float detectRange = 6f;
//     public float attackDamage = 10f;

//     // ⚔️ HEALTH & SÁT THƯƠNG TỪ PLAYER
//     [Header("Health")]
//     [Tooltip("Sát thương Player gây ra trong 1 cú đấm. (Nên là 2f)")]
//     [SerializeField] private float playerPunchDamage = 2f;
//     [Tooltip("Số lần Player phải đấm để Enemy chết. (Cần là 2)")]
//     [SerializeField] private int requiredPunchesToKill = 2; // Yêu cầu 2 đấm
//     private float maxHealth;
//     private float currentHealth;
//     // ---------------------------------------------

//     [Header("Animation Timings")]
//     public float attackAnimationDuration = 1.0f;
//     public float damageFrameTime = 0.3f;

//     [Header("References")]
//     public LayerMask playerLayer;

//     private Transform player;
//     private Animator animator;
//     private Rigidbody2D rb;

//     private bool isDead = false;
//     private bool isAttacking = false;
//     private bool isFacingRight = true;
//     private float lastAttackTime = 0f;

//     void Start()
//     {
//         animator = GetComponent<Animator>();
//         rb = GetComponent<Rigidbody2D>();
//         player = GameObject.FindGameObjectWithTag("Player")?.transform;

//         lastAttackTime = Time.time;

//         // 🔪 KHỞI TẠO MÁU: Máu = 2 * 2 = 4f
//         maxHealth = requiredPunchesToKill * playerPunchDamage;
//         currentHealth = maxHealth;
//     }

//     void Update()
//     {
//         if (isDead || player == null) return;
//         if (isAttacking)
//         {
//             rb.linearVelocity = Vector2.zero;
//             return;
//         }

//         float distance = Vector2.Distance(transform.position, player.position);

//         if (distance <= attackRange)
//         {
//             rb.linearVelocity = Vector2.zero;
//             animator.SetBool("isWalking", false);
//             FlipTowardsPlayer();

//             if (Time.time - lastAttackTime >= attackCooldown)
//             {
//                 StartCoroutine(Attack());
//             }
//         }
//         else if (distance <= detectRange)
//         {
//             MoveTowardsPlayer();
//         }
//         else
//         {
//             animator.SetBool("isWalking", false);
//         }
//     }

//     private void MoveTowardsPlayer()
//     {
//         animator.SetBool("isWalking", true);

//         Vector2 direction = (player.position - transform.position).normalized;
//         rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

//         if (direction.x > 0 && !isFacingRight) Flip();
//         else if (direction.x < 0 && isFacingRight) Flip();
//     }

//     private void FlipTowardsPlayer()
//     {
//         float directionX = player.position.x - transform.position.x;
//         if (directionX > 0 && !isFacingRight) Flip();
//         else if (directionX < 0 && isFacingRight) Flip();
//     }

//     private IEnumerator Attack()
//     {
//         isAttacking = true;
//         animator.SetTrigger("isAttacking");
//         rb.linearVelocity = Vector2.zero;

//         yield return new WaitForSeconds(attackAnimationDuration);

//         isAttacking = false;
//         lastAttackTime = Time.time;
//     }

//     public void ApplyDamageToPlayer()
//     {
//         if (player == null || isDead || !isAttacking) return;

//         float distance = Vector2.Distance(transform.position, player.position);
//         if (distance > attackRange) return;

//         float directionToPlayer = player.position.x - transform.position.x;
//         bool isPlayerInFront = (directionToPlayer > 0 && isFacingRight) ||
//                                (directionToPlayer < 0 && !isFacingRight);

//         if (isPlayerInFront)
//         {
//             var playerController = player.GetComponent<PlayerController>();
//             if (playerController != null)
//             {
//                 playerController.TakeDamage(attackDamage);
//             }
//         }
//     }

//     public void TakeDamage(float dmg)
//     {
//         if (isDead) return;

//         isAttacking = false;
//         if (rb != null) rb.linearVelocity = Vector2.zero;

//         currentHealth -= dmg;
//         Debug.Log(gameObject.name + " bị nhận " + dmg + " sát thương. Máu còn: " + currentHealth);

//         if (currentHealth <= 0)
//         {
//             Die();
//         }
//         else
//         {
//             animator.SetTrigger("isHurt");
//         }
//     }

//     public void Die()
//     {
//         if (isDead) return;
//         isDead = true;

//         // 1. Kích hoạt hoạt ảnh
//         animator.SetTrigger("isDeath");

//         // 2. Dừng vật lý ngay lập tức!
//         if (rb != null)
//         {
//             rb.linearVelocity = Vector2.zero;
//             // 🔑 QUAN TRỌNG: Tắt Simulation để vô hiệu hóa trọng lực!
//             rb.simulated = false;
//         }

//         // 3. Tắt Collider và script
//         GetComponent<Collider2D>().enabled = false;
//         this.enabled = false;

//         // 4. 🚀 ĐÃ SỬA: Tăng thời gian chờ hủy để khớp với hoạt ảnh (ví dụ 2 giây)
//         // Nếu hoạt ảnh chết của bạn dài hơn 2 giây, bạn cần tăng giá trị này.
//         Destroy(gameObject, 2f);
//     }

//     private void Flip()
//     {
//         isFacingRight = !isFacingRight;
//         Vector3 scale = transform.localScale;
//         scale.x *= -1;
//         transform.localScale = scale;
//     }

//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.red;
//         Gizmos.DrawWireSphere(transform.position, attackRange);
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, detectRange);
//     }
// }


=======
>>>>>>> Stashed changes
public class Dai : MonoBehaviour
{
    [Header("Components")]
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Stats")]
    public float moveSpeed = 2f;
<<<<<<< Updated upstream
    public float stopDistance = 1.5f;
    public bool isDead = false;

    private Transform player;
    private bool isFacingRight = true;
=======
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
>>>>>>> Stashed changes

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
<<<<<<< Updated upstream
=======

        // Khởi tạo Máu
        maxHealth = requiredPunchesToKill * playerPunchDamage;
        currentHealth = maxHealth;
        lastAttackTime = Time.time - attackCooldown; // Cho phép tấn công ngay lập tức
>>>>>>> Stashed changes
    }

    void Update()
    {
        if (isDead || player == null) return;

<<<<<<< Updated upstream
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            MoveTowardPlayer();
        }
        else
        {
            StopMoving();
        }
    }

    private void MoveTowardPlayer()
    {
        animator.SetBool("isWalk", true);
=======
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
            animator.SetBool("isWalking", false);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // ================== MOVEMENT ==================

    private void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);
>>>>>>> Stashed changes

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

<<<<<<< Updated upstream
        if (direction.x > 0 && !isFacingRight)
            Flip();
        else if (direction.x < 0 && isFacingRight)
            Flip();
=======
        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
>>>>>>> Stashed changes
    }

    private void StopMoving()
    {
<<<<<<< Updated upstream
        animator.SetBool("isWalk", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        FlipTowardsPlayer();
    }

=======
        animator.SetBool("isWalking", false);
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

>>>>>>> Stashed changes
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
<<<<<<< Updated upstream

        float directionX = player.position.x - transform.position.x;
        if (directionX > 0 && !isFacingRight)
            Flip();
        else if (directionX < 0 && isFacingRight)
            Flip();
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetBool("isWalk", false);
        animator.SetTrigger("TgDeath");

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 2f);
    }
=======
        float directionX = player.position.x - transform.position.x;

        bool shouldBeFacingRight = directionX > 0;
        if (shouldBeFacingRight != isFacingRight) Flip();
    }

    // ================== GIZMOS ==================
>>>>>>> Stashed changes

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
<<<<<<< Updated upstream
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
=======
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
>>>>>>> Stashed changes
