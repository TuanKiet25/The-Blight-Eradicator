using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
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
    [SerializeField] private int requiredPunchesToKill = 2;
    private float maxHealth;
    private float currentHealth;

    [Header("Animation Timings")]
    public float attackAnimationDuration = 1.0f;
    public float damageFrameTime = 0.3f;

    [Header("References")]
    public LayerMask playerLayer;

    [Header("Audio")]
    public AudioClip idleSound;
    public AudioClip walkSound;
    public AudioClip hurtSound;
    public AudioClip attackSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    private bool isDead = false;
    private bool isAttacking = false;
    private bool hasAppliedDamageThisAttack = false;
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

        audioSource.spatialBlend = 0f;
        audioSource.volume = Mathf.Clamp01(audioSource.volume <= 0f ? 0.8f : audioSource.volume);

        lastAttackTime = Time.time;

        maxHealth = requiredPunchesToKill * playerPunchDamage;
        currentHealth = maxHealth;
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

        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            FlipTowardsPlayer();

            if (Time.time - lastAttackTime >= attackCooldown)
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
            StopLoopSound();
        }
    }

    private void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();

        if (walkSound != null && audioSource != null)
        {
            PlayLoopSound(walkSound);
        }
    }

    private void FlipTowardsPlayer()
    {
        float directionX = player.position.x - transform.position.x;
        if (directionX > 0 && !isFacingRight) Flip();
        else if (directionX < 0 && isFacingRight) Flip();
    }

    private IEnumerator Attack()
    {
        hasAppliedDamageThisAttack = false;
        isAttacking = true;
        animator.SetTrigger("isAttacking");
        rb.linearVelocity = Vector2.zero;

        // ⏱️ Chờ tới khung animation vung tay
        yield return new WaitForSeconds(damageFrameTime);

        // ⚔️ Gây damage cho player tại đúng thời điểm
        ApplyDamageToPlayer();

        // 🎧 Phát âm thanh tấn công
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
            Debug.Log(gameObject.name + " played attack sound (Attack coroutine): " + attackSound.name);
        }

        // Chờ cho tới hết animation
        yield return new WaitForSeconds(attackAnimationDuration - damageFrameTime);

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    public void ApplyDamageToPlayer()
    {
        if (player == null || isDead || !isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange) return;

        bool isPlayerInFront = (player.position.x - transform.position.x > 0 && isFacingRight)
                            || (player.position.x - transform.position.x < 0 && !isFacingRight);

        if (isPlayerInFront)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null && !hasAppliedDamageThisAttack)
            {
                playerController.TakeDamage(attackDamage);
                hasAppliedDamageThisAttack = true;
                Debug.Log($"{gameObject.name} gây {attackDamage} damage lên Player!");
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        isAttacking = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        currentHealth -= dmg;
        Debug.Log(gameObject.name + " bị nhận " + dmg + " sát thương. Máu còn: " + currentHealth);

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

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger("isDeath");

        if (audioSource != null)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            if (deathSound != null) audioSource.PlayOneShot(deathSound);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        Destroy(gameObject, 2f);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void PlayLoopSound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        if (audioSource.isPlaying && audioSource.clip == clip) return;
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopLoopSound()
    {
        if (audioSource == null) return;
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.loop = false;
        audioSource.clip = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
