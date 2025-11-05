using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThuongHan : MonoBehaviour
{
    // ... (Giữ nguyên các khai báo Stats, Health, Animation Timings, References) ...
    [Header("Stats")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float detectRange = 6f;
    public float attackDamage = 10f;

    [Header("Health")]
    [SerializeField] private float playerPunchDamage = 2f;
    [SerializeField] private int requiredPunchesToKill = 2;
    private float maxHealth;
    private float currentHealth;

    [Header("Animation Timings")]
    public float attackAnimationDuration = 1.0f; // Vẫn cần dùng cho Coroutine

    [Header("References")]
    public LayerMask playerLayer;

    // 🔊 AUDIO DECLARATIONS
    [Header("Audio")]
    public AudioClip idleSound;
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

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

        // Bắt đầu phát âm thanh IDLE
        StartIdleSound();

        lastAttackTime = Time.time - attackCooldown;
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
            StopMoving(true); 
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
            StopMoving(false); 
        }
    }

    // ================== AUDIO HELPERS ==================

    private void StartIdleSound()
    {
        if (idleSound != null)
        {
            audioSource.clip = idleSound;
            audioSource.loop = true;
            if (!audioSource.isPlaying) audioSource.Play();
        }
    }

    private void StopAllLoopingSound()
    {
         if (audioSource.isPlaying && audioSource.loop)
         {
             audioSource.Stop();
         }
    }

    // ================== MOVEMENT ==================

    private void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);
        StopAllLoopingSound(); // Dừng Idle khi di chuyển

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
    }
    
    private void StopMoving(bool keepIdleSound) 
    {
        animator.SetBool("isWalking", false);
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (keepIdleSound)
        {
            StartIdleSound();
        }
        else
        {
             StopAllLoopingSound();
        }
    }


    private IEnumerator Attack()
    {
        isAttacking = true;
        // KHÔNG CÓ ANIMATION RIÊNG CHO ATTACK, NÊN NÓ SẼ CHƠI IDLE HOẶC DỪNG HẲN
        rb.linearVelocity = Vector2.zero; 
        
        // 🔊 PHÁT ÂM THANH TẤN CÔNG
        StopAllLoopingSound();
        if (attackSound != null) audioSource.PlayOneShot(attackSound);
        
        // Giả lập thời gian tấn công (Enemy bị đóng băng trong 1.0s)
        yield return new WaitForSeconds(attackAnimationDuration); 

        isAttacking = false;
        lastAttackTime = Time.time;
        
        // Bắt đầu lại Idle sound sau khi tấn công
        StartIdleSound();
    }

    // ... (Giữ nguyên ApplyDamageToPlayer) ...
    public void ApplyDamageToPlayer()
    {
        // ...
    }

    // ================== DAMAGE & DEATH ==================

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        isAttacking = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        // 🔊 PHÁT ÂM THANH BỊ ĐÁNH
        if (hurtSound != null)
        {
            // Âm thanh hurt (chơi một lần) có thể phát trong khi idle sound đang loop
            audioSource.PlayOneShot(hurtSound); 
        }
        
        currentHealth -= dmg;
        Debug.Log(gameObject.name + " bị nhận " + dmg + " sát thương. Máu còn: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // KHÔNG CÓ ANIM 'HURT', NÊN CHỈ PHÁT ÂM THANH
            // Enemy vẫn ở trạng thái Idle/Dừng.
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger("TgDeath"); 

        // 🔊 PHÁT ÂM THANH CHẾT
        StopAllLoopingSound(); 
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
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
    
    // ... (Giữ nguyên các hàm Flip, FlipTowardsPlayer, OnDrawGizmosSelected) ...

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void FlipTowardsPlayer()
    {
        float directionX = player.position.x - transform.position.x;
        if (directionX > 0 && !isFacingRight) Flip();
        else if (directionX < 0 && isFacingRight) Flip();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}