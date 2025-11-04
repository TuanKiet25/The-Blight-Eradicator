using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float maxEnergy = 50;
    [SerializeField] private float dashEnergyCost = 10;
    [SerializeField] private float PunchEnergyCost = 2;
    [SerializeField] private float DoubleJumpEnergyCost = 5;
    [SerializeField] private float energyRegenRate = 5f;
    [SerializeField] private int maxLives = 4;

    private int currentLives;
    private float currentHealth;
    private float currentEnergy;

    [Header("UI Elements")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private Image heartLivesImage;
    [SerializeField] private Sprite[] heartSprites;

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float runSpeed = 8.0f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 20.0f;
    [SerializeField] private int maxJumps = 1;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    [Header("Attacking")]
    [SerializeField] private float attackDuration = 0.5f;

    [Header("Attack Properties")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Dashing")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.5f;
    [Header("Colliders")]
    [SerializeField] private Collider2D standingCollider;
    [SerializeField] private Collider2D deathCollider;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip punchSound;
    public AudioClip runSound;
    public AudioClip dieSound;

    private Animator animator;
    private bool isGrounded;
    private Rigidbody2D rb;
    private bool isRunning;
    private float moveInput;
    private bool isDashing;
    private bool isAttacking = false;
    private bool isDead = false;
    private int jumpCount;
    private bool hasPlayedRunSound = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        standingCollider.enabled = true;
        deathCollider.enabled = false;

        currentLives = maxLives;
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;

        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;

        energySlider.maxValue = maxEnergy;
        energySlider.value = currentEnergy;

        jumpCount = maxJumps;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isDead) return;

        if (isDashing || isAttacking) return;

        moveInput = Input.GetAxis("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);

        HandleMovement();
        HandleJump();
        HandleDashInput();
        HandlePunchAttackInput();
        RegenEnergy(energyRegenRate * Time.deltaTime);

        // Test damage
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10f);
            Debug.Log("Đã nhận 10 sát thương! Máu còn: " + currentHealth);
        }

        UpdateAnimation();
    }

    private void HandleMovement()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);

        if (moveInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            PlayRunSoundOnce();
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            PlayRunSoundOnce();
        }
        else
        {
            hasPlayedRunSound = false;
        }
    }

    private void PlayRunSoundOnce()
    {
        if (!hasPlayedRunSound && runSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(runSound);
            hasPlayedRunSound = true;
        }
    }

    private void HandleJump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (isGrounded)
        {
            jumpCount = maxJumps;
        }
        if (Input.GetButtonDown("Jump"))
        {
            if (jumpCount > 0)
            {
                jumpCount--;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);

                if (jumpSound != null) audioSource.PlayOneShot(jumpSound);

                if (!isGrounded)
                {
                    animator.SetTrigger("isDoubleJumping");
                    UseEnergy(DoubleJumpEnergyCost);
                }
            }
        }
    }
    private void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && currentEnergy >= dashEnergyCost)
        {
            UseEnergy(dashEnergyCost);
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;

        if (dashSound != null) audioSource.PlayOneShot(dashSound);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    private void HandlePunchAttackInput()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking && currentEnergy >= PunchEnergyCost)
        {
            UseEnergy(PunchEnergyCost);
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking");

        if (punchSound != null) audioSource.PlayOneShot(punchSound);

        yield return new WaitForSeconds(0.1f);
        PunchDamage();

        float waitTimeRemaining = attackDuration - 0.1f;
        if (waitTimeRemaining > 0)
        {
            yield return new WaitForSeconds(waitTimeRemaining);
        }

        isAttacking = false;
    }

    private void PunchDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // ✅ Đổi sang Boss1
            var boss1 = enemy.GetComponent<BossController>();
            if (boss1 != null)
            {
                boss1.TakeDamage(PunchEnergyCost);
                continue;
            }

            var enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(PunchEnergyCost);
                continue;
            }

            var rangedEnemyController = enemy.GetComponent<RangedEnemyController>();
            if (rangedEnemyController != null)
            {
                rangedEnemyController.TakeDamage(PunchEnergyCost);
                continue;
            }

            var enemy1Controller = enemy.GetComponent<Enemy1Controller>();
            if (enemy1Controller != null)
            {
                enemy1Controller.TakeDamage(PunchEnergyCost);
            }
        }
    }

    public void UseEnergy(float amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        energySlider.value = currentEnergy;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        hpSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            currentLives--;
            if (currentLives <= 0)
            {
                HandleDie();
            }
            else
            {
                animator.SetTrigger("isLostHeart");
                currentHealth = maxHealth;
                hpSlider.value = currentHealth;
                UpdateHealthUI();
            }
        }
    }
    private void HandleDie()
    {
        isDead = true;
        animator.SetTrigger("isDeath");
        standingCollider.enabled = false;
        deathCollider.enabled = true;

        if (dieSound != null) audioSource.PlayOneShot(dieSound);

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    private void UpdateHealthUI()
    {
        int spriteIndex = maxLives - currentLives;
        if (spriteIndex >= 0 && spriteIndex < heartSprites.Length)
        {
            heartLivesImage.sprite = heartSprites[spriteIndex];
        }
    }

    private void RegenEnergy(float amount)
    {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        energySlider.value = currentEnergy;
    }
    private void UpdateAnimation()
    {
        bool isMoving = moveInput != 0;
        bool isJumping = !isGrounded;
        animator.SetBool("isWalking", isMoving && !isRunning);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isRunning", isMoving && isRunning);
        animator.SetBool("isDashing", isDashing);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    /// <summary>
    /// Dừng toàn bộ chuyển động, vật lý và animation.
    /// Dùng khi player bị stun, chết hoặc reset scene.
    /// </summary>
    public void StopAllMovement()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();

        // 1️⃣ Dừng vật lý ngay lập tức
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 2️⃣ Reset lại input (để phòng lỗi di chuyển sau khi dừng)
        moveInput = 0f;
        isRunning = false;
        isDashing = false;
        isAttacking = false;

        // 3️⃣ Reset lại animation (ngừng mọi hoạt ảnh di chuyển)
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isJumping", false);
        animator.SetBool("isDashing", false);
    }
}
