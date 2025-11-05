using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThanDichBenh : MonoBehaviour
{
    // ... (Giữ nguyên các khai báo Components, Audio, Stats, References) ...
    [Header("Components")]
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Audio")]
    public AudioClip idleSound;
    public AudioClip summonSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    private AudioSource audioSource; // Thêm private AudioSource

    [Header("Stats")]
    public float moveSpeed = 2f;
    public float chargeSpeed = 6f;
    public float chargeDelay = 1.2f;
    public float detectRange = 8f;
    public float summonRange = 3f;
    public float stopDistance = 0.4f;

    [Header("References")]
    public BossHealthBar healthBar;

    [Header("Summon Settings")]
    public GameObject summonPrefab;
    public Transform summonSpawnPoint;
    public Vector3 summonOffset = new Vector3(1f, 0f, 0f);
    public float summonCooldown = 5f;
    public int maxSummons = 2;
    public float summonAnimationDuration = 1f;

    private List<GameObject> activeSummons = new List<GameObject>(); 
    private float lastSummonTime = -999f;
    
    private Vector3 initialSummonPosition;
    private bool initialFacingRight;

    [Header("Health & Combat")]
    [SerializeField] private float playerPunchDamage = 2f;
    [SerializeField] private int requiredPunchesToKill = 5;
    private float maxHealth;
    private float currentHealth;

    [Header("State")]
    public bool isDead = false;
    private Transform player;
    private bool isFacingRight = true;
    private bool isChargingDelay = false;
    private bool isChargeMoving = false;
    private bool isSummoning = false;
    private Coroutine chargeCoroutine;

    // ... (Awake, StartIdleSound, StopLoopingSound giữ nguyên) ...
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        maxHealth = requiredPunchesToKill * playerPunchDamage;
        currentHealth = maxHealth;
        
        StartIdleSound();
    }
    
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
    
    private void StopLoopingSound()
    {
         if (audioSource != null && audioSource.isPlaying && audioSource.loop)
         {
             audioSource.Stop();
         }
    }


    void Update()
    {
        if (isDead || player == null) return;

        activeSummons.RemoveAll(summon => summon == null);

        float distance = Vector2.Distance(transform.position, player.position);

        if (isSummoning)
        {
            if (rb.linearVelocity != Vector2.zero) rb.linearVelocity = Vector2.zero; 
            return;
        }
        
        // 2. Trong tầm triệu hồi (summonRange) và đủ điều kiện
        if (distance <= summonRange)
        {
            StopChargeMovement(); 

            if (CanSummon())
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isWalking", false);
                StartCoroutine(PerformSummon());
                FlipTowardsPlayer();
            }
            return;
        }

        // 3. Trong tầm phát hiện (detectRange) -> Charge/Move
        if (distance <= detectRange)
        {
            if (!isChargingDelay && !isChargeMoving)
            {
                if (chargeCoroutine != null) StopCoroutine(chargeCoroutine);
                chargeCoroutine = StartCoroutine(ChargeDelayThenMove());
            }
        }
        // 4. Ngoài tầm phát hiện (Quay về trạng thái Idle)
        else
        {
            StopChargeMovement();
            animator.SetBool("isWalking", false);
        }

        HandleFacing();
        
        // 🔊 Đảm bảo Idle Sound chạy (áp dụng cho cả Idle và Walking)
        StartIdleSound(); 
    }

    // ================== LOGIC SUMMON ==================

    private bool CanSummon()
    {
        return !isSummoning &&
                Time.time - lastSummonTime >= summonCooldown &&
                activeSummons.Count < maxSummons &&
                summonPrefab != null;
    }

    private IEnumerator PerformSummon()
    {
        isSummoning = true;
        
        initialSummonPosition = transform.position;
        initialFacingRight = isFacingRight; 
        
        rb.linearVelocity = Vector2.zero;

        // 🔑 SỬ DỤNG TRIGGER MỚI: TgSummon
        animator.SetTrigger("TgSummon"); 
        animator.SetBool("isWalking", false);
        
        // 🔊 PHÁT ÂM THANH SUMMON
        StopLoopingSound();
        if (summonSound != null) audioSource.PlayOneShot(summonSound);
        
        yield return new WaitForSeconds(summonAnimationDuration);

        SpawnSummonedEnemy();

        lastSummonTime = Time.time;
        isSummoning = false;
        
        // 🔊 Tiếp tục Idle Sound
        StartIdleSound();

        Debug.Log($"ThanDichBenh đã triệu hồi Dai! Tổng số: {activeSummons.Count}/{maxSummons}");
    }

    // ... (SpawnSummonedEnemy giữ nguyên) ...

    public void SpawnSummonedEnemy()
    {
        if (summonPrefab == null || activeSummons.Count >= maxSummons) return;

        Vector3 spawnPosition;
        if (summonSpawnPoint != null)
        {
            spawnPosition = summonSpawnPoint.position; 
        }
        else
        {
            Vector3 offset = summonOffset;
            if (!initialFacingRight)
            {
                offset.x = -offset.x;
            }
            spawnPosition = initialSummonPosition + offset; 
        }

        GameObject summonedDai = Instantiate(summonPrefab, spawnPosition, Quaternion.identity);

        activeSummons.Add(summonedDai);

        if (summonedDai != null)
        {
            Vector3 daiScale = summonedDai.transform.localScale;
            daiScale.x = Mathf.Abs(daiScale.x) * (initialFacingRight ? 1 : -1); 
            summonedDai.transform.localScale = daiScale;
        }
    }


    // ================== LOGIC MOVEMENT (CHARGE) ==================

    private IEnumerator ChargeDelayThenMove()
    {
        isChargingDelay = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isWalking", false);
        
        // 🔊 Giữ Idle Sound trong thời gian delay.

        float elapsed = 0f;
        while (elapsed < chargeDelay)
        {
             if (player == null) yield break;
             float dist = Vector2.Distance(transform.position, player.position);

             if (dist <= summonRange || dist > detectRange)
             {
                 isChargingDelay = false;
                 yield break;
             }
             elapsed += Time.deltaTime;
             yield return null;
        }

        isChargingDelay = false;
        isChargeMoving = true;
        // 🔑 SỬ DỤNG BOOL MỚI: isWalking
        animator.SetBool("isWalking", true); 
        
        // 🔊 Idle Sound tiếp tục chạy trong Charge Move

        while (isChargeMoving && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            if (dist > detectRange || dist <= summonRange)
                break;

            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * chargeSpeed;

            HandleFacing();

            yield return null;
        }

        StopChargeMovement();
    }

    private void StopChargeMovement()
    {
        isChargeMoving = false;
        isChargingDelay = false;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 🔑 SỬ DỤNG BOOL MỚI: isWalking
        animator.SetBool("isWalking", false); 
        
        // 🔊 Tiếp tục Idle Sound
        StartIdleSound(); 

        if (chargeCoroutine != null)
        {
            StopCoroutine(chargeCoroutine);
            chargeCoroutine = null;
        }
    }

    // ================== LOGIC HEALTH & FACING ==================
    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        StopChargeMovement();
        isSummoning = false; 

        // 🔊 PHÁT ÂM THANH BỊ ĐÁNH
        StopLoopingSound();
        if (hurtSound != null) audioSource.PlayOneShot(hurtSound);

        currentHealth -= dmg;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 🔑 SỬ DỤNG TRIGGER MỚI: TgHurt
            animator.SetTrigger("TgHurt"); 
        }
        
        // 🔊 Tiếp tục Idle Sound
        StartIdleSound();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        StopChargeMovement();
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // 🔑 SỬ DỤNG TRIGGER MỚI: TgDeath
        animator.SetTrigger("TgDeath"); 
        
        // 🔊 PHÁT ÂM THANH CHẾT
        StopLoopingSound();
        if (deathSound != null) audioSource.PlayOneShot(deathSound);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 5f);

        this.enabled = false;
    }
    
    // ... (Giữ nguyên các hàm còn lại) ...
    private void HandleFacing()
    {
        if (player == null) return;
        float dx = player.position.x - transform.position.x;

        bool shouldBeFacingRight = dx > 0;

        if (shouldBeFacingRight != isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void FlipTowardsPlayer()
    {
        if (player == null) return;
        HandleFacing();
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, summonRange);

        // ... (Logic Gizmos giữ nguyên) ...
    }
}