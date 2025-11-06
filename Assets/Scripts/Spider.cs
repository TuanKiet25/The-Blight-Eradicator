using System.Collections;
using UnityEngine;

public class Boss2Controller : MonoBehaviour
{
    // CÁC THÔNG SỐ CHUNG CỦA BOSS (Tham khảo từ BossController gốc)
    [Header("Stats")]
    public float moveSpeed = 3f;
    public float attackRange = 2.5f; // Tầm đánh thường/phun lửa
    public float detectRange = 10f; // Tầm phát hiện Player
    public float baseAttackCooldown = 2f; // Thời gian hồi chiêu cơ bản (áp dụng sau 1 đòn)
    public float attackDamage = 15f;
    public float specialDamage = 25f; // Sát thương đòn đặc biệt

    // 🔥 THÔNG SỐ TẤN CÔNG ĐẶC BIỆT VÀ TỈ LỆ
    [Header("Attack Logic")]
    [Range(0f, 1f)] public float fireSpitChance = 0.3f; // 30% tỉ lệ phun lửa
    [Range(0f, 1f)] public float jumpSlamChance = 0.3f; // 30% tỉ lệ nhảy đáp đất (40% còn lại là Attack 1)
    public float jumpLandingRadius = 4f; // Bán kính sát thương đáp đất của Jump Slam
    [Tooltip("Lực nhảy khi Boss thực hiện Jump Slam.")]
    public float jumpForceForSlam = 18f;

    // 🔥 THỜI GIAN ANIMATION
    [Header("Animation Timings")]
    public float attack1Duration = 1.0f;
    public float attack1DamageFrameTime = 0.5f;

    public float fireSpitDuration = 1.2f;
    public float fireSpitDamageFrameTime = 0.8f;

    public float jumpSlamDuration = 1.8f;
    public float jumpSlamDamageFrameTime = 1.6f;

    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    // 🔥 --- THÊM LOGIC RỚT VÀNG ---
    [Header("Loot")]
    [Tooltip("Số lượng vàng rớt ra khi Boss 2 chết.")]
    public int goldDropAmount = 150; // Boss 2 rớt nhiều hơn
    // ---------------------------------

    [Header("Audio")]
    public AudioClip attack1Sound;
    public AudioClip attack2Sound_Spit;
    public AudioClip attack2Sound_Land;
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

    // TRẠNG THÁI
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isFacingRight = true;
    private float lastAttackTime = 0f;

    private bool isInvulnerable = false; // Miễn sát thương khi nhảy

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.8f;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        currentHealth = maxHealth;
        lastAttackTime = Time.time;

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        if (isDead || player == null || isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            StopWalkSoundLoop();
            FlipTowardsPlayer();

            if (Time.time - lastAttackTime >= baseAttackCooldown)
                StartCoroutine(DecideAndAttack());
        }
        else if (distance <= detectRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            StopWalkSoundLoop();
        }
    }

    private IEnumerator DecideAndAttack()
    {
        isAttacking = true;
        animator.SetBool("isWalking", false);
        StopWalkSoundLoop();

        float rand = Random.value;
        float totalSpecialChance = fireSpitChance + jumpSlamChance;
        if (totalSpecialChance > 1f) totalSpecialChance = 1f;

        if (rand < fireSpitChance)
        {
            animator.SetTrigger("isFireSpitting");
            yield return StartCoroutine(WaitForAttackAnim(fireSpitDuration));
        }
        else if (rand < totalSpecialChance)
        {
            animator.SetTrigger("isJumpSlamming");
            yield return StartCoroutine(WaitForAttackAnim(jumpSlamDuration));
        }
        else
        {
            animator.SetTrigger("isAttacking1");
            yield return StartCoroutine(WaitForAttackAnim(attack1Duration));
        }

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    private IEnumerator WaitForAttackAnim(float duration)
    {
        yield return new WaitForSeconds(duration);
    }

    // (Giữ nguyên các hàm Animation Event của bạn)
    public void AnimationEvent_ApplyJumpForce()
    {
        if (isDead) return;
        isInvulnerable = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForceForSlam);
    }

    public void AnimationEvent_ApplyAttack1Damage()
    {
        if (isDead) return;
        if (attack1Sound != null) audioSource.PlayOneShot(attack1Sound);
        ApplyDamageInCone(attackDamage, attackRange);
    }

    public void AnimationEvent_ApplyFireSpitDamage()
    {
        if (isDead) return;
        if (attack2Sound_Spit != null) audioSource.PlayOneShot(attack2Sound_Spit);
        ApplyDamageInCone(specialDamage * 0.5f, attackRange * 1.5f);
    }

    public void AnimationEvent_ApplyJumpLandDamage()
    {
        if (isDead) return;
        isInvulnerable = false;

        if (attack2Sound_Land != null)
        {
            audioSource.Stop(); // Dừng âm thanh loop (nếu có)
            audioSource.loop = false;
            audioSource.PlayOneShot(attack2Sound_Land);
        }

        ApplyAreaDamage(specialDamage, jumpLandingRadius);
        rb.linearVelocity = Vector2.zero;
    }


    private void ApplyDamageInCone(float damage, float range)
    {
        if (player == null) return;
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > range) return;

        bool isPlayerInFront =
            (player.position.x - transform.position.x > 0 && isFacingRight) ||
            (player.position.x - transform.position.x < 0 && !isFacingRight);

        if (isPlayerInFront)
            player.GetComponent<PlayerController>()?.TakeDamage(damage);
    }

    private void ApplyAreaDamage(float damage, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            var playerController = hit.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                break;
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

        if (walkSound != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = walkSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void StopWalkSoundLoop()
    {
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == walkSound && audioSource.loop)
            audioSource.Stop();
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        // Giữ nguyên logic bất tử
        if (isInvulnerable)
        {
            Debug.Log("Boss 2 bất tử khi đang nhảy!");
            return;
        }

        currentHealth -= dmg;
        Debug.Log("Boss 2 nhận " + dmg + " sát thương. Máu còn: " + currentHealth);

        if (healthBar != null) healthBar.SetHealth(currentHealth);
        if (hurtSound != null) audioSource.PlayOneShot(hurtSound);

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger("isDeath");
        StopWalkSoundLoop();
        if (deathSound != null) audioSource.PlayOneShot(deathSound);
        if (healthBar != null) healthBar.gameObject.SetActive(false);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        GetComponent<Collider2D>().enabled = false;

        // 🔥 --- BẮT ĐẦU LOGIC RỚT VÀNG ---
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.AddGold(goldDropAmount);
                Debug.Log("Boss 2 rớt ra " + goldDropAmount + " vàng.");
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

        StartCoroutine(FadeAndDestroy(4f));
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

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, jumpLandingRadius);
    }
}