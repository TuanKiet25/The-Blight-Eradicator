using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // ... (Các biến Stats, Health, Animation Timings giữ nguyên) ...
    [Header("Stats")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float detectRange = 6f;
    public float attackDamage = 10f;

    // ⚔️ HEALTH & SÁT THƯƠNG TỪ PLAYER
    [Header("Health")]
    [Tooltip("Sát thương Player gây ra trong 1 cú đấm. (Nên là 2f)")]
    [SerializeField] private float playerPunchDamage = 2f;
    [Tooltip("Số lần Player phải đấm để Enemy chết. (Cần là 2)")]
    [SerializeField] private int requiredPunchesToKill = 2; // Yêu cầu 2 đấm
    private float maxHealth;
    private float currentHealth;
    // ---------------------------------------------

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
    // Guard so we only apply damage/sound once per attack animation
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
        // Ensure 2D playback by default and sensible volume
        audioSource.spatialBlend = 0f;
        audioSource.volume = Mathf.Clamp01(audioSource.volume <= 0f ? 0.8f : audioSource.volume);

        lastAttackTime = Time.time;

        // 🔪 KHỞI TẠO MÁU: Máu = 2 * 2 = 4f
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
            // Player far -> stop any looped audio
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

        // Play walk loop
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
        // reset per-attack guard
        hasAppliedDamageThisAttack = false;
        isAttacking = true;
        animator.SetTrigger("isAttacking");
        rb.linearVelocity = Vector2.zero;

        // Wait until the damage frame, play attack one-shot there
        // Wait until the damage frame. The actual attack sound is played when damage is applied
        // in ApplyDamageToPlayer to ensure it lines up exactly with the hit event.
        yield return new WaitForSeconds(damageFrameTime);

        // Fallback: if the animation event or other system didn't call ApplyDamageToPlayer
        // (so we haven't applied damage/sound yet), play the attack sound here so the
        // player still hears feedback on the first hit.
        if (!hasAppliedDamageThisAttack)
        {
            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound);
                Debug.Log(gameObject.name + " played fallback attack sound (Attack coroutine): " + attackSound.name);
            }
            // mark applied so we don't double-apply
            hasAppliedDamageThisAttack = true;
        }

        // Wait the remainder of the attack animation
        yield return new WaitForSeconds(attackAnimationDuration - damageFrameTime);

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    public void ApplyDamageToPlayer()
    {
        if (player == null || isDead || !isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange) return;

        float directionToPlayer = player.position.x - transform.position.x;
        bool isPlayerInFront = (directionToPlayer > 0 && isFacingRight) ||
                               (directionToPlayer < 0 && !isFacingRight);

        if (isPlayerInFront)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Only apply damage once per attack animation/sequence
                if (!hasAppliedDamageThisAttack)
                {
                    playerController.TakeDamage(attackDamage);
                    hasAppliedDamageThisAttack = true;
                    // Play attack sound exactly at the moment damage is applied so it lines up
                    if (attackSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(attackSound);
                        Debug.Log(gameObject.name + " played attack sound (ApplyDamageToPlayer): " + attackSound.name);
                    }
                }
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

        // Play hurt sound if assigned
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
            Debug.Log(gameObject.name + " played hurt sound: " + hurtSound.name);
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

        // 1. Kích hoạt hoạt ảnh
        animator.SetTrigger("isDeath");

        // Stop looped audio and play death one-shot
        if (audioSource != null)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            if (deathSound != null) audioSource.PlayOneShot(deathSound);
        }

        // 2. Dừng vật lý ngay lập tức!
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            // 🔑 QUAN TRỌNG: Tắt Simulation để vô hiệu hóa trọng lực!
            rb.simulated = false;
        }

        // 3. Tắt Collider và script
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        // 4. 🚀 ĐÃ SỬA: Tăng thời gian chờ hủy để khớp với hoạt ảnh (ví dụ 2 giây)
        // Nếu hoạt ảnh chết của bạn dài hơn 2 giây, bạn cần tăng giá trị này.
        Destroy(gameObject, 2f);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // --- Audio helpers -------------------------------------------------
    private void PlayLoopSound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        if (audioSource.isPlaying && audioSource.clip == clip) return;
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
        Debug.Log(gameObject.name + " playing loop sound: " + clip.name);
    }

    private void StopLoopSound()
    {
        if (audioSource == null) return;
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log(gameObject.name + " stopped loop sound");
        }
        audioSource.loop = false;
        audioSource.clip = null;
    }

    [ContextMenu("Test Play Idle Sound")]
    private void TestPlayIdle()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (idleSound != null && audioSource != null)
        {
            PlayLoopSound(idleSound);
        }
        else Debug.LogWarning("Idle sound or AudioSource missing on " + gameObject.name);
    }

    [ContextMenu("Test Play Walk Sound")]
    private void TestPlayWalk()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (walkSound != null && audioSource != null)
        {
            PlayLoopSound(walkSound);
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

    [ContextMenu("Test Play Hurt Sound")]
    private void TestPlayHurt()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        else Debug.LogWarning("Hurt sound or AudioSource missing on " + gameObject.name);
    }

    [ContextMenu("Test Play Attack Sound")]
    private void TestPlayAttack()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
        else Debug.LogWarning("Attack sound or AudioSource missing on " + gameObject.name);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}