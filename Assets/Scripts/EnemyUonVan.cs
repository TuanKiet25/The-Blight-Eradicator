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
    public float stopDistance = 0.4f;

    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Health")]
    [Tooltip("Sát thương Player gây ra trong 1 cú đấm. (Nên là 2f)")]
    [SerializeField] private float playerPunchDamage = 2f;
    [Tooltip("Số lần Player phải đấm để Enemy chết. (Cần là 5)")]
    [SerializeField] private int requiredPunchesToKill = 5; // Yêu cầu 5 đấm
    private float maxHealth;
    private float currentHealth;

    [Header("References")]
    public LayerMask playerLayer;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;

    private bool isDead = false;
    private bool isChargingDelay = false;
    private bool isChargeMoving = false;
    private Coroutine chargeCoroutine;

    private bool isAttacking = false;
    private float lastAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        lastAttackTime = Time.time;

        maxHealth = requiredPunchesToKill * playerPunchDamage;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            StopChargeMovement();

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isWalking", false);
                SimpleAttack();
            }
            return;
        }

        if (distance <= detectRange)
        {
            if (!isChargingDelay && !isChargeMoving)
            {
                if (chargeCoroutine != null) StopCoroutine(chargeCoroutine);
                chargeCoroutine = StartCoroutine(ChargeDelayThenMove());
            }
        }
        else
        {
            StopChargeMovement();
            animator.SetBool("isWalking", false);
            animator.SetBool("isPreparing", false);
        }

        HandleFacing();
    }

    private void SimpleAttack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking");

        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(attackDamage);
        }

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    private IEnumerator ChargeDelayThenMove()
    {
        isChargingDelay = true;
        rb.linearVelocity = Vector2.zero;

        animator.SetBool("isPreparing", true);
        animator.SetBool("isWalking", false);

        float elapsed = 0f;
        while (elapsed < chargeDelay)
        {
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

        isChargingDelay = false;
        isChargeMoving = true;

        animator.SetBool("isPreparing", false);
        animator.SetBool("isWalking", true);

        while (isChargeMoving && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            if (dist > detectRange || dist <= attackRange)
                break;

            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * chargeSpeed;

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
        isChargeMoving = false;
        isChargingDelay = false;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        animator.SetBool("isWalking", false);
        animator.SetBool("isPreparing", false);

        if (chargeCoroutine != null)
        {
            StopCoroutine(chargeCoroutine);
            chargeCoroutine = null;
        }
        isAttacking = false;
    }

    private void HandleFacing()
    {
        if (player == null) return;
        float dx = player.position.x - transform.position.x;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dx > 0 ? 1 : -1);
        transform.localScale = scale;
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        StopChargeMovement();

        currentHealth -= dmg;
        Debug.Log(gameObject.name + " bị nhận " + dmg + " sát thương. Máu còn: " + currentHealth);

        if (currentHealth <= 0)
        {
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

        StopChargeMovement();
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // 🔑 ĐÃ SỬA: Gọi Trigger isDeath mới!
        animator.SetTrigger("isDeath");

        // ⚠️ LƯU Ý: Nếu vẫn còn isDead (Bool) trong Parameters, bạn phải xóa nó.
        // Tắt Collider để các đối tượng khác có thể đi qua xác Enemy đã chết
        GetComponent<Collider2D>().enabled = false;

        // Hủy đối tượng sau 5 giây
        Destroy(gameObject, 5f);

        // Tắt script này (nên đặt cuối cùng)
        this.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}