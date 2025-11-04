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
<<<<<<< HEAD
    [SerializeField] private int requiredPunchesToKill = 5;
=======
    [SerializeField] private int requiredPunchesToKill = 5; // Yêu cầu 5 đấm
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    private float maxHealth;
    private float currentHealth;

    [Header("References")]
    public LayerMask playerLayer;
<<<<<<< HEAD
    
    [Header("Audio")]
    public AudioClip idleSound;
    public AudioClip walkSound;
    public AudioClip deathSound;
    private AudioSource audioSource;
=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;

    private bool isDead = false;
    private bool isChargingDelay = false;
    private bool isChargeMoving = false;
    private Coroutine chargeCoroutine;

<<<<<<< HEAD
=======
    private bool isAttacking = false;
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    private float lastAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
<<<<<<< HEAD
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Do not auto-play idle on spawn. Idle will resume when enemy is stopped
        // and the player is nearby (handled in StopChargeMovement()).
=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc

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
<<<<<<< HEAD
=======
        isAttacking = true;
        animator.SetTrigger("isAttacking");

>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(attackDamage);
        }

<<<<<<< HEAD
=======
        isAttacking = false;
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
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

<<<<<<< HEAD
        // Play walk loop (switch from idle)
        if (walkSound != null && audioSource != null)
        {
            audioSource.clip = walkSound;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log(gameObject.name + " playing walk sound: " + walkSound.name);
        }


=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
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

<<<<<<< HEAD
        // Switch back to idle or stop audio — only resume idle if player is nearby
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

=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
        if (rb != null) rb.linearVelocity = Vector2.zero;

        animator.SetBool("isWalking", false);
        animator.SetBool("isPreparing", false);

        if (chargeCoroutine != null)
        {
            StopCoroutine(chargeCoroutine);
            chargeCoroutine = null;
        }
<<<<<<< HEAD
=======
        isAttacking = false;
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    }

    private void HandleFacing()
    {
        if (player == null) return;
        float dx = player.position.x - transform.position.x;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dx > 0 ? 1 : -1);
        transform.localScale = scale;
    }

<<<<<<< HEAD
    // Optional: small context menu tests from Inspector
    [ContextMenu("Test Play Idle Sound")]
    private void TestPlayIdle()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (idleSound != null && audioSource != null)
        {
            audioSource.clip = idleSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        else Debug.LogWarning("Idle sound or AudioSource missing on " + gameObject.name);
    }

    [ContextMenu("Test Play Walk Sound")]
    private void TestPlayWalk()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (walkSound != null && audioSource != null)
        {
            audioSource.clip = walkSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        else Debug.LogWarning("Walk sound or AudioSource missing on " + gameObject.name);
    }

    [ContextMenu("Test Play Death Sound")]
    private void TestPlayDeath()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        else Debug.LogWarning("Death sound or AudioSource missing on " + gameObject.name);
    }

=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
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

<<<<<<< HEAD
        animator.SetTrigger("isDeath");

        // Play death sound (one-shot)
        if (deathSound != null && audioSource != null)
        {
            // stop loop
            if (audioSource.isPlaying) audioSource.Stop();
            audioSource.PlayOneShot(deathSound);
            Debug.Log(gameObject.name + " played death sound: " + deathSound.name);
        }

        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 5f);

=======
        // 🔑 ĐÃ SỬA: Gọi Trigger isDeath mới!
        animator.SetTrigger("isDeath");

        // ⚠️ LƯU Ý: Nếu vẫn còn isDead (Bool) trong Parameters, bạn phải xóa nó.
        // Tắt Collider để các đối tượng khác có thể đi qua xác Enemy đã chết
        GetComponent<Collider2D>().enabled = false;

        // Hủy đối tượng sau 5 giây
        Destroy(gameObject, 5f);

        // Tắt script này (nên đặt cuối cùng)
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
        this.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
