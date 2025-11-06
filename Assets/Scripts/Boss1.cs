using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 2f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float attackDamage = 15f;
    public int attacksPerPhase = 3;
    public float restTimeBetweenPhases = 6f;

    [Tooltip("Phạm vi Boss bắt đầu đuổi theo Player.")]
    public float detectRange = 8f;

    [Header("Health")]
    public float maxHealth = 50f;
    private float currentHealth;

    // 🔥 --- THÊM LOGIC RỚT VÀNG ---
    [Header("Loot")]
    [Tooltip("Số lượng vàng rớt ra khi Boss chết.")]
    public int goldDropAmount = 100; // Boss rớt nhiều vàng hơn
    // ---------------------------------

    [Header("Animation Timings")]
    public float attackAnimationDuration = 1.2f;
    public float damageFrameTime = 0.4f;

    [Header("Audio")]
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip walkSound;
    private AudioSource audioSource;

    [Header("References")]
    public LayerMask playerLayer;
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    public BossHealthBar healthBar;

    private bool isDead = false;
    private bool isAttacking = false;
    private bool isResting = false;
    private bool isFacingRight = true;
    private float lastAttackTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        audioSource.spatialBlend = 0f;
        audioSource.volume = Mathf.Clamp01(audioSource.volume <= 0f ? 0.8f : audioSource.volume);

        // Biến 'player' đã được cache ở đây, rất tốt!
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        currentHealth = maxHealth;
        lastAttackTime = Time.time;

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        if (isDead || player == null || isResting) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (!isAttacking)
        {
            if (distance <= attackRange) // 1. TRONG TẦM TẤN CÔNG (STOP)
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isWalking", false);
                StopWalkSoundLoop();
                FlipTowardsPlayer();

                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    StartCoroutine(AttackPhase());
                }
            }
            else if (distance <= detectRange) // 2. TRONG TẦM PHÁT HIỆN (CHASE)
            {
                MoveTowardsPlayer();
            }
            else // 3. NGOÀI TẦM PHÁT HIỆN (IDLE/STANDBY)
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isWalking", false);
                StopWalkSoundLoop();
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();

        if (walkSound != null && audioSource != null && !audioSource.isPlaying && !isAttacking && !isResting)
        {
            audioSource.clip = walkSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void StopWalkSoundLoop()
    {
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == walkSound && audioSource.loop)
        {
            audioSource.Stop();
        }
    }

    private IEnumerator AttackPhase()
    {
        isAttacking = true;
        animator.SetBool("isWalking", false);

        for (int i = 0; i < attacksPerPhase; i++)
        {
            if (isDead) yield break;

            animator.SetTrigger("isAttacking");
            rb.linearVelocity = Vector2.zero;

            StopWalkSoundLoop();

            yield return new WaitForSeconds(damageFrameTime);
            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound);
            }

            ApplyDamageToPlayer();

            yield return new WaitForSeconds(attackAnimationDuration - damageFrameTime);
            yield return new WaitForSeconds(attackCooldown);
        }

        isAttacking = false;
        StartCoroutine(RestPhase());
    }

    private IEnumerator RestPhase()
    {
        isResting = true;
        animator.SetBool("isWalking", false);
        StopWalkSoundLoop();

        yield return new WaitForSeconds(restTimeBetweenPhases);
        isResting = false;
    }

    public void ApplyDamageToPlayer()
    {
        if (player == null || isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange) return;

        bool isPlayerInFront = (player.position.x - transform.position.x > 0 && isFacingRight)
                             || (player.position.x - transform.position.x < 0 && !isFacingRight);

        if (isPlayerInFront)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage);
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        Debug.Log("Boss nhận " + dmg + " sát thương. Máu còn: " + currentHealth);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("isHurt");
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger("isDeath");
        StopWalkSoundLoop();

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        GetComponent<Collider2D>().enabled = false;

        // 🔥 --- BẮT ĐẦU LOGIC RỚT VÀNG ---
        // 'player' (Transform) đã được cache trong Start()
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Gọi hàm AddGold() của Player
                playerController.AddGold(goldDropAmount);
                Debug.Log("Boss rớt ra " + goldDropAmount + " vàng.");
            }
            else
            {
                Debug.LogError("Lỗi: Player object thiếu script PlayerController!");
            }
        }
        else
        {
            // Dự phòng nếu cache 'player' bị null (mặc dù nó không nên)
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

        StartCoroutine(FadeAndDestroy(4f)); // 4 giây sau mới xoá
    }

    private IEnumerator FadeAndDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}