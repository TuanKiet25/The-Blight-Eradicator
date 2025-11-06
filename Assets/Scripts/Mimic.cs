using System.Collections;
using UnityEngine;

public class MimicController : MonoBehaviour
{
    [Header("Stats")]
    public float attackRange = 1.5f;
    public float attackCooldown = 2.0f;
    public float attackDamage = 15f;

    [Header("Health")]
    [Tooltip("Máu tối đa của Mimic (set trong Inspector).")]
    [SerializeField] private float maxHealth = 50f;
    [Tooltip("Số lần Player phải đấm để Mimic chết (tùy chọn, không bắt buộc).")]
    [SerializeField] private int requiredPunchesToKill = 5; // chỉ để tham khảo
    private float currentHealth;

    // 🔥 --- THÊM LOGIC RỚT VÀNG ---
    [Header("Loot")]
    [Tooltip("Số lượng vàng rớt ra khi Mimic chết.")]
    public int goldDropAmount = 75; // Mimic rớt nhiều vàng
    // ---------------------------------

    [Header("Invulnerability")]
    [Tooltip("Khoảng thời gian bất khả sát thương ngay sau khi nhận hit (giây).")]
    [SerializeField] private float invulnerabilityDuration = 0.25f;
    private bool isInvulnerable = false;

    [Header("Animation Timings")]
    public float attackAnimationDuration = 1.2f;
    public float damageFrameTime = 0.5f;

    [Header("References")]
    public LayerMask playerLayer;

    [Header("Audio")]
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
    private float lastAttackTime = 0f;

    private bool isDiscovered = false;
    private bool isFacingRight = false;

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
        isDiscovered = false;

        // HP đã được gán đúng từ maxHealth
        currentHealth = maxHealth;

        if (rb != null) rb.bodyType = RigidbodyType2D.Static;
    }

    void Update()
    {
        if (isDead || player == null) return;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                if (!isDiscovered)
                {
                    StartCoroutine(JumpscareAttack());
                }
                else
                {
                    StartCoroutine(CombatAttack());
                }
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;
        if (isInvulnerable) return;

        isAttacking = false;

        currentHealth -= dmg;
        StartCoroutine(TemporaryInvulnerability(invulnerabilityDuration));

        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        if (animator != null)
        {
            animator.SetTrigger("isHurt");
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private IEnumerator TemporaryInvulnerability(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    public void ApplyDamageToPlayer()
    {
        if (player == null || isDead || !isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange) return;

        float directionX = player.position.x - transform.position.x;
        bool isPlayerInFront = isFacingRight ? (directionX > 0) : (directionX < 0);

        if (isPlayerInFront)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null && !hasAppliedDamageThisAttack)
            {
                playerController.TakeDamage(attackDamage);
                hasAppliedDamageThisAttack = true;
            }
        }
    }

    private IEnumerator JumpscareAttack()
    {
        isDiscovered = true;
        hasAppliedDamageThisAttack = false;
        isAttacking = true;

        if (animator != null) animator.SetTrigger("isAttacking");

        yield return new WaitForSeconds(damageFrameTime);
        ApplyDamageToPlayer();

        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound);

        yield return new WaitForSeconds(Mathf.Max(0f, attackAnimationDuration - damageFrameTime));

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    private IEnumerator CombatAttack()
    {
        hasAppliedDamageThisAttack = false;
        isAttacking = true;

        if (animator != null) animator.SetTrigger("isAttacking");

        yield return new WaitForSeconds(damageFrameTime);
        ApplyDamageToPlayer();

        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound);

        yield return new WaitForSeconds(Mathf.Max(0f, attackAnimationDuration - damageFrameTime));

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null) animator.SetTrigger("isDeath");

        if (rb != null) rb.simulated = false;

        Collider2D[] allColliders = GetComponents<Collider2D>();
        foreach (Collider2D col in allColliders) col.enabled = false;

        if (audioSource != null && deathSound != null) audioSource.PlayOneShot(deathSound);

        // 🔥 --- BẮT ĐẦU LOGIC RỚT VÀNG ---
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.AddGold(goldDropAmount);
                Debug.Log("Mimic rớt ra " + goldDropAmount + " vàng.");
            }
            else
            {
                Debug.LogError("Lỗi: Player object (từ cache) thiếu script PlayerController!");
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

        this.enabled = false;
        Destroy(gameObject, 2f);
    }

    // (Giữ nguyên các hàm còn lại: Flip, PlayLoopSound, StopLoopSound, OnDrawGizmosSelected)
    private void FlipTowardsPlayer() { }
    private void Flip() { }

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
        if (audioSource.isPlaying) audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}