using System.Collections;
using UnityEngine;

public class Enemy1Controller : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 2f;
    public float chargeSpeed = 6f;
    public float chargeDelay = 1.2f;
    public float detectRange = 8f;
    public float attackRange = 1.5f;
    public float stopDistance = 0.4f; // khoảng dừng nếu bị chặn

    // ⚔️ ĐÃ THÊM: Stats Tấn Công
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f; // Thời gian chờ giữa các lần tấn công

    // 🕒 ĐÃ THÊM: Animation Timings (CẦN ĐIỀU CHỈNH TRONG INSPECTOR)
    [Tooltip("Tổng thời gian (giây) của hoạt ảnh tấn công.")]
    public float attackAnimationDuration = 1.0f; // <-- ĐIỀN THỜI GIAN HOẠT ẢNH
    [Tooltip("Thời gian (giây) từ đầu hoạt ảnh đến frame gây sát thương.")]
    public float damageFrameTime = 0.3f; // <-- THỜI ĐIỂM TÍNH SÁT THƯƠNG

    [Header("References")]
    public LayerMask playerLayer;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;

    private bool isDead = false;
    private bool isChargingDelay = false;
    private bool isChargeMoving = false;
    private Coroutine chargeCoroutine;

    // ⚔️ ĐÃ THÊM: Biến trạng thái tấn công
    private bool isAttacking = false;
    private float lastAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        lastAttackTime = Time.time;

        // Kiểm tra an toàn cho Animation Timings
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

        // Nếu trong tầm tấn công -> dừng VÀ TẤN CÔNG
        if (distance <= attackRange)
        {
            StopChargeMovement();

            // ⚔️ ĐÃ THÊM: Logic kiểm tra Cooldown và Tấn công
            if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
            {
                // Ngăn Enemy di chuyển ngay lập tức sau khi dừng
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isWalking", false);

                // Bắt đầu Coroutine Tấn công
                StartCoroutine(Attack());
            }
            return; // Quan trọng: Dừng lại để Enemy tấn công
        }

        // Nếu trong tầm phát hiện -> chuẩn bị charge
        if (distance <= detectRange)
        {
            // ⚔️ THÊM: Ngăn charge nếu đang trong hoạt ảnh tấn công
            if (!isChargingDelay && !isChargeMoving && !isAttacking)
            {
                if (chargeCoroutine != null) StopCoroutine(chargeCoroutine);
                chargeCoroutine = StartCoroutine(ChargeDelayThenMove());
            }
        }
        else
        {
            // Player ra khỏi vùng
            StopChargeMovement();
            animator.SetBool("isWalking", false);
            animator.SetBool("isPreparing", false);
        }

        HandleFacing();

        // 🔹 Nếu đang di chuyển mà bị chặn (tốc độ ≈ 0) => tắt walk
        // Logic này có thể gây xung đột với coroutine. Tốt nhất là xử lý trong FixedUpdate hoặc để coroutine tự lo.
        // Tôi sẽ để nó bị comment như trong code bạn gửi, vì coroutine đã xử lý logic dừng.
        //if (isChargeMoving && rb.linearVelocity.magnitude < 0.05f)
        //{
        //    animator.SetBool("isWalking", false);
        //}
    }

    // ⚔️ ĐÃ THÊM: Coroutine Tấn Công
    private IEnumerator Attack()
    {
        isAttacking = true;
        // 🚨 Cần đảm bảo Animator của bạn có Trigger tên là "isAttacking"
        animator.SetTrigger("isAttacking");

        // 1. Chờ đến Frame Gây Sát Thương
        yield return new WaitForSeconds(damageFrameTime);

        // 2. Gây Sát Thương
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            // Lấy script PlayerController để gây sát thương (giống EnemyController mẫu)
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage);
                Debug.Log(gameObject.name + " tấn công Player! Gây " + attackDamage + " sát thương.");
            }
            else
            {
                Debug.LogError("Không tìm thấy component PlayerController trên Player!");
            }
        }

        // 3. Chờ Hoạt ảnh Tấn công Kết thúc
        float waitTimeRemaining = attackAnimationDuration - damageFrameTime;
        if (waitTimeRemaining > 0)
        {
            yield return new WaitForSeconds(waitTimeRemaining);
        }

        // 4. Kết thúc Tấn công và Bắt đầu tính Cooldown
        isAttacking = false;
        lastAttackTime = Time.time;
    }

    // ... (Các hàm khác giữ nguyên)

    private IEnumerator ChargeDelayThenMove()
    {
        // ... (Giữ nguyên logic chuẩn bị và di chuyển)
        isChargingDelay = true;
        rb.linearVelocity = Vector2.zero;

        animator.SetBool("isPreparing", true);
        animator.SetBool("isWalking", false);

        float elapsed = 0f;
        while (elapsed < chargeDelay)
        {
            // ... (Giữ nguyên logic kiểm tra player)
            if (player == null) yield break;
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= attackRange || dist > detectRange)
            {
                isChargingDelay = false;
                animator.SetBool("isPreparing", false);
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Bắt đầu di chuyển
        isChargingDelay = false;
        isChargeMoving = true;

        animator.SetBool("isPreparing", false);
        animator.SetBool("isWalking", true);

        while (isChargeMoving && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            // Nếu player quá xa hoặc quá gần thì dừng
            if (dist > detectRange || dist <= attackRange)
                break;

            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * chargeSpeed;

            // Nếu bị chặn (vận tốc không tăng nổi)
            if (rb.linearVelocity.magnitude < 0.1f)
            {
                animator.SetBool("isWalking", false);
                yield return new WaitForSeconds(0.2f);
                break;
            }

            yield return null;
        }

        StopChargeMovement();
    }

    private void StopChargeMovement()
    {
        // ... (Giữ nguyên)
        isChargeMoving = false;
        isChargingDelay = false;

        rb.linearVelocity = Vector2.zero;

        animator.SetBool("isWalking", false);
        animator.SetBool("isPreparing", false);
        // ⚔️ THÊM: Đảm bảo dừng cả Coroutine tấn công nếu đang chạy
        // if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        if (chargeCoroutine != null)
        {
            StopCoroutine(chargeCoroutine);
            chargeCoroutine = null;
        }
    }

    private void HandleFacing()
    {
        // ... (Giữ nguyên)
        if (player == null) return;
        float dx = player.position.x - transform.position.x;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dx > 0 ? 1 : -1);
        transform.localScale = scale;
    }

    public void Die()
    {
        // ... (Giữ nguyên)
        if (isDead) return;
        isDead = true;

        StopChargeMovement();
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        animator.SetBool("isDead", true);

        Destroy(gameObject, 2f);
    }

    private void OnDrawGizmosSelected()
    {
        // ... (Giữ nguyên)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}