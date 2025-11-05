using System.Collections;
using UnityEngine;

public class DaiController : MonoBehaviour
{
    [Header("Components")]
    private Animator animator;
    private Rigidbody2D rb;

    // ... (Giữ nguyên Stats, Health, Animation Timings) ...
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
    
    [Header("Audio")]
    public AudioClip idleSound;
    public AudioClip hurtSound; 
    public AudioClip attackSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    // Trạng thái nội bộ
    private Transform player;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isFacingRight = true;
    private float lastAttackTime = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        // Setup Audio Source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Khởi tạo Máu
        maxHealth = requiredPunchesToKill * playerPunchDamage;
        currentHealth = maxHealth;
        lastAttackTime = Time.time - attackCooldown; 
        
        // Bắt đầu Idle Sound ngay
        StartIdleSound();
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
            // Trong tầm tấn công: Dừng, Quay mặt, Tấn công
            StopMoving(); // Dừng chuyển động, nhưng audio vẫn chạy
            FlipTowardsPlayer();

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(Attack());
            }
        }
        else if (distance <= detectRange)
        {
            // Trong tầm phát hiện: Di chuyển (và giữ Idle Sound)
            MoveTowardsPlayer();
        }
        else
        {
            // Ngoài tầm phát hiện: Đứng yên (và giữ Idle Sound)
            StopMoving(); 
        }
    }

    // ================== AUDIO HELPERS ==================
    // Hàm này đảm bảo Idle Sound LUÔN CHẠY LOOP nếu nó không phải là âm thanh khác (Attack/Death)
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

    // ================== MOVEMENT ==================

    private void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);
        StartIdleSound(); // Đảm bảo âm thanh chạy khi đi bộ

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
    }

    private void StopMoving() 
    {
        animator.SetBool("isWalking", false);
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        StartIdleSound(); // Đảm bảo âm thanh chạy khi đứng yên
    }

    // ================== COMBAT ==================

    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking"); 
        rb.linearVelocity = Vector2.zero; 
        
        // PHÁT ÂM THANH TẤN CÔNG (Dừng Idle Sound vì Attack cần nghe rõ)
        StopLoopingSound();
        if (attackSound != null) audioSource.PlayOneShot(attackSound);

        yield return new WaitForSeconds(attackAnimationDuration);

        isAttacking = false;
        lastAttackTime = Time.time;
        
        // Bắt đầu lại Idle sound sau khi tấn công
        StartIdleSound(); 
    }
    
    public void ApplyDamageToPlayer()
    {
        // ... (Giữ nguyên logic gây sát thương) ...
        if (player == null || isDead || !isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange + 0.1f) return; 

        float directionToPlayer = player.position.x - transform.position.x;
        bool isPlayerInFront = (directionToPlayer > 0 && isFacingRight) ||
                               (directionToPlayer < 0 && !isFacingRight);

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

        isAttacking = false;
        StopMoving(); // Dừng chuyển động, audio vẫn chạy (nhưng sẽ bị dừng khi chơi hurt sound)

        // PHÁT ÂM THANH BỊ ĐÁNH
        // Tắt Idle Sound loop TẠM THỜI để PlayOneShot rõ ràng hơn, sau đó StartIdleSound() sẽ bật lại
        StopLoopingSound(); 
        if (hurtSound != null) audioSource.PlayOneShot(hurtSound);

        currentHealth -= dmg;
        Debug.Log(gameObject.name + " bị nhận " + dmg + " sát thương. Máu còn: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("isHurt"); // Giả định bạn có Trigger này
        }
        // Bắt đầu lại Idle sound sau khi hurt (sau khi PlayOneShot xong)
        StartIdleSound();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger("isDeath");
        
        // PHÁT ÂM THANH CHẾT (Yêu cầu dừng loop)
        StopLoopingSound();
        if (deathSound != null) audioSource.PlayOneShot(deathSound);
        
        // Vô hiệu hóa vật lý và collider
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        this.enabled = false;
        Destroy(gameObject, 2f);
    }

    // ... (Giữ nguyên FACING & GIZMOS) ...
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
        float directionX = player.position.x - transform.position.x;

        bool shouldBeFacingRight = directionX > 0;
        if (shouldBeFacingRight != isFacingRight) Flip();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}