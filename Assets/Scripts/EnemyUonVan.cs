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

    // 🔥 --- PHẦN HEALTH ĐÃ SỬA ---
    [Header("Health")]
    [Tooltip("Tổng lượng máu của quái. Player (punchDamage) đang gây 15f damage mỗi cú đấm.")]
    // Player đấm 15f, quái cũ cần 5 cú đấm => 15 * 5 = 75f
    public float maxHealth = 75f;
    private float currentHealth;
    // ---------------------------------

    // 🔥 --- THÊM LOGIC RỚT VÀNG ---
    [Header("Loot")]
    [Tooltip("Số lượng vàng rớt ra khi quái chết.")]
    public int goldDropAmount = 20; // Tùy chỉnh số vàng rớt ra
    // ---------------------------------

    [Header("References")]
    public LayerMask playerLayer;

    [Header("Audio")]
    public AudioClip idleSound;
    public AudioClip walkSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;

    private bool isDead = false;
    private bool isChargingDelay = false;
    private bool isChargeMoving = false;
    private Coroutine chargeCoroutine;

    private float lastAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(attackDamage);
        }

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

        if (walkSound != null && audioSource != null)
        {
            audioSource.clip = walkSound;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log(gameObject.name + " playing walk sound: " + walkSound.name);
        }


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

        if (audioSource != null)
        {
            if (idleSound != null && player != null)
            {
                float pdist = Vector2.Distance(transform.position, player.position);
                if (pdist <= detectRange)
                {
                    audioSource.clip = idleSound;
                    audioSource.loop = true;
                    if (!audioSource.isPlaying) audioSource.Play();
                    Debug.Log(gameObject.name + " resumed idle sound: " + idleSound.name + " (player distance=" + pdist + ")");
                }
                else
                {
                    if (audioSource.isPlaying) audioSource.Stop();
                }
            }
            else
            {
                if (audioSource.isPlaying) audioSource.Stop();
            }
        }

        if (rb != null) rb.linearVelocity = Vector2.zero;

        animator.SetBool("isWalking", false);
        animator.SetBool("isPreparing", false);

        if (chargeCoroutine != null)
        {
            StopCoroutine(chargeCoroutine);
            chargeCoroutine = null;
        }
    }

    private void HandleFacing()
    {
        if (player == null) return;
        float dx = player.position.x - transform.position.x;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dx > 0 ? 1 : -1);
        transform.localScale = scale;
    }

    // [ContextMenu("Test Play Idle Sound")] // (Giữ lại các hàm Test nếu bạn muốn)
    // ...

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

        animator.SetTrigger("isDeath");

        if (deathSound != null && audioSource != null)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            audioSource.PlayOneShot(deathSound);
            Debug.Log(gameObject.name + " played death sound: " + deathSound.name);
        }

        // 🔥 --- BẮT ĐẦU LOGIC RỚT VÀNG ---
        // 'player' (Transform) đã được cache trong Start()
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Gọi hàm AddGold() của Player
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
            // Dự phòng nếu cache 'player' bị null
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                PlayerController playerController = playerObject.GetComponent<PlayerController>();
                if (playerController != null) playerController.AddGold(goldDropAmount);
            }
            else
            {
                Debug.LogError("Lỗi: Không tìm thấy Player trong Scene (Kiểm tra Tag 'Player')!");
            }
        }
        // 🔥 --- KẾT THÚC LOGIC RỚT VÀNG ---

        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 5f);

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