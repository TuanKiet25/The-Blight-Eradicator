using System.Collections;
using UnityEngine;

public class EnemyThanChet : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float detectRange = 6f;
    public float attackDamage = 15f;
    
    [Header("References")]
    public BossHealthBar healthBar; // Giữ nguyên, giả định có.

    [Header("Health")]
    [Tooltip("Sát thương Player gây ra trong 1 cú đấm. (Nên là 2f)")]
    [SerializeField] private float playerPunchDamage = 2f;
    [Tooltip("Số lần Player phải đấm để Enemy chết. (Cần là 5)")]
    [SerializeField] private int requiredPunchesToKill = 5;
    private float maxHealth;
    private float currentHealth;

    [Header("Animation Timings")]
    [Tooltip("Thời gian chặn Enemy di chuyển trong khi Attack")]
    public float attackAnimationDuration = 1.0f;
    
    [Header("References")]
    public LayerMask playerLayer;

    // 🔊 AUDIO DECLARATIONS
    [Header("Audio")]
    public AudioClip idleSound;
    public AudioClip attackSound;
    public AudioClip deathSound;
    private AudioSource audioSource; // Thêm private AudioSource

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

        // 🔊 SETUP AUDIO SOURCE
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        lastAttackTime = Time.time;

        // KHỞI TẠO MÁU
        maxHealth = requiredPunchesToKill * playerPunchDamage;
        currentHealth = maxHealth;

        if (player == null)
        {
            Debug.LogWarning("⚠️ Không tìm thấy Player! Hãy set Tag = 'Player' cho player GameObject");
        }
        
        // 🔊 BẮT ĐẦU IDLE SOUND (Áp dụng cho cả Idle và Walking)
        StartIdleSound();

        Debug.Log($"✅ Enemy {gameObject.name} đã khởi tạo! HP: {currentHealth}/{maxHealth}");
    }

    // ================== AUDIO HELPERS ==================

    // Bật Idle Sound (Loop)
    private void StartIdleSound()
    {
        if (idleSound != null && audioSource != null)
        {
            if (audioSource.clip != idleSound || !audioSource.isPlaying)
            {
                audioSource.clip = idleSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }
    
    // Dừng âm thanh Loop (Idle)
    private void StopLoopingSound()
    {
         if (audioSource != null && audioSource.isPlaying && audioSource.loop)
         {
             audioSource.Stop();
         }
    }

    // ============================================

    void Update()
    {
        if (isDead || player == null) return;
        
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // === ATTACK RANGE ===
        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            // 🔊 Giữ Idle/Walking Sound bằng StartIdleSound()
            animator.SetBool("isWalking", false); 
            StartIdleSound(); 
            FlipTowardsPlayer();

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(Attack());
            }
        }
        // === DETECT RANGE (CHASE) ===
        else if (distance <= detectRange)
        {
            MoveTowardsPlayer();
        }
        // === OUT OF RANGE ===
        else
        {
            rb.linearVelocity = Vector2.zero;
            // 🔊 Giữ Idle/Walking Sound
            animator.SetBool("isWalking", false);
            StartIdleSound(); 
        }
    }

    // ============================================
    // MOVE TOWARDS PLAYER - Di chuyển về phía player
    // ============================================
    private void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);
        StartIdleSound(); // 🔊 Đảm bảo âm thanh vẫn chạy khi di chuyển

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
    }

    // ============================================
    // ATTACK - Tấn công player
    // ============================================
    private IEnumerator Attack()
    {
        isAttacking = true;
        
        animator.SetTrigger("TgAttack");
        
        rb.linearVelocity = Vector2.zero;
        
        // 🔊 PHÁT ÂM THANH TẤN CÔNG (Dừng loop, phát OneShot)
        StopLoopingSound();
        if (attackSound != null) audioSource.PlayOneShot(attackSound);
        
        yield return new WaitForSeconds(attackAnimationDuration);

        isAttacking = false;
        lastAttackTime = Time.time;
        
        // Bắt đầu lại Idle Sound sau khi tấn công xong
        StartIdleSound(); 
    }

    // ============================================
    // TAKE DAMAGE - Nhận sát thương
    // ============================================
    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        isAttacking = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        // 🔊 KHÔNG CÓ HURTSOUND trong khai báo, nhưng nếu có thì code là:
        // if (hurtSound != null) audioSource.PlayOneShot(hurtSound);

        currentHealth -= dmg;
        Debug.Log($"💥 {gameObject.name} bị nhận {dmg} sát thương. Máu còn: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 🔑 LỖI CODE GỐC: Trigger "isHurt" không tồn tại. Đã xóa.
            // Giữ nguyên logic visual nếu cần: animator.SetTrigger("isHurt"); 
        }
        
        // Bắt đầu lại Idle Sound
        StartIdleSound();
    }

    // ============================================
    // DIE - Chết
    // ============================================
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger("TgDeath"); 
        
        // 🔊 PHÁT ÂM THANH CHẾT
        StopLoopingSound();
        if (deathSound != null) audioSource.PlayOneShot(deathSound);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log($"💀 {gameObject.name} đã chết!");

        Destroy(gameObject, 2f);

        this.enabled = false;
    }
    
    // ... (Giữ nguyên các hàm khác) ...
    // ============================================
    // FLIP TOWARDS PLAYER - Quay về phía player
    // ============================================
    private void FlipTowardsPlayer()
    {
        float directionX = player.position.x - transform.position.x;
        if (directionX > 0 && !isFacingRight) Flip();
        else if (directionX < 0 && isFacingRight) Flip();
    }
    
    // ... (Giữ nguyên ApplyDamageToPlayer, Flip, Gizmos) ...
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
                playerController.TakeDamage(attackDamage);
                Debug.Log($"⚔️ {gameObject.name} tấn công Player! Damage: {attackDamage}");
            }
        }
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}